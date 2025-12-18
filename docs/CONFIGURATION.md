# VaultGuard - Configuration Guide

## Local Development Setup

### User Secrets Configuration

This project uses ASP.NET Core User Secrets for local development to keep sensitive information out of source control.

### Initial Setup

1. Navigate to the API project directory:
   ```bash
   cd src/presentations/VaultGuard.Api
   ```

2. Initialize User Secrets (if not already done):
   ```bash
   dotnet user-secrets init
   ```

3. Set required secrets:

   ```bash
   # Database Connection Strings
   dotnet user-secrets set "ConnectionStrings:WriteDatabase" "Host=YOUR_HOST;Port=5432;Database=vaultguard;Username=YOUR_USER;Password=YOUR_PASSWORD;"
   dotnet user-secrets set "ConnectionStrings:ReadDatabase" "Host=YOUR_HOST;Port=5432;Database=vaultguard;Username=YOUR_USER;Password=YOUR_PASSWORD;"
   dotnet user-secrets set "ConnectionStrings:Redis" "YOUR_REDIS_HOST:6379"

   # JWT Settings
   dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_256_BIT_SECRET_KEY_HERE_MINIMUM_32_CHARACTERS"
   
   # Elasticsearch (optional)
   dotnet user-secrets set "Elasticsearch:Uri" "http://localhost:9200"
   ```

### Example Configuration

#### PostgreSQL (Local)
```bash
dotnet user-secrets set "ConnectionStrings:WriteDatabase" "Host=localhost;Port=5432;Database=vaultguard;Username=vaultguard_user;Password=YourSecurePassword123!"
dotnet user-secrets set "ConnectionStrings:ReadDatabase" "Host=localhost;Port=5432;Database=vaultguard;Username=vaultguard_user;Password=YourSecurePassword123!"
```

#### Redis (Local)
```bash
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
```

#### JWT Secret Key
Generate a secure key (minimum 256-bit / 32 characters):
```bash
# PowerShell
$key = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
dotnet user-secrets set "JwtSettings:SecretKey" $key
```

```bash
# Linux/macOS
openssl rand -base64 64 | tr -d '\n' | dotnet user-secrets set "JwtSettings:SecretKey"
```

### View Current Secrets

```bash
dotnet user-secrets list
```

### Remove a Secret

```bash
dotnet user-secrets remove "ConnectionStrings:WriteDatabase"
```

### Clear All Secrets

```bash
dotnet user-secrets clear
```

---

## Production/Staging Deployment

**DO NOT use User Secrets in production.** Use one of these approaches:

### Option 1: Environment Variables
```bash
export ConnectionStrings__WriteDatabase="Host=prod-db;Port=5432;Database=vaultguard;Username=app_user;Password=SecurePass"
export ConnectionStrings__ReadDatabase="Host=prod-replica;Port=5432;Database=vaultguard;Username=app_user;Password=SecurePass"
export ConnectionStrings__Redis="prod-redis:6379"
export JwtSettings__SecretKey="ProductionSecretKey256BitMinimum"
```

### Option 2: Azure Key Vault
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Option 3: AWS Secrets Manager / Parameter Store
```csharp
builder.Configuration.AddSecretsManager();
```

---

## Required Secrets Reference

| Key | Description | Example |
|-----|-------------|---------|
| `ConnectionStrings:WriteDatabase` | PostgreSQL write connection | `Host=localhost;Port=5432;Database=vaultguard;Username=user;Password=pass;` |
| `ConnectionStrings:ReadDatabase` | PostgreSQL read replica | `Host=localhost;Port=5432;Database=vaultguard;Username=user;Password=pass;` |
| `ConnectionStrings:Redis` | Redis cache connection | `localhost:6379` |
| `JwtSettings:SecretKey` | JWT signing key (min 256-bit) | `64+ character secure random string` |
| `Elasticsearch:Uri` (optional) | Elasticsearch endpoint | `http://localhost:9200` |

---

## Security Notes

- ✅ Never commit secrets to source control
- ✅ Use User Secrets for local development only
- ✅ Use managed secret stores (Key Vault, Secrets Manager) for production
- ✅ Rotate secrets regularly
- ✅ Use different secrets per environment
- ✅ Grant minimal necessary permissions to service accounts

---

## Troubleshooting

### "JWT SecretKey not configured" error
- Ensure you've set the `JwtSettings:SecretKey` via user-secrets or environment variable
- Verify the key is at least 32 characters (256-bit)

### Database connection fails
- Check your connection string format
- Verify database server is running and accessible
- Confirm credentials are correct

### User Secrets not loading
- Ensure you're running in Development environment
- Check that `<UserSecretsId>` exists in `.csproj`
- Verify secrets are set: `dotnet user-secrets list`
