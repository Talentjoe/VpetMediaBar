using System.Collections.Concurrent;
using System.IO.Pipes;
using Newtonsoft.Json;
using Windows.Media.Control;
using MediaServer;

class Program
{
    private static MediaControlCore mediaControl;
    private static ConcurrentQueue<GlobalSystemMediaTransportControlsSessionMediaProperties> mediaPropertiesQueue = new ConcurrentQueue<GlobalSystemMediaTransportControlsSessionMediaProperties>();
    private static ConcurrentQueue<GlobalSystemMediaTransportControlsSessionPlaybackInfo> playbackQueue = new ConcurrentQueue<GlobalSystemMediaTransportControlsSessionPlaybackInfo>();
    private static ConcurrentQueue<GlobalSystemMediaTransportControlsSessionTimelineProperties> timelinePropertiesQueue = new ConcurrentQueue<GlobalSystemMediaTransportControlsSessionTimelineProperties>();

    static async Task Main(string[] arg)
    {
        mediaControl = new MediaControlCore();
        
        var controlStream = new NamedPipeServerStream(arg[0]+"_control", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        var mediaPropertiesStream = new NamedPipeServerStream(arg[0]+"_media_properties", PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        var playbackInfoStream = new NamedPipeServerStream(arg[0]+"_playback_info", PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        var timelinePropertiesStream = new NamedPipeServerStream(arg[0]+"_timeline_properties", PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

        try
        {
            await controlStream.WaitForConnectionAsync();
            await mediaPropertiesStream.WaitForConnectionAsync();
            await playbackInfoStream.WaitForConnectionAsync();
            await timelinePropertiesStream.WaitForConnectionAsync();
            
            Console.WriteLine("Client connected.");

            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            var controlStreamTask = Task.Run(() => ReaderLoop(controlStream, cancellationTokenSource), token);
            var mediaPropertiesTask = Task.Run(() => MediaPropertiesWriterLoop(mediaPropertiesStream, token), token);
            var playbackInfoTask = Task.Run(() => PlaybackInfoWriterLoop(playbackInfoStream, token), token);
            var timelinePropertiesTask = Task.Run(() => TimelinePropertiesWriterLoop(timelinePropertiesStream, token), token);

            mediaControl.MediaPropertiesChanged += (props) =>
            {
                if (props != null)
                {
                    mediaPropertiesQueue.Enqueue(props);
                }
            };
            
            mediaControl.PlaybackInfoChanged += (props) =>
            {
                if (props != null)
                {
                    playbackQueue.Enqueue(props);
                }
            };
            
            mediaControl.TimelinePropertiesChanged += (props) =>
            {
                if (props != null)
                {
                    timelinePropertiesQueue.Enqueue(props);
                }
            };

            var initProps = mediaControl.GetCurrentMediaProperties();
            mediaPropertiesQueue.Enqueue(initProps);
            var initPlaybackInfo = mediaControl.CurrentSession.GetPlaybackInfo();
            playbackQueue.Enqueue(initPlaybackInfo);
            var initTimelineProps = mediaControl.CurrentSession.GetTimelineProperties();
            timelinePropertiesQueue.Enqueue(initTimelineProps);
            

            await Task.WhenAny(controlStreamTask);
            await cancellationTokenSource.CancelAsync();

            await Task.WhenAll(controlStreamTask, playbackInfoTask, timelinePropertiesTask, mediaPropertiesTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled exception: " + ex);
        }
        finally
        {
            await controlStream.DisposeAsync();
            await mediaPropertiesStream.DisposeAsync();
            await playbackInfoStream.DisposeAsync();
            await timelinePropertiesStream.DisposeAsync();
            Console.WriteLine("Client stopped.");
        }
    }

    static async Task DeserializeAnyAndSent<T>(T data, StreamWriter streamWriter)
    {
        
        var message = JsonConvert.SerializeObject(data, Formatting.Indented);
                    
        message = message.Replace("\r", "");
        message = message.Replace("\n", "");

        Console.WriteLine("Sending... ");

        try
        {
            await streamWriter.WriteLineAsync(message);
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Writer: Pipe closed.");
            return;
        }
    }
    
    static async Task MediaPropertiesWriterLoop(NamedPipeServerStream client, CancellationToken token)
    {
        try
        {
            var writer = new StreamWriter(client) { AutoFlush = true };

            while (!token.IsCancellationRequested)
            {
                while (mediaPropertiesQueue.TryDequeue(out var props))
                {
                    var mediaData = new MediaPropertiesSerializableDataGenerator(props,mediaControl.CurrentSession.SourceAppUserModelId);
                    await DeserializeAnyAndSent(mediaData, writer);
                }

                await Task.Delay(500, token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[WriterLoop error] " + ex);
        }
    }
    
    static async Task PlaybackInfoWriterLoop(NamedPipeServerStream client, CancellationToken token)
    {
        try
        {
            var writer = new StreamWriter(client) { AutoFlush = true };

            while (!token.IsCancellationRequested)
            {
                while (playbackQueue.TryDequeue(out var props))
                {
                    var mediaData = new PlayBackInfoSerializableDataGenerator(props);
                    await DeserializeAnyAndSent(mediaData, writer);
                }
                await Task.Delay(500, token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[WriterLoop error] " + ex);
        }
    }
    
    static async Task TimelinePropertiesWriterLoop(NamedPipeServerStream client, CancellationToken token)
    {
        try
        {
            var writer = new StreamWriter(client) { AutoFlush = true };

            while (!token.IsCancellationRequested)
            {
                while (timelinePropertiesQueue.TryDequeue(out var props))
                {
                    var mediaData = new TimelinePropertiesSerializableDataGenerator(props);
                    await DeserializeAnyAndSent(mediaData, writer);
                }

                await Task.Delay(500, token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[WriterLoop error] " + ex);
        }
    }
    
    
    static async Task WriterLoop(NamedPipeClientStream client, CancellationToken token)
    {
        try
        {
            using var writer = new StreamWriter(client) { AutoFlush = true };

            while (!token.IsCancellationRequested)
            {
                while (mediaPropertiesQueue.TryDequeue(out var props))
                {
                    var mediaData = new MediaPropertiesSerializableDataGenerator(props,mediaControl.CurrentSession.SourceAppUserModelId);
                    var message = JsonConvert.SerializeObject(mediaData, Formatting.Indented);
                    
                    message = message.Replace("\r", "");
                    message = message.Replace("\n", "");

                    Console.WriteLine("Sending... ");

                    try
                    {
                        await writer.WriteLineAsync(message);
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine("Writer: Pipe closed.");
                        return;
                    }
                }

                await Task.Delay(500, token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[WriterLoop error] " + ex);
        }
    }

    static async Task ReaderLoop(NamedPipeServerStream client, CancellationTokenSource cancelSource)
    {
        try
        {
            using var reader = new StreamReader(client);
            while (!cancelSource.IsCancellationRequested)
            {
                string? line;
                try
                {
                    line = await reader.ReadLineAsync();
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("Reader: Pipe closed.");
                    break;
                }

                if (line == null) break;

                Console.WriteLine("Received: " + line);

                var trimmed = line.Trim().ToUpperInvariant();

                if (trimmed == "STOP")
                {
                    cancelSource.Cancel();
                    break;
                }

                try
                {
                    switch (trimmed)
                    {
                        case "START_STOP_PLAY":
                            var status = mediaControl.CurrentSession?.GetPlaybackInfo()?.PlaybackStatus;
                            if (status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                                await mediaControl.CurrentSession?.TryPlayAsync();
                            else
                                await mediaControl.CurrentSession?.TryPauseAsync();
                            break;

                        case "NEXT":
                            await mediaControl.CurrentSession?.TrySkipNextAsync();
                            break;

                        case "PREVIOUS":
                            await mediaControl.CurrentSession?.TrySkipPreviousAsync();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Playback control error: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ReaderLoop error] " + ex);
        }
        finally
        {
            cancelSource.Cancel();
        }
    }
}
