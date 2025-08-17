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

                logger.LogInformation("Connecting to CloudAMQP, queue: {QueueName}", _rmqConfig.QueueName);

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
                        var routingKey = ea.RoutingKey;

                        var (formattedMessage, isJson) = FormatMessageAsJsonOrText(System.Text.Encoding.UTF8.GetString(body));
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (isJson)
                        {
                            logger.LogInformation("Received JSON message:\n      Routing key: {RoutingKey}\n      Message: {FormattedJson}",
                                routingKey, formattedMessage);
                        }
                        else
                        {
                            logger.LogInformation("Received unexpected non-JSON message with routing key: {RoutingKey}, Content: {MessageContent}",
                                routingKey, formattedMessage);
                        }
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

                logger.LogInformation("CloudAMQP connection initialized successfully");

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
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Disposing a connected to CloudAMQP (queue: {QueueName})", _rmqConfig.QueueName);
            _channel?.Dispose();
            _connection?.Dispose();
            await base.StopAsync(cancellationToken);
        }

        private static (string formattedMessage, bool isJson) FormatMessageAsJsonOrText(string messageRaw)
        {
            try
            {
                var jsonObj = System.Text.Json.JsonDocument.Parse(messageRaw);
                var formattedMessage = System.Text.Json.JsonSerializer.Serialize(
                    jsonObj,
#pragma warning disable CA1869
                    new System.Text.Json.JsonSerializerOptions
#pragma warning restore CA1869
                    {
                        WriteIndented = true
                    });
                return (formattedMessage, true);
            }
            catch (System.Text.Json.JsonException)
            {
                return (messageRaw, false);
            }
        }
    }
}