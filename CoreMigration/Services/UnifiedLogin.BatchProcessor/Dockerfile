# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["UnifiedLogin.BatchProcessor.csproj", "./"]
RUN dotnet restore "UnifiedLogin.BatchProcessor.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "UnifiedLogin.BatchProcessor.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UnifiedLogin.BatchProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r batchprocessor && useradd -r -g batchprocessor batchprocessor

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && chown -R batchprocessor:batchprocessor /app

# Switch to non-root user
USER batchprocessor

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD dotnet --version || exit 1

ENTRYPOINT ["dotnet", "UnifiedLogin.BatchProcessor.dll"]
