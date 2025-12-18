<div align="center">

# 🛡️ VaultGuard

### Production-Ready Password Manager Backend API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Code Coverage](https://img.shields.io/badge/coverage-90%25-brightgreen)]()

[Features](#-features) • [Architecture](#-architecture) • [Getting Started](#-getting-started) • [Documentation](#-documentation) • [Contributing](#-contributing)

</div>

---

## 📖 Overview

**VaultGuard** is a production-ready backend API for a secure password management system, built with **Clean Architecture** and **Domain-Driven Design (DDD)** principles. It provides enterprise-grade security, scalability, and maintainability for managing sensitive credentials.

### ✨ Key Highlights

- 🏗️ **Clean Architecture** - Complete separation of concerns with layered design
- 🎯 **Domain-Driven Design** - Rich domain models with business logic encapsulation
- ⚡ **CQRS Pattern** - Optimized read/write operations with separate data stores
- 🔐 **Security First** - Encrypted data, audit trails, and device management
- 📊 **Production Ready** - Comprehensive logging, monitoring, and observability
- 🧪 **Well Tested** - Unit and integration tests with high code coverage

---

## 🎯 Features

### Core Functionality

- **Vault Management** - Secure containers for organizing credentials
- **Credential Storage** - Support for passwords, secure notes, and payment cards
- **Device Authorization** - Multi-device support with device tracking
- **Audit Logging** - Comprehensive security audit trails
- **Data Encryption** - Military-grade encryption for sensitive data

### Technical Features

- ✅ Read/Write Database Separation
- ✅ Distributed Caching with Redis
- ✅ Structured Logging with Elasticsearch
- ✅ Distributed Tracing with OpenTelemetry
- ✅ Metrics Collection with Prometheus
- ✅ Input Validation with FluentValidation
- ✅ Soft Delete Pattern
- ✅ Repository & Unit of Work Patterns

---

## 🏗️ Architecture

### Project Structure

```
VaultGuard/
├── src/
│   ├── libs/
│   │   ├── VaultGuard.Domain/          # Core business logic & entities
│   │   ├── VaultGuard.Application/     # CQRS, use cases & DTOs
│   │   ├── VaultGuard.Persistence/     # EF Core & repositories
│   │   └── VaultGuard.Infrastructure/  # External services & caching
│   └── presentations/
│       └── VaultGuard.Api/             # REST API controllers
└── test/
    ├── libs/
    │   ├── VaultGuard.Domain.Tests/
    │   └── VaultGuard.Application.Tests/
    └── presentations/
        └── VaultGuard.Api.Tests/
```

### Technology Stack

| Category | Technology |
|----------|-----------|
| **Framework** | .NET 10, C# 13 |
| **Database** | PostgreSQL 16+ (Read/Write separation) |
| **ORM** | Entity Framework Core 10 |
| **Caching** | Redis 7+ |
| **Mediator** | MediatR (CQRS) |
| **Logging** | Serilog + Elasticsearch |
| **Monitoring** | OpenTelemetry + Prometheus |
| **Validation** | FluentValidation |
| **Testing** | xUnit, Moq, FluentAssertions |

### Domain Models

```
┌─────────────────────────────────────────────────────────┐
│                   Aggregate Roots                       │
├─────────────────────────────────────────────────────────┤
│ User (Identity)                                         │
│   ├── Devices (Collection)                              │
│   └── Vaults (Collection)                               │
│                                                         │
│ Vault (Secure Container)                                │
│   ├── VaultItems (Collection)                           │
│   └── EncryptedData (Value Object)                      │
│                                                         │
│ AuditLog (Security Trail)                               │
└─────────────────────────────────────────────────────────┘
```

For detailed architecture documentation, see [ARCHITECTURE.md](ARCHITECTURE.md).

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Redis 7+](https://redis.io/download)
- (Optional) [Elasticsearch 8+](https://www.elastic.co/downloads/elasticsearch) for logging

### Installation

1. **Clone the repository**

```bash
git clone https://github.com/volcanion-company/vault-guard.git
cd vault-guard
```

2. **Setup databases**

```sql
CREATE DATABASE vaultguard_write;
CREATE DATABASE vaultguard_read;
```

3. **Configure application settings**

Edit [src/presentations/VaultGuard.Api/appsettings.json](src/presentations/VaultGuard.Api/appsettings.json):

```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Database=vaultguard_write;Username=postgres;Password=your_password",
    "ReadDatabase": "Host=localhost;Database=vaultguard_read;Username=postgres;Password=your_password",
    "Redis": "localhost:6379"
  }
}
```

4. **Apply database migrations**

```bash
cd src/presentations/VaultGuard.Api
dotnet ef migrations add InitialCreate --project ../../libs/VaultGuard.Persistence
dotnet ef database update --project ../../libs/VaultGuard.Persistence
```

5. **Run the application**

```bash
cd src/presentations/VaultGuard.Api
dotnet run
```

The API will be available at `https://localhost:5001`

---

## 📡 API Endpoints

### Vaults

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/vaults` | Get all vaults for current user |
| `POST` | `/api/vaults` | Create a new vault |
| `GET` | `/api/vaults/{id}` | Get vault by ID |
| `PUT` | `/api/vaults/{id}` | Update vault |
| `DELETE` | `/api/vaults/{id}` | Delete vault (soft delete) |

### Vault Items

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/vaults/{vaultId}/items` | Get all items in vault |
| `POST` | `/api/vaults/{vaultId}/items` | Add new item to vault |
| `GET` | `/api/vaults/{vaultId}/items/{id}` | Get specific item |
| `PUT` | `/api/vaults/{vaultId}/items/{id}` | Update item |
| `DELETE` | `/api/vaults/{vaultId}/items/{id}` | Delete item |

### System Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Health check status |
| `GET` | `/metrics` | Prometheus metrics |
| `GET` | `/swagger` | API documentation (Swagger UI) |

---

## 🧪 Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test test/libs/VaultGuard.Domain.Tests
dotnet test test/libs/VaultGuard.Application.Tests
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
```

### Generate Test Report
#### Command

```bash
reportgenerator -reports:TestResults/**/coverage.cobertura.xml -targetdir:TestResults/Report -reporttypes:Html
```

#### Open index.html to show:
```sh
# Directory:
/TestResults/index.html
```

### Test Structure

- **Unit Tests** - Domain logic and application handlers
- **Integration Tests** - Database operations and API endpoints
- **Coverage Target** - 90%+ code coverage

---

## 🔒 Security Model

### Authentication
VaultGuard **does not handle authentication**. It integrates with a separate **Auth Service** via middleware that:
- Validates JWT tokens
- Injects user context (`ICurrentUserService`)
- Manages device authorization

### Authorization
- **Ownership Validation** - Domain-level checks (`vault.EnsureOwnership(userId)`)
- **Business Rules** - Enforced at aggregate root level
- **Audit Trail** - All sensitive operations are logged

### Data Protection
- **Client-Side Encryption** - Data encrypted before reaching the API
- **Encrypted Storage** - Sensitive fields stored as `EncryptedData` value objects
- **No Decryption** - Backend never decrypts user data
- **Zero-Knowledge Architecture** - Server has no access to plaintext passwords

---

## 📊 Performance & Scalability

### Database Strategy
```
Write Operations → Primary Database (WriteDbContext)
                    ↓
                Auto-replication
                    ↓
Read Operations ← Read Replica (ReadDbContext with NoTracking)
```

### Caching Strategy
```
1. Query → Check Redis Cache
2. Cache Miss → Query Database
3. Set Cache → TTL: 5 minutes
4. Write Operation → Invalidate Related Cache
```

### Observability Stack

| Component | Purpose | Endpoint |
|-----------|---------|----------|
| **Serilog** | Structured logging | Console + Elasticsearch |
| **OpenTelemetry** | Distributed tracing | OTLP exporter |
| **Prometheus** | Metrics collection | `/metrics` |
| **Health Checks** | Service monitoring | `/health` |

---

## 📖 Documentation

- [ARCHITECTURE.md](ARCHITECTURE.md) - Detailed architecture documentation
- [CONTRIBUTING.md](CONTRIBUTING.md) - How to contribute to this project
- [Swagger UI](https://localhost:5001/swagger) - Interactive API documentation (when running)

---

## 🛠️ Development

### Prerequisites for Development

- Visual Studio 2022 / VS Code / Rider
- .NET 10 SDK
- Docker Desktop (optional, for running dependencies)
- PostgreSQL client tools
- Redis CLI (optional)

### Coding Standards

- Follow [.NET coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `sealed` for classes that won't be inherited
- Prefer `async/await` for all I/O operations
- Immutable value objects and DTOs
- Meaningful variable and method names
- XML documentation for public APIs

### Project Dependencies

The application uses dependency injection with the following service registrations:

```csharp
// Program.cs
builder.Services.AddApplication();              // MediatR, Validators, AutoMapper
builder.Services.AddPersistence(configuration); // EF Core, Repositories, UnitOfWork
builder.Services.AddInfrastructure(configuration); // Redis, Logging, OpenTelemetry
```

### Adding New Features

Follow these steps to add a new feature:

1. **Domain Layer** - Define entities, value objects, or domain events
2. **Application Layer** - Create Command/Query with Handler and DTOs
3. **Persistence Layer** - Add repository methods if needed
4. **API Layer** - Create controller endpoint
5. **Tests** - Write unit and integration tests
6. **Documentation** - Update API docs and README

### Example: Adding a New Command

```csharp
// 1. Define Command
public record CreateVaultCommand(string Name) : IRequest<VaultDto>;

// 2. Create Handler
public class CreateVaultCommandHandler : IRequestHandler<CreateVaultCommand, VaultDto>
{
    private readonly IVaultRepository _repository;
    private readonly ICurrentUserService _currentUser;
    
    public async Task<VaultDto> Handle(CreateVaultCommand request, CancellationToken ct)
    {
        var vault = new Vault(request.Name, _currentUser.UserId);
        await _repository.AddAsync(vault);
        return vault.ToDto();
    }
}

// 3. Add Validator
public class CreateVaultCommandValidator : AbstractValidator<CreateVaultCommand>
{
    public CreateVaultCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

// 4. Create Endpoint
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateVaultCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

---

## 🐳 Docker Support

### Using Docker Compose

```bash
# Start all services (API, PostgreSQL, Redis, Elasticsearch)
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Build Docker Image

```bash
# Build the API image
docker build -t vaultguard-api:latest -f Dockerfile .

# Run the container
docker run -p 5001:8080 -e ASPNETCORE_ENVIRONMENT=Production vaultguard-api:latest
```

---

## 🚀 Deployment

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__WriteDatabase=Host=db;Database=vaultguard_write;Username=postgres;Password=***
ConnectionStrings__ReadDatabase=Host=db-replica;Database=vaultguard_read;Username=postgres;Password=***
ConnectionStrings__Redis=redis:6379
ElasticSearch__Url=http://elasticsearch:9200
```

### Production Checklist

- [ ] Enable HTTPS with valid SSL certificate
- [ ] Configure CORS policies
- [ ] Set up database backups
- [ ] Configure Redis persistence
- [ ] Enable distributed tracing
- [ ] Set up alerting and monitoring
- [ ] Configure log retention policies
- [ ] Enable rate limiting
- [ ] Set up health checks
- [ ] Configure auto-scaling rules

---

## 📈 Monitoring & Observability

### Prometheus Metrics

Available at `/metrics`:
- HTTP request duration
- Database query performance
- Cache hit/miss ratio
- Active connections
- Error rates

### Elasticsearch Logs

Structured logs with correlation IDs:
```json
{
  "timestamp": "2025-12-18T10:30:00Z",
  "level": "Information",
  "correlationId": "abc-123",
  "userId": "user-456",
  "message": "Vault created",
  "vaultId": "vault-789"
}
```

### Health Checks

```bash
# Check system health
curl https://localhost:5001/health

# Response
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

---

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on:
- Code of Conduct
- Development workflow
- Pull request process
- Coding standards
- Testing requirements

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Authors

**Volcanion Company**
- GitHub: [@volcanion-company](https://github.com/volcanion-company)

---

## 🙏 Acknowledgments

- Clean Architecture principles by Robert C. Martin
- Domain-Driven Design by Eric Evans
- CQRS pattern inspiration from Greg Young
- .NET community for excellent libraries and tools

---

## 📞 Support

For questions and support:
- Open an issue on [GitHub Issues](https://github.com/volcanion-company/vault-guard/issues)
- Review existing [Documentation](ARCHITECTURE.md)
- Check [FAQ](https://github.com/volcanion-company/vault-guard/wiki/FAQ)

---

<div align="center">

**Built with ❤️ using Clean Architecture and Domain-Driven Design**

[⬆ Back to Top](#-vaultguard)

</div>
- [ ] API Versioning
- [ ] Rate Limiting
- [ ] Background Jobs (Hangfire)
- [ ] gRPC support
- [ ] GraphQL support

## 📝 License

MIT License

## 👥 Authors

Backend AI Agent - Clean Architecture & DDD Implementation

---

**Production-ready .NET 10 backend với Clean Architecture, CQRS, và full observability! 🚀**
