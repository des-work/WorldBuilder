param(
    [string]$ComposeFile = "docker-compose.windows.yml"
)

$ErrorActionPreference = 'Stop'

Write-Host "[WorldBuilder] Running tests in Windows container..."

if (-not (Test-Path ./artifacts)) {
    New-Item -ItemType Directory -Path ./artifacts | Out-Null
}

docker compose -f $ComposeFile build tests
docker compose -f $ComposeFile run --rm tests

Write-Host "[WorldBuilder] Test results available in ./artifacts" -ForegroundColor Green

