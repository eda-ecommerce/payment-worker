using ECommerceConsumerPlayground.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceConsumerPlayground.Services;

public sealed class App : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<App> _logger;
    
    /// <summary>
    /// Worker service implementation - running Kafka consumer as loop on execute
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public App(IServiceProvider serviceProvider, ILogger<App> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Worker App STARTED at: {DateTimeOffset.Now}");

        // Background service workaround for injecting container as scoped and not default singleton
        using IServiceScope scope = _serviceProvider.CreateScope();
        IConsumerService imConsumerService = scope.ServiceProvider.GetRequiredService<IConsumerService>();
        // Begin loop
        await imConsumerService.ConsumerLoopAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning($"Worker App STOPPED at: {DateTimeOffset.Now}");
        return base.StopAsync(cancellationToken);
    }
}