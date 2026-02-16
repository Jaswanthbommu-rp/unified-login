# Health Check Implementation

This document describes the health check functionality added to the UnifiedLogin.BatchProcessor service.

## Overview

The health check system monitors the following aspects of the application:

1. **Database Connectivity** - Verifies SQL Server connection is working
2. **Redis Cache** - Checks if Redis is available (degrades gracefully if not)
3. **Background Services** - Reports which batch processing jobs are enabled
4. **Application Status** - Overall health status of the worker service

## Endpoints

The health check web server listens on port **5000** by default (configurable in appsettings.json).

### Available Endpoints

#### 1. Detailed Health Check
**URL:** `http://localhost:5000/health/`

Returns comprehensive health status of all components with detailed information:

```json
{
  "status": "Healthy",
  "timestamp": "2025-12-10T12:00:00.000Z",
  "duration": "00:00:00.1234567",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database connection is healthy",
      "duration": "00:00:00.0123456",
      "tags": ["ready", "db"],
      "data": {}
    },
    {
      "name": "redis",
      "status": "Degraded",
      "description": "Redis connection is not available. Service is running with in-memory cache only.",
      "duration": "00:00:00.0056789",
      "tags": ["ready", "cache"],
      "data": {}
    },
    {
      "name": "background-services",
      "status": "Healthy",
      "description": "10 background services are enabled and running",
      "duration": "00:00:00.0001234",
      "tags": ["ready", "services"],
      "data": {
        "enabledServicesCount": 10,
        "disabledServicesCount": 0,
        "enabledServices": "PendingBatch, RetryBatch, ...",
        "disabledServices": ""
      }
    }
  ]
}
```

**Status Codes:**
- `200 OK` - All checks are Healthy or Degraded
- `503 Service Unavailable` - One or more checks are Unhealthy

#### 2. Readiness Check
**URL:** `http://localhost:5000/health/ready`

Used by orchestrators (Kubernetes, Docker Swarm) to determine if the service is ready to accept traffic. Returns simplified status:

```json
{
  "status": "Healthy",
  "timestamp": "2025-12-10T12:00:00.000Z"
}
```

**Status Codes:**
- `200 OK` - Service is ready
- `503 Service Unavailable` - Service is not ready

#### 3. Liveness Check
**URL:** `http://localhost:5000/health/live`

Simple check to verify the application is running. Always returns 200 OK if the process is alive:

```json
{
  "status": "Healthy",
  "timestamp": "2025-12-10T12:00:00.000Z"
}
```

**Status Codes:**
- `200 OK` - Application is running

## Configuration

Configure the health check port in `appsettings.json`:

```json
{
  "HealthCheck": {
    "Port": 5000
  }
}
```

## Health Check Components

### 1. DatabaseHealthCheck
- **Location:** `HealthChecks/DatabaseHealthCheck.cs`
- **Purpose:** Verifies SQL Server connectivity by executing a simple query
- **Timeout:** 5 seconds
- **Status:**
  - `Healthy` - Database is reachable and responsive
  - `Unhealthy` - Cannot connect to database or query timeout

### 2. RedisHealthCheck
- **Location:** `HealthChecks/RedisHealthCheck.cs`
- **Purpose:** Checks Redis cache connectivity by pinging the server
- **Status:**
  - `Healthy` - Redis is connected and responsive
  - `Degraded` - Redis is not available (application continues with in-memory cache)
  - Never returns `Unhealthy` as Redis is optional

### 3. BackgroundServiceHealthCheck
- **Location:** `HealthChecks/BackgroundServiceHealthCheck.cs`
- **Purpose:** Reports which batch processing jobs are enabled/disabled
- **Status:**
  - `Healthy` - At least one service is enabled
  - `Degraded` - No services are enabled
  - `Unhealthy` - Configuration error

### 4. HealthCheckWebServer
- **Location:** `Services/HealthCheckWebServer.cs`
- **Purpose:** Hosts the HTTP listener for health check endpoints
- **Implementation:** Runs as a background service using `HttpListener`

## Usage in Orchestrators

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: unifiedlogin-batchprocessor
spec:
  template:
    spec:
      containers:
      - name: batchprocessor
        image: unifiedlogin-batchprocessor:latest
        ports:
        - containerPort: 5000
          name: health
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 10
```

### Docker Compose

```yaml
services:
  batchprocessor:
    image: unifiedlogin-batchprocessor:latest
    ports:
      - "5000:5000"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health/live"]
      interval: 30s
      timeout: 5s
      retries: 3
      start_period: 10s
```

## Testing

To test the health check endpoints locally:

```bash
# Detailed health check
curl http://localhost:5000/health/

# Readiness probe
curl http://localhost:5000/health/ready

# Liveness probe
curl http://localhost:5000/health/live
```

Or use PowerShell:

```powershell
# Detailed health check
Invoke-WebRequest -Uri http://localhost:5000/health/ | Select-Object -Expand Content

# Readiness probe
Invoke-WebRequest -Uri http://localhost:5000/health/ready | Select-Object -Expand Content

# Liveness probe
Invoke-WebRequest -Uri http://localhost:5000/health/live | Select-Object -Expand Content
```

## Monitoring and Alerting

The health check endpoints can be integrated with monitoring tools:

- **Prometheus:** Use blackbox_exporter to scrape the `/health/` endpoint
- **Nagios/Icinga:** Configure HTTP checks for `/health/ready`
- **Azure Monitor:** Create availability tests for the health endpoints
- **DataDog/New Relic:** Configure synthetic monitoring for health URLs

## Troubleshooting

### Port Already in Use

If port 5000 is already in use, change the port in `appsettings.json`:

```json
{
  "HealthCheck": {
    "Port": 5001
  }
}
```

### Health Check Returns Unhealthy

1. Check the detailed health check endpoint (`/health/`) to see which component is unhealthy
2. Review application logs for detailed error messages
3. Verify database and Redis connectivity
4. Ensure background services are properly configured

### Firewall Issues

Ensure the health check port is open in your firewall:

```powershell
# Windows Firewall
New-NetFirewallRule -DisplayName "BatchProcessor Health Check" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

## Package Dependencies

The following NuGet packages are required for health checks:

- `Microsoft.Extensions.Diagnostics.HealthChecks` - Core health check framework
- `AspNetCore.HealthChecks.SqlServer` - SQL Server health check
- `AspNetCore.HealthChecks.Redis` - Redis health check
- `Microsoft.AspNetCore.Diagnostics.HealthChecks` - Health check middleware

All packages are managed centrally in `Directory.Packages.props`.
