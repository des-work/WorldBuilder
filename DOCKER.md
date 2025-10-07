# WorldBuilder AI — Docker Guide (Windows Containers)

This repository includes Windows container support to standardize build, test, and publish workflows for the WPF application.

Important: WPF is a desktop UI and is not intended to run inside containers. Docker support here is for build/test/publish only.

- Base Image: `mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022`
- Files:
  - `Dockerfile.windows` — Build/Test image definition
  - `docker-compose.windows.yml` — Compose services for tests and publish
  - `scripts/docker-test.ps1` — Builds image and runs tests, output in `./artifacts`
  - `scripts/docker-publish.ps1` — Builds image and publishes app to `./artifacts/publish`
  - `.dockerignore` — Trims build context (bin/obj/.git/etc.)

## Prerequisites

- Docker Desktop for Windows, switched to Windows Containers
- Windows 10/11 or Windows Server that supports the selected base image
- Adequate disk space for Windows base layers

## Quick Start

- Run tests in container
  - PowerShell: `scripts/docker-test.ps1`
  - Test results (TRX) appear in `./artifacts`

- Publish the app in container
  - PowerShell: `scripts/docker-publish.ps1`
  - Published output appears in `./artifacts/publish`

## Compose Services

- `tests` (default command from Dockerfile)
  - Restores, builds, and runs tests; writes TRX to `C:\out` mapped to `./artifacts`
- `publish` (overrides command)
  - Restores, builds, and publishes WPF for `win-x64` (`self-contained`), output to `C:\out\publish`

## Notes

- Running the WPF UI inside a container is not supported; use containers for CI-like workflows.
- The test project references the WPF project; using a Windows container ensures the WPF build workload is available.
- If you prefer Linux-based CI for non-UI code, consider adding a separate test project that does not reference WPF and a Linux Dockerfile to run only Core/Application/Infrastructure tests.

## Troubleshooting

- Switch Docker Desktop to Windows Containers if builds fail with platform/OS errors.
- Ensure the base image tag matches your host OS (e.g., LTSC version compatibility).
- Clean up old images and containers if disk space runs low: `docker system prune -a` (use with care).

