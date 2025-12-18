# VaultGuard – Remediation TODO

> Based on [security-risk-review.md](security-risk-review.md) | Created: 2025-12-18

## Legend
- [ ] Not started
- [x] Completed
- 🔴 High | 🟠 Medium | 🟢 Low

---

## Phase 1: Authentication & Authorization (Critical) ✅

### 🔴 1.1 Configure ASP.NET Core JWT Authentication
- [x] Add `Microsoft.AspNetCore.Authentication.JwtBearer` package (if not present)
- [x] Register JWT Bearer authentication in `Program.cs`:
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => { /* config from JwtSettings */ });
  ```
- [x] Add `app.UseAuthentication()` before `app.UseAuthorization()`
- [x] File: `src/presentations/VaultGuard.Api/Program.cs`

### 🔴 1.2 Add Authorization Attributes to Controllers
- [x] Add `[Authorize]` attribute to `VaultsController`
- [x] Add `[Authorize]` attribute to `ItemsController`
- [x] Files:
  - `src/presentations/VaultGuard.Api/Controllers/VaultsController.cs`
  - `src/presentations/VaultGuard.Api/Controllers/ItemsController.cs`

### 🟠 1.3 Refactor Custom JWT Middleware
- [x] Option A: Remove custom middleware, rely on framework auth
- [x] File: `src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs`
- [x] **Implementation:** [PHASE1_IMPLEMENTATION.md](PHASE1_IMPLEMENTATION.md)

---

## Phase 2: Secrets Management (Critical) ✅

### 🔴 2.1 Remove Hardcoded Secrets from Config
- [x] Remove `ConnectionStrings` with actual credentials from `appsettings.Development.json`
- [x] Remove `JwtSettings.SecretKey` from `appsettings.Development.json`
- [x] Replace with placeholders or empty values
- [x] File: `src/presentations/VaultGuard.Api/appsettings.Development.json`

### 🔴 2.2 Implement User Secrets for Local Development
- [x] Initialize User Secrets: `dotnet user-secrets init` in Api project
- [x] Document required secrets in README or `.env.example`
- [x] Update `Program.cs` to load User Secrets in Development
- [x] **Documentation:** [CONFIGURATION.md](CONFIGURATION.md)

### 🔴 2.3 Credential Rotation
- [ ] Rotate PostgreSQL credentials
- [ ] Generate new JWT SecretKey (min 256-bit)
- [ ] Update deployed environments
- [x] **Implementation:** [PHASE2_IMPLEMENTATION.md](PHASE2_IMPLEMENTATION.md)

---

## Phase 3: Logging & PII Protection (High) ✅

### 🔴 3.1 Remove PII from Request Logging Middleware
- [x] Remove `UserId` and `Email` from log messages
- [x] File: `src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs`

### 🔴 3.2 Remove PII from JWT Middleware Logs
- [x] Remove `UserId`, `Email`, `Roles` from debug logs
- [x] File: `src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs`

### 🟠 3.3 Hide Exception Details in Production
- [x] Check `IHostEnvironment.IsProduction()` in exception handler
- [x] Return generic message + `RequestId` for 500 errors
- [x] Keep detailed errors only in Development
- [x] File: `src/presentations/VaultGuard.Api/Middleware/GlobalExceptionHandlingMiddleware.cs`
- [x] **Implementation:** [PHASE3_IMPLEMENTATION.md](PHASE3_IMPLEMENTATION.md)

---

## Phase 4: Redis Caching Improvements (High) ✅

### 🔴 4.1 Replace KEYS/SCAN-based Prefix Deletion
- [x] Implement key tracking with Redis Sets per prefix
- [x] On `SetAsync`: add key to tracking set
- [x] On `RemoveByPrefixAsync`: read set, delete keys, delete set
- [x] File: `src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs`

### 🟠 4.2 Enforce Default TTL
- [x] Add default expiration (30 minutes) when `expiration` is null
- [x] File: `src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs`
- [x] **Implementation:** [PHASE4_IMPLEMENTATION.md](PHASE4_IMPLEMENTATION.md)

---

## Phase 5: Configuration Robustness (Medium) ✅

### 🟠 5.1 Guard Elasticsearch Sink Registration
- [x] Check if `Elasticsearch:Uri` is configured before adding sink
- [x] Make Elasticsearch logging optional
- [x] File: `src/libs/VaultGuard.Infrastructure/DependencyInjection.cs`

### 🟠 5.2 Improve Serilog Bootstrap
- [x] Use `builder.Host.UseSerilog((ctx, lc) => ...)` pattern
- [x] Read from `ctx.Configuration` to respect environment overrides
- [x] File: `src/presentations/VaultGuard.Api/Program.cs`

### 🟠 5.3 Implement CORS Policy
- [x] Add `builder.Services.AddCors()` with named policy
- [x] Read allowed origins from `AllowedOrigins` config
- [x] Add `app.UseCors("PolicyName")` in pipeline
- [x] File: `src/presentations/VaultGuard.Api/Program.cs`

### 🟢 5.4 Make Health Checks Conditional
- [x] Guard health check registration when connection strings missing
- [x] Provide fallback/warning in development
- [x] File: `src/presentations/VaultGuard.Api/Program.cs`

---

## Phase 6: Code Hygiene (Low) ✅

### 🟠 6.1 Fix Namespace Inconsistencies
- [x] Rename `VaultGuard.API.Middleware` → `VaultGuard.Api.Middleware`
- [x] Rename `VaultGuard.API.Extensions` → `VaultGuard.Api.Extensions`
- [x] Update all `using` statements
- [x] Files:
  - `src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs`
  - `src/presentations/VaultGuard.Api/Extensions/HttpContextExtensions.cs`
  - `src/presentations/VaultGuard.Api/Program.cs`

### 🟢 6.2 Add API Response Types
- [x] Add `[ProducesResponseType]` attributes to controller actions
- [x] Document 200, 201, 400, 401, 404, 500 responses
- [x] Files:
  - `src/presentations/VaultGuard.Api/Controllers/VaultsController.cs`
  - `src/presentations/VaultGuard.Api/Controllers/ItemsController.cs`

### 🟢 6.3 Consider Rate Limiting
- [ ] Evaluate `Microsoft.AspNetCore.RateLimiting`
- [ ] Add rate limit policies for sensitive endpoints
- [ ] File: `src/presentations/VaultGuard.Api/Program.cs`

**Implementation:** [PHASE5-6_IMPLEMENTATION.md](PHASE5-6_IMPLEMENTATION.md)

---

## Phase 7: Testing & Validation

### 🟠 7.1 Add Security Tests
- [ ] Test unauthorized access returns 401
- [ ] Test CORS headers
- [ ] Test rate limiting (if implemented)

### 🟠 7.2 Add Cache Behavior Tests
- [ ] Test TTL expiration
- [ ] Test prefix deletion
- [ ] Mock Redis for unit tests

### 🟢 7.3 Run Existing Test Suite
- [ ] Verify all existing tests pass after changes
- [ ] Update tests if behavior changed

---

## Summary by Priority

| Priority | Count | Estimated Effort |
|----------|-------|------------------|
| 🔴 High  | 9     | 4-6 hours        |
| 🟠 Medium| 10    | 3-4 hours        |
| 🟢 Low   | 4     | 2-3 hours        |

**Total Estimated Effort:** 9-13 hours

---

## Execution Order

1. **Phase 2** (Secrets) – Immediate security risk
2. **Phase 1** (Auth) – Critical access control
3. **Phase 3** (PII) – Compliance requirement
4. **Phase 4** (Redis) – Production stability
5. **Phase 5** (Config) – Robustness
6. **Phase 6** (Hygiene) – Code quality
7. **Phase 7** (Testing) – Validation

---

## Notes

- Create feature branch for each phase
- Run tests after each phase
- Document breaking changes
- Update README with new setup requirements (User Secrets)
