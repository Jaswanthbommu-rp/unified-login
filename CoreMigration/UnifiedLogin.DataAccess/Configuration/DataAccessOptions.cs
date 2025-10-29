using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.DataAccess.Configuration;

/// <summary>
/// Configuration options for the DataAccess layer.
/// Contains all settings required for database connectivity, performance monitoring, and operational behavior.
/// These options are typically bound from the "DataAccess" section of appsettings.json.
/// </summary>
public sealed class DataAccessOptions
{
    #region Constants

    /// <summary>
    /// The configuration section name used to bind these options from appsettings.json.
    /// </summary>
    public const string SectionName = "DataAccess";

    #endregion

    #region Connection Configuration

    /// <summary>
    /// Gets or sets the primary database connection string.
    /// This connection string will be used for all database operations unless overridden.
    /// </summary>
    /// <value>A valid SQL Server connection string</value>
    /// <example>Server=localhost;Database=UnifiedLogin;Integrated Security=true;TrustServerCertificate=true</example>
    [Required(ErrorMessage = "ConnectionString is required for database operations")]
    public required string ConnectionString { get; set; }

    #endregion

    #region Command Configuration

    /// <summary>
    /// Gets or sets the default command timeout in seconds for database operations.
    /// This timeout applies to individual SQL commands when no specific timeout is provided.
    /// </summary>
    /// <value>Timeout duration in seconds (1-3600). Default is 30 seconds.</value>
    [Range(1, 3600, ErrorMessage = "DefaultCommandTimeoutSeconds must be between 1 and 3600 seconds")]
    public int DefaultCommandTimeoutSeconds { get; init; } = 30;

    #endregion

    #region Retry Configuration

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient database failures.
    /// Set to 0 to disable retry logic.
    /// </summary>
    /// <value>Maximum retry attempts (0-10). Default is 3.</value>
    [Range(0, 10, ErrorMessage = "MaxRetryAttempts must be between 0 and 10")]
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets or sets the base delay in milliseconds between retry attempts.
    /// Actual delay may be increased using exponential backoff strategy.
    /// </summary>
    /// <value>Base delay in milliseconds (100-10000). Default is 1000ms.</value>
    [Range(100, 10000, ErrorMessage = "BaseRetryDelayMs must be between 100 and 10000 milliseconds")]
    public int BaseRetryDelayMs { get; init; } = 1000;

    #endregion

    #region Performance Monitoring

    /// <summary>
    /// Gets or sets a value indicating whether detailed performance logging is enabled.
    /// When enabled, database operation execution times will be logged for monitoring and optimization.
    /// </summary>
    /// <value><c>true</c> to enable performance logging; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool EnablePerformanceLogging { get; init; }

    /// <summary>
    /// Gets or sets the performance logging threshold in milliseconds.
    /// Operations taking longer than this threshold will be logged as warnings for performance analysis.
    /// Only effective when <see cref="EnablePerformanceLogging"/> is <c>true</c>.
    /// </summary>
    /// <value>Threshold in milliseconds (0-60000). Default is 1000ms.</value>
    [Range(0, 60000, ErrorMessage = "PerformanceLoggingThresholdMs must be between 0 and 60000 milliseconds")]
    public int PerformanceLoggingThresholdMs { get; init; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether connection pooling metrics are enabled.
    /// When enabled, additional metrics about connection pool usage will be collected and logged.
    /// </summary>
    /// <value><c>true</c> to enable connection pooling metrics; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool EnableConnectionPoolingMetrics { get; init; }

    #endregion

    #region Health Check Configuration

    /// <summary>
    /// Gets or sets the timeout in seconds for health check operations.
    /// Health checks will fail if the database doesn't respond within this timeframe.
    /// </summary>
    /// <value>Timeout in seconds (1-30). Default is 5 seconds.</value>
    [Range(1, 30, ErrorMessage = "HealthCheckTimeoutSeconds must be between 1 and 30 seconds")]
    public int HealthCheckTimeoutSeconds { get; init; } = 5;

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates the configuration options and returns any validation errors.
    /// This method can be used for custom validation beyond data annotations.
    /// </summary>
    /// <returns>A collection of validation results, or empty if validation passes</returns>
    public IEnumerable<ValidationResult> Validate()
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(this);
        
        Validator.TryValidateObject(this, context, validationResults, validateAllProperties: true);
        
        // Additional custom validation logic can be added here
        if (EnablePerformanceLogging && PerformanceLoggingThresholdMs <= 0)
        {
            validationResults.Add(new ValidationResult(
                "PerformanceLoggingThresholdMs must be greater than 0 when EnablePerformanceLogging is true",
                new[] { nameof(PerformanceLoggingThresholdMs) }));
        }
        
        return validationResults;
    }

    #endregion
}