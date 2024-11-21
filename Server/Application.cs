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

        private static Dictionary<int,(List<ulong> deliveryTags, Spot message)> messageBatch = new Dictionary<int, (List<ulong>, Spot)>();
        private static readonly object batchLock = new object();

        private static int batchSizeThreshold = 10;
        private static int batchTimeIntervalMs = 1000;

        private static bool isProcessingBatch = false;
        private static bool resetTimer = false;

        string queueName = "DemoQueue";

        public Application(IMainService mainService, IHubContext<NotificationHub> hubContext)
        {
            this.mainService = mainService;
            this.hubContext = hubContext;
        }

        public async Task Run()
        {
            ConnectionFactory factory = new();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Rabbit Receiver App";

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queueName, false, false, false);
            channel.BasicQos(0, (ushort)batchSizeThreshold, false);


            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                ulong deliveryTag = ea.DeliveryTag;
                var spot = JsonConvert.DeserializeObject<Spot>(message) ?? throw new Exception("spot is null");

                bool shouldProcessBatch = false;

                lock (batchLock)
                {
                    if (messageBatch.ContainsKey(spot.Id)) 
                    {
                        var messageFromBatch = messageBatch[spot.Id];
                        messageFromBatch.deliveryTags.Add(deliveryTag);

                        messageBatch[spot.Id] = (messageFromBatch.deliveryTags, spot);
                    }
                    else
                    {
                        messageBatch[spot.Id] = (new List<ulong> { deliveryTag }, spot);
                    }
                    Console.WriteLine($"Added message to batch: '{message}'");

                    if (messageBatch.Count >= batchSizeThreshold && !isProcessingBatch)
                    {
                        isProcessingBatch = true;
                        shouldProcessBatch = true;
                    }
                }

                if (shouldProcessBatch)
                {
                    await ProcessBatchAsync(channel);
                    lock (batchLock)
                    {
                        resetTimer = true;
                    }
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            await StartBatchTimerAsync(channel);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private async Task ProcessBatchAsync(IModel channel)
        {
            List<Spot> messages;
            List<ulong> deliveryTags;

            // Safely copy the batch to process
            lock (batchLock)
            {
                if (messageBatch.Count == 0 || isProcessingBatch == false)
                {
                    return;
                }

                messages = new List<Spot>();
                deliveryTags = new List<ulong>();

                foreach (var message in messageBatch)
                {
                    messages.Add(message.Value.Item2);
                    deliveryTags.AddRange(message.Value.Item1);
                }

                messageBatch.Clear();
            }

            Console.WriteLine("\nProcessing batch:");
            try
            {
                await ConsumerLogicAsync(messages);

                foreach (var deliveryTag in deliveryTags)
                {
                    channel.BasicAck(deliveryTag, multiple: false);
                }
                Console.WriteLine("Batch processed and all messages acknowledged.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing batch: {ex.Message}");
            }
            finally
            {
                lock (batchLock)
                {
                    isProcessingBatch = false;
                }
            }
        }

        private async Task StartBatchTimerAsync(IModel channel)
        {
            int timerInterval = batchTimeIntervalMs;

            while (true)
            {
                while (timerInterval > 0)
                {
                    await Task.Delay(100); // Sleep to allow reset
                    lock (batchLock)
                    {
                        if (resetTimer)
                        {
                            Console.WriteLine("\nBatch processed, resetting the timer...");
                            timerInterval = batchTimeIntervalMs;
                            resetTimer = false;
                        }
                    }

                    timerInterval -= 100;
                }

                bool shouldProcessBatch = false;

                lock (batchLock)
                {
                    if (messageBatch.Count > 0 && !isProcessingBatch)
                    {
                        isProcessingBatch = true;
                        shouldProcessBatch = true;
                    }
                }

                if (shouldProcessBatch)
                {
                    await ProcessBatchAsync(channel);
                    lock (batchLock)
                    {
                        resetTimer = true;
                    }
                }

                timerInterval = batchTimeIntervalMs;
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

            spots.ForEach(spot => {
                var elementFromDb = databaseSpots.FirstOrDefault(dbSpot => spot.Id == dbSpot.Id);
                spot.Longitude = elementFromDb.Longitude;
                spot.Latitude = elementFromDb.Latitude;
            });

            await hubContext.Clients.All.SendAsync("BatchReceiveMessage", spots);

            Console.WriteLine($"SignalR result : good");
        }
    }
}
