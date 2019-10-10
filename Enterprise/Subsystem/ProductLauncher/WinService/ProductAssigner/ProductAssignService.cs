using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Model;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Repository;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner
{
    public partial class ProductAssignService : ServiceBase
    {
        #region Private Variables

        private CancellationTokenSource _cts = new CancellationTokenSource();

        // batch related config
        static readonly int ThreadCount = int.Parse(ConfigReader.GetThreadCounth);
        static readonly int BatchSize = int.Parse(ConfigReader.GetBatchSize);
        static readonly int PollingInterval = int.Parse(ConfigReader.GetPollingInterval);
        static readonly int RetryPollingInterval = int.Parse(ConfigReader.GetRetryPollingInterval);
        static readonly int ExceptionWaitInterval = int.Parse(ConfigReader.GetExceptionWaitInterval);

        // Define a waiting interval between each database polling
        readonly TimeSpan _waitAfterSuccessInterval = new TimeSpan(0, 0, PollingInterval);

        // Define a waiting interval between each database polling
        readonly TimeSpan _waitForRetryInterval = new TimeSpan(0, 0, RetryPollingInterval);

        // Define a waiting interval if any errors happens
        readonly TimeSpan _waitAfterErrorInterval = new TimeSpan(0, 0, ExceptionWaitInterval);

        #endregion

        #region Ctor

        public ProductAssignService()
        {
            InitializeComponent();
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
                Log.Write(LogType.Information, new LogDetails { Message = "Product Assigner Windows Service Starting..." });

                Task assignPendingProductTask = new Task(AssignPendingProduct, _cts.Token, TaskCreationOptions.LongRunning);
                assignPendingProductTask.Start();

                Log.Write(LogType.Information, new LogDetails { Message = "Launched polling task..." });

                Task assignRetryProductsTask = new Task(AssignRetryProducts, _cts.Token, TaskCreationOptions.LongRunning);
                assignRetryProductsTask.Start();

                Log.Write(LogType.Information, new LogDetails { Message = "Launched retry polling task..." });

                Logger.ConsoleOut("Threads started");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails { Message = "Exception in OnStart task." });
                Log.Write(LogType.Error, new LogDetails { Exception = ex });
                Logger.ConsoleOut(ex.Message);
            }
        }

        public new void Stop()
        {
            OnStop();
        }

        protected override void OnStop()
        {
            Log.Write(LogType.Information, new LogDetails { Message = "Product Assigner Windows Service Stopping..." });
            _cts.Cancel();
        }

        #endregion

        #region Retry-Polling Tasks-Threads

        private void AssignRetryProducts()
        {
            Log.Write(LogType.Information, new LogDetails { Message = $"AssignRetryProducts - Starting in polling main task :threadCount{ThreadCount}, batchSize{BatchSize}, pollingInterval{RetryPollingInterval}" });

            TimeSpan interval = TimeSpan.Zero;
            CancellationToken cancellation = _cts.Token;
            IList<ProductBatch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    Log.Write(LogType.Information, new LogDetails { Message = "AssignRetryProducts - Getting product batch to retry process." });

                    // Get Db data by batchSize
                    var repository = new ProductBatchRepository();
                    batch = repository.GetProductBatchToProcess(BatchSize, true);

                    if (batch == null || batch.Count <= 0)
                    {
                        Logger.ConsoleOut("AssignRetryProducts - No batch to process.");
                        Log.Write(LogType.Information, new LogDetails { Message = "AssignRetryProducts - No items to process in the batch." });
                        interval = _waitForRetryInterval;
                        continue;
                    }

                    Logger.ConsoleOut($"AssignRetryProducts - Launching threads to process {batch.Count} producs.");

                    Log.Write(LogType.Information, new LogDetails { Message = $"AssignRetryProducts - Launching threads to process {batch.Count} producs." });

                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToAssignProducts);

                    Log.Write(LogType.Information, new LogDetails { Message = "AssignRetryProducts - All threads processed." });

                    Logger.ConsoleOut("AssignRetryProducts - All threads processed.");

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Write(LogType.Information, new LogDetails { Message = "AssignRetryProducts - Thread cancellation requested." });
                        break;
                    }

                    interval = _waitForRetryInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception.
                    Log.Write(LogType.Information, new LogDetails { Message = "AssignRetryProducts - Exception in main task." });
                    Log.Write(LogType.Error, new LogDetails { Exception = ex });

                    Logger.ConsoleOut(ex.InnerException != null
                        ? $"AssignRetryProducts - {ex.InnerException.Message}"
                        : $"AssignRetryProducts - {ex.Message}");

                    // update complete batch with error status
                    if (batch != null && batch.Count > 0)
                    {
                        new ProductBatchRepository().UpdateBatch(batch, BatchStatusType.Error, null, ex.Message);
                    }

                    interval = _waitAfterErrorInterval;
                }
            }
        }

        #endregion

        #region Normal Polling Tasks-Threads

        private void AssignPendingProduct()
        {
            Log.Write(LogType.Information, new LogDetails { Message = $"AssignPendingProduct - Starting in polling main task :threadCount{ThreadCount}, batchSize{BatchSize}, pollingInterval{PollingInterval}" });

            CancellationToken cancellation = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            IList<ProductBatch> batch = null;

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    Log.Write(LogType.Information, new LogDetails { Message = "AssignPendingProduct - Getting product batch to process." });

                    // Get Db data by batchSize
                    var repository = new ProductBatchRepository();
                    batch = repository.GetProductBatchToProcess(BatchSize, false);

                    if (batch == null || batch.Count <= 0)
                    {
                        Logger.ConsoleOut("AssignRetryProducts - No batch to process.");
                        Log.Write(LogType.Information, new LogDetails { Message = "AssignPendingProduct - No items to process in the batch." });
                        interval = _waitAfterSuccessInterval;
                        continue;
                    }

                    Log.Write(LogType.Information, new LogDetails { Message = $"AssignPendingProduct - Launching threads to process {batch.Count} producs." });

                    // Launch threads
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, CallApiToAssignProducts);

                    Log.Write(LogType.Information, new LogDetails { Message = "AssignPendingProduct - All threads processed." });

                    Logger.ConsoleOut("AssignPendingProduct - All threads processed.");

                    // Occasionally check the cancellation state.
                    if (cancellation.IsCancellationRequested)
                    {
                        Log.Write(LogType.Information, new LogDetails { Message = "AssignPendingProduct - Thread cancellation requested." });
                        break;
                    }
                    interval = _waitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    // Log the exception.
                    Log.Write(LogType.Information, new LogDetails { Message = "AssignPendingProduct - Exception in main task." });
                    Log.Write(LogType.Error, new LogDetails { Exception = ex });
                    Logger.ConsoleOut(ex.InnerException != null
                        ? $"AssignPendingProduct - {ex.InnerException.Message}"
                        : $"AssignPendingProduct - {ex.Message}");

                    // update complete batch with error status
                    if (batch != null && batch.Count > 0)
                    {
                        new ProductBatchRepository().UpdateBatch(batch, BatchStatusType.Error, null, ex.Message);
                    }

                    interval = _waitAfterErrorInterval;
                }
            }
        }

        #endregion

        #region Assign Product by calling API

        private void CallApiToAssignProducts(ProductBatch batch)
        {
            var additionalInfo = new Dictionary<string, object>();

            try
            {
                additionalInfo = new Dictionary<string, object>
                {
                    {"CreateUserRealPageId", batch.RealPageId},
                    {"AssignUserPersonaId", batch.AssignUserPersonaId},
                    {"CreateUserPersonaId", batch.CreateUserPersonaId},
                    {"ProductBatchId", batch.ProductBatchId},
                    {"ProductId", batch.ProductId},
                    {"ProductName", ((ProductEnum)batch.ProductId).ToString()},
                    {"RetryCount", batch.RetryCount},
                    {"StatusId", batch.StatusTypeId}
                };

                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToAssignProducts-Working to assign product {batch.ProductId} to user {batch.AssignUserPersonaId}.",
                    AdditionalInfo = additionalInfo
                });

                var input = new ProductUserProperitiesRoles
                {
                    RealPageId = batch.RealPageId,
                    ProductBatchId = batch.ProductBatchId,
                    CreateUserPersonaId = batch.CreateUserPersonaId,
                    AssignUserPersonaId = batch.AssignUserPersonaId,
                    ProductName = (ProductEnum)batch.ProductId,
                    InputJson = batch.InputJson
                };

                var landingApiCaller = new LandingApiCaller();
                var result = landingApiCaller.CreateProductUser(input);

                Logger.ConsoleOut($"CallApiToAssignProducts-Result received for Product {input.ProductName} & for User {batch.AssignUserPersonaId} - {result.Result}.");
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToAssignProducts-Result received for Product {input.ProductName} & for User {batch.AssignUserPersonaId} - {result.Result}. Calling API Completed to assign product {input.ProductName} to user {batch.AssignUserPersonaId}.",
                    AdditionalInfo = additionalInfo
                });

                Logger.ConsoleOut($"CallApiToAssignProducts-Calling API Completed to assign product {input.ProductName} to user {batch.AssignUserPersonaId}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails { Message = "Exception while calling API for product {}.", AdditionalInfo = additionalInfo });
                Log.Write(LogType.Error, new LogDetails { Exception = ex, AdditionalInfo = additionalInfo });
                Logger.ConsoleOut(ex.InnerException != null
                        ? $"CallApiToAssignProducts - {ex.InnerException.Message}"
                        : $"CallApiToAssignProducts - {ex.Message}");
                // update a batch records with error status
                if (batch != null && batch.ProductBatchId > 0)
                {
                    new ProductBatchRepository().UpdateBatchRecord(batch.ProductBatchId, BatchStatusType.Error, null, ex.Message);
                }
            }
        }

        #endregion
    }
}
