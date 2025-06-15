
using System.Collections.Concurrent;
using System.IO.Pipes;
using Windows.Media.Control;

class Program
{
    private static MediaControlCore mediaControl;
    private static ConcurrentQueue<GlobalSystemMediaTransportControlsSessionMediaProperties> mediaPropertiesQueue = new ConcurrentQueue<GlobalSystemMediaTransportControlsSessionMediaProperties>();
    static void Main()
    {
        mediaControl = new MediaControlCore();
        
        using (var client = new NamedPipeClientStream(".", "audio_info", PipeDirection.InOut, PipeOptions.Asynchronous))
        {
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
                
            // 等待 reader 发出停止信号
            Task.WaitAny(readerTask);
            cancellationTokenSource.Cancel();

            // 确保两个任务结束
            Task.WaitAll(writerTask, readerTask);
            Console.WriteLine("Client stopped.");
        }
    }

    static async Task WriterLoop(NamedPipeClientStream client, CancellationToken token)
    {
        using (var writer = new StreamWriter(client) { AutoFlush = true })
        {
            while (!token.IsCancellationRequested)
            {
                while (mediaPropertiesQueue.TryDequeue(out var props))
                {
                    await writer.WriteLineAsync(props.Title ?? "No Title");
                }
                await Task.Delay(500, token);
            }
        }
    }

    static async Task ReaderLoop(NamedPipeClientStream client, CancellationTokenSource cancelSource)
    {
        using (var reader = new StreamReader(client))
        {
            while (!cancelSource.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync();
                if (line != null)
                {
                    if (line.Trim().Equals("STOP", StringComparison.OrdinalIgnoreCase))
                    {
                        cancelSource.Cancel();
                        break;
                    }
                    
                    Console.WriteLine("Received: " + line);
                }
            }
        }
    }
}
