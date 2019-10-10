using System;
using RP.Enterprise.Foundation.DataAccess.Component;
using System.Configuration;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Repository
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

        public BaseRepository(DbConnectionEnum dbConnection)
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

        public string GetConnectionString(DbConnectionEnum dbConnectionType)
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