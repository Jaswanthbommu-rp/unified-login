using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;

namespace RP.Enterprise.Foundation.Audit.Core.Component
{
    public static class Log
    {
        #region Private Members

        private static readonly ILogger _perfLogger;
        private static readonly ILogger _errorLogger;

        private static readonly ILogger _diagnosticLogger;

        //private static readonly ILogger _utilizationLogger;
        private static readonly ILogger _informationLogger;

        #endregion

        #region Ctor

        static Log()
        {

            if (ConfigReader.ShouldWriteInFile)
            {
                _errorLogger = GetLoggerConfigurationWithFile("error", "LogDetails")?
                    .CreateLogger();
                //_utilizationLogger = GetLoggerConfigurationWithFile("utilization", "UtilizationLogDetails").CreateLogger();

                _informationLogger = GetLoggerConfigurationWithFile("info", "InformationLogDetails")?.CreateLogger();

                if (ConfigReader.ShouldLogPerformance)
                    _perfLogger = GetLoggerConfigurationWithFile("performance", "PerformanceLogDetails")?.CreateLogger();

                if (ConfigReader.ShouldLogDiagnostic)
                    _diagnosticLogger = GetLoggerConfigurationWithFile("diagnostic", "LogDetails")?.CreateLogger();
            }
            else
            {
                _errorLogger = GetLoggerConfiguration("error", "LogDetails")?.CreateLogger();
                //_utilizationLogger = GetLoggerConfiguration("utilization", "UtilizationLogDetails").CreateLogger();

                _informationLogger = GetLoggerConfiguration("info", "InformationLogDetails")?.CreateLogger();

                if (ConfigReader.ShouldLogPerformance)
                    _perfLogger = GetLoggerConfiguration("performance", "PerformanceLogDetails")?.CreateLogger();

                if (ConfigReader.ShouldLogDiagnostic)
                    _diagnosticLogger = GetLoggerConfiguration("diagnostic", "LogDetails")?.CreateLogger();
            }
        }

        #endregion

        #region Public Methods

        public static void Write(LogType logType, LogDetails logDetails)
        {
            if (logDetails != null)
            {
                if (string.IsNullOrEmpty(logDetails.ProductName))
                    logDetails.ProductName = ConfigReader.LogProductName;

                if (string.IsNullOrEmpty(logDetails.Environment))
                    logDetails.Environment = ConfigReader.Environment;

                switch (logType)
                {
                    case LogType.Error:
                        if (string.IsNullOrEmpty(logDetails.Message))
                        {
                            logDetails.Message = GetMessageFromException(logDetails.Exception);
                        }

                        _errorLogger?.Write(LogEventLevel.Error, "{@LogDetails}", logDetails);
                        break;
                    case LogType.Performance:
                        if (ConfigReader.ShouldLogPerformance)
                        {
                            // map logDetails to performance log details
                            var perfDetails = new PerformanceLogDetails
                            {
                                ProductName = logDetails.ProductName,
                                ElapsedMilliseconds = logDetails.ElapsedMilliseconds,
                                CorrelationId = logDetails.CorrelationId,
                                Message = logDetails.Message,
                                AdditionalInfo = logDetails.AdditionalInfo,
                                ServerName = logDetails.ServerName,
                                UserId = logDetails.UserId,
                                UserName = logDetails.UserName,
                                ProductLocation = logDetails.ProductLocation,
                                PmcId = logDetails.PmcId,
                                PmcName = logDetails.PmcName,
                                SiteId = logDetails.SiteId,
                                SiteName = logDetails.SiteName,
                                Environment = logDetails.Environment,
                                ProductModule = logDetails.ProductModule,
                                ProductWorkflow = logDetails.ProductWorkflow,
                                ProductStep = logDetails.ProductStep,
                            };

                            _perfLogger?.Write(LogEventLevel.Information, "{@PerformanceLogDetails}", perfDetails);
                        }

                        break;
                    case LogType.Diagnostic:
                        if (ConfigReader.ShouldLogDiagnostic)
                        {
                            if (string.IsNullOrEmpty(logDetails.Message))
                            {
                                logDetails.Message = GetMessageFromException(logDetails.Exception);
                            }

                            _diagnosticLogger?.Write(LogEventLevel.Information, "{@LogDetails}", logDetails);
                        }

                        break;
                    case LogType.Information:
                        // map logDetails to info details  
                        var infoDetails = new InformationLogDetails
                        {
                            ProductName = logDetails.ProductName,
                            ElapsedMilliseconds = logDetails.ElapsedMilliseconds,
                            CorrelationId = logDetails.CorrelationId,
                            Message = logDetails.Message,
                            AdditionalInfo = logDetails.AdditionalInfo,
                            ServerName = logDetails.ServerName,
                            UserId = logDetails.UserId,
                            UserName = logDetails.UserName,
                            ProductLocation = logDetails.ProductLocation,
                            PmcId = logDetails.PmcId,
                            PmcName = logDetails.PmcName,
                            SiteId = logDetails.SiteId,
                            SiteName = logDetails.SiteName,
                            Environment = logDetails.Environment,
                            ProductModule = logDetails.ProductModule,
                            ProductWorkflow = logDetails.ProductWorkflow,
                            ProductStep = logDetails.ProductStep,
                        };

                        _informationLogger?.Write(LogEventLevel.Information, "{@InformationLogDetails}", infoDetails);
                        break;
                }
            }
        }

