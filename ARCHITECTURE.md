# VaultGuard - Password Manager Backend Architecture

## 📋 Tổng Quan Hệ Thống

VaultGuard là backend API cho hệ thống Password Manager được xây dựng theo **Clean Architecture** và **Domain-Driven Design (DDD)**, tuân thủ các nguyên tắc SOLID và tối ưu cho môi trường production.

### Technology Stack

- **.NET 10** - Web API Framework
- **C# 13** - Programming Language
- **PostgreSQL** - Database (Read/Write separation)
- **Entity Framework Core 10** - ORM
- **Redis** - Distributed Caching
- **Serilog + Elasticsearch** - Logging & Observability
- **OpenTelemetry + Prometheus** - Metrics & Monitoring
- **MediatR** - CQRS Pattern Implementation
- **FluentValidation** - Input Validation

---

## 🏗️ Clean Architecture Layers

### 1. Domain Layer (`VaultGuard.Domain`)
**Trách nhiệm:** Core business logic, không phụ thuộc vào infrastructure

**Thành phần:**
- **Entities:** User, Vault, VaultItem, Device, AuditLog
- **Aggregates:** Vault (Aggregate Root), User (Aggregate Root)
- **Value Objects:** EncryptedData
- **Domain Events:** VaultCreatedEvent, VaultItemCreatedEvent
- **Repository Interfaces:** IVaultRepository, IVaultItemRepository, IUnitOfWork

**Quyết định thiết kế:**
- Không reference bất kỳ external library nào (trừ System)
- Entity methods enforce business rules (ví dụ: `vault.EnsureOwnership()`)
- Value Objects immutable để đảm bảo data integrity

### 2. Application Layer (`VaultGuard.Application`)
**Trách nhiệm:** Orchestrate business flows, CQRS implementation

**Thành phần:**
- **Commands:** CreateVaultCommand, CreateVaultItemCommand
- **Queries:** GetVaultsQuery, GetVaultItemsQuery
- **Handlers:** MediatR Request Handlers
- **DTOs:** VaultDto, VaultItemDto
- **Validators:** FluentValidation validators
- **Interfaces:** ICurrentUserService, ICacheService

**Quyết định thiết kế:**
- **CQRS:** Tách biệt Command (Write) và Query (Read) operations
- **Commands** sử dụng WriteDbContext → Primary database
- **Queries** sử dụng ReadDbContext → Read replica
- Validators tách biệt từ Handlers để dễ maintain
- DTOs không expose domain entities ra ngoài

### 3. Persistence Layer (`VaultGuard.Persistence`)
**Trách nhiệm:** Data access, EF Core configuration

**Thành phần:**
- **DbContexts:** 
  - `WriteDbContext` - Primary database cho write operations
  - `ReadDbContext` - Read replica cho query operations (no tracking)
- **Entity Configurations:** Fluent API configurations
- **Repositories:** VaultRepository, VaultItemRepository, AuditLogRepository
- **UnitOfWork:** Transaction management

**Quyết định thiết kế:**
- **Read/Write Database Separation:**
  - Write operations → Primary database (WriteDbContext)
  - Read operations → Read replica (ReadDbContext với `NoTracking`)
  - Tăng performance và scalability
- **Repository Pattern:** Abstraction layer giữa domain và data access
- **Unit of Work:** Quản lý transactions, đảm bảo atomicity
- **Value Object Mapping:** EncryptedData mapped as owned entity
- **Soft Delete:** Global query filter `HasQueryFilter(x => !x.IsDeleted)`

### 4. Infrastructure Layer (`VaultGuard.Infrastructure`)
**Trách nhiệm:** External services, cross-cutting concerns

**Thành phần:**
- **Redis Caching:** RedisCacheService implementation
- **Logging:** Serilog configuration với Elasticsearch sink
- **OpenTelemetry:** Distributed tracing và metrics
- **Prometheus:** Metrics exporter

**Quyết định thiết kế:**
- **Cache-Aside Pattern:** Query check cache → DB → set cache
- **Cache Invalidation:** Xóa cache sau mỗi write operation
- **Structured Logging:** Serilog với correlation ID
- **Elasticsearch Sink:** Centralized logging cho production
- **OpenTelemetry:** Standard observability framework

### 5. API Layer (`VaultGuard.Api`)
**Trách nhiệm:** HTTP endpoints, middleware pipeline

**Thành phần:**
- **Controllers:** VaultsController, ItemsController
- **Middleware:** 
  - JwtAuthenticationMiddleware (từ Auth Service)
  - UserContextMiddleware (từ Auth Service)
  - RequestLoggingMiddleware
- **Services:** CurrentUserService (adapts IUserContextService)
- **Program.cs:** DI configuration, middleware pipeline

**Quyết định thiết kế:**
- **NO AUTH IMPLEMENTATION:** Sử dụng middleware từ Auth Service
- **Adapter Pattern:** CurrentUserService bridge IUserContextService → ICurrentUserService
- **Health Checks:** PostgreSQL, Redis health monitoring
- **Prometheus Endpoint:** `/metrics` cho monitoring
- **Swagger:** `/swagger` cho API documentation

---

## 🔑 Các Quyết Định Kiến Trúc Quan Trọng

### 1. CQRS với Read/Write Database Separation
**Vấn đề:** Performance bottleneck khi scale read operations
**Giải pháp:**
- Write operations → Primary PostgreSQL database
- Read operations → Read replica database
- Commands và Queries tách biệt hoàn toàn

**Lợi ích:**
- Read queries không block write operations
- Scale read replicas independently
- Optimize read database (indexes, materialized views)

