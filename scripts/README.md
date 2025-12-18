# Database Migration Scripts

Scripts PowerShell để quản lý EF Core migrations cho VaultGuard project.

## Yêu cầu

- .NET SDK 10.0 trở lên
- PostgreSQL server đang chạy
- Connection string đã được cấu hình trong `appsettings.json`

## Scripts có sẵn

### 1. add-migration.ps1
Tạo migration mới từ các thay đổi trong Entity models.

**Sử dụng:**
```powershell
# Tạo migration mới
.\add-migration.ps1 -Name "InitialCreate"

# Tạo migration với custom project paths
.\add-migration.ps1 -Name "AddUserTable" -ProjectPath "..\src\libs\VaultGuard.Persistence" -StartupProjectPath "..\src\presentations\VaultGuard.Api"
```

**Tham số:**
- `-Name` (bắt buộc): Tên của migration
- `-ProjectPath` (tùy chọn): Đường dẫn đến project chứa DbContext
- `-StartupProjectPath` (tùy chọn): Đường dẫn đến startup project
- `-Context` (tùy chọn): Tên của DbContext (mặc định: WriteDbContext)

### 2. update-database.ps1
Áp dụng migrations vào database.

**Sử dụng:**
```powershell
# Cập nhật database lên migration mới nhất
.\update-database.ps1

# Cập nhật database lên một migration cụ thể
.\update-database.ps1 -TargetMigration "InitialCreate"

# Rollback về migration trước đó
.\update-database.ps1 -TargetMigration "PreviousMigration"

# Sử dụng connection string tùy chỉnh
.\update-database.ps1 -ConnectionString "Host=localhost;Port=5432;Database=vaultguard;Username=postgres;Password=postgres;"
```

**Tham số:**
- `-TargetMigration` (tùy chọn): Migration cụ thể cần áp dụng
- `-ProjectPath` (tùy chọn): Đường dẫn đến project chứa DbContext
- `-StartupProjectPath` (tùy chọn): Đường dẫn đến startup project
- `-Context` (tùy chọn): Tên của DbContext (mặc định: WriteDbContext)
- `-ConnectionString` (tùy chọn): Connection string tùy chỉnh

### 3. remove-migration.ps1
Xóa migration cuối cùng (chưa được apply vào database).

**Sử dụng:**
```powershell
# Xóa migration cuối cùng (có prompt xác nhận)
.\remove-migration.ps1

# Xóa migration cuối cùng (không cần xác nhận)
.\remove-migration.ps1 -Force
```

**Tham số:**
- `-Force` (tùy chọn): Bỏ qua prompt xác nhận
- `-ProjectPath` (tùy chọn): Đường dẫn đến project chứa DbContext
- `-StartupProjectPath` (tùy chọn): Đường dẫn đến startup project
- `-Context` (tùy chọn): Tên của DbContext (mặc định: WriteDbContext)

**Lưu ý:** Không thể xóa migration đã được apply vào database. Nếu muốn rollback, sử dụng `update-database.ps1` với `-TargetMigration`.

### 4. list-migrations.ps1
Liệt kê tất cả migrations trong project.

**Sử dụng:**
```powershell
# Liệt kê tất cả migrations
.\list-migrations.ps1
```

**Tham số:**
- `-ProjectPath` (tùy chọn): Đường dẫn đến project chứa DbContext
- `-StartupProjectPath` (tùy chọn): Đường dẫn đến startup project
- `-Context` (tùy chọn): Tên của DbContext (mặc định: WriteDbContext)

### 5. generate-sql-script.ps1
Tạo SQL script từ migrations để chạy trực tiếp trên database.

**Sử dụng:**
```powershell
# Tạo SQL script cho tất cả migrations
.\generate-sql-script.ps1 -OutputFile "migration.sql"

# Tạo SQL script từ migration cụ thể
.\generate-sql-script.ps1 -From "Migration1" -To "Migration2" -OutputFile "migration.sql"

# Tạo SQL script có thể chạy nhiều lần (idempotent)
.\generate-sql-script.ps1 -OutputFile "migration.sql" -Idempotent
```

**Tham số:**
- `-OutputFile` (bắt buộc): Tên file SQL output
- `-From` (tùy chọn): Migration bắt đầu
- `-To` (tùy chọn): Migration kết thúc
- `-Idempotent` (tùy chọn): Tạo script có thể chạy nhiều lần
- `-ProjectPath` (tùy chọn): Đường dẫn đến project chứa DbContext
- `-StartupProjectPath` (tùy chọn): Đường dẫn đến startup project
- `-Context` (tùy chọn): Tên của DbContext (mặc định: WriteDbContext)

## Workflow thông thường

### Tạo và áp dụng migration mới

1. Thay đổi Entity models trong Domain layer
2. Tạo migration:
   ```powershell
   .\add-migration.ps1 -Name "DescribeYourChanges"
   ```
3. Kiểm tra migration được tạo trong thư mục `Migrations`
4. Áp dụng migration vào database:
   ```powershell
   .\update-database.ps1
   ```

### Rollback migration

1. Liệt kê các migrations:
   ```powershell
   .\list-migrations.ps1
   ```
2. Rollback về migration trước đó:
   ```powershell
   .\update-database.ps1 -TargetMigration "PreviousMigrationName"
   ```

### Deploy sang môi trường production

1. Tạo SQL script:
   ```powershell
   .\generate-sql-script.ps1 -OutputFile "production-migration.sql" -Idempotent
   ```
2. Review SQL script
3. Chạy SQL script trực tiếp trên production database

## Troubleshooting

### Lỗi "No DbContext was found"
- Kiểm tra đường dẫn project có đúng không
- Đảm bảo DbContext được config trong DependencyInjection.cs

### Lỗi "Connection failed"
- Kiểm tra PostgreSQL server có đang chạy không
- Kiểm tra connection string trong appsettings.json
- Kiểm tra firewall và network connectivity

### Lỗi "Cannot remove migration"
- Migration đã được apply vào database không thể xóa
- Sử dụng `update-database.ps1` để rollback thay vì `remove-migration.ps1`

### Tool 'dotnet-ef' không tìm thấy
Scripts sẽ tự động cài đặt `dotnet-ef` tool nếu chưa có. Nếu vẫn gặp lỗi, cài đặt thủ công:
```powershell
dotnet tool install --global dotnet-ef
```

## Lưu ý quan trọng

- **Backup database** trước khi chạy migrations trong production
- **Test migrations** trong môi trường development/staging trước
- Luôn **review SQL scripts** trước khi áp dụng vào production
- Migrations nên có tên mô tả rõ ràng về những thay đổi
- Không chỉnh sửa migrations đã được apply vào production
- Sử dụng `-Idempotent` flag khi tạo SQL scripts cho production
