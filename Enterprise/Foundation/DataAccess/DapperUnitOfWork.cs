using System;
using System.Data;
using System.Transactions;

namespace RP.Enterprise.Foundation.DataAccess.Component
{
    /// <summary>
    /// DapperUnitOfWork: This class is used to open and close the connections. It also used to start and end transactions.
    /// </summary>
    public class DapperUnitOfWork : IUnitOfWork
    {
        #region Private Variables

        private TransactionScope _transaction;
        private IConnectionFactory _connectionFactory;
        private IDbConnection _connection;
        private string _connectionString;

        #endregion

        #region Ctor
        /// <summary>
        /// Ctor
        /// </summary>
        public DapperUnitOfWork(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Getter to get new connection and also to ensure connection is open
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                _connection = _connection ?? GetNewConnection();
                EnsureConnectionIsOpen();
                return _connection;
            }
        }

        /// <summary>
        /// Set the connection string as passed value of parameter and also get the new connection.
        /// </summary>
        /// <param name="connectionString"></param>
        public void Initialize(string connectionString)
        {
            _connectionString = connectionString;
            GetNewConnection();
        }

        /// <summary>
        /// Begin the transaction 
        /// </summary>
        public void BeginTransaction()
        {
            _connection = GetNewConnection();
            var txOptions = new TransactionOptions();
            txOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            _transaction = new TransactionScope(TransactionScopeOption.RequiresNew, txOptions);
            _connection.Open();
        }

        /// <summary>
        /// Commit and end transaction.
        /// </summary>
        public void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Complete();
            }
            EndTransaction();
        }

        /// <summary>
        /// Rollback transaction and dispose it.
        /// </summary>
        public void Rollback()
        {
            EndTransaction();
        }

        /// <summary>
        /// End the transaction and close the connection.
        /// </summary>
        public void Dispose()
        {
            EndTransaction();
            EndConnection();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// End transaction and dispose it
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
        /// Close and dispose the connection.
        /// </summary>
        private void EndConnection()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        /// <summary>
        /// Ensures the connection is opens the connection if it is not in open state.
        /// </summary>
        private void EnsureConnectionIsOpen()
        {
            if (_connection.State != ConnectionState.Connecting && _connection.State != ConnectionState.Open)
                _connection.Open();
        }

        /// <summary>
        /// Get new connection based on the connection string already set
        /// </summary>
        /// <returns></returns>
        private IDbConnection GetNewConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new ApplicationException("Not initialized.  You must call initialize before use");

            return _connectionFactory.GetConnection(_connectionString);
        }

        #endregion
    }
}
