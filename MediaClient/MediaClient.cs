using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using MediaClientDataInterFace;

namespace MediaClient;

public class MediaClient : IDisposable
{
    
    private NamedPipeServerStream _pipeStream;
    private StreamReader _reader;
    private StreamWriter _writer;
    private bool _isConnected = false;
    private readonly string _pipeName;

    public MediaClient(string path)
    {
        var pipeName=StartServer(path).GetAwaiter().GetResult();
        _pipeName = pipeName;
        _pipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
    }
    
    public MediaClient(string pipeName, int mod)
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

    public event Action<MediaPropertiesSerializableData> OnMediaInfoReceived;
    
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

            try
            {
                MediaPropertiesSerializableData? mediaData =
                    JsonConvert.DeserializeObject<MediaPropertiesSerializableData>(message);
                if (mediaData == null)
                {
                    Console.Write("Received invalid media data.");
                    continue;
                }
                OnMediaInfoReceived?.Invoke(mediaData);

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

        await _writer.WriteLineAsync(message);
    }

    public void Dispose()
    {
        try { _writer?.Dispose(); } catch { }
        try { _reader?.Dispose(); } catch { }
        try { _pipeStream?.Dispose(); } catch { }
    }

    private async Task<String> StartServer(String path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine("Directory not found: " + path);
        }
        try
        {
            var arguments = "m_info_" + Random.Shared.Next().ToString();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = arguments,
            };
            
            Process process = Process.Start(startInfo);
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