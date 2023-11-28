using Microsoft.EntityFrameworkCore;
using ECommerceConsumerPlayground.Models;

namespace eCommerceConsumerPlayground.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }
}