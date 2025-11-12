# Order Processing Service

This is a microservice for processing orders, built with .NET 8, EF Core, and MassTransit (RabbitMQ). It handles order creation, processing, and inventory management asynchronously.

---

## Table of Contents

- [Getting Started](#getting-started)  
- [Running the Service](#running-the-service)  
- [Design Decisions](#design-decisions)  
- [Assumptions](#assumptions)  
- [Future Improvements](#future-improvements)  

---

## Getting Started

### Prerequisites

- Docker & Docker Compose
- .NET 8 SDK (optional, for running locally outside Docker)

---

## Running the Service

1. Clone the repository:

```bash
git clone https://github.com/maryna-petrova-1/order-processing.git
cd order-processing
```

2. Go to the docker folder:
```bash
cd docker
```

3. Build and start the containers:
```bash
docker-compose up --build -d
```
This will start:

- **sqlserver** (SQL Server 2022)  
- **rabbitmq** (with management UI on port 15672)  
- **order-api** (.NET API)  
- **order-worker** (background worker processing orders)  

4. Ensure there are no orders yet:

```bash
curl -X GET "http://localhost:5050/api/orders" \
     -H "Accept: application/json"
```

5. Create a new order (note: client and product are pre-seeded):

```bash
curl -X POST http://localhost:5050/api/orders \
     -H "Content-Type: application/json" \
     -d '{
           "clientId": 1,
           "items": [
             {
               "productId": 1,
               "quantity": 5
             }
           ]
         }'
```

6. Verify that the order was created and processed:

```bash
curl -X GET "http://localhost:5050/api/orders/1" \
     -H "Accept: application/json"
```

## Seeded Data

The service seeds sample data on startup to facilitate local development and testing. The following entities are pre-populated:

### Clients
| Name       | Email             |
|------------|-----------------|
| John Doe   | john@example.com |
| Jane Smith | jane@example.com |

### Products
| Name        | Price (USD) |
|------------|-------------|
| Laptop     | 1200        |
| Phone      | 800         |
| Headphones | 150         |

### Inventories
| Product     | Quantity Available |
|------------|------------------|
| Laptop     | 10               |
| Phone      | 20               |
| Headphones | 50               |

> **Note:** Orders can be created for these pre-seeded clients and products.

## Design Decisions

- **MassTransit + RabbitMQ**: Used for asynchronous order processing. Decouples the API from heavy processing, allowing the API to respond quickly and orders to be processed reliably in the background.

- **EF Core with SQL Server**: Handles relationships between orders, items, products, and inventories.

- **Minimal API**: Lightweight and easy to maintain; reduces boilerplate code.

- **Integer IDs (`int`) instead of `Guid`**: Chosen for performance in relational database indexing and joins.

- **Seeding**: Sample clients, products, and inventories are seeded on startup for easier local development.

## Trade-offs

- Minimal API simplifies structure but lacks controller features like filters or attributes; manual validations are implemented.
- Inventory updates are inside the same transaction as order processing; concurrency might need extra handling in production.
- Logging is currently only to Docker console.

## Assumptions

- Orders must contain at least one item with a quantity greater than zero.
- Products exist before orders are created, and inventory is sufficient unless marked otherwise.
- Orders with zero or negative total amounts are automatically cancelled.
- Only one instance of the service interacts with the database at a time in this setup.

## Future Improvements

- **Draft Orders**: Introduce a separate entity for draft orders before full validation.
- **Database Seeding**: Move seeding logic to a separate file or migration.
- **Logging**: Expand logging to file or external log providers.
- **AutoMapper**: Add for mapping between entities and response models.
- **Validation**: Improve model validation with FluentValidation or similar.
- **Secrets Management**: Move sensitive values (connection strings, RabbitMQ creds) to Azure Key Vault or environment variables.
- **CRUD Endpoints**: Add endpoints to manage inventories, products, and clients.
- **Health Checks**: Add endpoint to monitor service health.
- **API Versioning**: Add versioning for future backward compatibility.
- **Docker `.env` File**: Allow overriding configuration values via environment variables.
- **Unit and Integration Tests**: Write automated tests to ensure correctness and reliability of API and background processing.

## RabbitMQ Usage

RabbitMQ is used to decouple order submission from processing. When a client creates an order, it is published to a queue. A background worker consumes the message, processes the order, and updates inventory. This ensures:

- The API responds quickly without waiting for heavy database operations.
- Order processing can be retried automatically if it fails.
- The system is more resilient and scalable.
