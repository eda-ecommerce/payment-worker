using System.Text.Json;
using ECommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerceConsumerPlayground.Services;

/// <summary>
/// Implementation of Kafka Consumer Service
/// </summary>
public class ConsumerService : IConsumerService
{
    private readonly ILogger<ConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _kafkaConsumer;
    private readonly IConfiguration _configuration;
    private readonly IUserStore _userStore;
    private readonly string KAFKA_BROKER;
    private readonly string KAFKA_TOPIC1;
    private readonly string KAFKA_GROUPID;
    private readonly string KAFKA_TOPIC2;

    public ConsumerService(ILogger<ConsumerService> logger, IConfiguration configuration, IUserStore userStore)
    {
        _configuration = configuration;
        // Get appsettings and set as static variable
        KAFKA_BROKER = _configuration.GetValue<string>("Kafka:Broker")!;
        KAFKA_TOPIC1 = _configuration.GetValue<string>("Kafka:Topic1")!;
        KAFKA_TOPIC2 = _configuration.GetValue<string>("Kafka:Topic2")!;
        KAFKA_GROUPID = _configuration.GetValue<string>("Kafka:GroupId")!;

        //KAFKA_BROKER = "localhost:29092";
        //KAFKA_GROUPID = "ecommerce-gp";
        //KAFKA_TOPIC1 = "test-1";
        //KAFKA_TOPIC2 = "payment";

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = KAFKA_BROKER,
            GroupId = KAFKA_GROUPID,
            AutoOffsetReset = AutoOffsetReset.Earliest, // if Earliest - begins at offset 0 | if Latest - begins at now
            EnableAutoOffsetStore = false // if false - always begins at offset 0
        };
        
        _logger = logger;
        _kafkaConsumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _userStore = userStore;
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
                    
//                     
                    // Handle message...
                    var user = JsonSerializer.Deserialize<Payment>(consumeResult.Message.Value)!;

                    // Produce messages
                    ProducerConfig configProducer = new ProducerConfig
                    {
                        BootstrapServers = KAFKA_BROKER,
                        ClientId = Dns.GetHostName()
                    };

                    using var producer = new ProducerBuilder<Null, string>(configProducer).Build();

                    var result = await producer.ProduceAsync(KAFKA_TOPIC2, new Message<Null, string>
                    {
                        Value = JsonSerializer.Serialize<Payment>(user)
                    });



                    // Persistence
                    await _userStore.SaveDataAsync(user);
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    _logger.LogWarning(2000, $"Error on consuming Kafka Message. Reason: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        _logger.LogError(3000, "Fatal error on consuming Kafka Message..");
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
}