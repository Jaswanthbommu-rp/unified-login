using Microsoft.Extensions.Caching.Memory;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System;
using System.Configuration;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    public abstract class BaseRepository
	{
        #region Private Variables
        public static IFusionCache _cache;
        public static MemoryCache _MemoryCache;
        IConnectionFactory _connectionFactory = new ConnectionFactory();
		IUnitOfWork _uow;
		IRepository _repository;
		private DbConnectionEnum _dbConnectionType;
        private bool _mockRepository = false;
		#endregion

		#region Public Methods

		public BaseRepository(DbConnectionEnum dbConnection, IFusionCache cache = null)
		{
			_dbConnectionType = dbConnection;
			_cache = cache;

            //if (_cache == null)
            //{
            //    var fusionOptions = new FusionCacheOptions()
            //    {
            //        CacheName = "landingapi",
            //        DefaultEntryOptions = new FusionCacheEntryOptions
            //        {
            //            Duration = TimeSpan.FromMinutes(2),
            //            IsFailSafeEnabled = true,
            //            FailSafeMaxDuration = TimeSpan.FromMinutes(2),
            //            FailSafeThrottleDuration = TimeSpan.FromSeconds(30),
            //
            //            //FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
            //            //FactoryHardTimeout = TimeSpan.FromMilliseconds(1500)
            //        },
            //    };
            //
            //    var redisOptions = new RedisCacheOptions()
            //    {
            //        Configuration = "localhost:6379",
            //        InstanceName = "landingapi",
            //        ConfigurationOptions = new ConfigurationOptions() { }
            //    };
            //    _cache = new FusionCache(fusionOptions);
            //    //_cache.SetupDistributedCache(new RedisCache(redisOptions), new FusionCacheNewtonsoftJsonSerializer());
            //}
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