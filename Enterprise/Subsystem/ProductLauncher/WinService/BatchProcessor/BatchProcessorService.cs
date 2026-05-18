using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor
{
    public partial class BatchProcessorService : ServiceBase
    {
        #region Private Constants & Variables

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string _dbServerName = "";
        private readonly FeatureFlagService _featureFlagService = new FeatureFlagService();

        private const string UseCoreApiV2FlagKey = "use-core-api-v2-for-service";

        // batch related config
        private int ThreadCount = int.Parse(ConfigReader.GetThreadCount);
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
                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStart", $"Batch Processor Windows Service Starting... Database used -{_dbServerName}" });

                Task assignPendingProductTask = new Task(RunPendingProcess, _cts.Token, TaskCreationOptions.LongRunning);
                assignPendingProductTask.Start();

                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStart", "Launched retry polling task..." });

                Task assignRetryProductsTask = new Task(RunRetryProcess, _cts.Token, TaskCreationOptions.LongRunning);
                assignRetryProductsTask.Start();

                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStart", "Launched Enterprise Role Update task..." });

                Task enterpriseRoleProductUpdateTask = new Task(RunEnterpriseRoleUpdateProcess, _cts.Token, TaskCreationOptions.LongRunning);
                enterpriseRoleProductUpdateTask.Start();

                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStart", "Launched Primary Properties Update task..." });

                Task primaryPropertyProductUpdateTask = new Task(RunPrimaryPropertiesUpdateProcess, _cts.Token, TaskCreationOptions.LongRunning);
                primaryPropertyProductUpdateTask.Start();

                Task bulkUsersUpdateTask = new Task(RunBulkUserUpdateProcess, _cts.Token, TaskCreationOptions.LongRunning);
                bulkUsersUpdateTask.Start();

                // Update associated properties in company

                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStart", "Company and Properties Update task..." });
                Task companyAndPropertiesUpdateTask = new Task(RunCompanyAndPropertiesUpdateProcess, _cts.Token, TaskCreationOptions.LongRunning);
                companyAndPropertiesUpdateTask.Start();

                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStart", "Launched Bulk Reset Password task..." });
                Task bulkResetPasswordTask = new Task(RunBulkResetPasswordProcess, _cts.Token, TaskCreationOptions.LongRunning);
                bulkResetPasswordTask.Start();

                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStart", "Launched enterprise role product update polling task..." });
#if (DEBUG)
                Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            }
            catch (Exception ex)
            {
                // Log the exception.                
                Log.Error(ex, "{ActionName} - {state}", new object[] { "OnStart", "Exception in OnStart task." });
            }
        }

        public new void Stop()
        {
            OnStop();
        }

        protected override void OnStop()
        {
            Log.Information("{ActionName} - {state}", propertyValues: new object[] { "OnStop", "Batch Processor Windows Service Stopping..." });
            _cts.Cancel();
            _featureFlagService?.Dispose();
        }

        #endregion

        #region Retry-Polling Tasks-Threads

        private void RunRetryProcess()
        {
            Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunRetryProcess", $"Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {RetryPollingInterval}" });
            TimeSpan interval = TimeSpan.Zero;
            CancellationToken cancellation = _cts.Token;
            IList<Batch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunRetryProcess", "Getting product batch to retry process." });

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetBatchToProcess(BatchSize, true);

                    if (batch == null || batch.Count <= 0)
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunRetryProcess", "No items to process in the batch." });
                        interval = _waitForRetryInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunRetryProcess", $"Launching threads to process {batch.Count} products." });

                    ThreadCount = GetProductInternalSettings("BatchProcessorRetryThread");

                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessBatchRecord);

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunRetryProcess", "All threads processed." });

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunRetryProcess", "Thread cancellation requested." });
                        break;
                    }

                    interval = _waitForRetryInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception.
                    Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "RunRetryProcess", "RunRetryProcess - Exception in main task." });

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
            Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPendingProcess", $"Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}" });
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
                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPendingProcess", "Getting product batch to process." });

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetBatchToProcess(BatchSize, false);

                    if (batch == null || batch.Count <= 0)
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPendingProcess", "No items to process in the batch." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPendingProcess", $"Launching threads to process {batch.Count} products." });

                    ThreadCount = GetProductInternalSettings("BatchProcessorPendingThread");
                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessBatchRecord);

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPendingProcess", "All threads processed." });

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunPendingProcess", "Thread cancellation requested." });
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception. 
                    Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "RunPendingProcess", "Exception in main task." });

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

        #region Company and Properties Update Tasks-Threads
        private void RunCompanyAndPropertiesUpdateProcess()
        {
            Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", $"Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}" });
