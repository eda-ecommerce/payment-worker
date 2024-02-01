using Castle.Core.Configuration;
using Confluent.Kafka;
using ECommerceConsumerPlayground.Services;
using ECommerceConsumerPlayground.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace PaymentWorkerTest;

public class PaymentServiceTest
{
    private readonly WorkerService _sut;
    private readonly Mock<ILogger<WorkerService>> _mockLogger = new Mock<ILogger<WorkerService>>();
    private readonly Mock<IPaymentStore> _mockPaymentStore = new Mock<IPaymentStore>();
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(
            new Dictionary<string, string> {
                {"Kafka:Broker", "placeholder"},
                {"Kafka:Topic", "placeholder"},
                {"Kafka:GroupId", "placeholder"},
            }!)
        .Build();
    
    public PaymentServiceTest()
    {
        _sut = new WorkerService(_mockLogger.Object, _configuration, _mockPaymentStore.Object);
    }

    [Fact]
    public async Task DeserializeKafkaMessage_ShouldReturnValidModel_WhenMessageModelIsCorrect()
    {
        // Arrage
        var orderId = "25428531-3d60-4ba6-9409-383ed99b3e9d";
        
        var kafkaMessage = new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string>
            {
                Value = "{\"OrderId\":\"25428531-3d60-4ba6-9409-383ed99b3e9d\",\"CustomerId\":\"42196569-8808-4d8d-a358-8351b6e5d500\",\"OrderDate\":\"2010-08-18\",\"Status\":false,\"TotalPrice\":97.665436,\"Items\":[{\"Quantity\":1,\"Offering\":{\"OfferingId\":\"ab64cc0d-5a0b-4643-93e7-7912e6d6f78f\",\"ProductId\":\"2a52dc3a-2941-4e57-8e99-94a60191d0ac\",\"Quantity\":9,\"Price\":25.224644,\"Status\":false}},{\"Quantity\":10,\"Offering\":{\"OfferingId\":\"c1e52f83-0b08-49ee-a6c6-2de0b8bcbbac\",\"ProductId\":\"f1a962a7-da53-4cfc-bb27-615a32d24a5c\",\"Quantity\":2,\"Price\":60.26885,\"Status\":false}},{\"Quantity\":5,\"Offering\":{\"OfferingId\":\"5b59459c-8bd8-4ca5-aedb-561a224bdb93\",\"ProductId\":\"155e83ec-080b-4604-b0e9-00763ccd1131\",\"Quantity\":1,\"Price\":79.15119,\"Status\":false}},{\"Quantity\":8,\"Offering\":{\"OfferingId\":\"489660ff-a590-48bd-bf80-defa3c92ef39\",\"ProductId\":\"99ab7acd-cc02-48d1-90b7-e2e889003302\",\"Quantity\":2,\"Price\":61.953033,\"Status\":false}},{\"Quantity\":1,\"Offering\":{\"OfferingId\":\"f9821925-7d9e-4175-a742-0a95f0c9e414\",\"ProductId\":\"1e61ccdd-1b84-4b41-914a-85d3dc9958fb\",\"Quantity\":5,\"Price\":99.37913,\"Status\":false}},{\"Quantity\":9,\"Offering\":{\"OfferingId\":\"68fe7cec-1cf2-48cc-abc3-dbd99a2a76db\",\"ProductId\":\"04a859ff-e101-433d-b764-602125c957f8\",\"Quantity\":10,\"Price\":45.165924,\"Status\":false}}]}\n"
            }
        };
        
        // Act
        var (isValid, deserializedOrder) = _sut.DeserializeKafkaMessage(kafkaMessage);
        
        // Assert
        isValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task DeserializeKafkaMessage_ShouldReturnInvalidModel_WhenMessageModelIsNotCorrect()
    {
        // Arrage
        var orderId = "25428531-3d60-4ba6-9409-383ed99b3e9d";
        
        var kafkaMessage = new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string>
            {
                Value = "{\"OrderId\":\"123123\",\"CustomerId\":\"42196569-8808-4d8d-a358-8351b6e5d500\",\"OrderDate\":\"2010-08-18\",\"Status\":false,\"TotalPrice\":97.665436,\"Items\":[{\"Quantity\":1,\"Offering\":{\"OfferingId\":\"ab64cc0d-5a0b-4643-93e7-7912e6d6f78f\",\"ProductId\":\"2a52dc3a-2941-4e57-8e99-94a60191d0ac\",\"Quantity\":9,\"Price\":25.224644,\"Status\":false}},{\"Quantity\":10,\"Offering\":{\"OfferingId\":\"c1e52f83-0b08-49ee-a6c6-2de0b8bcbbac\",\"ProductId\":\"f1a962a7-da53-4cfc-bb27-615a32d24a5c\",\"Quantity\":2,\"Price\":60.26885,\"Status\":false}},{\"Quantity\":5,\"Offering\":{\"OfferingId\":\"5b59459c-8bd8-4ca5-aedb-561a224bdb93\",\"ProductId\":\"155e83ec-080b-4604-b0e9-00763ccd1131\",\"Quantity\":1,\"Price\":79.15119,\"Status\":false}},{\"Quantity\":8,\"Offering\":{\"OfferingId\":\"489660ff-a590-48bd-bf80-defa3c92ef39\",\"ProductId\":\"99ab7acd-cc02-48d1-90b7-e2e889003302\",\"Quantity\":2,\"Price\":61.953033,\"Status\":false}},{\"Quantity\":1,\"Offering\":{\"OfferingId\":\"f9821925-7d9e-4175-a742-0a95f0c9e414\",\"ProductId\":\"1e61ccdd-1b84-4b41-914a-85d3dc9958fb\",\"Quantity\":5,\"Price\":99.37913,\"Status\":false}},{\"Quantity\":9,\"Offering\":{\"OfferingId\":\"68fe7cec-1cf2-48cc-abc3-dbd99a2a76db\",\"ProductId\":\"04a859ff-e101-433d-b764-602125c957f8\",\"Quantity\":10,\"Price\":45.165924,\"Status\":false}}]}\n"
            }
        };
        
        // Act
        var (isValid, deserializedOrder) = _sut.DeserializeKafkaMessage(kafkaMessage);
        
        // Assert
        isValid.Should().BeFalse();
    }
}