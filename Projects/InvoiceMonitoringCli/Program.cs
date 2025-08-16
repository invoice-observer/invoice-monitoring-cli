using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvoiceMonitoringCli.Services;
using InvoiceMonitoringCli.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqConfiguration>(
            context.Configuration.GetSection("RabbitMQ"));
            
        services.AddHostedService<RabbitMqConsumerService>();
    });

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started.");

await host.StartAsync();
logger.LogInformation("Application is running. Press Ctrl+C to shut down.");
await host.WaitForShutdownAsync();