#if (DEBUG)
            Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            CancellationToken cancellation = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            IList<CompanyPropertyBatch> batch = null;
            
            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    if (_featureFlagService.GetBoolFlag(UseCoreApiV2FlagKey))
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", $"Skipping: '{UseCoreApiV2FlagKey}' flag is enabled. Core API v2 handles this process." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", "Getting CompanyandProperties batch to process." });

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetCompanyBatchByStatus(BatchSize, BatchStatusType.Waiting);
                    if (batch == null || batch.Count <= 0)
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", "No items to process in the batch." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", $"Launching threads to process {batch.Count} companies." });
                    ThreadCount = GetProductInternalSettings("BatchProcessorPendingThread");
                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessCompanyBatchRecord);

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", "All threads processed." });
                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", "Thread cancellation requested." });
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception. 
                    Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "RunCompanyAndPropertiesUpdateProcess", "Exception in main task." });

                    // update complete batch with error status
                    if (batch != null && batch.Count > 0)
                    {
                        Exception realError = ex;
                        while (realError.InnerException != null)
                            realError = realError.InnerException;

                        new BatchRepository().UpdateCompanyPropertyBatches(batch, BatchStatusType.Error);
                    }

                    interval = _waitAfterErrorInterval;
                }
            }
        }
        #endregion

        #region Bulk Reset Password Task-Thread

        /// <summary>
        /// Polling task that picks up unprocessed rows from [Batch].[BulkResetPassword]
        /// (Status = 0) and runs the existing single-user ClearPasswordAndQuestions logic
        /// in-process for each one. On completion (success or email failure) the row's
        /// Status is set to 1. No retry — single-instance deployment assumed.
        /// </summary>
        private void RunBulkResetPasswordProcess()
        {
            Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkResetPasswordProcess", $"Starting polling loop: threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}" });
