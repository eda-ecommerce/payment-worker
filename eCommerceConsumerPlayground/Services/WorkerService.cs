using System.Text.Json;
using ECommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using paymentWorker.Models;

namespace ECommerceConsumerPlayground.Services;

/// <summary>
/// Implementation of Kafka Consumer Service
/// </summary>
public class WorkerService : IWorkerService
{
    private readonly ILogger<WorkerService> _logger;
    private readonly IConsumer<Ignore, string> _kafkaConsumer;
    private readonly IConfiguration _configuration;
    private readonly IPaymentStore _paymentStore;
    private readonly string KAFKA_BROKER;
    private readonly string KAFKA_TOPIC1;
    private readonly string KAFKA_GROUPID;
    private readonly string KAFKA_TOPIC2;

    public WorkerService(ILogger<WorkerService> logger, IConfiguration configuration, IPaymentStore paymentStore)
    {
        _configuration = configuration;
        // Get appsettings and set as static variable
        KAFKA_BROKER = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("KAFKABROKER")) ? Environment.GetEnvironmentVariable("KAFKABROKER") : _configuration.GetValue<string>("Kafka:Broker");
        KAFKA_TOPIC1 = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("KAFKATOPIC1")) ? Environment.GetEnvironmentVariable("KAFKATOPIC1") : _configuration.GetValue<string>("Kafka:Topic1");
        KAFKA_TOPIC2 = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("KAFKATOPIC2")) ? Environment.GetEnvironmentVariable("KAFKATOPIC2") : _configuration.GetValue<string>("Kafka:Topic2");
        KAFKA_GROUPID = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("KAFKAGROUPID")) ? Environment.GetEnvironmentVariable("KAFKAGROUPID") : _configuration.GetValue<string>("Kafka:GroupId");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = KAFKA_BROKER,
            GroupId = KAFKA_GROUPID,
            AutoOffsetReset = AutoOffsetReset.Earliest, // if Earliest - begins at offset 0 | if Latest - begins at now
            EnableAutoOffsetStore = false // if false - always begins at offset 0
        };
        
        _logger = logger;
        _kafkaConsumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _paymentStore = paymentStore;
    }

    public async Task ConsumerLoopAsync(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Subscribe(new string[]
        {
            KAFKA_TOPIC1,
        });
        
        // Try and catch to ensure the consumer leaves the group cleanly and final offsets are committed.
        try
        {
            _logger.LogInformation("Loop");
            // Endless consume loop until interrupt
            while (true)
            {
                try
                {
                    var consumeResult = _kafkaConsumer.Consume(cancellationToken);

                    // Handle message...
                    var (isValid,order) = DeserializeKafkaMessage(consumeResult);
                   
                    var payment = new Payment()
                    {
                        PaymentId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        PaymentDate = null,
                        CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                        Status = Status.Unpayed
                    };
                    
                    // if statement is required so that a message is only produced if an order does not yet exist.
                    if (!await _paymentStore.CheckIfEntryAlreadyExistsAsync(payment))
                    {
                        SendKafkaMessageForUpdatePayment(payment);
                    }

                    // Persistence
                    await _paymentStore.SaveDataAsync(payment);
                    _logger.LogInformation($" Payment stored: {payment.PaymentId}");
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    _logger.LogWarning($"Error on consuming Kafka Message. Reason: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        _logger.LogError("Fatal error on consuming Kafka Message..");
                        break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Unsubscribe and close
            CloseConsumer();
        }
    }

    public void CloseConsumer()
    {
        // Ensure the consumer leaves the group cleanly and final offsets are committed.
        _logger.LogWarning(2001, "Closing Kafka (unsubscribe and close events)..");
        _kafkaConsumer.Unsubscribe();
        _kafkaConsumer.Close();
    }

    public (bool, Order?) DeserializeKafkaMessage(ConsumeResult<Ignore, string> consumeResult)
    {
        Order? order = null;
        try
        {
            order = JsonSerializer.Deserialize<Order>(consumeResult.Message.Value);
            return (true, order);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(205, "Message could not be deserialized into the default model.");
            _logger.LogWarning(205, $"Exception: {ex.Message}");
            return (false, null);
        }
    }
    
    private async void SendKafkaMessageForUpdatePayment(Payment payment)
    {
        _logger.LogInformation($" Create Kafka message for payment: {payment.PaymentId}");
        // Produce messages
        ProducerConfig configProducer = new ProducerConfig
        {
            BootstrapServers = KAFKA_BROKER,
            ClientId = Dns.GetHostName()
        };
                        
        // Create Kafka Header
        var header = new Headers();
        header.Add("Source", Encoding.UTF8.GetBytes("payment"));
        header.Add("Timestamp", Encoding.UTF8.GetBytes(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()));
        header.Add("Operation", Encoding.UTF8.GetBytes("created"));
                        
        using var producer = new ProducerBuilder<Null, string>(configProducer).Build();

        var result = await producer.ProduceAsync(KAFKA_TOPIC2, new Message<Null, string>
        {
            Value = JsonSerializer.Serialize<Payment>(payment),
            Headers = header
        });
        _logger.LogInformation($" Kafka message was produced for payment: {payment.PaymentId}");
    }
}