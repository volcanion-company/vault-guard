# Script để liệt kê tất cả migrations
# Sử dụng: .\list-migrations.ps1

param(
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

Write-Host "=== Danh Sách Migrations ===" -ForegroundColor Cyan
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

# Liệt kê migrations
Write-Host "Đang lấy danh sách migrations..." -ForegroundColor Green
Write-Host ""
dotnet ef migrations list `
    --project "$resolvedProjectPath" `
    --startup-project "$resolvedStartupPath" `
    --context $Context

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[OK] Đã lấy danh sách migrations" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[ERR] Lỗi khi lấy danh sách migrations" -ForegroundColor Red
    exit 1
}
