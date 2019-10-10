using System;
using System.Data;

namespace RP.Enterprise.Foundation.DataAccess.Component
{
    /// <summary>
    /// Interface for transaction management.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Get Connection
        /// </summary>
        IDbConnection Connection { get; }
        
        /// <summary>
        /// Initialize connection
        /// </summary>
        void Initialize(string connectionString);
        
        /// <summary>
        /// Begin Transaction
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commit Transaction
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollback Transaction
        /// </summary>
        void Rollback();
    }
}