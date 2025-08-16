using InvoiceMonitoringCli.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace InvoiceMonitoringCli.Services
{
    public class RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        IOptions<RabbitMqConfiguration> rmqConfig)
        : BackgroundService
    {
        private readonly RabbitMqConfiguration _rmqConfig = rmqConfig.Value;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("CloudAMQP Consumer Service starting...");

            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_rmqConfig.ConnectionString)
                };

                await using var connection = await factory.CreateConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(null, cancellationToken);

                logger.LogInformation("Connected to CloudAMQP, queue: {QueueName}", _rmqConfig.QueueName);


                // Mock message processing loop
                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Waiting for messages...");
                    await Task.Delay(5000, cancellationToken);
                }

            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in CloudAMQP Consumer Service");
            }
            finally
            {
                logger.LogInformation("CloudAMQP Consumer Service stopping...");
            }
        }
    }
}