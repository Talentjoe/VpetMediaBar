using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Mime;
using System.Text;

public class MediaClient : IDisposable
{
    
    private NamedPipeServerStream _pipeStream;
    private StreamReader _reader;
    private StreamWriter _writer;
    private bool _isConnected = false;
    private readonly string _pipeName;

    public MediaClient(string pipeName = "audio_info")
    {
        _pipeName = pipeName;
        _pipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
    }

    public async Task WaitForConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_isConnected)
            throw new InvalidOperationException("Pipe already connected.");

        await _pipeStream.WaitForConnectionAsync(cancellationToken);
        _reader = new StreamReader(_pipeStream, Encoding.UTF8);
        _writer = new StreamWriter(_pipeStream, Encoding.UTF8) { AutoFlush = true };
        _isConnected = true;
    }

    private async Task<string?> ReceiveMessageAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        return await _reader.ReadLineAsync();
    }
    
    public class MediaInfo
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public long ThumbnailSize { get; set; }
        public string ThumbnailBase64 { get; set; }
        
    }

    public event Action<MediaInfo> OnMediaInfoReceived;
    
    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        while (!_pipeStream.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
        }

        while (_isConnected && !cancellationToken.IsCancellationRequested)
        {
            var message = await ReceiveMessageAsync(cancellationToken);
            if (message == null) continue;

            var parts = message.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                var mediaInfo = new MediaInfo
                {
                    Title = parts[0].Trim(),
                    Artist = parts[1].Trim(),
                    Album = parts[2].Trim(),
                    ThumbnailSize = 0,
                    ThumbnailBase64 = ""
                };
                OnMediaInfoReceived?.Invoke(mediaInfo);
            }
            else
            {
                var mediaInfo = new MediaInfo
                {
                    Title = parts[0].Trim(),
                    Artist = parts[1].Trim(),
                    Album = parts[2].Trim(),
                    ThumbnailSize = 1,
                    ThumbnailBase64 = parts.Length > 4 ? parts[4].Trim() : string.Empty
                };

                OnMediaInfoReceived?.Invoke(mediaInfo);
                
            }

        }
    }
    

    private async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        await _writer.WriteLineAsync(message);
    }

    public void Dispose()
    {
        try { _writer?.Dispose(); } catch { }
        try { _reader?.Dispose(); } catch { }
        try { _pipeStream?.Dispose(); } catch { }
    }

    public static async void StartServer(String path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine("Directory not found: " + path);
        }
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            Process process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start process.");
                return;
            }
            Console.WriteLine($"启动成功: {path}");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
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