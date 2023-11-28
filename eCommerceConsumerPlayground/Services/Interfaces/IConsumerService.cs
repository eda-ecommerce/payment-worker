namespace ECommerceConsumerPlayground.Services.Interfaces;

/// <summary>
/// Interface for Identity Management (IM) Kafka Consumer Service
/// </summary>
public interface IConsumerService
{
    Task ConsumerLoopAsync(CancellationToken cancellationToken);
    void CloseConsumer();
}