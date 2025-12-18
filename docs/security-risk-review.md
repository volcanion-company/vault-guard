# VaultGuard – Code Risk Review

Date: 2025-12-18

This document summarizes security and reliability risks identified across the solution and provides targeted recommendations. File links reference the current workspace.

## High Severity

- Hardcoded credentials and secrets in development config: Plaintext Postgres credentials and JWT secret present in configuration.
  - Evidence: [src/presentations/VaultGuard.Api/appsettings.Development.json](src/presentations/VaultGuard.Api/appsettings.Development.json)
  - Risk: Credential leakage, accidental commit/rotation burden; default `postgres/postgres` is particularly risky.
  - Fix:
    - Move secrets to environment variables or User Secrets (for local), or a secret manager (Key Vault/Parameter Store).
    - Remove defaults from repo; rotate credentials and JWT keys immediately.

- Missing authentication pipeline and authorization attributes: Custom JWT middleware is used, but no `AddAuthentication`/`UseAuthentication` and no `[Authorize]` on controllers.
  - Evidence: [src/presentations/VaultGuard.Api/Program.cs](src/presentations/VaultGuard.Api/Program.cs), [src/presentations/VaultGuard.Api/Controllers/VaultsController.cs](src/presentations/VaultGuard.Api/Controllers/VaultsController.cs), [src/presentations/VaultGuard.Api/Controllers/ItemsController.cs](src/presentations/VaultGuard.Api/Controllers/ItemsController.cs)
  - Risk: Endpoints are likely accessible anonymously; authorization may not be enforced consistently.
  - Fix:
    - Register and configure ASP.NET Core authentication with JWT bearer (`AddAuthentication().AddJwtBearer(...)`).
    - Add `app.UseAuthentication();` before `UseAuthorization();`.
    - Decorate controllers/actions with `[Authorize]` and use policies/roles as needed.

- PII in logs at Information level (email, user id):
  - Evidence: [src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs), [src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs)
  - Risk: Privacy impact and compliance issues (PII in logs), potential data leakage.
  - Fix:
    - Remove or mask PII fields; lower verbosity; use GDPR/PII scrubbing enrichers.

- Redis key deletion by prefix scans entire keyspace:
  - Evidence: [src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs](src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs)
  - Risk: `server.Keys(pattern)` causes O(N) scans; can block Redis in production; not safe at scale.
  - Fix:
    - Avoid SCAN/KEYS in application paths. Use a key index set per prefix, namespacing, or tagging; delete using stored key lists; consider Redis modules or per-vault key sets.

## Medium Severity

- Exception details returned to clients:
  - Evidence: [src/presentations/VaultGuard.Api/Middleware/GlobalExceptionHandlingMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/GlobalExceptionHandlingMiddleware.cs)
  - Risk: Leaks internal error messages (`exception.Message`) in generic 500s.
  - Fix:
    - In production, return a generic message and a correlation id; hide internal messages.

- Custom JWT auth middleware swallows non-expiry exceptions and continues pipeline:
  - Evidence: [src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs)
  - Risk: Requests proceed unauthenticated without clear failure; may bypass expectations.
  - Fix:
    - Rely on framework auth (`AddJwtBearer`) to challenge/forbid; or ensure failures short-circuit with 401/403 consistently.

- Serilog Elastic sink hard dependency on config key:
  - Evidence: [src/libs/VaultGuard.Infrastructure/DependencyInjection.cs](src/libs/VaultGuard.Infrastructure/DependencyInjection.cs)
  - Risk: `configuration["Elasticsearch:Uri"]!` will throw if unset, preventing startup.
  - Fix:
    - Guard the sink registration; enable only when configured. Prefer `HostBuilder` integration using the runtime configuration.

- CORS settings present but unused:
  - Evidence: AllowedOrigins in [src/presentations/VaultGuard.Api/appsettings.Development.json](src/presentations/VaultGuard.Api/appsettings.Development.json)
  - Risk: Either overly permissive default or missing required CORS; can cause integration issues or unsafe defaults if later enabled loosely.
  - Fix:
    - Add explicit `AddCors` policy using configured origins and `app.UseCors("namedPolicy")`.

