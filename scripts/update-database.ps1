# Script để cập nhật database với migrations
# Sử dụng: .\update-database.ps1 
# Hoặc: .\update-database.ps1 -TargetMigration "MigrationName"

param(
    [Parameter(Mandatory=$false)]
    [string]$TargetMigration,
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "..\src\libs\VaultGuard.Persistence",
    
    [Parameter(Mandatory=$false)]
    [string]$StartupProjectPath = "..\src\presentations\VaultGuard.Api",
    
    [Parameter(Mandatory=$false)]
    [string]$Context = "WriteDbContext",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString
)

# Resolve absolute paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$resolvedProjectPath = Resolve-Path (Join-Path $scriptDir $ProjectPath) -ErrorAction Stop
$resolvedStartupPath = Resolve-Path (Join-Path $scriptDir $StartupProjectPath) -ErrorAction Stop

Write-Host "=== Cập Nhật Database ===" -ForegroundColor Cyan
Write-Host "Project: $resolvedProjectPath" -ForegroundColor Yellow
Write-Host "Startup Project: $resolvedStartupPath" -ForegroundColor Yellow
Write-Host "DbContext: $Context" -ForegroundColor Yellow

if ($TargetMigration) {
    Write-Host "Target Migration: $TargetMigration" -ForegroundColor Yellow
} else {
    Write-Host "Target Migration: Latest" -ForegroundColor Yellow
}
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

# Build command
$command = "dotnet ef database update"

if ($TargetMigration) {
    $command += " $TargetMigration"
}

$command += " --project `"$resolvedProjectPath`" --startup-project `"$resolvedStartupPath`" --context $Context --verbose"

if ($ConnectionString) {
    $command += " --connection `"$ConnectionString`""
}

# Thực hiện cập nhật database
Write-Host "Đang cập nhật database..." -ForegroundColor Green
Write-Host "Command: $command" -ForegroundColor Gray
Write-Host ""

Invoke-Expression $command

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[OK] Database đã được cập nhật thành công!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[ERR] Lỗi khi cập nhật database" -ForegroundColor Red
    Write-Host ""
    Write-Host "Kiểm tra:" -ForegroundColor Yellow
    Write-Host "  1. Connection string trong appsettings.json có đúng không" -ForegroundColor White
    Write-Host "  2. PostgreSQL server có đang chạy không" -ForegroundColor White
    Write-Host "  3. User có quyền truy cập database không" -ForegroundColor White
    exit 1
}
