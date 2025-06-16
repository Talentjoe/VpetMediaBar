
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Net.Mime;
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
            
            mediaPropertiesQueue.Enqueue(mediaControl.GetCurrentMediaProperties());
                
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
                    string message = $"Title: {props.Title}, Artist: {props.Artist}, Album: {props.AlbumTitle}";
                    var thumbnail = props.Thumbnail;
                    
                    Console.WriteLine($"Sending message: {message}");
                    
                    if (thumbnail != null)
                    {
                        var thumbnailStream = await thumbnail.OpenReadAsync();
                        if (thumbnailStream != null)
                        {
                            message += $", Thumbnail Size: {thumbnailStream.Size} bytes";
                            using (var dataReader = new Windows.Storage.Streams.DataReader(thumbnailStream))
                            {
                                uint size = (uint)thumbnailStream.Size;
                                await dataReader.LoadAsync(size);
                                byte[] buffer = new byte[size];
                                dataReader.ReadBytes(buffer);

                                // 将字节数组转换为 Base64 字符串
                                string thumbnailBase64 = Convert.ToBase64String(buffer);
                                message += $", Thumbnail Base64: {thumbnailBase64}";
                            }
                            
                        }
                    }
                    await writer.WriteLineAsync(message);
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
                    
                    if (line.Trim().Equals("START_STOP_PLAY", StringComparison.OrdinalIgnoreCase))
                    {
                        var playbackStatus = mediaControl.CurrentSession?.GetPlaybackInfo()?.PlaybackStatus;
                        
                        if (playbackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                            mediaControl.CurrentSession?.TryPlayAsync();
                        else
                             mediaControl.CurrentSession?.TryPauseAsync();
                    }
                    if (line.Trim().Equals("NEXT", StringComparison.OrdinalIgnoreCase))
                    {
                        mediaControl.CurrentSession?.TrySkipNextAsync();
                    }
                    if (line.Trim().Equals("PREVIOUS", StringComparison.OrdinalIgnoreCase))
                    {
                        mediaControl.CurrentSession?.TrySkipPreviousAsync();
                    }
                    
                    Console.WriteLine("Received: " + line);
                }
            }
        }
    }
}
