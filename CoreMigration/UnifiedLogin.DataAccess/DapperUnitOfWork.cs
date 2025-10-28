using System.Data;
using System.Transactions;

namespace UnifiedLogin.Core.DataAccess;

/// <summary>
/// Dapper-based Unit of Work implementation that manages database connections and transactions.
/// Implements the Unit of Work pattern to coordinate multiple repository operations within a single transaction scope.
/// Uses TransactionScope for distributed transaction support and automatic transaction management.
/// </summary>
public sealed class DapperUnitOfWork : IUnitOfWork
{
    #region Private Fields

    private TransactionScope? _transaction;
    private readonly IConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private string? _connectionString;
    private bool _disposed;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperUnitOfWork"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory used to create database connections</param>
    /// <exception cref="ArgumentNullException">Thrown when connectionFactory is null</exception>
    public DapperUnitOfWork(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the current database connection.
    /// Creates and opens a new connection if one doesn't exist or if the current connection is closed.
    /// The connection remains active until the unit of work is disposed.
    /// </summary>
    /// <value>An active <see cref="IDbConnection"/> ready for database operations</value>
    /// <exception cref="InvalidOperationException">Thrown when the unit of work is not initialized</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the unit of work has been disposed</exception>
    public IDbConnection Connection
    {
        get
        {
            ThrowIfDisposed();
            
            _connection ??= GetNewConnection();
            EnsureConnectionIsOpen();
            
            return _connection;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the unit of work with the specified connection string.
    /// This method must be called before accessing the Connection property or performing any database operations.
    /// </summary>
    /// <param name="connectionString">The database connection string to use for all operations</param>
    /// <exception cref="ArgumentException">Thrown when connectionString is null, empty, or whitespace</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the unit of work has been disposed</exception>
    public void Initialize(string connectionString)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
        
        _connectionString = connectionString;
        
        // Pre-create connection to validate the connection string early
        _connection = GetNewConnection();
    }

    /// <summary>
    /// Begins a new database transaction with ReadCommitted isolation level.
    /// Creates a new TransactionScope that will enlist all subsequent database operations.
    /// The transaction must be committed via <see cref="Commit"/> or rolled back via <see cref="Rollback"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already active or the unit of work is not initialized</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the unit of work has been disposed</exception>
    public void BeginTransaction()
    {
        ThrowIfDisposed();
        
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already active. Complete the current transaction before starting a new one.");
        }
        
        // Create a new connection for the transaction to ensure isolation
        EndConnection();
        _connection = GetNewConnection();
        
        // Configure transaction options for optimal performance and consistency
        var txOptions = new TransactionOptions
        {
            IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
            Timeout = TimeSpan.FromMinutes(10) // Reasonable timeout to prevent hanging transactions
        };
        
        _transaction = new TransactionScope(TransactionScopeOption.RequiresNew, txOptions, TransactionScopeAsyncFlowOption.Enabled);
        _connection.Open();
    }

    /// <summary>
    /// Commits the current transaction, making all changes permanent in the database.
    /// Completes the TransactionScope and releases all transaction-related resources.
    /// After calling this method, a new transaction can be started if needed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no transaction is active</exception>
    /// <exception cref="TransactionException">Thrown when the transaction cannot be committed due to database errors</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the unit of work has been disposed</exception>
    public void Commit()
    {
        ThrowIfDisposed();
        
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit. Call BeginTransaction() first.");
        }
        
        try
        {
            _transaction.Complete();
        }
        finally
        {
            EndTransaction();
        }
    }

    /// <summary>
    /// Rolls back the current transaction, discarding all changes made within the transaction scope.
    /// Disposes the TransactionScope without completing it, which automatically rolls back all operations.
    /// After calling this method, a new transaction can be started if needed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the unit of work has been disposed</exception>
    public void Rollback()
    {
        ThrowIfDisposed();
        EndTransaction();
    }

    /// <summary>
    /// Releases all resources used by the unit of work.
    /// Automatically rolls back any active transaction and closes the database connection.
    /// After calling this method, the unit of work cannot be reused.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            EndTransaction();
            EndConnection();
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Ends and disposes the current transaction scope.
    /// Called internally by Commit(), Rollback(), and Dispose() methods.
    /// </summary>
    private void EndTransaction()
    {
        if (_transaction != null)
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    /// <summary>
    /// Closes and disposes the current database connection.
    /// Called internally when cleaning up resources or preparing for a new connection.
    /// </summary>
    private void EndConnection()
    {
        if (_connection != null)
        {
            try
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
            }
            finally
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }

    /// <summary>
    /// Ensures the current connection is open and ready for database operations.
    /// Opens the connection if it's closed or in a connecting state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the connection cannot be opened</exception>
    private void EnsureConnectionIsOpen()
    {
        if (_connection != null && 
            _connection.State != ConnectionState.Connecting && 
            _connection.State != ConnectionState.Open)
        {
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open database connection. Please check connection string and database availability.", ex);
            }
        }
    }

    /// <summary>
    /// Creates a new database connection using the configured connection string.
    /// </summary>
    /// <returns>A new <see cref="IDbConnection"/> instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when the unit of work is not initialized</exception>
    private IDbConnection GetNewConnection()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Unit of work is not initialized. Call Initialize(connectionString) before accessing the connection.");
        }

        return _connectionFactory.GetConnection(_connectionString);
    }

    /// <summary>
    /// Throws <see cref="ObjectDisposedException"/> if the unit of work has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DapperUnitOfWork), "The unit of work has been disposed and cannot be used.");
        }
    }

    #endregion
}