using InvoiceMonitoringCli.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Channels;

namespace InvoiceMonitoringCli.Services
{
    public class RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        IOptions<RabbitMqConfiguration> rmqConfig)
        : BackgroundService
    {
        private readonly RabbitMqConfiguration _rmqConfig = rmqConfig.Value;

        private IConnection? _connection;
        private IChannel? _channel;


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("CloudAMQP Consumer Service starting...");

            try
            {
                // Create new factory, connection and channel...
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_rmqConfig.ConnectionString)
                };
                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(null, cancellationToken);

                logger.LogInformation("Connected to CloudAMQP, queue: {QueueName}", _rmqConfig.QueueName);

                await _channel.QueueDeclareAsync(
                    queue: _rmqConfig.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();


                        await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
//                      await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Message processing failed");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: _rmqConfig.QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken);

                await Task.Delay(Timeout.Infinite, cancellationToken);
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