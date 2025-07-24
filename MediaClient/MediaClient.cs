using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using MediaClientDataInterFace;

namespace MediaClient;

public class MediaClient : IDisposable
{
    private readonly NamedPipeClientStream _controlStream;
    private readonly NamedPipeClientStream _mediaPropertiesStream;
    private readonly NamedPipeClientStream _playbackInfoStream;
    private readonly NamedPipeClientStream _timelinePropertiesStream;
    private StreamWriter _controlWriter;
    private StreamReader _mediaPropertiesReader;
    private StreamReader _playbackInfoReader;
    private StreamReader _timelinePropertiesReader;
    
    public event Action<MediaPropertiesSerializableData> OnMediaInfoReceived;
    public event Action<PlayBackInfoSerializableData> OnPlaybackInfoReceived;
    public event Action<TimelinePropertiesSerializableData> OnTimelinePropertiesReceived;
    
    
    private bool _isConnected = false;

    public static MediaClient CreateMediaClientWithServerStart(string path)
    {
        var pipeName = StartServer(path).GetAwaiter().GetResult();
        return new MediaClient(pipeName);
    }

    public MediaClient(string pipeName)
    {
        _controlStream = new NamedPipeClientStream(".", pipeName+"_control", PipeDirection.Out, PipeOptions.Asynchronous);
        _mediaPropertiesStream =new NamedPipeClientStream(".", pipeName+"_media_properties", PipeDirection.In, PipeOptions.Asynchronous); 
        _playbackInfoStream = new NamedPipeClientStream(".", pipeName+"_playback_info", PipeDirection.In, PipeOptions.Asynchronous); 
        _timelinePropertiesStream = new NamedPipeClientStream(".", pipeName+"_timeline_properties", PipeDirection.In, PipeOptions.Asynchronous);    
    }

    public async Task WaitForConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_isConnected)
            throw new InvalidOperationException("Pipe already connected.");

        await _controlStream.ConnectAsync(cancellationToken);
        await _mediaPropertiesStream.ConnectAsync(cancellationToken);
        await _playbackInfoStream.ConnectAsync(cancellationToken);
        await _timelinePropertiesStream.ConnectAsync(cancellationToken);
        _controlWriter = new StreamWriter(_controlStream, Encoding.UTF8) { AutoFlush = true };
        _mediaPropertiesReader = new StreamReader(_mediaPropertiesStream, Encoding.UTF8);
        _playbackInfoReader = new StreamReader(_playbackInfoStream, Encoding.UTF8);
        _timelinePropertiesReader = new StreamReader(_timelinePropertiesStream, Encoding.UTF8);
        _isConnected = true;
    }

    private async Task<string?> ReceiveMessageAsync(StreamReader reader ,CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        return await reader.ReadLineAsync(cancellationToken);
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        _ = StartListeningMediaPropertiesAsync(cancellationToken);
        _ = StartListeningTimelinePropertiesAsync(cancellationToken);
        _ = StartListeningPlaybackPropertiesAsync(cancellationToken);
    }
    
    public async Task StartListeningMediaPropertiesAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");
        await StartListeningAsync<MediaPropertiesSerializableData>(_mediaPropertiesReader, cancellationToken);
    }
    
    public async Task StartListeningTimelinePropertiesAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");
        await StartListeningAsync<TimelinePropertiesSerializableData>(_timelinePropertiesReader, cancellationToken);
    }
    
    public async Task StartListeningPlaybackPropertiesAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");
        await StartListeningAsync<PlayBackInfoSerializableData>(_playbackInfoReader, cancellationToken);
    }
    
    private async Task StartListeningAsync<T>(StreamReader reader,CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        while (_isConnected && !cancellationToken.IsCancellationRequested)
        {
            var message = await ReceiveMessageAsync(reader,cancellationToken);
            if (message == null) continue;

            try
            {
                T? data =
                    JsonConvert.DeserializeObject<T>(message);
                if (data == null)
                {
                    Console.Write("Received invalid media data.");
                    continue;
                }

                if (data is MediaPropertiesSerializableData media)
                {
                    OnMediaInfoReceived?.Invoke(media);
                }
                else if (data is PlayBackInfoSerializableData playback)
                {
                    OnPlaybackInfoReceived?.Invoke(playback);
                }
                else if (data is TimelinePropertiesSerializableData timeline)
                {
                    OnTimelinePropertiesReceived?.Invoke(timeline);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    

    private async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        await _controlWriter.WriteLineAsync(message);
    }

    public void Dispose()
    {
        try { _controlWriter?.Dispose(); } catch { }
        try { _controlStream?.Dispose(); } catch { }
        try { _mediaPropertiesReader?.Dispose(); } catch { }
        try { _mediaPropertiesStream?.Dispose(); } catch { }
        try { _playbackInfoReader?.Dispose(); } catch { }
        try { _playbackInfoStream?.Dispose(); } catch { }
        try { _timelinePropertiesReader?.Dispose(); } catch { }
        try { _timelinePropertiesStream?.Dispose(); } catch { }
    }

    private static async Task<string> StartServer(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine("Directory not found: " + path);
        }
        try
        {
            var arguments = "m_info_" + Random.Shared.Next().ToString();
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = arguments,
            };
            
            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start process.");
                return "";
            }
            Console.WriteLine($"启动成功: {path}");
            return arguments;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        return "";
    }
    
    public async Task SentStopOrStartAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        await SendMessageAsync("START_STOP_PLAY", cancellationToken);
    }
    
    public async Task SentNextAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        await SendMessageAsync("NEXT", cancellationToken);
    }
    
    public async Task SentPrevAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        await SendMessageAsync("PREVIOUS", cancellationToken);
    }
    
    public async Task SentStopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        await SendMessageAsync("STOP", cancellationToken);
    }
}