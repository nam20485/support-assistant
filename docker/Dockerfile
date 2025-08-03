# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY *.sln ./
COPY global.json ./
COPY src/*/*.csproj ./src/
COPY tests/*/*.csproj ./tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY src/ ./src/
COPY tests/ ./tests/

# Build the solution
RUN dotnet build --configuration Release --no-restore

# Run tests
RUN dotnet test --configuration Release --no-build

# Runtime stage (for future use when the app is ready)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install required dependencies for Avalonia (when needed)
# RUN apt-get update && apt-get install -y \
#     libx11-6 \
#     libxext6 \
#     libxrender1 \
#     libxtst6 \
#     libxi6 \
#     libxrandr2 \
#     libasound2

# Copy build artifacts
COPY --from=build /src/src/SupportAssistant.Desktop/bin/Release/net8.0/ ./

# Note: This Dockerfile is for build verification only
# Avalonia desktop apps typically run natively on the host OS
# For actual deployment, use the build artifacts directly