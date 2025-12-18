# Script để xóa migration cuối cùng (chưa apply)
# Sử dụng: .\remove-migration.ps1

param(
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "..\src\libs\VaultGuard.Persistence",
    
    [Parameter(Mandatory=$false)]
    [string]$StartupProjectPath = "..\src\presentations\VaultGuard.Api",
    
    [Parameter(Mandatory=$false)]
    [string]$Context = "WriteDbContext",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# Resolve absolute paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$resolvedProjectPath = Resolve-Path (Join-Path $scriptDir $ProjectPath) -ErrorAction Stop
$resolvedStartupPath = Resolve-Path (Join-Path $scriptDir $StartupProjectPath) -ErrorAction Stop

Write-Host "=== Xóa Migration Cuối Cùng ===" -ForegroundColor Cyan
Write-Host "Project: $resolvedProjectPath" -ForegroundColor Yellow
Write-Host "Startup Project: $resolvedStartupPath" -ForegroundColor Yellow
Write-Host "DbContext: $Context" -ForegroundColor Yellow
Write-Host ""

if (-not $Force) {
    $confirmation = Read-Host "Bạn có chắc muốn xóa migration cuối cùng? (y/N)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Host "Đã hủy thao tác" -ForegroundColor Yellow
        exit 0
    }
}

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

# Thực hiện xóa migration
Write-Host "Đang xóa migration..." -ForegroundColor Green
dotnet ef migrations remove `
    --project "$resolvedProjectPath" `
    --startup-project "$resolvedStartupPath" `
    --context $Context `
    --force `
    --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[OK] Migration đã được xóa thành công!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[ERR] Lỗi khi xóa migration" -ForegroundColor Red
    Write-Host ""
    Write-Host "Lưu ý: Không thể xóa migration đã được apply vào database" -ForegroundColor Yellow
    Write-Host "Nếu migration đã được apply, hãy sử dụng: .\update-database.ps1 -TargetMigration <PreviousMigrationName>" -ForegroundColor Yellow
    exit 1
}
