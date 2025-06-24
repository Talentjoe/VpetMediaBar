
using System.Diagnostics.CodeAnalysis;

class Program
{
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: ReadWriteValueTaskSource; size: 1018MB")]
    static async Task Main()
    {
        using var server = new MediaClient();

       // MediaClient.StartServer(@"D:\Coding\VpetMediaBar\MediaServer\bin\Release\net8.0-windows10.0.19041.0\MediaServer.exe");

        Console.WriteLine("Waiting for client to connect...");
        await server.WaitForConnectionAsync();

        Console.WriteLine("Client connected!");

        var cts = new CancellationTokenSource();

        server.StartListeningAsync();
        
        server.OnMediaInfoReceived += (mediaInfo) =>
        {
            if (mediaInfo == null)
                return;

            Console.WriteLine($"Title: {mediaInfo.Title}");
            Console.WriteLine($"Artist: {mediaInfo.Artist}");
            Console.WriteLine($"Album: {mediaInfo.AlbumTitle}");
            Console.WriteLine($"Thumbnail Base64: {mediaInfo.ThumbnailBase64}");
        };

        while (true)
        {
            var a = Console.ReadLine();
            if(a == "exit") 
            {
                cts.Cancel();
                break;
            }
            else
            {
                Console.WriteLine("Unknown command. Type 'exit' to quit or 'stop' to stop the server.");
            }
        }
        Console.WriteLine("Server shutting down.");
    }
}