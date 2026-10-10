namespace InvoiceMonitoringCli.Configuration
{
    public class RabbitMqConfiguration
    {
        // Broker URI without credentials, e.g. amqps://host/vhost
        public string ConnectionString { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        // Secret: supply via user-secrets / environment variable only, never commit
        public string UserPassword { get; init; } = string.Empty;
        public string QueueName { get; init; } = string.Empty;
    }
}
