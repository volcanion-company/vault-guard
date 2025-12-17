# VaultGuard - Password Manager Backend

Backend API production-ready cho hệ thống Password Manager, xây dựng theo Clean Architecture và DDD.

## 🏗️ Kiến Trúc

```
/src
 ├── libs
 │   ├── VaultGuard.Domain          # Core business logic, entities, value objects
 │   ├── VaultGuard.Application     # CQRS, commands, queries, handlers
 │   ├── VaultGuard.Persistence     # EF Core, repositories, UnitOfWork
 │   └── VaultGuard.Infrastructure  # Redis, Serilog, OpenTelemetry
 └── presentations
     └── VaultGuard.Api             # Controllers, middleware, Program.cs

/test
 ├── libs
 │   ├── VaultGuard.Domain.Tests
 │   └── VaultGuard.Application.Tests
 └── presentations
     └── VaultGuard.Api.Tests
```

## ⚡ Technology Stack

- **.NET 10** - Web API
- **PostgreSQL** - Primary database (Read/Write separation)
- **Entity Framework Core 10** - ORM
- **Redis** - Distributed caching
- **MediatR** - CQRS pattern
- **Serilog + Elasticsearch** - Logging
- **OpenTelemetry + Prometheus** - Observability
- **FluentValidation** - Input validation
- **xUnit + Moq + FluentAssertions** - Testing

## 🎯 Core Features

### Domain Models
- **User** - Aggregate root cho user management
- **Vault** - Aggregate root cho password vaults
- **VaultItem** - Items trong vault (passwords, notes, cards)
- **Device** - User's registered devices
- **AuditLog** - Security audit trail

### CQRS Implementation
**Commands (Write):**
- `CreateVaultCommand` - Tạo vault mới
- `CreateVaultItemCommand` - Thêm item vào vault

**Queries (Read):**
- `GetVaultsQuery` - Lấy danh sách vaults
- `GetVaultItemsQuery` - Lấy items trong vault

### Patterns Implemented
- ✅ Clean Architecture
- ✅ Domain-Driven Design (DDD)
- ✅ CQRS (Command Query Responsibility Segregation)
- ✅ Repository Pattern
- ✅ Unit of Work Pattern
- ✅ Cache-Aside Pattern
- ✅ Value Objects

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL 16+
- Redis 7+
- Elasticsearch 8+ (optional, for logging)

### Database Setup

1. **Tạo databases:**
```sql
CREATE DATABASE vaultguard_write;
CREATE DATABASE vaultguard_read;
```

2. **Update connection strings** trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Database=vaultguard_write;Username=postgres;Password=your_password",
    "ReadDatabase": "Host=localhost;Database=vaultguard_read;Username=postgres;Password=your_password",
    "Redis": "localhost:6379"
  }
}
```

3. **Run migrations:**
```bash
cd src/presentations/VaultGuard.Api
dotnet ef migrations add InitialCreate --project ../../libs/VaultGuard.Persistence
dotnet ef database update --project ../../libs/VaultGuard.Persistence
```

### Run Application

```bash
cd src/presentations/VaultGuard.Api
dotnet run
```

API sẽ chạy tại: `https://localhost:5001`

## 📡 API Endpoints

### Vaults
- `GET /api/vaults` - Lấy danh sách vaults
- `POST /api/vaults` - Tạo vault mới

### Vault Items
- `GET /api/vaults/{vaultId}/items` - Lấy items trong vault
- `POST /api/vaults/{vaultId}/items` - Thêm item vào vault

### Monitoring
- `GET /health` - Health check endpoint
- `GET /metrics` - Prometheus metrics
- `GET /swagger` - API documentation

## 🧪 Testing

Run unit tests:
```bash
dotnet test
```

Run specific test project:
```bash
dotnet test test/libs/VaultGuard.Application.Tests
```

## 📊 Architecture Highlights

### Read/Write Database Separation
- **Write operations** → Primary PostgreSQL database
- **Read operations** → Read replica database
- Tối ưu performance và scalability

### Caching Strategy
- Redis distributed cache
- Cache-aside pattern
- Automatic cache invalidation on writes
- TTL: 5 minutes

### Observability
- **Logging:** Serilog → Console + Elasticsearch
- **Tracing:** OpenTelemetry distributed tracing
- **Metrics:** Prometheus endpoint at `/metrics`
- **Health Checks:** Database + Redis health monitoring

### Security
- **NO AUTH LOGIC:** Tích hợp middleware từ Auth Service
- **Ownership Validation:** Domain-level authorization
- **Audit Logging:** Track mọi sensitive operations
- **Encrypted Data:** Client-side encryption only

## 🔒 Security Model

Backend **KHÔNG** handle authentication. Auth được xử lý bởi Auth Service riêng biệt.

**Backend chỉ:**
- Validate ownership (`vault.EnsureOwnership(userId)`)
- Log audit trail
- Store encrypted data (không decrypt)

## 📁 Project Structure

### Domain Layer
```
Domain/
├── Common/          # Base entities, interfaces
├── Entities/        # User, Vault, VaultItem, Device, AuditLog
├── ValueObjects/    # EncryptedData
├── Events/          # Domain events
├── Repositories/    # Repository interfaces
└── Enums/           # Domain enums
```

### Application Layer
```
Application/
├── Common/
│   └── Interfaces/  # ICurrentUserService, ICacheService
├── Features/
│   ├── Vaults/
│   │   ├── Commands/
│   │   └── Queries/
│   └── VaultItems/
│       ├── Commands/
│       └── Queries/
└── DTOs/            # Data Transfer Objects
```

### Persistence Layer
```
Persistence/
├── Contexts/        # WriteDbContext, ReadDbContext
├── Configurations/  # EF Core entity configurations
└── Repositories/    # Repository implementations
```

## 📖 Documentation

Xem [ARCHITECTURE.md](./ARCHITECTURE.md) để hiểu chi tiết về:
- Clean Architecture layers
- CQRS implementation
- Data flow diagrams
- Design decisions
- Best practices

## 🛠️ Development

### Code Style
- Follow .NET coding conventions
- Use `sealed` for classes that won't be inherited
- Async/await everywhere
- Immutable value objects

### Dependency Injection
```csharp
builder.Services.AddApplication();        // MediatR, Validators
builder.Services.AddPersistence(config);  // EF Core, Repositories
builder.Services.AddInfrastructure(config); // Redis, Logging, OTel
```

### Adding New Feature

1. **Domain:** Tạo entity/value object nếu cần
2. **Application:** Tạo Command/Query + Handler
3. **Persistence:** Thêm repository method nếu cần
4. **API:** Tạo controller endpoint
5. **Tests:** Viết unit tests

## 🐳 Docker Support

```bash
# Build image
docker build -t vaultguard-api .

# Run with docker-compose
docker-compose up
```

## 📈 Performance

- **Caching:** Redis giảm database load
- **Read Replica:** Queries không impact writes
- **No Tracking:** ReadDbContext optimize cho queries
- **Connection Pooling:** EF Core connection pooling
- **Async:** Non-blocking operations

## 🔄 Future Enhancements

- [ ] Domain Events publishing
- [ ] Outbox Pattern cho reliability
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
