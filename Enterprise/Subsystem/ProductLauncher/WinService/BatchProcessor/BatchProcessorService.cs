using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository;
using Serilog;
using Serilog.Events;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor
{
    public partial class BatchProcessorService : ServiceBase
    {
        #region Private Constants & Variables

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string _dbServerName = "";

        // batch related config
        private static readonly int ThreadCount = int.Parse(ConfigReader.GetThreadCount);
        private static readonly int BatchSize = int.Parse(ConfigReader.GetBatchSize);
        private static readonly int PollingInterval = int.Parse(ConfigReader.GetPollingInterval);
        private static readonly int RetryPollingInterval = int.Parse(ConfigReader.GetRetryPollingInterval);
        private static readonly int ExceptionWaitInterval = int.Parse(ConfigReader.GetExceptionWaitInterval);

        // Define a waiting interval between each database polling
        private readonly TimeSpan _waitAfterSuccessInterval = new TimeSpan(0, 0, PollingInterval);

        // Define a waiting interval between each database polling
        private readonly TimeSpan _waitForRetryInterval = new TimeSpan(0, 0, RetryPollingInterval);

        // Define a waiting interval if any errors happens
        private readonly TimeSpan _waitAfterErrorInterval = new TimeSpan(0, 0, ExceptionWaitInterval);

        #endregion

        #region Ctor

        public BatchProcessorService()
        {
            InitializeComponent();
            var connectionString = (ConfigurationManager.ConnectionStrings["IdpConfigurationDb"].ConnectionString);
            if (!string.IsNullOrEmpty(connectionString))
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                // Retrieve the DataSource property.    
                _dbServerName = $"{builder.DataSource}.  Db - {builder.InitialCatalog}";
            }
        }

        #endregion

        #region Windows Service Start-Stop

        public void Start()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Information($"Batch Processor Windows Service Starting... Database used -{_dbServerName}");

                Task assignPendingProductTask = new Task(RunPendingProcess, _cts.Token, TaskCreationOptions.LongRunning);
                assignPendingProductTask.Start();

                Log.Information("Launched polling task...");

                Task assignRetryProductsTask = new Task(RunRetryProcess, _cts.Token, TaskCreationOptions.LongRunning);
                assignRetryProductsTask.Start();

                Log.Information("Launched retry polling task...");
#if (DEBUG)
                Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            }
            catch (Exception ex)
            {
                // Log the exception.                
                Log.Error(ex, "Exception in OnStart task.");
            }
        }

        public new void Stop()
        {
            OnStop();
        }

        protected override void OnStop()
        {            
            Log.Information("Batch Processor Windows Service Stopping...");
            _cts.Cancel();
        }

        #endregion

        #region Retry-Polling Tasks-Threads

        private void RunRetryProcess()
        {            
            Log.Information($"RunRetryProcess - Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {RetryPollingInterval}");
            TimeSpan interval = TimeSpan.Zero;
            CancellationToken cancellation = _cts.Token;
            IList<Batch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    //Logger.Write(LogType.Diagnostic, new LogDetails { Message = "RunRetryProcess - Getting product batch to retry process." });
                    Log.Verbose("RunRetryProcess - Getting product batch to retry process.");

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetBatchToProcess(BatchSize, true);

                    if (batch == null || batch.Count <= 0)
                    {
                        //Logger.Write(LogType.Diagnostic, new LogDetails { Message = "RunRetryProcess - No items to process in the batch." });
                        Log.Verbose("RunRetryProcess - No items to process in the batch.");
                        interval = _waitForRetryInterval;
                        continue;
                    }

                    //Logger.Write(LogType.Diagnostic, new LogDetails { Message = $"RunRetryProcess - Launching threads to process {batch.Count} products." });
                    Log.Verbose($"RunRetryProcess - Launching threads to process {batch.Count} products.");

                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessBatchRecord);

                    //Logger.Write(LogType.Diagnostic, new LogDetails { Message = "RunRetryProcess - All threads processed." });
                    Log.Verbose("RunRetryProcess - All threads processed." );

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information( "RunRetryProcess - Thread cancellation requested." );
                        break;
                    }

                    interval = _waitForRetryInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception.
                    Log.Error(ex, "RunRetryProcess - Exception in main task.");

                    // update complete batch with error status
                    if (batch != null && batch.Count > 0)
                    {
                        Exception realError = ex;
                        while (realError.InnerException != null)
                            realError = realError.InnerException;

                        new BatchRepository().UpdateBatch(batch, BatchStatusType.Error, null, realError.Message);
                    }

                    interval = _waitAfterErrorInterval;
                }
            }
        }

        #endregion

        #region Normal Polling Tasks-Threads

        private void RunPendingProcess()
        {
            //Logger.Write(LogType.Diagnostic, new LogDetails { Message = $"RunPendingProcess - Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}" });
            Log.Verbose($"RunPendingProcess - Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}");
#if (DEBUG)
            Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            CancellationToken cancellation = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            IList<Batch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    //Logger.Write(LogType.Diagnostic, new LogDetails { Message = "RunPendingProcess - Getting product batch to process." });
                    Log.Verbose("RunPendingProcess - Getting product batch to process.");

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetBatchToProcess(BatchSize, false);

                    if (batch == null || batch.Count <= 0)
                    {
                        //Logger.Write(LogType.Diagnostic, new LogDetails { Message = "RunPendingProcess - No items to process in the batch." });
                        Log.Verbose("RunPendingProcess - No items to process in the batch.");
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    //Logger.Write(LogType.Diagnostic, new LogDetails { Message = $"RunPendingProcess - Launching threads to process {batch.Count} products." });
                    Log.Verbose($"RunPendingProcess - Launching threads to process {batch.Count} products.");

                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessBatchRecord);

                    Log.Verbose("RunPendingProcess - All threads processed.");

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("RunPendingProcess - Thread cancellation requested.");
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception. 
                    Log.Error(ex, "RunPendingProcess - Exception in main task.");

                    // update complete batch with error status
                    if (batch != null && batch.Count > 0)
                    {
                        Exception realError = ex;
                        while (realError.InnerException != null)
                            realError = realError.InnerException;

                        new BatchRepository().UpdateBatch(batch, BatchStatusType.Error, null, realError.Message);
                    }

                    interval = _waitAfterErrorInterval;
                }
            }
        }

        #endregion

        #region Assign Product by calling API

        private void CallApiToProcessBatchRecord(Batch batch)
        {
            var additionalInfo = new Dictionary<string, object>();

            try
            {
                var processEndpoint = GetBatchConfigurationByType(batch.BatchProcessTypeId, ConfigurationType.ProcessApiEndpoint.ToString());

                additionalInfo = new Dictionary<string, object>
                {
                    {"CorrelationId", batch.CorrelationId},
                    {"EditorUserPersonaId", batch.EditorUserPersonaId},
                    {"SubjectUserPersonaId", batch.SubjectUserPersonaId},
                    {"EditorUserRealPageId", batch.EditorUserRealPageId},
                    {"BatchProcessorId", batch.BatchProcessorId},
                    {"ProductId", batch.ProductId},
                    {"RetryCount", batch.RetryCount},
                    {"StatusTypeId", batch.StatusTypeId},
                    {"BatchProcessTypeId",batch.BatchProcessTypeId},
                    {"InputJson", batch.InputJson},
                    {"processEndpoint",processEndpoint }
                };

                //Logger.Write(LogType.Diagnostic, new LogDetails
                //{
                //    Message = $"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}.",
                //    AdditionalInfo = additionalInfo
                //});

                Log.Write(LogEventLevel.Verbose, $"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}.", additionalInfo);

                var input = new BatchProcessorInput
                {
                    RealPageId = batch.EditorUserRealPageId,
                    ProductBatchId = batch.BatchProcessorId,
                    CreateUserPersonaId = batch.EditorUserPersonaId,
                    AssignUserPersonaId = batch.SubjectUserPersonaId,
                    ProductName = batch.ProductId,
                    InputJson = batch.InputJson,
                    CorrelationId = batch.CorrelationId,
                    BatchProcessType = batch.BatchProcessTypeId,
                    ProcessApiEndPoint = processEndpoint
                };

                var landingApiCaller = new ProductApiCaller();
                var result = landingApiCaller.ProcessBatchRecord(input);

                Log.Information($"CallApiToAssignProducts-Result received for Product {input.ProductName} & for User {batch.SubjectUserPersonaId} - {result.Result}. Calling API Completed to assign product {input.ProductName} to user {batch.SubjectUserPersonaId }.");
            }
            catch (Exception ex)
            {
                // Log the exception. 
                Log.Error(ex, $"Exception while calling API for ProductId {batch.ProductId}.", additionalInfo);
                // update a batch records with error status
                if (batch.BatchProcessorId > 0)
                {
                    Exception realError = ex;
                    while (realError.InnerException != null)
                        realError = realError.InnerException;

                    new BatchRepository().UpdateBatchRecord(batch.BatchProcessorId, BatchStatusType.Error, null, realError.Message);
                }
            }
        }

        private string GetBatchConfigurationByType(int batchBatchProcessTypeId, string configurationTypeName)
        {
            //Logger.Write(LogType.Diagnostic, new LogDetails
            //{
            //    Message = $"GetBatchConfigurationByType - Get information for batchBatchProcessTypeId {batchBatchProcessTypeId}"
            //});

            Log.Debug($"GetBatchConfigurationByType - Get information for batchBatchProcessTypeId {batchBatchProcessTypeId}");

            var batchConfigList = new BatchRepository().GetBatchConfigurations();

            if (batchConfigList == null || batchConfigList.Count == 0)
            {
                throw new Exception($"GetBatchConfigurationByType - No configs received from database.");
            }

            string endpoint = batchConfigList.FirstOrDefault(x => x.BatchProcessTypeId == batchBatchProcessTypeId && x.ConfigurationTypeName == configurationTypeName)?.Value;

            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception($"GetBatchConfigurationByType - No endpoint received for batchBatchProcessTypeId {batchBatchProcessTypeId}");
            }

            //Logger.Write(LogType.Diagnostic, new LogDetails
            //{
            //    Message = $"GetBatchConfigurationByType - Endpoint received for {batchBatchProcessTypeId} - {endpoint}"
            //});

            Log.Verbose($"GetBatchConfigurationByType - Endpoint received for {batchBatchProcessTypeId} - {endpoint}");

            return endpoint;
        }

        #endregion
    }
}