# UnifiedLogin.UserNotification

A modern .NET 8 Worker Service for processing user notifications once per day at a configurable time using non-blocking asynchronous patterns.

## Overview

This service demonstrates modern .NET 8 best practices for scheduled background job processing:

- ✅ **BackgroundService** - Built-in hosted service framework
- ✅ **Daily Scheduling** - Timezone-aware daily execution at specific time
- ✅ **Channels** - Thread-safe work queues with backpressure
- ✅ **Async/Await** - Throughout the entire pipeline
- ✅ **Dependency Injection** - Full DI with scoped services
- ✅ **Structured Logging** - Serilog with correlation context
- ✅ **OpenTelemetry** - Distributed tracing support
- ✅ **Health Checks** - Built-in health monitoring
- ✅ **Docker Ready** - Multi-stage Dockerfile for containerization

## Architecture

```
UserNotificationWorker (BackgroundService)
├── ScheduleDailyJobsAsync() [Daily Scheduler]
│   └── Calculate next run time
│       └── Task.Delay until scheduled time
│           └── ProcessJobsAsync() [Main job method]
│               └── Enqueue notifications to Channel
└── ProcessNotificationsAsync() x N workers
    └── Process from Channel
        └── ProcessSingleNotificationAsync()
            └── SendNotificationAsync()
```

### Key Components

1. **Daily Scheduler**: Timezone-aware daily execution at specific time
2. **Bounded Channel**: Thread-safe queue with backpressure for notification jobs
3. **Worker Pool**: Configurable number of concurrent workers processing notifications
4. **Timeout Protection**: Per-notification timeout to prevent hung operations
5. **Graceful Shutdown**: Completes in-flight work before stopping
6. **Timezone Support**: Respects configured timezone for daily executions

## Configuration

### appsettings.json

```json
{
  "UserNotification": {
    "WorkerThreads": 3,               // Number of concurrent workers
    "BatchSize": 50,                  // Notifications per batch
    "MaxRetryAttempts": 3,            // Retry attempts for failures
    "ProcessingTimeoutSeconds": 30,   // Timeout per notification
    "ConnectionString": "Server=...", // Database connection
    "NotificationApiBaseUrl": "...",  // API endpoint
    "EnableEmailNotifications": true,
    "EnableSmsNotifications": true,
    "EnablePushNotifications": true,
    "DailyExecutionTime": "02:00:00", // Time to run daily (HH:mm:ss format)
    "DailyExecutionTimeZone": "UTC"   // Timezone for execution
  }
}
```

### Daily Scheduling

The service runs **once per day** at the configured time:
- Executes once per day at `DailyExecutionTime`
- Respects the configured `DailyExecutionTimeZone`
- Best for batch processing or daily reports
- Example: Process notifications at 2:30 AM EST every day

**Configuration Example:**
```json
{
  "UserNotification": {
    "DailyExecutionTime": "02:30:00",
    "DailyExecutionTimeZone": "Eastern Standard Time"
  }
}
```

**Supported Timezone Formats:**
- "UTC" - Coordinated Universal Time
- "Eastern Standard Time" - US Eastern Time
- "Pacific Standard Time" - US Pacific Time
- "Central Standard Time" - US Central Time
- Any valid TimeZoneInfo ID for your system

**Note:** If an invalid timezone is specified, the service will fall back to UTC with a warning.

### Environment-Specific Settings

- **Development** (`appsettings.Development.json`): 2:30 AM EST, 2 workers, smaller batches
- **Production** (`appsettings.Production.json`): 2:00 AM UTC, 5 workers, larger batches

## Running the Service

### Local Development

```bash
cd C:\01UnityRepos\unified-login-main\CoreMigration\Services\UnifiedLogin.UserNotification

# Run with Development settings
dotnet run --environment Development
```

