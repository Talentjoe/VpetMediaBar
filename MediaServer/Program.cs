using System.Collections.Concurrent;
using System.IO.Pipes;
using Windows.Media.Control;
using Windows.Storage.Streams;

class Program
{
    private static MediaControlCore mediaControl;
    private static ConcurrentQueue<GlobalSystemMediaTransportControlsSessionMediaProperties> mediaPropertiesQueue = new ConcurrentQueue<GlobalSystemMediaTransportControlsSessionMediaProperties>();

    static async Task Main()
    {
        mediaControl = new MediaControlCore();
        NamedPipeClientStream client = null;

        try
        {
            client = new NamedPipeClientStream(".", "audio_info", PipeDirection.InOut, PipeOptions.Asynchronous);
            client.Connect();
            Console.WriteLine("Client connected.");

            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            var writerTask = Task.Run(() => WriterLoop(client, token), token);
            var readerTask = Task.Run(() => ReaderLoop(client, cancellationTokenSource), token);

            mediaControl.MediaPropertiesChanged += (props) =>
            {
                if (props != null)
                {
                    mediaPropertiesQueue.Enqueue(props);
                }
            };

            var initProps = mediaControl.GetCurrentMediaProperties();
            if (initProps != null)
                mediaPropertiesQueue.Enqueue(initProps);

            await Task.WhenAny(readerTask);
            cancellationTokenSource.Cancel();

            await Task.WhenAll(writerTask, readerTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled exception: " + ex);
        }
        finally
        {
            client?.Dispose();
            Console.WriteLine("Client stopped.");
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
                    string message = $"Title: {props.Title}, Artist: {props.Artist}, Album: {props.AlbumTitle}";

                    if (props.Thumbnail != null)
                    {
                        try
                        {
                            var thumbnailStream = await props.Thumbnail.OpenReadAsync();
                            if (thumbnailStream != null)
                            {
                                message += $", Thumbnail Size: {thumbnailStream.Size} bytes";

                                using var dataReader = new DataReader(thumbnailStream);
                                uint size = (uint)thumbnailStream.Size;

                                await dataReader.LoadAsync(size);
                                byte[] buffer = new byte[size];
                                dataReader.ReadBytes(buffer);

                                string thumbnailBase64 = Convert.ToBase64String(buffer);
                                message += $", Thumbnail Base64: {thumbnailBase64}";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[Thumbnail error] " + ex.Message);
                        }
                    }

                    Console.WriteLine("Sending: " + message);

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

    static async Task ReaderLoop(NamedPipeClientStream client, CancellationTokenSource cancelSource)
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
