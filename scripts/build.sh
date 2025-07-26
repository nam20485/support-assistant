#!/bin/bash

# Build script for SupportAssistant
# This script builds the entire solution and runs tests

set -e

echo "Building SupportAssistant..."

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean

# Restore dependencies
echo "Restoring NuGet packages..."
dotnet restore

# Build the solution
echo "Building solution..."
dotnet build --configuration Release --no-restore

# Run tests
echo "Running tests..."
dotnet test --configuration Release --no-build --verbosity normal

# Build desktop application specifically
echo "Building desktop application..."
dotnet build src/SupportAssistant.Desktop --configuration Release --no-restore

echo "Build completed successfully!"
echo ""
echo "To run the application:"
echo "  dotnet run --project src/SupportAssistant.Desktop --configuration Release"