**Expected Output:**
```
[10:30:15 INF] Starting UnifiedLogin.UserNotification
[10:30:15 INF] Configuration loaded: ExecutionTime=02:30:00 (Eastern Standard Time), Workers=2, BatchSize=10
[10:30:15 INF] User Notification Worker starting. Execution time: 02:30:00 (Eastern Standard Time), Workers: 2, BatchSize: 10
[10:30:15 DBG] Worker 0 started
[10:30:15 DBG] Worker 1 started
[10:30:15 INF] Daily scheduler started. Will execute at 02:30:00 (UTC-05:00) Eastern Time every day
[10:30:15 INF] Next execution scheduled for 2024-01-16 07:30:00 UTC (in 21:00:00)
```

### Build

```bash
dotnet build
```

### Publish

```bash
dotnet publish -c Release -o ./publish
```

## Docker Deployment

### Build Image

```bash
docker build -t unifiedlogin-usernotification:latest .
```

### Run Container

```bash
docker run -d --name notification-service \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e UserNotification__DailyExecutionTime="02:00:00" \
  -e UserNotification__DailyExecutionTimeZone="UTC" \
  -e UserNotification__WorkerThreads=5 \
  -e UserNotification__ConnectionString="Server=..." \
  -v ./logs:/app/logs \
  unifiedlogin-usernotification:latest
```

### Docker Compose

```bash
docker-compose up -d
```

### View Logs

```bash
docker logs -f notification-service
```

## ProcessJobsAsync() Method

The `ProcessJobsAsync()` method is the main job execution entry point:

```csharp
private async Task ProcessJobsAsync(CancellationToken stoppingToken)
{
    // 1. Log job start
    _logger.LogInformation("Starting job execution at {StartTime}", DateTime.UtcNow);

    // 2. Fetch pending notifications
    var pendingNotifications = await FetchPendingNotificationsAsync(stoppingToken);

    // 3. Enqueue for worker processing
    foreach (var notification in pendingNotifications)
    {
        await _notificationQueue.Writer.WriteAsync(notification, stoppingToken);
    }

    // 4. Log completion
    _logger.LogInformation("Job execution completed. Enqueued {Count} notifications", count);
}
```

**Key Features:**
- ✅ Non-blocking async execution
- ✅ Proper cancellation token support
- ✅ Structured logging with context
- ✅ Exception handling with recovery
- ✅ Performance metrics (duration tracking)

## Worker Pattern

The service uses a producer-consumer pattern:

1. **Producer** (PeriodicTimer): Fetches notifications and enqueues them
2. **Channel** (Bounded): Thread-safe queue with backpressure
3. **Consumers** (Worker pool): Process notifications concurrently

This pattern provides:
- ✅ Decoupling of fetching and processing
- ✅ Backpressure control (prevents memory issues)
- ✅ Concurrent processing with configurable parallelism
- ✅ Graceful degradation under load

## Monitoring

### Logs

Logs are written to:
- Console (stdout) - Real-time monitoring
- File: `logs/user-notification-<date>.log` - Persistent storage

### Health Checks

Health check endpoint (when hosted in ASP.NET Core):
```bash
GET /health
```

Response:
```json
{
  "status": "Healthy",
  "results": {
    "notification_service": {
      "status": "Healthy",
      "data": {
        "IntervalSeconds": 60,
        "WorkerThreads": 3,
        "BatchSize": 50,
        "EmailEnabled": true,
        "SmsEnabled": true,
        "PushEnabled": true
      }
    }
  }
}
```

### Metrics to Monitor

- Daily job execution (should run once per day at scheduled time)
- Notifications processed per execution
- Worker utilization (active vs idle)
- Queue depth (notifications waiting)
- Processing duration per notification
- Error rate and retry count

## Performance Characteristics

### Non-Blocking Daily Scheduling

**Traditional Approach (Blocking):**
```csharp
// ❌ Blocks thread while waiting
Thread.Sleep(delayUntilNextRun);
ProcessJobs(); // Synchronous
```

