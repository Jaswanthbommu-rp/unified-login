using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Enum;
using System;
using System.Configuration;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    public abstract class BaseRepository
	{
		#region Private Variables

		IConnectionFactory _connectionFactory = new ConnectionFactory();
		IUnitOfWork _uow;
		IRepository _repository;
		private DbConnectionEnum _dbConnectionType;
        private bool _mockRepository = false;
		#endregion

		#region Public Methods

		public BaseRepository(DbConnectionEnum dbConnection)
		{
			_dbConnectionType = dbConnection;
		}

        public BaseRepository(IRepository repository)
        {
            _repository = repository;
            _mockRepository = true;
        }

        public IRepository GetRepository()
        {
            if (_mockRepository)
            {
                return _repository;
            }

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