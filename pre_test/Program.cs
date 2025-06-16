
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

        var receiveTask = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(1000, cts.Token);
                
                         
                // var msg = await server.ReceiveMessageAsync();
                // if (msg != null)
                // {
                //     Console.WriteLine("Received: " + msg);
                //     if (msg.Trim().Equals("STOP", StringComparison.OrdinalIgnoreCase))
                //     {
                //         cts.Cancel();
                //     }
                //     else
                //     {
                //         await server.SendMessageAsync("Echo: " + msg);
                //     }
                // }
            }
        });

        await receiveTask;
        Console.WriteLine("Server shutting down.");
    }
}