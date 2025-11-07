#!/usr/bin/env bash
# Build script for Render deployment

set -e

echo "Starting build process..."

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build the project
echo "Building project..."
dotnet build --configuration Release --no-restore

# Publish the project
echo "Publishing project..."
dotnet publish --configuration Release --no-build --output ./publish

echo "Build completed successfully!"