### 2. Repository Pattern + Unit of Work
**Vấn đề:** Tight coupling với EF Core, khó test
**Giải pháp:**
- Repository interfaces trong Domain layer
- Implementations trong Persistence layer
- Unit of Work quản lý transactions

**Lợi ích:**
- Domain layer không phụ thuộc vào EF Core
- Dễ dàng mock repositories trong unit tests
- Transaction management tập trung

### 3. Cache-Aside Pattern với Redis
**Vấn đề:** Database query performance
**Giải pháp:**
```
Query → Check Redis → 
  If Hit → Return cached data
  If Miss → Query DB → Cache result → Return
```

**Cache Invalidation:**
- Write operations invalidate cache by prefix
- Example: `vault:created` → invalidate `vaults:{userId}*`

### 4. Value Objects cho Encrypted Data
**Vấn đề:** CipherText và IV luôn đi cùng nhau
**Giải pháp:**
```csharp
public class EncryptedData
{
    public string CipherText { get; }
    public string InitializationVector { get; }
}
```

**Lợi ích:**
- Type safety
- Immutability
- Validation tập trung

### 5. Domain Events (Future Enhancement)
**Mục đích:** Decouple domain logic
**Ví dụ:**
```csharp
VaultCreatedEvent → 
  - Send notification
  - Update analytics
  - Audit logging
```

### 6. Auth Service Integration
**Vấn đề:** Backend KHÔNG triển khai authentication
**Giải pháp:**
- Sử dụng middleware từ Auth Service
- IUserContextService → Adapter → ICurrentUserService
- Application layer chỉ biết ICurrentUserService

**Lợi ích:**
- Separation of concerns
- Auth Service có thể thay đổi independently
- Backend chỉ focus vào business logic

---

## 📊 Data Flow Examples

### Command Flow (Write Operation)
```
Client → Controller → MediatR → CommandHandler →
  → Domain Logic (Aggregate) →
  → Repository (WriteDbContext) →
  → UnitOfWork.SaveChanges →
  → AuditLog →
  → Cache Invalidation →
  → Response
```

### Query Flow (Read Operation)
```
Client → Controller → MediatR → QueryHandler →
  → Check Cache →
    If Hit → Return
    If Miss → Repository (ReadDbContext) →
      → Set Cache → Return
```

---

## 🧪 Testing Strategy

### Unit Tests (Application Layer)
- Mock ICurrentUserService (no real auth needed)
- Mock repositories
- Test business logic in isolation
- Example: `CreateVaultCommandHandlerTests`

### Integration Tests (Future)
- Test with real database (TestContainers)
- Test middleware pipeline
- Test cache behavior

---

## 📈 Scalability & Performance

### Database
- Read replica cho queries → horizontal scaling
- Write database với connection pooling
- Indexes trên UserId, VaultId, CreatedAt

### Caching
- Redis distributed cache
- Cache TTL: 5 minutes
- Cache invalidation on writes

### Observability
- OpenTelemetry tracing
- Prometheus metrics
- Elasticsearch logging
- Health checks endpoint

---

## 🔒 Security Considerations

### Data Encryption
- Backend KHÔNG handle encryption/decryption
- Client-side encryption only
- Backend store encrypted payloads as-is

### Authorization
- Ownership checks: `vault.EnsureOwnership(userId)`
- Middleware validates JWT
- Application layer validates business rules

### Audit Logging
- Mọi sensitive operations được audit
- Log gồm: UserId, Action, IP, UserAgent, Timestamp

---

## 🚀 Deployment Considerations

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`
- Database connection strings
- Redis connection string
- Elasticsearch URI

### Docker Support
- Multi-stage build
- PostgreSQL container
- Redis container
- Elasticsearch container

### Monitoring
- Prometheus scraping `/metrics`
- Grafana dashboards
- Elasticsearch + Kibana logs

---

## 📝 Best Practices Implemented

1. **SOLID Principles**
2. **Clean Architecture** - Dependency rule
3. **Domain-Driven Design** - Aggregates, Value Objects
4. **CQRS** - Command Query Separation
5. **Repository Pattern**
6. **Unit of Work Pattern**
7. **Immutable Value Objects**
8. **Async/Await** everywhere
9. **Structured Logging**
10. **Health Checks**

---

## 🎯 Production Readiness Checklist

✅ **Architecture**
- Clean Architecture layers
- CQRS implementation
- Repository + UnitOfWork

✅ **Data Access**
- Read/Write database separation
- EF Core migrations ready
- Proper indexing strategy

✅ **Caching**
- Redis distributed cache
- Cache invalidation logic

✅ **Observability**
- Structured logging (Serilog)
- Distributed tracing (OpenTelemetry)
- Metrics (Prometheus)
- Health checks

✅ **Security**
- Auth middleware integration
- Ownership validation
- Audit logging

✅ **Testing**
- Unit tests with mocks
- Application layer coverage

---

## 🔄 Future Enhancements

1. **Domain Events Publishing**
   - MediatR domain event dispatcher
   - Decouple side effects

2. **Outbox Pattern**
   - Reliable event publishing
   - Transaction consistency

3. **API Versioning**
   - URL-based versioning
   - Backward compatibility

4. **Rate Limiting**
   - Per-user rate limits
   - Redis-based throttling

5. **Background Jobs**
   - Hangfire/Quartz.NET
   - Cleanup old audit logs

---

**Kiến trúc này production-ready, stateless, và sẵn sàng scale theo chiều ngang!** 🚀
