
class Program
{
    static async Task Main()
    {
        using var server = new MediaClient();

        Console.WriteLine("Waiting for client to connect...");
        await server.WaitForConnectionAsync();

        Console.WriteLine("Client connected!");

        var cts = new CancellationTokenSource();

        var receiveTask = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                var msg = await server.ReceiveMessageAsync();
                if (msg != null)
                {
                    Console.WriteLine("Received: " + msg);
                    if (msg.Trim().Equals("STOP", StringComparison.OrdinalIgnoreCase))
                    {
                        cts.Cancel();
                    }
                    else
                    {
                        await server.SendMessageAsync("Echo: " + msg);
                    }
                }
            }
        });

        await receiveTask;
        Console.WriteLine("Server shutting down.");
    }
}