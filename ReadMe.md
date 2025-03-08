# Inventory Management System

A RESTful API-based inventory management system built with ASP.NET Core that handles product tracking, inventory operations, and reporting with a clean architecture approach.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [API Documentation](#api-documentation)
- [Architecture](#architecture)
- [Testing](#testing)
- [Assumptions](#assumptions)
- [Areas for Improvement](#areas-for-improvement)

## Prerequisites

Before diving in, make sure you have the following tools installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server - either:
   - [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) for a traditional setup
   - [SQL Server via Docker](https://hub.docker.com/_/microsoft-mssql-server) for a containerized approach:
     ```bash
     docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
     ```
- [Git](https://git-scm.com/downloads)

## Installation

1. Clone the repository
   ```bash
   git clone https://github.com/ianMuchesia/inventory-management-system.git
   cd inventory-management-system
   ```

2. Set up the database:
   - Find `InventoryManagementSQL.sql` in the root directory
   - Execute the script in your SQL Server environment
   - The database will be created as `InventoryDB`

3. Configure JWT authentication:
   Generate a secure key using:
   ```bash
   dotnet user-secrets set JwtSettings:Secret "$(openssl rand -base64 32)"
   ```
   Or manually add a strong key to `appsettings.json`

4. Update connection settings:
   In the `InventoryManagement.API` directory, edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=inventoryDB;User Id=your_user;Password=your_password;TrustServerCertificate=True"
     },
     "JwtSettings": {
       "Secret": "your_generated_secret_key",
       "Issuer": "inventory_api",
       "Audience": "inventory_clients",
       "ExpirationInMinutes": 60
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning"
       }
     }
   }
   ```

5. Launch the application:
   ```bash
   cd InventoryManagement.API
   dotnet watch run
   ```

6. Explore the API through Swagger:
   ```
   https://localhost:5001/swagger
   ```

## API Documentation

The API implements RESTful principles with consistent resource naming and appropriate HTTP methods:

### Authentication

| Method | Endpoint               | Description                  | Auth Required | Sample Request Body |
|--------|------------------------|------------------------------|---------------|---------------------|
| POST   | /api/v1/auth/register | Create new user account      | No            | `{"username": "admin", "email": "admin@example.com", "password": "SecurePass123!"}` |
| POST   | /api/v1/auth/login    | Authenticate and get token   | No            | `{"email": "admin@example.com", "password": "SecurePass123!"}` |
| GET    | /api/v1/auth/me       | Get current user profile     | Yes           | N/A |

### Products

| Method | Endpoint               | Description                  | Auth Required | Sample Request Body |
|--------|------------------------|------------------------------|---------------|---------------------|
| POST   | /api/v1/products      | Create product               | Yes           | `{"name": "Office Chair", "description": "Ergonomic chair", "category": "Furniture", "unitPrice": 199.99, "quantityInStock": 25, "reorderLevel": 5}` |
| GET    | /api/v1/products      | List all products            | Yes           | N/A |
| GET    | /api/v1/products/{id} | Get product details          | Yes           | N/A |
| PUT    | /api/v1/products/{id} | Update product               | Yes           | `{"name": "Deluxe Office Chair", "description": "Premium ergonomic chair", "category": "Furniture", "unitPrice": 249.99,  "reorderLevel": 5}` |
| DELETE | /api/v1/products/{id} | Remove product               | Yes           | N/A |

### Inventory Operations

| Method | Endpoint                                  | Description                    | Auth Required | Sample Request Body |
|--------|-------------------------------------------|--------------------------------|---------------|---------------------|
| POST   | /api/v1/inventories/add-stock            | Add inventory                  | Yes           | `{"productId": 1, "quantity": 10, "notes": "Received from supplier XYZ"}` |
| POST   | /api/v1/inventories/withdraw-stock       | Remove inventory               | Yes           | `{"productId": 1, "quantity": 3, "notes": "Customer order #12345"}` |
| GET    | /api/v1/inventories/transactions/{productId} | View product history        | Yes           | N/A |

### Reports

| Method | Endpoint                           | Description                  | Auth Required | Sample Request Body |
|--------|-----------------------------------|------------------------------|---------------|---------------------|
| GET    | /api/v1/reports/inventory-valuation | Get total inventory value   | Yes           | N/A |
| GET    | /api/v1/reports/low-stock        | Find products below reorder   | Yes           | N/A |
| POST   | /api/v1/reports/search-products  | Search with filters & paging  | Yes           | `{"searchTerm": "chair", "category": "Furniture", "page": 1, "pageSize": 10}` |

## Architecture

The solution follows a clean architecture approach that emphasizes separation of concerns, testability, and maintainability:

### Project Structure

- **InventoryManagement.API**
  - Controllers
  - Configuration
  - Filters
  - Startup and Program files
  
- **InventoryManagement.Application**
  - Services
  - DTOs
  - Interfaces
 
  
- **InventoryManagement.Domain**
  - Entities
  - Value objects
  - Domain services
  - Business rules and invariants
  - Domain events 
  
- **InventoryManagement.Infrastructure**
  - Data access (EF Core implementation)
  - External service integrations
  - Middleware components
  - Authentication handlers

  
- **InventoryManagement.Tests**
  - Integration tests for API endpoints
  - Database fixture setup
  - Test utilities
  
- **InventoryManagement.DomainTests**
  - Unit tests for domain entities and business rules
  - Mock setup
  
- **InventoryManagement.ApplicationTests**
  - Unit tests for application services
  - Mocked repositories

### Design Patterns

- **Repository Pattern**: Abstracts data access logic, making the application more testable and maintainable
- **Dependency Injection**: Promotes loose coupling between components


### Database Design

The database uses a straightforward relational model with the following key tables:

- **Products**: Stores product information
- **InventoryTransactions**: Records all stock movements with transaction types
- **Users**: Authentication and authorization

Foreign key constraints and appropriate indexes have been implemented for data integrity and performance.

## Testing

The solution includes both unit tests and integration tests to ensure reliability:

To run all tests from the root directory:
```bash
dotnet test
```

The testing approach includes:

- **Unit tests** for domain and application logic in their respective test projects
- **Integration tests** for API endpoints with an in-memory test database
- **Test data builders** to create test fixtures
- **Mock repositories** to isolate components during testing

## Assumptions

Several pragmatic assumptions were made during development:

1. **Authentication**: JWT-based authentication is sufficient for this proof-of-concept. Production environments would require additional security measures.

2. **Product Uniqueness**: Products have unique names within the system to avoid ambiguity in inventory tracking.

3. **Stock Validation**: The system prevents negative stock levels by validating withdrawal operations against available quantities.

4. **Single User Role**: For simplicity, the current implementation has a single user role. A real-world application would require more granular permissions.

5. **Database Choice**: SQL Server is the primary data store, though the repository abstraction would allow for different providers with minimal changes.

6. **Transaction Isolation**: Default isolation levels in SQL Server are adequate for the current transaction volume.

7. **Data Volume**: The system is designed for moderate data volumes; additional optimizations would be needed for high-scale operations.

## Areas for Improvement

With additional time, I would enhance the system in the following ways:

1. **Unit of Work Implementation**: 
   - Implement proper Unit of Work pattern to handle multiple database operations as a single transaction
   - Ensure consistency across repository operations
   - Add transaction management for operations that modify multiple entities

2. **CQRS Implementation**: 
   - Separate read and write models to improve performance and scalability
   - Create dedicated command and query objects
   - Implement separate handlers for commands and queries
   - Consider using MediatR library for CQRS implementation

3. **Role-Based Access Control (RBAC)**: 
   - Implement different user roles with specific permissions
   - Add role-based authorization at API and business logic levels
   - Create admin, manager, and standard user roles with appropriate access restrictions

4. **Audit Logging**: 
   - Track all changes to the inventory with timestamps and user information
   - Implement audit tables to maintain history of all operations
   - Add capability to review change history for compliance purposes

5. **Domain Events**: 
   - Implement event-driven architecture for better decoupling
   - Dispatch events when products reach reorder level
   - Integrate with message brokers like RabbitMQ for notifications
   - Allow external systems to subscribe to inventory events

6. **Extended Product Information**:
   - Add supplier details
   - Track purchasing costs separately from selling prices
   - Implement proper category management with relationships
   - Support product variants and multiple SKUs per product

7. **Performance Optimizations**:
   - Implement caching with Redis for frequently accessed data
   - Add pagination for all list endpoints
   - Optimize database queries with proper indexing
   - Consider read replicas for high-volume systems

8. **Logging Improvements**:
   - Implement structured logging with Serilog
   - Set up centralized logging with ELK stack or similar
   - Add correlation IDs for request tracing
   - Implement log levels appropriate for different environments

9. **Additional Features**:
   - Warehouse/location management for larger inventories
   - Batch/lot tracking for better inventory control
   - Implement bulk product import/export functionality

10. **Testing Improvements**:
    - Increase test coverage
    - Add performance testing
    - Implement contract testing for API
    - Add more integration tests for complex workflows

11. **API Versioning**: 
    - Implement proper API versioning for future changes
    - Support multiple API versions simultaneously
    - Provide migration paths for clients when breaking changes occur

12. **Documentation Enhancements**: 
    - Add API response examples
    - Improve error handling documentation
    - Create setup troubleshooting guide
    - Provide usage examples for common workflows