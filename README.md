# Invoice Observer: Invoice Monitoring

Part of the Invoice Observer application, the overall README here: [Invoice Observer and Invoice Core)](https://github.com/invoice-observer/invoice-core-app/blob/main/README.md)

## Overview

A monitoring CLI that subscribes Invoice Core events, published on RabbitMQ, see
the accompanying [Invoice Core Application](https://github.com/invoice-observer/invoice-core-app.git)

## Key Features

1. CloudAMQP message consumption with reliable acknowledgment.
2. JSON message parsing and pretty-printing.
3. Support for invoice event routing keys: added, updated, deleted. The updated and deleted don't come (never send by the message supplier)

## System Architecture

This application serves as the monitoring component for the Invoice Core Application:

**Monitoring Application (.NET 8 Console)**
- CloudAMQP queue consumer
- Message processing and display functionality
- Structured logging with configurable levels

## Message Types

Supported routing keys:
- `invoice.added` - New invoice creation events
- `invoice.updated` - Invoice modification events  
- `invoice.deleted` - Invoice removal events

## Development Environment

### Technologies Used

- .NET 8 SDK
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
- Production: environment variable `RabbitMQ__UserPassword` (or a Key Vault / Kubernetes secret mapped to it).

Use `amqps://` (TLS) outside of local development and a dedicated least-privilege broker user per service.

## Related Projects

Works together with [Invoice Core](https://github.com/invoice-observer/invoice-core-app.git) which publishes the messages consumed by this service.
