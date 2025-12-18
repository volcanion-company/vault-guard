# Script để tạo EF Core migration mới
# Sử dụng: .\add-migration.ps1 -Name "InitialCreate"

param(
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "..\src\libs\VaultGuard.Persistence",
    
    [Parameter(Mandatory=$false)]
    [string]$StartupProjectPath = "..\src\presentations\VaultGuard.Api",
    
    [Parameter(Mandatory=$false)]
    [string]$Context = "WriteDbContext"
)

# Resolve absolute paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$resolvedProjectPath = Resolve-Path (Join-Path $scriptDir $ProjectPath) -ErrorAction Stop
$resolvedStartupPath = Resolve-Path (Join-Path $scriptDir $StartupProjectPath) -ErrorAction Stop

Write-Host "=== Tạo Migration Mới ===" -ForegroundColor Cyan
Write-Host "Migration Name: $Name" -ForegroundColor Yellow
Write-Host "Project: $resolvedProjectPath" -ForegroundColor Yellow
Write-Host "Startup Project: $resolvedStartupPath" -ForegroundColor Yellow
Write-Host "DbContext: $Context" -ForegroundColor Yellow
Write-Host ""

# Kiểm tra xem dotnet ef có được cài đặt chưa
$efTool = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efTool) {
    Write-Host "dotnet-ef tool chưa được cài đặt. Đang cài đặt..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Lỗi khi cài đặt dotnet-ef tool" -ForegroundColor Red
        exit 1
    }
}

# Thực hiện tạo migration
Write-Host "Đang tạo migration..." -ForegroundColor Green
dotnet ef migrations add $Name `
    --project "$resolvedProjectPath" `
    --startup-project "$resolvedStartupPath" `
    --context $Context `
    --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[OK] Migration '$Name' đã được tạo thành công!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Các bước tiếp theo:" -ForegroundColor Cyan
    Write-Host "  1. Kiểm tra migration vừa tạo trong thư mục Migrations" -ForegroundColor White
    Write-Host "  2. Chạy .\update-database.ps1 để áp dụng migration vào database" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "[ERR] Lỗi khi tạo migration" -ForegroundColor Red
    exit 1
}