#if (DEBUG)
            Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            CancellationToken cancellation = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            IList<BulkResetPasswordBatch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkResetPasswordProcess", "Polling for pending bulk reset password rows." });

                    var repository = new BatchRepository();
                    batch = repository.GetPendingBulkResetPassword(BatchSize);
                    if (batch == null || batch.Count <= 0)
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkResetPasswordProcess", "No pending rows to process." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkResetPasswordProcess", $"Launching threads to process {batch.Count} rows." });
                    ThreadCount = GetProductInternalSettings("BatchProcessorPendingThread");

                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, ProcessBulkResetPasswordRecord);

                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunBulkResetPasswordProcess", "Thread cancellation requested." });
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "RunBulkResetPasswordProcess", "Exception in main polling loop." });

                    // Mark the in-flight batch as processed (Status=1) to avoid infinite re-pickup.
                    // BIT status has no Error state — failures are recorded in Serilog only.
                    if (batch != null && batch.Count > 0)
                    {
                        var repo = new BatchRepository();
                        foreach (var row in batch)
                        {
                            try { repo.UpdateBulkResetPasswordStatus(row.Id, 1); }
                            catch (Exception inner)
                            {
                                Log.Error(inner, "{ActionName} - {state}", propertyValues: new object[] { "RunBulkResetPasswordProcess", $"Failed to mark row {row.Id} as processed during error recovery." });
                            }
                        }
                    }

                    interval = _waitAfterErrorInterval;
                }
            }
        }

        /// <summary>
        /// Per-row processor: looks up the user's primary org, builds a synthetic claim,
        /// invokes the existing ManageUserLogin.ClearPasswordAndQuestions in-process, and
        /// marks the row Status=1.
        /// </summary>
        private void ProcessBulkResetPasswordRecord(BulkResetPasswordBatch row)
        {
            try
            {
                Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "ProcessBulkResetPasswordRecord", $"Processing row Id={row.Id}, RealPageId={row.RealPageId}" });

                var userLoginRepo = new UserLoginRepository();
                var userLoginOnly = userLoginRepo.GetUserLoginOnly(row.RealPageId);
                if (userLoginOnly == null)
                {
                    Log.Warning("{ActionName} - {state}", propertyValues: new object[] { "ProcessBulkResetPasswordRecord", $"UserLogin not found for RealPageId={row.RealPageId}. Marking processed." });
                    new BatchRepository().UpdateBulkResetPasswordStatus(row.Id, 1);
                    return;
                }

                var primaryOrg = userLoginRepo.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);
                if (primaryOrg == null)
                {
                    Log.Warning("{ActionName} - {state}", propertyValues: new object[] { "ProcessBulkResetPasswordRecord", $"Primary org not found for RealPageId={row.RealPageId}. Marking processed." });
                    new BatchRepository().UpdateBulkResetPasswordStatus(row.Id, 1);
                    return;
                }

                // Synthetic claim — system identity, real org context.
                // Per-user activity logs written by ClearPasswordAndQuestions will show
                // "System BulkResetPassword" as the editor; admin attribution is preserved
                // by the bulk-initiated audit entry written at API insert time.
                var claim = new DefaultUserClaim
                {
                    OrganizationPartyId = primaryOrg.PartyId,
                    CorrelationId       = Guid.NewGuid(),
                    FirstName           = "System",
                    LastName            = "BulkResetPassword",
                    ImpersonatedBy      = Guid.Empty
                };

                var manage = new ManageUserLogin(claim);
                bool emailSent = manage.ClearPasswordAndQuestions(row.RealPageId);

                Log.Information("{ActionName} - {state}", propertyValues: new object[] { "ProcessBulkResetPasswordRecord", $"Row Id={row.Id}, RealPageId={row.RealPageId}, emailSent={emailSent}" });

                // Mark processed regardless of email outcome (no retry path for BIT status).
                new BatchRepository().UpdateBulkResetPasswordStatus(row.Id, 1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "ProcessBulkResetPasswordRecord", $"Exception processing row Id={row.Id}, RealPageId={row.RealPageId}" });
                try { new BatchRepository().UpdateBulkResetPasswordStatus(row.Id, 1); }
                catch (Exception inner)
                {
                    Log.Error(inner, "{ActionName} - {state}", propertyValues: new object[] { "ProcessBulkResetPasswordRecord", $"Failed to mark row {row.Id} as processed after exception." });
                }
            }
        }

        #endregion

            #region Enterprise Role Product Update Tasks-Threads

        private void RunEnterpriseRoleUpdateProcess()
        {
            Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", $"Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}" });
#if (DEBUG)
            Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            CancellationToken cancellation = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            IList<EnterpriseRoleBatch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    if (_featureFlagService.GetBoolFlag(UseCoreApiV2FlagKey))
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", $"Skipping: '{UseCoreApiV2FlagKey}' flag is enabled. Core API v2 handles this process." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", "Getting product batch to process." });

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetEnterpriseRoleProductUpdateBatchToProcess(BatchSize);

                    if (batch == null || batch.Count <= 0)
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", "No items to process in the batch." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", $"Launching threads to process {batch.Count} products." });

                    ThreadCount = GetProductInternalSettings("BatchProcessorEnterpriseRoleThread");
                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessEnterpriseRoleBatchRecord);

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", "All threads processed." });

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", "Thread cancellation requested." });
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception. 
                    Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "RunEnterpriseRoleUpdateProcess", "Exception in main task." });

                    interval = _waitAfterErrorInterval;
                }
            }
        }

        #endregion

        #region Primary Properties Product Update Tasks-Threads

        private void RunPrimaryPropertiesUpdateProcess()
        {
            Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", $"Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}" });
