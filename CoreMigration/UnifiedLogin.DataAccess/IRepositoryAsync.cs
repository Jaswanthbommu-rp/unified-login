using System.Data;
using Dapper;
using UnifiedLogin.Core.DataAccess.Model;

namespace UnifiedLogin.Core.DataAccess;

/// <summary>
/// Modern async interface for repository operations
/// </summary>
public interface IRepositoryAsync : IAsyncDisposable
{
    /// <summary>
    /// Get an object of type IUnitOfWork
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    #region Async ExecuteNonQuery

    /// <summary>
    /// Executes the stored procedure with passed parameters and returns the number of rows affected.
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the stored procedure with passed parameters and command timeout, returns the number of rows affected.
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default);

    #endregion

    #region Async Execute

    /// <summary>
    /// Executes the stored procedure with passed parameters and returns the result in generic form.
    /// </summary>
    Task<T?> ExecuteAsync<T>(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the stored procedure with passed parameters and command timeout, returns the result in generic form.
    /// </summary>
    Task<T?> ExecuteAsync<T>(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default);

    #endregion

    #region Async GetOne

    /// <summary>
    /// Executes the stored procedure with passed parameters and returns the first row 
    /// </summary>
    Task<T?> GetOneAsync<T>(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the stored procedure with passed parameters and command timeout, returns the first row 
    /// </summary>
    Task<T?> GetOneAsync<T>(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default);

    #endregion

    #region Async GetMany

    /// <summary>
    /// Executes the stored procedure with passed parameters and returns the result in form of generic enumerator.
    /// </summary>
    Task<IEnumerable<T>> GetManyAsync<T>(string storedProcedureName, object? param = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the stored procedure with passed parameters and command timeout, returns the result in form of generic enumerator.
    /// </summary>
    Task<IEnumerable<T>> GetManyAsync<T>(string storedProcedureName, object? param = null, int? commandTimeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the sql with multiple input parameters and returns result in generic enumerator form.
    /// </summary>
    Task<IEnumerable<TReturn>> GetManyAsync<TFirst, TSecond, TReturn>(
        string sql, 
        Func<TFirst, TSecond, TReturn> map, 
        object? param = null, 
        CancellationToken cancellationToken = default);

    #endregion

    #region Async QueryMultiple

    /// <summary>
    /// Execute the stored proc with dynamic parameters and returns the grid reader
    /// </summary>
    Task<SqlMapper.GridReader> QueryMultipleAsync(
        string sql, 
        object? param = null, 
        IDbTransaction? transaction = null, 
        int? commandTimeout = null, 
        CancellationToken cancellationToken = default);

    #endregion

    #region Async TVP Operations

    /// <summary>
    /// Executes the stored procedure by passing a list of values as Table Value Parameters
    /// and returns the result.
    /// </summary>
    Task<int> ExecuteStoredProcWithTvpAsync<T>(
        TableValueParmInfo tableInfo, 
        IEnumerable<T>? tableparam = null,
        object? param = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the stored procedure by passing a list of values as Table Value Parameters
    /// and returns the IEnumerable result.
    /// </summary>
    Task<IEnumerable<TReturn>> GetManyWithTvpAsync<T, TReturn>(
        TableValueParmInfo tableInfo, 
        IEnumerable<T>? tableparam = null,
        object? param = null, 
        CancellationToken cancellationToken = default);

    #endregion
}