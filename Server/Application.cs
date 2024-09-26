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
            var spotList = new List<Spot>();

            int timeIntervalToWaitSec = 10;
            uint maxMessagesToConsume = 20;

            while (true)
            {
                uint messagesToConsume = 0;
                var waitTillMessagesConsuming = true;
                var consumerTag = string.Empty;
                var startTime = DateTime.Now;
                var messagesAtBatch = 0;

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
                    channel.BasicQos(0, (ushort)messagesToConsume, false);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (sender, args) =>
                    {
                        if (messagesAtBatch + 1 >= messagesToConsume)
                        {
                            channel.BasicCancel(consumerTag); // Stop consuming new messages until process collected
                        }

                        var body = args.Body.ToArray();
                        string message = Encoding.UTF8.GetString(body);
                        var spot = JsonConvert.DeserializeObject<Spot>(message) ?? throw new Exception("spot is null");

                        if (messagesAtBatch < messagesToConsume)
                        {
                            spotList.Add(spot);
                            messagesAtBatch++;
                        }

                        if (messagesAtBatch >= messagesToConsume)
                        {
                            await ConsumerLogicAsync(spotList);

                            channel.BasicAck(args.DeliveryTag, true); // Acknowledge aggregated messages
                            spotList.Clear();
                            waitTillMessagesConsuming = false;
                        }
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

        private async Task ConsumerLogicAsync(List<Spot> spots)
        {
            Console.WriteLine($"Message Received:");

            foreach (var spot in spots)
            {
                var result = await mainService.UpdateIsEmptyParkSpotAsync(spot);
                Console.WriteLine($"Update result : {result}");
            }

            var databaseSpots = await mainService.GetParkSpotsAsync(spots.Select(spot => spot.Id).ToList());

            databaseSpots.ForEach(dbSpot => dbSpot.TimeStamp = spots.FirstOrDefault(spot => spot.Id == dbSpot.Id)?.TimeStamp);

            await hubContext.Clients.All.SendAsync("BatchReceiveMessage", databaseSpots);

            Console.WriteLine($"SignalR result : good");
        }
    }
}
