#!/usr/bin/env bash
# Start script for Render deployment

set -e

echo "Starting application..."

# Run Entity Framework migrations
echo "Running database migrations..."
dotnet ef database update || echo "Migration failed or already up to date"

# Create upload directories
mkdir -p wwwroot/uploads/avatars
mkdir -p wwwroot/uploads/documents
mkdir -p wwwroot/uploads/videos

# Start the application
echo "Starting API server..."
cd publish
exec dotnet IconIK.API.dll