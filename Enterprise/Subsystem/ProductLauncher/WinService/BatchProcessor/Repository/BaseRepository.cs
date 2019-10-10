using System;
using System.Configuration;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository
{
    public abstract class BaseRepository
    {
        #region Private Variables

        IConnectionFactory _connectionFactory = new ConnectionFactory();
        IUnitOfWork _uow;
        IRepository _repository;
        private DbConnectionEnum _dbConnectionType;

        #endregion

        #region Public Methods

        protected BaseRepository(DbConnectionEnum dbConnection)
        {
            _dbConnectionType = dbConnection;
        }

        public IRepository GetRepository()
        {
            _uow = new DapperUnitOfWork(_connectionFactory);
            _repository = new DapperRepository(_uow);
            _repository.UnitOfWork.Initialize(GetConnectionString(_dbConnectionType));

            return _repository;
        }

        private string GetConnectionString(DbConnectionEnum dbConnectionType)
        {
            string connectionString;

            try
            {
                connectionString = (ConfigurationManager.ConnectionStrings[dbConnectionType.ToString()].ConnectionString);
                if (connectionString == null)
                    throw new Exception("Database connection settings have not been set in Web.config file");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while getting Database connection." + ex.Message);
            }

            return connectionString;
        }

        #endregion
    }
}
