using System;
using System.Runtime;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

class Program
{
    const string ORDER_QUEUE = "orders";

    static AmazonSQSClient _client = null!;
    static string _queueUrl = "https://sqs.us-east-1.amazonaws.com/905418171130/sqstest"!;
    static Random _random = new Random();

    // Specify number of orders to create on the command line (default: 1)

    static async Task Main(string[] args)
    {
        //var orderCount = args.Length > 0 ? Convert.ToInt32(args[0]) : 0;
        var orderCount = 2;

        Console.WriteLine("Connecting to SQS");


        var credentials = new BasicAWSCredentials("accessKey", "secretKey");

        //using (var sqsClient = new AmazonSQSClient(credentials, RegionEndpoint.USEast1)) //RegionEndpoint.USEast1 is a region name


        //    var config = new AmazonSQSConfig()
        //{
        //    RegionEndpoint = Amazon.RegionEndpoint.USWest1,
        //    };
        _client = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);

        //_queueUrl = await GetOrCreateQueue();

        // ReceiveMessagefromSQSAsync();

        if (orderCount > 0)
        {
            Console.WriteLine("Generating orders");
            for (int orderNo = 1; orderNo <= orderCount; orderNo++)
            {
                var order = GenerateRandomOrder(orderNo.ToString());
                Console.WriteLine($"Order {order?.Id}, {order?.Items.Count} items");
                var message = JsonSerializer.Serialize(order);
                Console.WriteLine(message);
                await SendMessage(message);
            }
        }
    }

    // Create orders queue if it doesn't exist, and return queue URL.

    static async Task<string> GetOrCreateQueue()
    {
        string url;
        try
        {
            var getQueueUrlResponse = await _client.GetQueueUrlAsync("orders");
            url = getQueueUrlResponse.QueueUrl;
            Console.WriteLine("Orders queue exists");
        }
        catch (QueueDoesNotExistException)
        {
            Console.WriteLine("Creating orders queue");
            var createQueueRequest = new CreateQueueRequest()
            {
                QueueName = ORDER_QUEUE
            };
            var createQueueResponse = await _client.CreateQueueAsync(createQueueRequest);
            url = createQueueResponse.QueueUrl;
        }
        return url;
    }

    static async Task SendMessage(string message)
    {
        var sendMessageRequest = new SendMessageRequest()
        {
            QueueUrl = _queueUrl,
            MessageBody = message
        };
        var sendMessageResponse = await _client.SendMessageAsync(sendMessageRequest);

       
    }


    public static async Task<List<Message>> ReceiveMessagefromSQSAsync()
    {
        try
        {
            // Create new instance
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl
                //MaxNumberOfMessages = 1,
                //WaitTimeSeconds = 5
            };
            // Check if there are any new messages available to process
            var result = await _client.ReceiveMessageAsync(request);

            Console.WriteLine(result.Messages);

            return result.Messages.Any() ? result.Messages : new List<Message>();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    static Order GenerateRandomOrder(string id)
    {
        var items = new List<string>();
        for (int i = 0; i < _random.Next(5) + 1; i++)
        {
            switch (_random.Next(5))
            {
                case 0:
                    items.Add("Widget");
                    break;
                case 1:
                    items.Add("Sprocket");
                    break;
                case 2:
                    items.Add("Gasket");
                    break;
                case 3:
                    items.Add("Washer");
                    break;
                case 4:
                    items.Add("Spring");
                    break;
            }
        }

        Order order = new Order()
        {
            Id = id,
            Items = items
        };

        return order;
    }
}

public class Order
{
    public string Id { get; set; } = null!;
    public List<string> Items { get; set; } = null!;

    public Order() { }

    public Order(string id, string[] items)
    {
        Id = id;
        Items = new List<string>(items);
    }

}
