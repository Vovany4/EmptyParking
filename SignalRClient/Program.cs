using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5007/chatHub")
    .Build();

connection.StartAsync().Wait();
connection.InvokeCoreAsync("SendMessage", args: new[] { "Vova", "Hello" });

connection.On("ReceiveMessage", (string userName, string message) =>
{
    Console.WriteLine(userName + ":" + message);
});

Console.ReadLine();