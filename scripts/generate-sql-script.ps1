# Script để tạo SQL script từ migrations
# Sử dụng: .\generate-sql-script.ps1 -OutputFile "migration.sql"
# Hoặc: .\generate-sql-script.ps1 -From "Migration1" -To "Migration2" -OutputFile "migration.sql"

param(
    [Parameter(Mandatory=$true)]
    [string]$OutputFile,
    
    [Parameter(Mandatory=$false)]
    [string]$From,
    
    [Parameter(Mandatory=$false)]
    [string]$To,
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "..\src\libs\VaultGuard.Persistence",
    
    [Parameter(Mandatory=$false)]
    [string]$StartupProjectPath = "..\src\presentations\VaultGuard.Api",
    
    [Parameter(Mandatory=$false)]
    [string]$Context = "WriteDbContext",
    
    [Parameter(Mandatory=$false)]
    [switch]$Idempotent
)

# Resolve absolute paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$resolvedProjectPath = Resolve-Path (Join-Path $scriptDir $ProjectPath) -ErrorAction Stop
$resolvedStartupPath = Resolve-Path (Join-Path $scriptDir $StartupProjectPath) -ErrorAction Stop

Write-Host "=== Tạo SQL Script từ Migrations ===" -ForegroundColor Cyan
Write-Host "Project: $resolvedProjectPath" -ForegroundColor Yellow
Write-Host "Startup Project: $resolvedStartupPath" -ForegroundColor Yellow
Write-Host "DbContext: $Context" -ForegroundColor Yellow
Write-Host "Output File: $OutputFile" -ForegroundColor Yellow

if ($From) {
    Write-Host "From Migration: $From" -ForegroundColor Yellow
}
if ($To) {
    Write-Host "To Migration: $To" -ForegroundColor Yellow
}
if ($Idempotent) {
    Write-Host "Idempotent: Yes (có thể chạy nhiều lần)" -ForegroundColor Yellow
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
$command = "dotnet ef migrations script"

if ($From) {
    $command += " $From"
}

if ($To) {
    $command += " $To"
}

$command += " --project `"$resolvedProjectPath`" --startup-project `"$resolvedStartupPath`" --context $Context --output `"$OutputFile`""

if ($Idempotent) {
    $command += " --idempotent"
}

# Thực hiện tạo SQL script
Write-Host "Đang tạo SQL script..." -ForegroundColor Green
Write-Host "Command: $command" -ForegroundColor Gray
Write-Host ""

Invoke-Expression $command

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[OK] SQL script đã được tạo thành công!" -ForegroundColor Green
    Write-Host "File: $OutputFile" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[ERR] Lỗi khi tạo SQL script" -ForegroundColor Red
    exit 1
}
