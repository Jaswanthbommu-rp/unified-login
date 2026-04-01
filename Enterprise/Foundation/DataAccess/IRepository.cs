using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using RP.Enterprise.Foundation.DataAccess.Component.Model;

namespace RP.Enterprise.Foundation.DataAccess.Component
{
    /// <summary>
    /// Interface to execute stored procedures 
    /// </summary>
    public interface IRepository : IDisposable
    {
        #region UnitOfWork

        /// <summary>
        /// Get an object of type IUnitOfWork
        /// </summary>
        IUnitOfWork UnitOfWork { get; set; }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the number of rows affected.
        /// </summary>
        int ExecuteNonQuery(string storedProcedureName, object param = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and command timeout time , returns the number of rows affected.
        /// </summary>
        int ExecuteNonQuery(string storedProcedureName, object param = null, int? commandTimeout = null);

        #endregion

        #region Execute

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the result in generic form.
        /// </summary>
        T Execute<T>(string storedProcedureName, object param = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and command timeout time,  returns the result in generic form.
        /// </summary>
        T Execute<T>(string storedProcedureName, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the result in dynamic form.
        /// </summary>
        dynamic Execute(string storedProcedureName, object param = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and command timeout time, returns the result in dynamic form.
        /// </summary>
        dynamic Execute(string storedProcedureName, object param = null, int? commandTimeout = null);

        #endregion

        #region GetOne (Returns the first row in generic form)

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the first row 
        /// </summary>
        T GetOne<T>(string storedProcedureName, object param = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and command timeout time, returns the first row 
        /// </summary>
        T GetOne<T>(string storedProcedureName, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the first row  
        /// </summary>
        dynamic GetOne(string storedProcedureName, object param = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and command timeout time, returns the first row  
        /// </summary>
        dynamic GetOne(string storedProcedureName, object param = null, int? commandTimeout = null);

        #endregion

        #region GetMany

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the result in form of generic enumerator.
        /// </summary>
        IEnumerable<T> GetMany<T>(string storedProcedureName, object param = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and command timeout time, returns the result in form of generic enumerator.
        /// </summary>
        IEnumerable<T> GetMany<T>(string storedProcedureName, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the result in form of dynamic enumerator.
        /// </summary>
        IEnumerable<dynamic> GetMany(string storedProcedureName, object param = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and command timeout time, returns the result in form of dynamic enumerator.
        /// </summary>
        IEnumerable<dynamic> GetMany(string storedProcedureName, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the sql with two input and one return parameters and returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null);

        /// <summary>
        ///  Executes the sql with two input and one return parameters and command timeout time, returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the sql with three input and one return parameters and returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null);

        /// <summary>
        /// Executes the sql with three input and one return parameters and command timeout time, returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the sql with four input and one return parameters and returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null);

        /// <summary>
        /// Executes the sql with four input and one return parameters and command timeout time, returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the sql with five input and one return parameters and returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null);

        /// <summary>
        /// Executes the sql with five input and one return parameters and command timeout time,  returns result in generic enumerator form.
        /// </summary>
        IEnumerable<TReturn> GetMany<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, int? commandTimeout = null);


        #endregion

        #region Get Many with SplitOn

        /// <summary>
        /// Execute the sql with two input and one return parameters and returns result in generic enumerator form with split based on Ids.
        /// </summary>
        IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map,
            object param = null, string splitOn = null);

        /// <summary>
        /// Execute the sql with three input and one return parameters and returns result in generic enumerator form with split based on Ids.
        /// </summary>
        IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = null);
        
        /// <summary>
        /// Executes the sql with four input and one return parameters and command timeout time,  returns result in generic enumerator form with split based on Ids.
        /// </summary>
        IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, string splitOn = null);

        /// <summary>
        /// Executes the sql with four input and one return parameters, returns result in generic enumerator form with split based on Ids. When buffered is false, rows are streamed and not double-buffered.
        /// </summary>
        IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param, string splitOn, bool buffered);

        /// <summary>
        /// Executes the sql with five input and one return parameters and command timeout time,  returns result in generic enumerator form with split based on Ids.
        /// </summary>
        IEnumerable<TReturn> GetManyWithSpliOn<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, string splitOn = null);
        
        #endregion

        #region Other methods
        /// <summary>
        /// Execute the stored proc with dynamic parameters, transaction, command timeout and command type parameters and returns the grid reader
        /// </summary>
        SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, IDbTransaction transaction = null, int? commandTimeout = null);

        /// <summary>
        /// Executes the stored procedure with passed parameters and returns the result in generic form.
        /// </summary>
        int ExecuteStoredProcWithTvp<T>(TableValueParmInfo tableInfo, IEnumerable<T> tableparam = null,
            object param = null);


        /// <summary>
        /// Executes the stored procedure by passing a list of values as Table Value Parameters
        /// and returns the IEnumerable result.
        /// </summary>
        IEnumerable<TReturn> GetManyWithTvp<T, TReturn>(TableValueParmInfo tableInfo, IEnumerable<T> tableparam = null,
            object param = null);

        #endregion
    }

}
