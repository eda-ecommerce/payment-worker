namespace ECommerceConsumerPlayground.Services.Interfaces;

/// <summary>
/// Interface for Identity Management (IM) Kafka Consumer Service
/// </summary>
public interface IWorkerService
{
    Task ConsumerLoopAsync(CancellationToken cancellationToken);
    void CloseConsumer();
}