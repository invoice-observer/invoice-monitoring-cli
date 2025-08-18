# Invoice Monitoring CLI

Part of the Invoice Core System, the overall README here: [Invoice Core System](https://github.com/e-danz/invoice-core-app.git)

## Overview

Invoice monitoring console application built with .NET 8, implementing CloudAMQP message consumption
for invoice processing events, see the accompanying [Invoice Core Application](https://github.com/e-danz/invoice-core-app.git)

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

## Development

### Requirements

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
    "ConnectionString": "amqps://username:password@hostname/vhost",
    "QueueName": "invoices"
  }
}
```

## Related Projects

Works together with [Invoice Core Application](https://github.com/e-danz/invoice-core-app.git) which produces the messages consumed by this service.