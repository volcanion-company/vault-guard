# Docker Setup cho VaultGuard

## Cấu trúc Docker

Project sử dụng Docker multi-stage build với các services sau:

### Services
1. **postgres** - PostgreSQL 17 database
2. **redis** - Redis 7 cache
3. **elasticsearch** - Elasticsearch 8.11 cho logging
4. **kibana** - Kibana UI để xem logs (optional)
5. **api** - VaultGuard .NET 10 API

## Yêu cầu

- Docker Desktop hoặc Docker Engine 20.10+
- Docker Compose v2.0+
- Ít nhất 4GB RAM khả dụng cho Docker

## Sử dụng

### 1. Build và chạy tất cả services

```bash
# Chạy tất cả services
docker-compose up -d

# Xem logs
docker-compose logs -f

# Chỉ xem logs của API
docker-compose logs -f api
```

### 2. Chạy riêng lẻ services

```bash
# Chỉ chạy database và cache
docker-compose up -d postgres redis

# Chạy API (sẽ tự động start dependencies)
docker-compose up -d api
```

### 3. Development mode với hot reload

```bash
# Sử dụng override file cho development
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Hoặc nếu chỉ có docker-compose.yml và docker-compose.override.yml
docker-compose up -d  # Tự động merge override file
```

### 4. Rebuild sau khi thay đổi code

```bash
# Rebuild và restart API
docker-compose up -d --build api

# Rebuild toàn bộ
docker-compose up -d --build
```

### 5. Dừng và xóa containers

```bash
# Dừng containers
docker-compose stop

# Xóa containers nhưng giữ volumes
docker-compose down

# Xóa containers và volumes (MẤT DATA!)
docker-compose down -v
```

## Ports

- **8080** - VaultGuard API
- **5432** - PostgreSQL
- **6379** - Redis
- **9200** - Elasticsearch HTTP
- **9300** - Elasticsearch Transport
- **5601** - Kibana UI

## Truy cập các services

### API
- URL: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- Health Check: http://localhost:8080/health

### Kibana (Logs)
- URL: http://localhost:5601
- Index pattern: `vaultguard-logs-*`

### Database
```bash
# Connect qua psql
docker exec -it vaultguard-postgres psql -U vguser -d vaultguard

# Hoặc qua connection string
Host=localhost;Port=5432;Database=vaultguard;Username=vguser;Password=U1b01LwWsj67FFqRjEv0Q95K
```

### Redis
```bash
# Connect qua redis-cli
docker exec -it vaultguard-redis redis-cli

# Test connection
docker exec -it vaultguard-redis redis-cli ping
```

## Database Migrations

### Áp dụng migrations trong Docker

```bash
# Option 1: Exec vào container và chạy migrations
docker exec -it vaultguard-api dotnet ef database update

# Option 2: Tạo SQL script và chạy trực tiếp
cd scripts
.\generate-sql-script.ps1 -OutputFile "migration.sql" -Idempotent
docker exec -i vaultguard-postgres psql -U vguser -d vaultguard < migration.sql
```

### Tạo migration mới (chạy local)

```bash
cd scripts
.\add-migration.ps1 -Name "YourMigrationName"
.\update-database.ps1
```

## Health Checks

Tất cả services đều có health checks được cấu hình:

```bash
# Xem health status
docker-compose ps

# Kiểm tra health của API
curl http://localhost:8080/health

# Kiểm tra health của Elasticsearch
curl http://localhost:9200/_cluster/health
```

## Volumes

Data được persist trong Docker volumes:

- `postgres_data` - PostgreSQL data
- `redis_data` - Redis persistence
- `elasticsearch_data` - Elasticsearch indices

### Backup volumes

```bash
# Backup PostgreSQL
docker exec vaultguard-postgres pg_dump -U vguser vaultguard > backup.sql

# Restore PostgreSQL
docker exec -i vaultguard-postgres psql -U vguser vaultguard < backup.sql
```

## Environment Variables

Cấu hình trong `docker-compose.yml` có thể override bằng `.env` file:

```env
# .env file
POSTGRES_PASSWORD=your_secure_password
JWT_SECRET_KEY=your_jwt_secret
```

## Troubleshooting

### Container không start

```bash
# Xem logs chi tiết
docker-compose logs api

# Xem status
docker-compose ps
```

### Database connection failed

```bash
# Kiểm tra PostgreSQL đã ready chưa
docker exec vaultguard-postgres pg_isready -U vguser

# Kiểm tra logs
docker-compose logs postgres
```

### Out of memory

```bash
# Tăng memory cho Docker Desktop
# Settings -> Resources -> Memory: 4GB+

# Giảm memory cho Elasticsearch
# Sửa ES_JAVA_OPTS trong docker-compose.yml
```

### Port đã được sử dụng

```bash
# Thay đổi port mapping trong docker-compose.yml
# Ví dụ: "8081:8080" thay vì "8080:8080"
```

## Production Deployment

Để deploy production:

1. Tạo `docker-compose.prod.yml` với cấu hình production
2. Sử dụng secrets thay vì environment variables
3. Cấu hình reverse proxy (nginx/traefik)
4. Enable HTTPS
5. Cấu hình backup tự động

```bash
# Production deployment
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Security Notes

⚠️ **QUAN TRỌNG**: 
- Đổi tất cả passwords mặc định trong production
- Không commit `.env` file với credentials
- Sử dụng Docker secrets cho production
- Enable firewall và restrict port access
- Regular security updates cho base images

## Monitoring

Xem resource usage:

```bash
# CPU/Memory usage
docker stats

# Disk usage
docker system df

# Container details
docker inspect vaultguard-api
```
