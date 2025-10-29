using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnifiedLogin.DataAccess.Configuration;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.DataAccess.Model;
using System.Diagnostics;

namespace UnifiedLogin.DataAccess;

/// <summary>
/// Modern async repository implementation with performance monitoring and enhanced error handling
/// </summary>
public sealed class DapperRepositoryAsync(
    IUnitOfWork unitOfWork,
    ILogger<DapperRepositoryAsync> logger,
    IOptions<DataAccessOptions> options) : IRepositoryAsync
{
    private readonly DataAccessOptions _options = options.Value;
    private readonly ActivitySource _activitySource = new("DataAccess.Repository");

    public IUnitOfWork UnitOfWork { get; } = unitOfWork;

    #region Async ExecuteNonQuery

    public async Task<int> ExecuteNonQueryAsync(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default)
        => await ExecuteNonQueryAsync(storedProcedureName, param, _options.DefaultCommandTimeoutSeconds, cancellationToken);

    public async Task<int> ExecuteNonQueryAsync(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedureName);
        
        using var activity = _activitySource.StartActivity($"ExecuteNonQuery.{storedProcedureName}");
        activity?.SetTag("operation", "ExecuteNonQuery");
        activity?.SetTag("storedProcedure", storedProcedureName);
        
        var stopwatch = _options.EnablePerformanceLogging ? Stopwatch.StartNew() : null;
        
        try
        {
            logger.LogDebug("Executing non-query stored procedure: {ProcName}", storedProcedureName);
            
            var result = await UnitOfWork.Connection.ExecuteAsync(
                storedProcedureName, 
                param, 
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout ?? _options.DefaultCommandTimeoutSeconds);
            
            LogPerformanceIfEnabled(stopwatch, storedProcedureName, "ExecuteNonQuery");
            logger.LogDebug("Successfully executed {ProcName}, rows affected: {RowsAffected}", storedProcedureName, result);
            
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to execute non-query stored procedure: {ProcName}", storedProcedureName);
            throw new DataAccessException($"Failed to execute stored procedure '{storedProcedureName}'", ex);
        }
    }

    #endregion

    #region Async Execute

    public async Task<T?> ExecuteAsync<T>(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default)
        => await ExecuteAsync<T>(storedProcedureName, param, _options.DefaultCommandTimeoutSeconds, cancellationToken);

    public async Task<T?> ExecuteAsync<T>(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
        => await GetOneAsync<T>(storedProcedureName, param, commandTimeout, cancellationToken);

    #endregion

    #region Async GetOne

    public async Task<T?> GetOneAsync<T>(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default)
        => await GetOneAsync<T>(storedProcedureName, param, _options.DefaultCommandTimeoutSeconds, cancellationToken);

    public async Task<T?> GetOneAsync<T>(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        var results = await GetManyAsync<T>(storedProcedureName, param, commandTimeout, cancellationToken);
        return results.FirstOrDefault();
    }

    #endregion

    #region Async GetMany

    public async Task<IEnumerable<T>> GetManyAsync<T>(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default)
        => await GetManyAsync<T>(storedProcedureName, param, _options.DefaultCommandTimeoutSeconds, cancellationToken);

    public async Task<IEnumerable<T>> GetManyAsync<T>(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedureName);
        
        using var activity = _activitySource.StartActivity($"GetMany.{storedProcedureName}");
        activity?.SetTag("operation", "GetMany");
        activity?.SetTag("storedProcedure", storedProcedureName);
        activity?.SetTag("resultType", typeof(T).Name);
        
        var stopwatch = _options.EnablePerformanceLogging ? Stopwatch.StartNew() : null;
        
        try
        {
            logger.LogDebug("Executing query stored procedure: {ProcName} -> {ResultType}", storedProcedureName, typeof(T).Name);
            
            var result = await UnitOfWork.Connection.QueryAsync<T>(
                storedProcedureName, 
                param, 
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout ?? _options.DefaultCommandTimeoutSeconds);
            
            var resultList = result.ToList(); // Materialize to count
            LogPerformanceIfEnabled(stopwatch, storedProcedureName, "GetMany", resultList.Count);
            
            logger.LogDebug("Successfully executed {ProcName}, returned {Count} records", storedProcedureName, resultList.Count);
            
            return resultList;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to execute query stored procedure: {ProcName}", storedProcedureName);
            throw new DataAccessException($"Failed to execute stored procedure '{storedProcedureName}'", ex);
        }
    }

    public async Task<IEnumerable<TReturn>> GetManyAsync<TFirst, TSecond, TReturn>(
        string sql, 
        Func<TFirst, TSecond, TReturn> map, 
        object? param = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(map);
        
        using var activity = _activitySource.StartActivity($"GetManyMultiMap.{sql.Split(' ').FirstOrDefault()}");
        
        try
        {
            logger.LogDebug("Executing multi-map query: {Sql}", sql);
            
            var result = await UnitOfWork.Connection.QueryAsync<TFirst, TSecond, TReturn>(
                sql, 
                map, 
                param, 
                commandType: CommandType.StoredProcedure,
                commandTimeout: _options.DefaultCommandTimeoutSeconds);
            
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to execute multi-map query: {Sql}", sql);
            throw new DataAccessException($"Failed to execute multi-map query", ex);
        }
    }

    #endregion

    #region Async QueryMultiple

    public async Task<SqlMapper.GridReader> QueryMultipleAsync(
        string sql, 
        object? param = null, 
        IDbTransaction? transaction = null, 
        int? commandTimeout = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        
        using var activity = _activitySource.StartActivity($"QueryMultiple.{sql.Split(' ').FirstOrDefault()}");
        
        try
        {
            logger.LogDebug("Executing multiple result query: {Sql}", sql);
            
            return await UnitOfWork.Connection.QueryMultipleAsync(
                sql, 
                param, 
                transaction,
                commandTimeout: commandTimeout ?? _options.DefaultCommandTimeoutSeconds,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to execute multiple result query: {Sql}", sql);
            throw new DataAccessException($"Failed to execute multiple result query", ex);
        }
    }

    #endregion

    #region Async TVP Operations

    public async Task<int> ExecuteStoredProcWithTvpAsync<T>(
        TableValueParmInfo tableInfo, 
        IEnumerable<T>? tableparam = null,
        object? param = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableInfo);
        
        try
        {
            var dparm = param as DynamicParameters ?? new DynamicParameters();

            if (tableparam != null)
            {
                var dataTable = tableparam.ConvertToTableValuedParameter(
                    tableInfo.TableParamTypeName,
                    tableInfo.OrderedColumnName);
                dparm.Add(tableInfo.TableVariableName, dataTable);
            }

            return await UnitOfWork.Connection.ExecuteAsync(
                tableInfo.StoredProcedureName, 
                dparm,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _options.DefaultCommandTimeoutSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute stored procedure with TVP: {ProcName}", tableInfo.StoredProcedureName);
            throw new DataAccessException($"Failed to execute stored procedure '{tableInfo.StoredProcedureName}' with TVP", ex);
        }
    }

    public async Task<IEnumerable<TReturn>> GetManyWithTvpAsync<T, TReturn>(
        TableValueParmInfo tableInfo, 
        IEnumerable<T>? tableparam = null,
        object? param = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableInfo);
        
        try
        {
            var dparm = param as DynamicParameters ?? new DynamicParameters();

            if (tableparam != null)
            {
                var dataTable = tableparam.ConvertToTableValuedParameter(
                    tableInfo.TableParamTypeName,
                    tableInfo.OrderedColumnName);
                dparm.Add(tableInfo.TableVariableName, dataTable);
            }

            return await UnitOfWork.Connection.QueryAsync<TReturn>(
                tableInfo.StoredProcedureName, 
                dparm,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _options.DefaultCommandTimeoutSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query stored procedure with TVP: {ProcName}", tableInfo.StoredProcedureName);
            throw new DataAccessException($"Failed to query stored procedure '{tableInfo.StoredProcedureName}' with TVP", ex);
        }
    }

    #endregion

    #region Private Methods

    private void LogPerformanceIfEnabled(Stopwatch? stopwatch, string operation, string type, int? recordCount = null)
    {
        if (!_options.EnablePerformanceLogging || stopwatch == null) return;
        
        stopwatch.Stop();
        var elapsed = stopwatch.ElapsedMilliseconds;
        
        if (elapsed > _options.PerformanceLoggingThresholdMs)
        {
            logger.LogWarning(
                "Slow database operation detected: {Type} {Operation} took {ElapsedMs}ms" + (recordCount.HasValue ? " and returned {RecordCount} records" : ""),
                type, operation, elapsed, recordCount);
        }
        else if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Database operation completed: {Type} {Operation} took {ElapsedMs}ms" + (recordCount.HasValue ? " and returned {RecordCount} records" : ""),
                type, operation, elapsed, recordCount);
        }
    }

    #endregion

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        _activitySource.Dispose();
        UnitOfWork.Dispose();
        await Task.CompletedTask;
    }

    #endregion
}

/// <summary>
/// Custom exception for data access operations
/// </summary>
public class DataAccessException : Exception
{
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
}