using eCommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Services;
using ECommerceConsumerPlayground.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        // Read appsettings
        var connectionstring = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DBSTRING")) ? Environment.GetEnvironmentVariable("DBSTRING") : configuration.GetConnectionString("SqlServer");
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionstring));

        // DI services
        services.AddScoped<App>();
        services.AddScoped<IPaymentStore, PaymentStore>();
        services.AddScoped<IWorkerService, WorkerService>();

        // Definition of startup service
        services.AddHostedService<App>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

await host.RunAsync();