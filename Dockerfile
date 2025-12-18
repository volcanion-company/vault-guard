# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file
COPY ["vault-guard.sln", "./"]

# Copy project files
COPY ["src/presentations/VaultGuard.Api/VaultGuard.Api.csproj", "src/presentations/VaultGuard.Api/"]
COPY ["src/libs/VaultGuard.Application/VaultGuard.Application.csproj", "src/libs/VaultGuard.Application/"]
COPY ["src/libs/VaultGuard.Domain/VaultGuard.Domain.csproj", "src/libs/VaultGuard.Domain/"]
COPY ["src/libs/VaultGuard.Infrastructure/VaultGuard.Infrastructure.csproj", "src/libs/VaultGuard.Infrastructure/"]
COPY ["src/libs/VaultGuard.Persistence/VaultGuard.Persistence.csproj", "src/libs/VaultGuard.Persistence/"]

# Restore dependencies
RUN dotnet restore "src/presentations/VaultGuard.Api/VaultGuard.Api.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/src/presentations/VaultGuard.Api"
RUN dotnet build "VaultGuard.Api.csproj" -c Release -o /app/build
RUN dotnet publish "VaultGuard.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "VaultGuard.Api.dll"]