- Redis cache entries default to no expiration:
  - Evidence: [src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs](src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs)
  - Risk: Unbounded growth; stale data.
  - Fix:
    - Set sensible default TTLs; require explicit expiration in call sites.

- Namespace inconsistencies (`VaultGuard.API.*` vs `VaultGuard.Api.*`):
  - Evidence: [src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs), [src/presentations/VaultGuard.Api/Extensions/HttpContextExtensions.cs](src/presentations/VaultGuard.Api/Extensions/HttpContextExtensions.cs)
  - Risk: Confusion, potential import mistakes.
  - Fix:
    - Align namespaces to `VaultGuard.Api.*`.

## Low Severity / Design Considerations

- Logging configuration bootstrap reads only base appsettings for Serilog:
  - Evidence: [src/presentations/VaultGuard.Api/Program.cs](src/presentations/VaultGuard.Api/Program.cs)
  - Risk: May ignore environment-specific Serilog settings.
  - Fix:
    - Use `builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));` and remove manual config bootstrap.

- Controllers lack `ProducesResponseType` and validation summaries:
  - Evidence: [src/presentations/VaultGuard.Api/Controllers](src/presentations/VaultGuard.Api/Controllers)
  - Improvement: Improve API surface clarity and client expectations.

- DTOs include encrypted payload and IV as a combined string:
  - Evidence: [src/libs/VaultGuard.Application/Features/VaultItems/Queries/GetVaultItemsQueryHandler.cs](src/libs/VaultGuard.Application/Features/VaultItems/Queries/GetVaultItemsQueryHandler.cs)
  - Note: Ensure clients never log/store these values in plaintext logs; audit masking.

- Health checks assume configured connection strings:
  - Evidence: [src/presentations/VaultGuard.Api/Program.cs](src/presentations/VaultGuard.Api/Program.cs)
  - Risk: Startup failure when missing.
  - Fix:
    - Make health checks conditional or fail-soft in dev.

## Recommended Remediation Plan

1) Authentication and Authorization
- Add framework auth: register JWT bearer and call `UseAuthentication()` before `UseAuthorization()`.
- Add `[Authorize]` to controllers/actions; define policies/roles.

2) Secrets Management
- Remove secrets from repo; use User Secrets/env vars locally and secret store in non-dev.
- Rotate DB/JWT credentials.

3) Logging and PII
- Remove or mask PII (emails, user IDs) from logs, especially at `Information`.
- In exception middleware, hide internal messages in production; include `RequestId`.

4) Redis Caching
- Replace prefix deletion via SCAN/KEYS with key-index sets or logical namespaces and tracked key lists.
- Enforce TTL defaults for all cache sets.

5) Configuration Robustness
- Integrate Serilog via `HostBuilder` with environment-aware config; guard optional sinks (Elasticsearch).
- Implement CORS policy from config and apply explicitly.

6) Hygiene and Robustness
- Normalize namespaces (`VaultGuard.Api`).
- Add response types and validation problem details to APIs.
- Consider rate limiting and request size limits.

## Quick Diffs/Files to Update

- Auth/CORS/bootstrap: [src/presentations/VaultGuard.Api/Program.cs](src/presentations/VaultGuard.Api/Program.cs)
- Secrets out of configs: [src/presentations/VaultGuard.Api/appsettings.Development.json](src/presentations/VaultGuard.Api/appsettings.Development.json)
- PII logging: [src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/RequestLoggingMiddleware.cs), [src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/JwtAuthenticationMiddleware.cs)
- Exception responses: [src/presentations/VaultGuard.Api/Middleware/GlobalExceptionHandlingMiddleware.cs](src/presentations/VaultGuard.Api/Middleware/GlobalExceptionHandlingMiddleware.cs)
- Redis scan removal and TTLs: [src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs](src/libs/VaultGuard.Infrastructure/Caching/RedisCacheService.cs)
- Serilog sink guard: [src/libs/VaultGuard.Infrastructure/DependencyInjection.cs](src/libs/VaultGuard.Infrastructure/DependencyInjection.cs)

## Notes

- Tests exist for exceptions and handlers; after changes, run the suite and validate.
- Consider adding security-focused tests (auth required, CORS headers, cache behaviors) and integration tests.
