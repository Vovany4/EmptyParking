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

            using var cnn = factory.CreateConnection();
            using var channel = cnn.CreateModel();

            string queueName = "DemoQueue";
            int timeIntervalToWaitSec = 30;
            uint maxMessagesToConsume = 100;

            while (true)
            {
                uint messagesToConsume = 0;
                var waitTillMessagesConsuming = true;
                var consumerTag = string.Empty;
                var startTime = DateTime.Now;
                var aggregatedMessages = 0;

                while ((DateTime.Now - startTime).TotalSeconds < timeIntervalToWaitSec)
                {
                    var queueDeclareOk = channel.QueueDeclare(queueName, false, false, false);
                    messagesToConsume = queueDeclareOk.MessageCount;

                    Console.WriteLine($"Current message count in the queue '{queueName}': {messagesToConsume}");

                    if (messagesToConsume >= maxMessagesToConsume)
                    {
                        Console.WriteLine($"Message count is {messagesToConsume} or more, starting to consume messages.");
                        break;
                    }

                    // Wait for a short period before checking again
                    Thread.Sleep(1000);
                }

                if (messagesToConsume > 0)
                {
                    channel.BasicQos(0, 1, false);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (sender, args) =>
                    {
                        aggregatedMessages++;

                        if (aggregatedMessages >= messagesToConsume)
                        {
                            channel.BasicCancel(consumerTag); // Stop consuming new messages until process collected
                            waitTillMessagesConsuming = false;
                        }

                        await ConsumerLogicAsync(channel, args);
                    };

                    consumerTag = channel.BasicConsume(queueName, false, consumer);

                    WaitTillMessagesConsuming(ref waitTillMessagesConsuming);
                }
            }
        }

        private static void WaitTillMessagesConsuming(ref bool waitTillMessagesConsuming)
        {
            while (waitTillMessagesConsuming)
            {
                Thread.Sleep(100); // Small delay to avoid busy waiting
            }
        }

        private async Task ConsumerLogicAsync(IModel channel, BasicDeliverEventArgs args)
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

                var databaseSpot = await mainService.GetParkSpotAsync(spot.Id) ?? throw new Exception("databaseSpot is null");
                Console.WriteLine($"Database Spot:");
                Console.WriteLine($"{nameof(databaseSpot.Longitude)}: {databaseSpot.Longitude}");
                Console.WriteLine($"{nameof(databaseSpot.Latitude)}: {databaseSpot.Latitude}");

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

            channel.BasicAck(args.DeliveryTag, false);
        }
    }
}
