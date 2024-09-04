using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using Models;
using Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Server
{
    public class Application : IApplication
    {
        private readonly IMainService mainService;
        private readonly IHubContext<NotificationHub> hubContext;
        public Application(IMainService mainService, IHubContext<NotificationHub> hubContext)
        {
            this.mainService = mainService;
            this.hubContext = hubContext; 
        }

        public void Run()
        {
            ConnectionFactory factory = new();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Rabbit Receiver App";

            using (var cnn = factory.CreateConnection())
            using (var channel = cnn.CreateModel())
            {
                string queueName = "DemoQueue";
                while (true)
                {
                    int messagesToConsume = 10;
                    int consumedMessages = 0;
                    string consumerTag = string.Empty;

                    // Poll the queue until there are 10 messages
                    while (true)
                    {
                        var queueDeclareOk = channel.QueueDeclare(queueName, false, false, false);
                        channel.BasicQos(0, 1, false);
                        var messageCount = queueDeclareOk.MessageCount;

                        Console.WriteLine($"Current message count in the queue '{queueName}': {messageCount}");

                        if (messageCount >= messagesToConsume)
                        {
                            Console.WriteLine($"Message count is {messagesToConsume} or more, starting to consume messages.");
                            break;
                        }

                        // Wait for a short period before checking again
                        Thread.Sleep(1000); // Adjust the polling interval as needed
                    }


                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (sender, args) =>
                    {
                        await ConsumerLogicAsync(args);

                        channel.BasicAck(args.DeliveryTag, false);
                        consumedMessages++;

                        if (consumedMessages >= messagesToConsume)
                        {
                            Console.WriteLine("Processed 10 messages, stopping consumption.");
                            channel.BasicCancel(consumerTag); // Stop consuming after 10 messages
                        }
                    };

                    consumerTag = channel.BasicConsume(queueName, false, consumer);

                    // Wait until 10 messages have been consumed before continuing the loop
                    while (consumedMessages < messagesToConsume)
                    {
                        Thread.Sleep(100); // Small delay to avoid busy waiting
                    }
                }
            }
        }

        private async Task ConsumerLogicAsync(BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();

            string message = Encoding.UTF8.GetString(body);

            var spot = JsonConvert.DeserializeObject<Spot>(message);

            if (spot is not null)
            {
                Console.WriteLine($"Message Received:");
                Console.WriteLine($"{nameof(spot.Id)}: {spot.Id}");
                Console.WriteLine($"{nameof(spot.IsEmpty)} : {spot.IsEmpty}");

                var result = await mainService.UpdateIsEmptyParkSpotAsync(spot);
                Console.WriteLine($"Update result : {result}");

                var databaseSpot = await mainService.GetParkSpotAsync(spot.Id);
                Console.WriteLine($"Database Spot:");
                Console.WriteLine($"{nameof(spot.Longitude)}: {spot.Longitude}");
                Console.WriteLine($"{nameof(spot.Latitude)}: {spot.Latitude}");

                await hubContext.Clients.All.SendAsync(
                    "ReceiveMessage",
                    spot.Id,
                    spot.IsEmpty,
                    databaseSpot.Latitude,
                    databaseSpot.Longitude,
                    spot.TimeStamp);

                Console.WriteLine($"SignalR result : good");
            }
            else
            {
                Console.WriteLine($"Spot is null");
            }
        }
    }
}
