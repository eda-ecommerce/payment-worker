using eCommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceConsumerPlayground.Services;

public sealed class App : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<App> _logger;
    //private readonly AppDbContext _context;

    /// <summary>
    /// Worker service implementation - running Kafka consumer as loop on execute
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    /// <param name="context"></param>
    public App(IServiceProvider serviceProvider, ILogger<App> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Worker App STARTED at: {DateTimeOffset.Now}");
        
        // if (_context.Database.GetPendingMigrations().Any())
        // {
        //     Console.WriteLine("Running migrations");
        //     _context.Database.Migrate();
        //     Console.WriteLine("Migrations have been successfull");
        // }
        
        // Background service workaround for injecting container as scoped and not default singleton
        using IServiceScope scope = _serviceProvider.CreateScope();
        IWorkerService imWorkerService = scope.ServiceProvider.GetRequiredService<IWorkerService>();
        // Begin loop
        await imWorkerService.ConsumerLoopAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning($"Worker App STOPPED at: {DateTimeOffset.Now}");
        return base.StopAsync(cancellationToken);
    }
}