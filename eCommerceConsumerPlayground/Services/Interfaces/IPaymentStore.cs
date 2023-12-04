using ECommerceConsumerPlayground.Models;

namespace ECommerceConsumerPlayground.Services.Interfaces;

/// <summary>
/// Interface for User objects in database
/// </summary>
public interface IPaymentStore
{
    Task SaveDataAsync(Payment payment);
    Task<bool> CheckIfEntryAlreadyExistsAsync(Payment payment);
}