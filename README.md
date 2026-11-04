# Invoice Monitoring

.NET console application that consumes the invoice events published on RabbitMQ by
[Invoice Core](https://github.com/invoice-observer/invoice-core-app).

Part of Invoice Observer System. For the overall system architecture, see the
[Invoice Observer README](https://github.com/invoice-observer/.github/blob/main/profile/README.md).

## Key Features

1. CloudAMQP message consumption with reliable acknowledgment.
2. JSON message parsing and pretty-printing.
3. Support for invoice event routing keys: added, updated, deleted.
4. Structured logging with configurable levels.

## Message Types

Supported routing keys:
- `invoice.added` - New invoice creation events
- `invoice.updated` - Invoice modification events  
- `invoice.deleted` - Invoice removal events

## Development Environment

### Technologies Used

- .NET 10 SDK
- CloudAMQP account (or local RabbitMQ instance)

### Getting Started

1. Clone the repository
2. Configure CloudAMQP connection string in appsettings.json, if default is not suitable
3. Run the monitoring application
```bash
dotnet run
```

### Configuration

```json
{
  "RabbitMQ": {
    "ConnectionString": "amqps://hostname/vhost",
    "UserName": "username",
    "UserPassword": "",
    "QueueName": "invoices"
  }
}
```

### RabbitMQ Configuration and Secrets

The `RabbitMQ` section of `appsettings.json` holds only non-secret values: the broker URI **without credentials**
(`amqps://host/vhost`), `UserName` and the queue name. The `UserPassword` is never committed;
it is supplied through the standard configuration providers and set on the `ConnectionFactory` directly,
so no URI escaping of special characters is needed and the password never appears in a string that could be logged.

- Development (User Secrets):
  ```
  dotnet user-secrets set "RabbitMQ:UserPassword" "<password>" --project Projects/InvoiceMonitoringCli
  ```
  User secrets are only loaded when `DOTNET_ENVIRONMENT=Development` (set it in your shell or run configuration), e.g.:
  ```
  $env:DOTNET_ENVIRONMENT="Development"; dotnet run --project Projects/InvoiceMonitoringCli
  ```
- Production: environment variable `RabbitMQ__UserPassword` (or a Key Vault / Kubernetes secret mapped to it).

Use `amqps://` (TLS) outside of local development and a dedicated least-privilege broker user per service.
