using System;
using System.Configuration;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Command.Repository
{
    public abstract class BaseRepository
    {
        #region Private Variables

        IConnectionFactory _connectionFactory = new ConnectionFactory();
        IUnitOfWork _uow;
        IRepository _repository;

        #endregion

        #region Public Methods

        public BaseRepository()
        {
        }

        public IRepository GetRepository()
        {
            _uow = new DapperUnitOfWork(_connectionFactory);
            _repository = new DapperRepository(_uow);
            _repository.UnitOfWork.Initialize(GetConnectionString());

            return _repository;
        }

        private string GetConnectionString()
        {
            string connectionString;

            try
            {
                connectionString = (ConfigurationManager.ConnectionStrings["AuditDbCnn"].ConnectionString);
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