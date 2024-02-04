using eCommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceConsumerPlayground.Services;

/// <summary>
/// Implementation of User objects in database
/// </summary>
public class PaymentStore : IPaymentStore
{
    private readonly ILogger<PaymentStore> _logger;
    private readonly AppDbContext _context;

    public PaymentStore(ILogger<PaymentStore> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task SaveDataAsync(Payment payment)
    {
        _logger.LogInformation($"Starting persistence operations for user object '{payment}' in database.");
        try
        {
            // Check if entry already exists
            var paymentExists = await CheckIfEntryAlreadyExistsAsync(payment);
            if (paymentExists)
            {
                _logger.LogInformation($"Payment with Id: '{payment.PaymentId}' already exists in database. No new persistence.");
                return;
            }
            
            // If not already exists, than persist
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            _logger.LogInformation($@"Payment with Id: '{payment.PaymentId}' successfully saved in database.
OrderId: {payment.OrderId}
CreatedDate: {payment.CreatedDate}
Status: {payment.Status}");
        }
        catch (Exception e)
        {
            _logger.LogError($"User object '{payment}' could not be saved on database. Message: {e.Message}");
        }
    }

    public async Task<bool> CheckIfEntryAlreadyExistsAsync(Payment payment)
    {
        var paymentExists = await _context.Payments.AnyAsync(u => u.OrderId == payment.OrderId);
        
        return paymentExists;
    }
}