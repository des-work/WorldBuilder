#!/bin/bash

# WorldBuilder AI Build and Deploy Script
set -e

echo "🚀 Starting WorldBuilder AI build and deployment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if .NET CLI is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET CLI is not installed. Please install .NET 8 SDK."
    exit 1
fi

# Clean previous builds
print_status "Cleaning previous builds..."
dotnet clean

# Restore packages
print_status "Restoring NuGet packages..."
dotnet restore

# Build all projects
print_status "Building all projects..."
dotnet build --configuration Release --no-restore

# Run tests
print_status "Running tests..."
dotnet test --configuration Release --no-build --verbosity normal

# Publish the main application
print_status "Publishing WorldBuilder AI..."
dotnet publish Genisis.Presentation.Wpf/Genisis.Presentation.Wpf.csproj \
    --configuration Release \
    --framework net8.0-windows \
    --runtime win-x64 \
    --self-contained true \
    --output ./publish/WorldBuilderAI \
    --no-build

# Create deployment package
print_status "Creating deployment package..."
cd publish
tar -czf WorldBuilderAI-linux.tar.gz WorldBuilderAI/ 2>/dev/null || zip -r WorldBuilderAI-windows.zip WorldBuilderAI/
cd ..

# Generate deployment documentation
print_status "Generating deployment documentation..."
cat > DEPLOYMENT.md << 'EOF'
# WorldBuilder AI Deployment Guide

## Prerequisites
- .NET 8 Runtime (for non-self-contained deployment)
- Windows 10/11 (primary target platform)

## Installation

### Self-Contained Deployment (Recommended)
1. Extract the deployment package to your desired location
2. Run `WorldBuilderAI/Genisis.Presentation.Wpf.exe`

### Framework-Dependent Deployment
1. Install .NET 8 Runtime
2. Extract and run the application

## Configuration
The application stores data in:
- Windows: `%APPDATA%\WorldBuilderAI\worldbuilder.db`
- Linux/macOS: `~/.config/WorldBuilderAI/worldbuilder.db`

## Troubleshooting
- Ensure all prerequisites are installed
- Check application logs in the data directory
- Verify file permissions for the data directory

## Support
For issues and questions, please check the application logs or contact support.
EOF

print_success "Build and deployment completed successfully!"
print_success "Deployment package created in: ./publish/"
print_success "Deployment documentation: DEPLOYMENT.md"

echo ""
echo "📦 Deployment Summary:"
echo "  - Application: WorldBuilderAI"
echo "  - Platform: Windows (win-x64)"
echo "  - Type: Self-contained"
echo "  - Size: $(du -sh ./publish/WorldBuilderAI/ | cut -f1)"
echo ""
echo "🎯 Next Steps:"
echo "  1. Test the deployment package"
echo "  2. Distribute to users"
echo "  3. Monitor application logs"
echo "  4. Update as needed"
