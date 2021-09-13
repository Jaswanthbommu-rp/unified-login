using System;
using System.Configuration;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Repository
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

        public IRepository GetRepository(bool isAuditArchive = false)
        {
            _uow = new DapperUnitOfWork(_connectionFactory);
            _repository = new DapperRepository(_uow);
            _repository.UnitOfWork.Initialize(GetConnectionString(isAuditArchive));

            return _repository;
        }

        private string GetConnectionString(bool isAuditArchive = false)
        {
            string connectionString;

            try
            {
                connectionString = isAuditArchive ? (ConfigurationManager.ConnectionStrings["AuditArchiveDbCnn"].ConnectionString): (ConfigurationManager.ConnectionStrings["AuditDbCnn"].ConnectionString);
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