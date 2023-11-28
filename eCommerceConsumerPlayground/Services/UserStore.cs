using eCommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceConsumerPlayground.Services;

/// <summary>
/// Implementation of User objects in database
/// </summary>
public class UserStore : IUserStore
{
    private readonly ILogger<UserStore> _logger;
    private readonly AppDbContext _context;

    public UserStore(ILogger<UserStore> logger, AppDbContext context)
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
                _logger.LogInformation($"User object '{payment.Username}' already exists in database. No new persistence.");
                return;
            }



            // If not already exists, than persist
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            
        }
        catch (Exception e)
        {
            _logger.LogError($"User object '{payment}' could not be saved on database. Message: {e.Message}");
        }
    }

    private async Task<bool> CheckIfEntryAlreadyExistsAsync(Payment payment)
    {
        var paymentExists = await _context.Payments.AnyAsync(u => u.Username == payment.Username);
        return paymentExists;
    }
}