// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    public async Task<string?> ReceiveMessageAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Pipe not connected.");

        return await _reader.ReadLineAsync();
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
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
}