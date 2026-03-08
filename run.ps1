param([switch]$NoBuild)

$ErrorActionPreference = "Stop"
$project = "$PSScriptRoot\Splitza.Api\Splitza.Api.csproj"

if (-not $NoBuild) {
    Write-Host "Building to bin-run..." -ForegroundColor Cyan
    dotnet build $project /p:RunOutput=true
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "Starting Splitza API..." -ForegroundColor Green
dotnet "$PSScriptRoot\Splitza.Api\bin-run\net10.0\Splitza.Api.dll"
