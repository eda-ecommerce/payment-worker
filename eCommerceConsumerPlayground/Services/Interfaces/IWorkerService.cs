namespace ECommerceConsumerPlayground.Services.Interfaces;

public interface IWorkerService
{
    Task ConsumerLoopAsync(CancellationToken cancellationToken);
    void CloseConsumer();
}