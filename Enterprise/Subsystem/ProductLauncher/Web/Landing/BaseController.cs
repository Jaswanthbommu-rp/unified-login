using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Routing;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public abstract class BaseController : Controller
	{
		/// <summary>
		/// Holds default user claim related information
		/// </summary>
		public DefaultUserClaim _userClaims;
        public static IFusionCache FusionCache;
        private static FusionCacheOptions fusionOptions;
        private static RedisCacheOptions redisOptions;

        /// <summary>
        /// base constructor
        /// </summary>
        protected BaseController() { }

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);

            if (FusionCache == null)
            {
                fusionOptions = new FusionCacheOptions()
                {
                    CacheName = "landingapi",
                    DefaultEntryOptions = new FusionCacheEntryOptions
                    {
                        Duration = TimeSpan.FromMinutes(2),
                        IsFailSafeEnabled = true,
                        FailSafeMaxDuration = TimeSpan.FromMinutes(2),
                        FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

                        //FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
                        //FactoryHardTimeout = TimeSpan.FromMilliseconds(1500)
                    },
                };

                redisOptions = new RedisCacheOptions()
                {
                    Configuration = "localhost:6379",
                    InstanceName = "landingapi",
                    ConfigurationOptions = new ConfigurationOptions() { }
                };
                FusionCache = new FusionCache(fusionOptions);
                FusionCache.SetupDistributedCache(new RedisCache(redisOptions), new FusionCacheNewtonsoftJsonSerializer());
            }
            
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
			if (currentClaimPrincipal.Identity.IsAuthenticated)
			{
				_userClaims = new DefaultUserClaim(currentClaimPrincipal);
				List<string> userRights = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaims, FusionCache);

				if (userRights != null && userRights.Count > 0)
				{
					var identity = (ClaimsIdentity)currentClaimPrincipal.Identity;
					identity.AddClaims(userRights.Select(a => new Claim("right", a)).ToList());
				}
				_userClaims.Rights = userRights;
			}
			else
			{
				// if the call is anonymous, build a default guid and later the code may override it if one has been stored somewhere else
				_userClaims = new DefaultUserClaim() { CorrelationId = Guid.NewGuid() };
			}
		}

		#region Logging

		/// <summary>
		/// Used to write data to the information log for diagnostic
		/// </summary>
		protected void WriteToInformationLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
			WriteToLog(logType: LogEventLevel.Information, message: message, logData: logData, messageProperties: messageProperties);

		}

		/// <summary>
		/// Used to write data to the error log
		/// </summary>
		protected void WriteToErrorLog(string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
		{
			WriteToLog(logType: LogEventLevel.Error, message: message, logData: logData, exception: exception, messageProperties: messageProperties);
		}

		/// <summary>
		/// Used to write data to the diagnostic log
		/// </summary>
		protected void WriteToDiagnosticLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
			WriteToLog(logType: LogEventLevel.Debug, message: message, logData: logData, messageProperties: messageProperties);
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            string correlationId = "";
            if (_userClaims != null)
            {
                correlationId = (_userClaims.CorrelationId != Guid.Empty) ? _userClaims.CorrelationId.ToString() : "";
            }
            var logger = Log.Logger;
			if (logData?.Keys != null)
			{
				logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
			}
			logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }

		#endregion
	}
}