#if (DEBUG)
            Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            CancellationToken cancellation = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            IList<PrimaryPropertyBatch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    if (_featureFlagService.GetBoolFlag(UseCoreApiV2FlagKey))
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", $"Skipping: '{UseCoreApiV2FlagKey}' flag is enabled. Core API v2 handles this process." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", "Getting product batch to process." });

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetPrimaryPropertyProductUpdateBatchToProcess(BatchSize);

                    if (batch == null || batch.Count <= 0)
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", "No items to process in the batch." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", $"Launching threads to process {batch.Count} products." });
                    ThreadCount = GetProductInternalSettings("BatchProcessorPrimaryPropertiesThread");
                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessPrimaryPropertiesBatchRecord);

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", "All threads processed." });

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", "Thread cancellation requested." });
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception. 
                    Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "RunPrimaryPropertiesUpdateProcess", "Exception in main task." });

                    interval = _waitAfterErrorInterval;
                }
            }
        }

        #endregion

        #region Bulk user Update batch process  Tasks-Threads

        private void RunBulkUserUpdateProcess()
        {
            Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", $"Starting in polling main task :threadCount - {ThreadCount}, batchSize - {BatchSize}, pollingInterval - {PollingInterval}" });
#if (DEBUG)
            Console.WriteLine("-------------------------------------------------------------------------------");
#endif
            CancellationToken cancellation = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            IList<BulkUserBatch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    if (_featureFlagService.GetBoolFlag(UseCoreApiV2FlagKey))
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", $"Skipping: '{UseCoreApiV2FlagKey}' flag is enabled. Core API v2 handles this process." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", "Getting product batch to process." });

                    // Get Db data by batchSize
                    var repository = new BatchRepository();
                    batch = repository.GetBulkUsersUpdateBatchToProcess(BatchSize);

                    if (batch == null || batch.Count <= 0)
                    {
                        Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", "No items to process in the batch." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", $"Launching threads to process {batch.Count} products." });
                    ThreadCount = GetProductInternalSettings("BatchProcessorBulkThread");
                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToProcessBulkUserBatchRecord);

                    Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", "All threads processed." });

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Information("{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", "Thread cancellation requested." });
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception. 
                    Log.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "RunBulkUserUpdateProcess", "Exception in main task." });

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
                var processEndpoint = GetBatchConfigurationByType(batch.CorrelationId, batch.BatchProcessTypeId, ConfigurationType.ProcessApiEndpoint.ToString());

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

                //Log.Debug($"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", batch.CorrelationId);
                logger.Debug("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessBatchRecord", $"Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}." });

                var input = new BatchProcessorInput
                {
                    RealPageId = batch.EditorUserRealPageId,
                    ProductBatchId = batch.BatchProcessorId,
                    CreateUserPersonaId = batch.EditorUserPersonaId,
                    AssignUserPersonaId = batch.SubjectUserPersonaId,
                    ProductId = batch.ProductId,
                    InputJson = batch.InputJson,
                    CorrelationId = batch.CorrelationId,
                    BatchProcessType = batch.BatchProcessTypeId,
                    ProcessApiEndPoint = processEndpoint,
                    BatchProcessorGroupId = batch.BatchProcessorGroupId,
                    ImpersonatorUserId = batch.ImpersonatorUserId
                };

                var landingApiCaller = new ProductApiCaller();
                var result = landingApiCaller.ProcessBatchRecord(input);

                //Handle records which never comeback and update status to failed
                if(result.Result == null)
                {
                    throw new Exception($"Unable to process the batch request - {batch.BatchProcessorId}. Response is null.");
                }

                logger.Information("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessBatchRecord", $"Result received for Product {input.ProductId} & for User {batch.SubjectUserPersonaId} - {result.Result}. Calling API Completed to assign product {input.ProductId} to user {batch.SubjectUserPersonaId}." });
            }
            catch (Exception ex)
            {
                // Log the exception. 
                //Log.Error(ex, $"Exception while calling API for ProductId {batch.ProductId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", batch.CorrelationId);
                logger.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessBatchRecord", $"Exception while calling API for ProductId {batch.ProductId}." });
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

        private void CallApiToProcessEnterpriseRoleBatchRecord(EnterpriseRoleBatch batch)
        {
            var additionalInfo = new Dictionary<string, object>();

            try
            {
                var processEndpoint = GetBatchConfigurationByType(new Guid(), batch.BatchProcessTypeId, ConfigurationType.EnterpriseRoleProcessApiEndpoint.ToString());

                additionalInfo = new Dictionary<string, object>
                {
                    {"EditorUserPersonaId", batch.EditorUserPersonaId},
                    {"SubjectUserPersonaId", batch.SubjectUserPersonaId},
                    {"BatchProcessorId", batch.EnterpriseRoleBatchProcessId},
                    {"EnterpriseRoleTemplateId", batch.EnterpriseRoleTemplateId},
                    {"StatusTypeId", batch.StatusTypeId},
                    {"BatchProcessTypeId",batch.BatchProcessTypeId},
                    {"CreatedDateTime",batch.CreatedDateTime},
                    {"processEndpoint",processEndpoint }
                };

                //Log.Debug($"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger.Debug("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessEnterpriseRoleBatchRecord", $"Working to assign products to user from enterprise role template {batch.EnterpriseRoleTemplateId} to user {batch.SubjectUserPersonaId}." });

                var landingApiCaller = new ProductApiCaller();
                var result = landingApiCaller.ProcessEnterpriseRoleBatchRecord(batch, processEndpoint);

                logger.Information("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessEnterpriseRoleBatchRecord", $"Result received for User {batch.SubjectUserPersonaId} - {result.Result}." });
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;
                // Log the exception. 
                //Log.Error(ex, $"Exception while calling API for ProductId {batch.ProductId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("InnerException", realError);
                logger.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessEnterpriseRoleBatchRecord", $"Exception while CallApiToProcessEnterpriseRoleBatchRecord {batch.EnterpriseRoleTemplateId}." });
                if (batch.EnterpriseRoleBatchProcessId > 0)
                {
                    new BatchRepository().UpdateEnterpriseRoleProductBatch(batch.EnterpriseRoleBatchProcessId, (int)BatchStatusType.Error);
                }
            }
        }

        private void CallApiToProcessPrimaryPropertiesBatchRecord(PrimaryPropertyBatch batch)
        {
            var additionalInfo = new Dictionary<string, object>();

            try
            {
                var processEndpoint = GetBatchConfigurationByType(new Guid(), batch.BatchProcessTypeId, ConfigurationType.PrimaryPropertiesBulkUpdateApiEndpoint.ToString());

                additionalInfo = new Dictionary<string, object>
                {
                    {"EditorUserPersonaId", batch.EditorUserPersonaId},
                    {"SubjectUserPersonaId", batch.SubjectUserPersonaId},
                    {"BatchProcessorId", batch.PrimaryPropertyBatchProcessId},
                    {"StatusTypeId", batch.StatusTypeId},
                    {"BatchProcessTypeId",batch.BatchProcessTypeId},
                    {"processEndpoint",processEndpoint }
                };

                //Log.Debug($"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger.Debug("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessPrimaryPropertiesBatchRecord", $"Working to update products to user with primary properties {batch.PrimaryPropertyBatchProcessId} to user {batch.SubjectUserPersonaId}." });

                var landingApiCaller = new ProductApiCaller();
                var result = landingApiCaller.ProcessPrimaryPropertyBatchRecord(batch, processEndpoint);

                logger.Information("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessPrimaryPropertiesBatchRecord", $"Result received for User {batch.SubjectUserPersonaId} - {result.Result}." });
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;
                // Log the exception. 
                //Log.Error(ex, $"Exception while calling API for ProductId {batch.ProductId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("InnerException", realError);
                logger.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessPrimaryPropertiesBatchRecord", $"Exception while {batch.PrimaryPropertyBatchProcessId}." });
                if (batch.PrimaryPropertyBatchProcessId > 0)
                {
                    new BatchRepository().UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, (int)BatchStatusType.Error);
                }
            }
        }

        private void CallApiToProcessBulkUserBatchRecord(BulkUserBatch batch)
        {
            var additionalInfo = new Dictionary<string, object>();

            try
            {
                var processEndpoint = GetBatchConfigurationByType(new Guid(), batch.BatchProcessTypeId, ConfigurationType.BulkUserProcessApiEndpoint.ToString());

                additionalInfo = new Dictionary<string, object>
                {
                    {"EditorUserPersonaId", batch.EditorUserPersonaId},
                    {"SubjectUserPersonaId", batch.SubjectUserPersonaId},
                    {"BatchProcessorId", batch.BulkUserBatchProcessId},
                    {"StatusTypeId", batch.StatusTypeId},
                    {"BatchProcessTypeId",batch.BatchProcessTypeId},
                    {"processEndpoint",processEndpoint }
                };

                //Log.Debug($"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger.Debug("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessBulkUserBatchRecord", $"Working to update products to user {batch.BulkUserBatchProcessId} to user {batch.SubjectUserPersonaId}." });

                var landingApiCaller = new ProductApiCaller();
                var result = landingApiCaller.ProcessBulkUserBatchRecord(batch, processEndpoint);

                logger.Information("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessBulkUserBatchRecord", $"Result received for User {batch.SubjectUserPersonaId} - {result.Result}." });
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;
                // Log the exception. 
                //Log.Error(ex, $"Exception while calling API for ProductId {batch.ProductId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("InnerException", realError);
                logger.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessBulkUserBatchRecord", $"Exception while {batch.BulkUserBatchProcessId}." });
                if (batch.BulkUserBatchProcessId > 0)
                {
                    new BatchRepository().UpdateBulkUserBatch(batch.BulkUserBatchProcessId, (int)BatchStatusType.Error);
                }
            }
        }

        private void CallApiToProcessCompanyBatchRecord(CompanyPropertyBatch batch)
        {
            var additionalInfo = new Dictionary<string, object>();

            try
            {
                var processEndpoint = GetBatchConfigurationByType(new Guid(), batch.BatchProcessorTypeId, ConfigurationType.CompanyPropertiesApiEndpoint.ToString());

                additionalInfo = new Dictionary<string, object>
                {
                    {"CompanyBatchJobId", batch.CompanyBatchJobId},
                    {"CompanyInstanceSourceId", batch.CompanyInstanceSourceId},
                    {"IsActive", batch.IsActive},
                    {"CreateUserPersonaId", batch.CreateUserPersonaId},
                    {"BatchProcessorTypeId",batch.BatchProcessorTypeId},
                    {"processEndpoint",processEndpoint }
                };

                //Log.Debug($"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.SubjectUserPersonaId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger.Debug("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessCompanyBatchRecord", $"Working to update products to user {batch.CompanyBatchJobId}." });

                var landingApiCaller = new ProductApiCaller();
                var result = landingApiCaller.ProcessCompanyBatchRecord(batch, processEndpoint);

                logger.Information("{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessCompanyBatchRecord", $"Result received for User {batch.CreateUserPersonaId}" });
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;
                // Log the exception. 
                //Log.Error(ex, $"Exception while calling API for ProductId {batch.ProductId}.", additionalInfo);
                var logger = Log.Logger;
                logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("InnerException", realError);
                logger.Error(ex, "{ActionName} - {state}", propertyValues: new object[] { "CallApiToProcessCompanyBatchRecord", $"Exception while {batch.CompanyBatchJobId}." });
                if (batch.CompanyBatchJobId > 0)
                {
                    new BatchRepository().UpdateCompanyPropertyBatch(batch.CompanyBatchJobId, (int)BatchStatusType.Error);
                }
            }
        }

        private string GetBatchConfigurationByType(Guid correlationId, int batchBatchProcessTypeId, string configurationTypeName)
        {
            var logger = Log.Logger;
            logger = logger.ForContext("CorrelationId", correlationId.ToString());
            logger.Debug("{ActionName} - {state}", propertyValues: new object[] { "GetBatchConfigurationByType", $"Get information for batchBatchProcessTypeId {batchBatchProcessTypeId}" });

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

            logger.Debug("{ActionName} - {state}", propertyValues: new object[] { "GetBatchConfigurationByType", $"Endpoint received for {batchBatchProcessTypeId} - {endpoint}" });

            return endpoint;
        }

        private int GetProductInternalSettings(string key)
        {
            var settings = new BatchRepository().GetProductInternalSettings(3).ToList();
            if(!string.IsNullOrEmpty(settings.FirstOrDefault(a => a.Name.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value))
            {
                return Convert.ToInt32(settings.FirstOrDefault(a => a.Name.Equals(key, StringComparison.OrdinalIgnoreCase)).Value);
            }
            else
            {
                return int.Parse(ConfigReader.GetThreadCount);
            }
        }

        #endregion
    }
}