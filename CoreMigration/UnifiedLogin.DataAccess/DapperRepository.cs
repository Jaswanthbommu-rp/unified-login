using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.DataAccess.Model;

namespace UnifiedLogin.DataAccess;

/// <summary>
/// DapperRepository: This class is used to implement available public methods in Dapper.
/// </summary>
public class DapperRepository : IRepository
{
    #region Private Variables

    private IUnitOfWork _unitOfWork;
    private readonly ILogger<DapperRepository>? _logger;

    #endregion

    #region Public Methods

    #region Ctor

    /// <summary>
    /// Ctor
    /// </summary>
    public DapperRepository(IUnitOfWork unitOfWork, ILogger<DapperRepository>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #endregion

    #region IUnitOfWork

    /// <summary>
    /// Gets IUnitOfWork
    /// </summary>
    public IUnitOfWork UnitOfWork
    {
        get { return _unitOfWork; }
        set { _unitOfWork = value; }
    }

    #endregion

    #region ExecuteNonQuery

    /// <summary>
    /// Executes passed stored procedure with passed parameters and returns the number of rows affected.
    /// </summary>
    public int ExecuteNonQuery(string storedProcedureName, object? param = null)
    {
        _logger?.LogDebug("Executing non-query stored procedure: {ProcName}", storedProcedureName);
        
        try
        {
            var result = _unitOfWork.Connection.Execute(storedProcedureName, param, commandType: CommandType.StoredProcedure);
            _logger?.LogDebug("Successfully executed {ProcName}, rows affected: {RowsAffected}", storedProcedureName, result);
            return result;
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            _logger?.LogError(ex, "Failed to execute {ProcName}", storedProcedureName);
            throw;
        }
    }

    /// <summary>
    /// Execute Non Query Stored Proc
    /// </summary> 
    public int ExecuteNonQuery(string storedProcedureName, object? param = null, int? commandTimeout = null)
    {
        _logger?.LogDebug("Executing non-query stored procedure: {ProcName} with timeout: {Timeout}", storedProcedureName, commandTimeout);
        
        try
        {
            var result = _unitOfWork.Connection.Execute(storedProcedureName, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
            _logger?.LogDebug("Successfully executed {ProcName}, rows affected: {RowsAffected}", storedProcedureName, result);
            return result;
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            _logger?.LogError(ex, "Failed to execute {ProcName}", storedProcedureName);
            throw;
        }
    }

    #endregion

    #region Execute

    /// <summary>
    /// Executes the stored procedure for passed params and returns the first record in generic form
    /// </summary>
    public T? Execute<T>(string storedProcedureName, object? param = null)
    {
        try
        {
            return GetOne<T>(storedProcedureName, param);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Executes the stored procedure for passed params and returns the first record in generic form
    /// </summary>
    public T? Execute<T>(string storedProcedureName, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return GetOne<T>(storedProcedureName, param, commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Executes the stored procedure for passed params and returns the first record in dynamic form
    /// </summary>
    public dynamic? Execute(string storedProcedureName, object? param = null)
    {
        try
        {
            return GetOne(storedProcedureName, param);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Executes the stored procedure for passed params and returns the first record in dynamic form
    /// </summary>
    public dynamic? Execute(string storedProcedureName, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return GetOne(storedProcedureName, param, commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    #endregion

    #region GetOne

    /// <summary>
    /// Executes the stored procedure for passed params and returns the first record in generic form
    /// </summary>
    public T? GetOne<T>(string storedProcedureName, object? param = null)
    {
        var results = GetMany<T>(storedProcedureName, param);
        return results.Any() ? results.First() : default(T);
    }

    /// <summary>
    /// Executes the stored procedure for passed params and returns the first record in generic form
    /// </summary>
    public T? GetOne<T>(string storedProcedureName, object? param = null, int? commandTimeout = null)
    {
        var results = GetMany<T>(storedProcedureName, param, commandTimeout);
        return results.Any() ? results.First() : default(T);
    }

    /// <summary>
    /// Executes the stored procedure for passed params and returns the first record in dynamic form
    /// </summary>
    public dynamic? GetOne(string storedProcedureName, object? param = null)
    {
        var results = GetMany(storedProcedureName, param);
        return results.Any() ? results.First() : null;
    }

    public dynamic? GetOne(string storedProcedureName, object? param = null, int? commandTimeout = null)
    {
        var results = GetMany(storedProcedureName, param, commandTimeout);
        return results.Any() ? results.First() : null;
    }

    #endregion

    #region Get Many

    /// <summary>
    /// Gets the result of stored procedure for passed parameters in form of generic enumerator.
    /// </summary>
    public IEnumerable<T> GetMany<T>(string storedProcedureName, object? param = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<T>(storedProcedureName, param, commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Gets the result of stored procedure for passed parameters in form of generic enumerator.
    /// </summary>
    public IEnumerable<T> GetMany<T>(string storedProcedureName, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<T>(storedProcedureName, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Gets the result of stored procedure for passed parameters in form of dynamic enumerator.
    /// </summary>
    public IEnumerable<dynamic> GetMany(string storedProcedureName, object? param = null)
    {
        try
        {
            return _unitOfWork.Connection.Query(storedProcedureName, param, commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Gets the result of stored procedure for passed parameters in form of dynamic enumerator.
    /// </summary>
    public IEnumerable<dynamic> GetMany(string storedProcedureName, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return _unitOfWork.Connection.Query(storedProcedureName, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", storedProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with two input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with three input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary> 
    /// <returns></returns>
    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with four input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TFourth, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TFourth, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with five input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, int? commandTimeout = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with five input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    #endregion

    #region QueryMultiple

    /// <summary>
    /// Execute the Stored Proc with dynamic parameters and returns the grid reader
    /// </summary>
    public SqlMapper.GridReader QueryMultiple(string sql, dynamic? param = null, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        try
        {
            return SqlMapper.QueryMultiple(_unitOfWork.Connection, sql, param, transaction, commandTimeout, commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    #endregion

    #region Get Many with SplitOn

    /// <summary>
    /// Execute the sql with three input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, string? splitOn = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, splitOn: splitOn ?? "Id");
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with four input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, string? splitOn = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TFourth, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, splitOn: splitOn ?? "Id");
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with four input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, string? splitOn = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, splitOn: splitOn ?? "Id");
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Execute the sql with four input and one return parameters and returns result in generic enumerator form.
    /// </summary>
    public IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, string? splitOn = null)
    {
        try
        {
            return _unitOfWork.Connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, map, param, commandType: CommandType.StoredProcedure, splitOn: splitOn ?? "Id");
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", sql);
            AddParametersToException(ex, param);
            throw;
        }
    }

    #endregion

    #region Execute or Get With TVP

    /// <summary>
    /// Executes the stored procedure by passing a list of values as Table Value Parameters
    /// and returns the result.
    /// </summary>
    public int ExecuteStoredProcWithTvp<T>(TableValueParmInfo tableInfo, IEnumerable<T>? tableparam = null, object? param = null)
    {
        try
        {
            DynamicParameters? dparm = param as DynamicParameters;

            if (tableparam != null)
            {
                // Convert the list of values to a DataTable 
                var dataTable = tableparam.ConvertToTableValuedParameter(tableInfo.TableParamTypeName,
                    tableInfo.OrderedColumnName);
                dparm?.Add(tableInfo.TableVariableName, dataTable);
            }

            return _unitOfWork.Connection.Execute(tableInfo.StoredProcedureName, dparm,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", tableInfo.StoredProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    /// <summary>
    /// Executes the stored procedure by passing a list of values as Table Value Parameters
    /// and returns the IEnumerable result.
    /// </summary>
    public IEnumerable<TReturn> GetManyWithTvp<T, TReturn>(TableValueParmInfo tableInfo, IEnumerable<T>? tableparam = null,
        object? param = null)
    {
        try
        {
            DynamicParameters? dparm = param as DynamicParameters;

            if (tableparam != null)
            {
                // Convert the list of values to a DataTable 
                var dataTable = tableparam.ConvertToTableValuedParameter(tableInfo.TableParamTypeName,
                    tableInfo.OrderedColumnName);
                dparm?.Add(tableInfo.TableVariableName, dataTable);
            }

            return _unitOfWork.Connection.Query<TReturn>(tableInfo.StoredProcedureName, dparm,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception orig)
        {
            var ex = new Exception("Dapper proc execution failed!", orig);
            ex.Data.Add("ProcName", tableInfo.StoredProcedureName);
            AddParametersToException(ex, param);
            throw;
        }
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Dispose method to dispose resources.
    /// </summary>
    public void Dispose()
    {
        _unitOfWork.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #endregion

    #region Private Method

    private static void AddParametersToException(Exception ex, object? paramList)
    {
        if (paramList != null)
        {
            if (paramList.GetType() == typeof(DynamicParameters))
            {
                foreach (var p in ((DynamicParameters)paramList).ParameterNames)
                {
                    ex.Data.Add(p, p);
                }
            }
            else
            {
                var props = paramList.GetType().GetProperties();
                foreach (var prop in props)
                {
                    if (prop.GetValue(paramList) != null)
                        ex.Data.Add(prop.Name, prop.GetValue(paramList)?.ToString());
                }
            }
        }
    }

    #endregion
}