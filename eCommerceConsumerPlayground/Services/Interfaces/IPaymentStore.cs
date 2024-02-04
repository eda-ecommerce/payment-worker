using ECommerceConsumerPlayground.Models;

namespace ECommerceConsumerPlayground.Services.Interfaces;

public interface IPaymentStore
{
    Task SaveDataAsync(Payment payment);
    Task<bool> CheckIfEntryAlreadyExistsAsync(Payment payment);
}