        #endregion

        #region Private Methods

        private static string GetMessageFromException(Exception ex)
        {
            if (ex == null) return "";
            if (ex.InnerException != null)
            {
                return GetMessageFromException(ex.InnerException);
            }

            return ex.Message;
        }

        private static LoggerConfiguration GetLoggerConfigurationWithFile(string logType, string jsonFormatter)
        {
            var elasticSearchUri = ConfigReader.ElasticSearchUri;
            var elasticSearchIndexTypeName = ConfigReader.ElasticSearchIndexTypeName;

            if (!string.IsNullOrEmpty(elasticSearchUri) && !string.IsNullOrEmpty(elasticSearchIndexTypeName))
            {
                return new LoggerConfiguration()
                    .WriteTo.File(formatter: new JsonFormatter(jsonFormatter),
                        path: $"{ConfigReader.LogPath}\\{ConfigReader.LogProductName}-{logType}.json")
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticSearchUri))
                    {
                        AutoRegisterTemplate = true,
                        CustomFormatter = new JsonFormatter(jsonFormatter),
                        TypeName = $"{elasticSearchIndexTypeName}-{logType}",
                        IndexFormat = $"{elasticSearchIndexTypeName}-{logType}-{{0:yyy.MM.dd}}",
                        ModifyConnectionSettings = (c) =>
                        {
                            var elasticSearchAuth = ConfigReader.ElasticSearchAuthDetails?.Split(':');
                            if (elasticSearchAuth?.Length == 2)
                            {
                                return c.BasicAuthentication(elasticSearchAuth[0], elasticSearchAuth[1]);
                            }
                        
                            return c;
                        }
                    });
            }

            return null;
        }

        private static LoggerConfiguration GetLoggerConfiguration(string logType, string jsonFormatter)
        {
            var elasticSearchUri = ConfigReader.ElasticSearchUri;
            var elasticSearchIndexTypeName = ConfigReader.ElasticSearchIndexTypeName;

            if (!string.IsNullOrEmpty(elasticSearchUri) && !string.IsNullOrEmpty(elasticSearchIndexTypeName))
            {
                return new LoggerConfiguration()
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticSearchUri))
                    {
                        AutoRegisterTemplate = true,
                        CustomFormatter = new JsonFormatter(jsonFormatter),
                        TypeName = $"{elasticSearchIndexTypeName}-{logType}",
                        IndexFormat = $"{elasticSearchIndexTypeName}-{logType}-{{0:yyy.MM.dd}}",
                        //ModifyConnectionSettings = (c) => c.BasicAuthentication("devgold", "k1b@na")
                        ModifyConnectionSettings = (c) =>
                        {
                            var elasticSearchAuth = ConfigReader.ElasticSearchAuthDetails?.Split(':');
                            if (elasticSearchAuth?.Length == 2)
                            {
                                return c.BasicAuthentication(elasticSearchAuth[0], elasticSearchAuth[1]);
                            }
                        
                            return c;
                        }
                    });
            }

            return null;
        }

        #endregion
    }
}