**Modern Approach (Non-Blocking):**
```csharp
// ✅ Yields CPU while waiting
var delay = CalculateDelayUntilNextExecution(executionTime, timeZone);
await Task.Delay(delay, stoppingToken);
await ProcessJobsAsync(stoppingToken); // Asynchronous
```

**Benefits:**
- Non-blocking wait until scheduled time
- Proper timezone handling with DST support
- Better CPU utilization
- Lower memory footprint
- Graceful cancellation support

### Resource Usage

| Configuration | Threads | Memory | CPU |
|--------------|---------|--------|-----|
| Development (2 workers) | ~5 | ~40MB | <5% |
| Production (5 workers) | ~10 | ~80MB | <10% |

## Notification Types

The service supports multiple notification types:

1. **Email** - SMTP-based email delivery
2. **SMS** - SMS gateway integration
3. **Push** - Mobile push notifications
4. **InApp** - In-application notifications

Each type can be enabled/disabled via configuration.

## Error Handling

### Retry Logic

Failed notifications are automatically retried based on `MaxRetryAttempts`:

1. First attempt fails → Retry 1
2. Retry 1 fails → Retry 2
3. Retry 2 fails → Retry 3
4. All retries exhausted → Mark as Failed

### Timeout Protection

Each notification processing has a timeout:
```csharp
using var timeout = new CancellationTokenSource(
    TimeSpan.FromSeconds(_options.Value.ProcessingTimeoutSeconds));
```

This prevents hung operations from blocking workers.

### Graceful Shutdown

On shutdown signal:
1. Stop accepting new jobs (complete PeriodicTimer)
2. Complete channel (no more enqueuing)
3. Wait for workers to finish in-flight work
4. Log completion and exit

## Development

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Docker (optional, for containerization)

### Project Structure

```
UnifiedLogin.UserNotification/
├── Configuration/
│   └── UserNotificationOptions.cs
├── Models/
│   └── NotificationJob.cs
├── Services/
│   ├── UserNotificationWorker.cs
│   └── NotificationHealthCheck.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
├── Dockerfile
└── docker-compose.yml
```

### Adding Database Integration

To integrate with a real database:

1. Create repository interface:
```csharp
public interface INotificationRepository
{
    Task<List<NotificationJob>> GetPendingNotificationsAsync(int batchSize, CancellationToken cancellationToken);
    Task UpdateNotificationStatusAsync(long notificationId, NotificationStatus status, CancellationToken cancellationToken);
}
```

2. Implement with Dapper:
```csharp
public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public async Task<List<NotificationJob>> GetPendingNotificationsAsync(...)
    {
        await using var connection = new SqlConnection(_connectionString);
        // Use Dapper to query database
    }
}
```

3. Register in Program.cs:
```csharp
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
```

## Troubleshooting

### Service not starting
- Check configuration values (especially connection strings)
- Verify all required settings are present
- Check logs in `logs/` directory

### No jobs executing
- Verify `DailyExecutionTime` is set correctly
- Check if daily scheduler is running (log entries with next execution time)
- Wait for scheduled time to arrive
- Ensure database has pending notifications

### High memory usage
- Reduce `BatchSize` to process fewer items per cycle
- Reduce `WorkerThreads` to lower concurrency
- Check for memory leaks in notification processing

### Workers idle
- Increase `BatchSize` to fetch more items
- Check if channel queue is empty (no pending notifications)
- Verify database connection is working

## Best Practices

1. **Configuration**: Always use environment-specific settings
2. **Logging**: Include correlation IDs for tracking
3. **Monitoring**: Set up alerts on error rates and queue depth
4. **Scaling**: Increase `WorkerThreads` for higher throughput
5. **Testing**: Test graceful shutdown behavior
6. **Security**: Never log sensitive notification content
7. **Performance**: Monitor processing duration and optimize slow operations

## License

Copyright © RealPage Inc. All rights reserved.
