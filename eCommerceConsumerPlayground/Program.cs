using System.Diagnostics;
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
        var connectionstring = configuration.GetConnectionString("SqlServer");
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionstring));

        // DI services
        services.AddScoped<App>();
        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<IConsumerService, ConsumerService>();

        // Definition of startup service
        services.AddHostedService<App>();
    })
    .Build();

await host.RunAsync();