using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Enum;
using System;
using UnifiedLogin.SharedObjects.Landing.Enum;
using Microsoft.Extensions.Options; // added for Options.Create
using Microsoft.Extensions.Logging.Abstractions; // added for NullLogger
using UnifiedLogin.DataAccess.Configuration; // added for DataAccessOptions
using Microsoft.Extensions.Configuration; // added for appsettings.json access

namespace UnifiedLogin.BusinessLogic.Repository
{
    public abstract class BaseRepository
	{
		#region Private Variables

		IConnectionFactory _connectionFactory; // removed direct instantiation to supply required ctor params explicitly
		IUnitOfWork _uow;
		IRepository _repository;
		private DbConnectionEnum _dbConnectionType;
        private bool _mockRepository = false;
        private static IConfiguration? _configuration; // cached configuration instance
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

            // Lazily create ConnectionFactory with required options & logger
            if (_connectionFactory == null)
            {
                var connectionString = GetConnectionString(_dbConnectionType);
                var options = Options.Create(new DataAccessOptions { ConnectionString = connectionString });
                var logger = NullLogger<ConnectionFactory>.Instance;
                _connectionFactory = new ConnectionFactory(options, logger);
                // Initialize UnitOfWork below will reuse same connection string
            }

            _uow = new DapperUnitOfWork(_connectionFactory);
			_repository = new DapperRepository(_uow);

            // Ensure UnitOfWork initialized with correct connection string (from enum)
			_repository.UnitOfWork.Initialize(GetConnectionString(_dbConnectionType));

			return _repository;
		}

		public string GetConnectionString(DbConnectionEnum dbConnectionType)
		{
            // Build configuration only once (first call); assumes appsettings.json present at base directory
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }

			try
			{
                var connectionString = _configuration.GetConnectionString("DBConnection");
				if (string.IsNullOrWhiteSpace(connectionString))
					throw new InvalidOperationException($"Connection string '{dbConnectionType}' not found in appsettings.json");
                return connectionString;
			}
			catch (Exception ex)
			{
				throw new Exception("Exception while getting Database connection. " + ex.Message, ex);
            }
		}

        #endregion
    }
}