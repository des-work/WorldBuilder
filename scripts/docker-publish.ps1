param(
    [string]$ComposeFile = "docker-compose.windows.yml"
)

$ErrorActionPreference = 'Stop'

Write-Host "[WorldBuilder] Publishing app in Windows container..."

if (-not (Test-Path ./artifacts)) {
    New-Item -ItemType Directory -Path ./artifacts | Out-Null
}

docker compose -f $ComposeFile build publish
docker compose -f $ComposeFile run --rm publish

Write-Host "[WorldBuilder] Publish output available in ./artifacts/publish" -ForegroundColor Green

