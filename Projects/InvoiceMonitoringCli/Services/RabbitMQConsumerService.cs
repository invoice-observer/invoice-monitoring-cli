using InvoiceMonitoringCli.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace InvoiceMonitoringCli.Services
{
    public class RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IOptions<RabbitMQConfiguration> rmqConfig)
        : BackgroundService
    {
        private readonly RabbitMQConfiguration _rmqConfig = rmqConfig.Value;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("CloudAMQP Consumer Service starting...");

            try
            {
                // Here you would connect to CloudAMQP using the connection string
                // For example:
                // var factory = new ConnectionFactory { Uri = new Uri(_rmqConfig.ConnectionString) };

                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_rmqConfig.ConnectionString)
                };


                logger.LogInformation("Connected to CloudAMQP, queue: {QueueName}", _rmqConfig.QueueName);

                // Create new connection and channel...
                await using var connection = await factory.CreateConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(null, cancellationToken);


                // Simulate connection and consumption
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