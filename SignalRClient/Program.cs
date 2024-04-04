using Microsoft.AspNetCore.SignalR.Client;

internal class Program
{
    private static void Main(string[] args)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/chatHub")
            .Build();

        connection.StartAsync().Wait();
        connection.InvokeCoreAsync("SendMessage", args: new[] { "Vova", "Hello" });

        connection.On("ReceiveMessage", (string userName, string message) =>
        {
            Console.WriteLine(userName + ":" + message);
        });

        Console.ReadLine();
    }
}