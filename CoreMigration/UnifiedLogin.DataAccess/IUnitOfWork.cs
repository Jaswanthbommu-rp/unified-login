using System.Data;

namespace UnifiedLogin.DataAccess;

/// <summary>
/// Defines the contract for managing database connections and transactions.
/// Implements the Unit of Work pattern to maintain consistency across database operations
/// and provides transaction management capabilities.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the current database connection.
    /// If no connection exists, a new one will be created and opened automatically.
    /// The connection remains open until the unit of work is disposed.
    /// </summary>
    /// <value>An active <see cref="IDbConnection"/> instance ready for database operations</value>
    IDbConnection Connection { get; }
    
    /// <summary>
    /// Initializes the unit of work with the specified connection string.
    /// This method must be called before accessing the Connection property.
    /// </summary>
    /// <param name="connectionString">The database connection string to use for all operations</param>
    /// <exception cref="ArgumentException">Thrown when connectionString is null, empty, or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when the unit of work is already initialized</exception>
    void Initialize(string connectionString);
    
    /// <summary>
    /// Begins a new database transaction with ReadCommitted isolation level.
    /// All subsequent database operations will be part of this transaction until
    /// either Commit() or Rollback() is called.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already active or the unit of work is not initialized</exception>
    void BeginTransaction();

    /// <summary>
    /// Commits the current transaction, making all changes permanent.
    /// After calling this method, the transaction is completed and resources are cleaned up.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no transaction is active</exception>
    /// <exception cref="TransactionException">Thrown when the transaction cannot be committed due to database errors</exception>
    void Commit();

    /// <summary>
    /// Rolls back the current transaction, discarding all changes made within the transaction.
    /// After calling this method, the transaction is completed and resources are cleaned up.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no transaction is active</exception>
    void Rollback();
}