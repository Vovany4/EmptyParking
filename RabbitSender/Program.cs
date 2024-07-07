using Models;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

ConnectionFactory factory = new ();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
factory.ClientProvidedName = "Rabbit Sender App";

IConnection cnn = factory.CreateConnection();

IModel channel = cnn.CreateModel();

string exchangeName = "DemoExchange";
string routingKey = "demo-routing-key";
string queueName = "DemoQueue";

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare(queueName, false, false, false);
channel.QueueBind(queueName, exchangeName, routingKey, null);

/*for (int i = 0; i < 60; i++)
{*/

var spot = new Spot
{ 
    Id = 3,
    IsEmpty = true
};

var jsonSpot = JsonConvert.SerializeObject(spot);

Console.WriteLine($"Message send: {jsonSpot}");
byte[] messageBodyBytes = Encoding.UTF8.GetBytes(jsonSpot);
channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
Thread.Sleep(1000);

//}

channel.Close();
cnn.Close();