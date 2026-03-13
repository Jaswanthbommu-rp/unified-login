using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Model;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Repository;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification
{
    public partial class UserNotificationService : ServiceBase
    {
        static readonly string _productName = "User Service Multi Company";

        private System.ComponentModel.IContainer components;
        private Timer _timer;
        private readonly FeatureFlagService _featureFlagService = new FeatureFlagService();

        private const string UseCoreApiV2FlagKey = "use-core-api-v2-for-service";

        static readonly int _threadCount = int.Parse(ConfigReader.GetThreadCount);
        static readonly int _batchSize = int.Parse(ConfigReader.GetBatchSize);
        static readonly int _interval = int.Parse(ConfigReader.GetCallDuration);
        static readonly string _ScheduledTime = ConfigReader.GetScheduledTime;

        public UserNotificationService()
        {
            InitializeComponent();
            _timer = new Timer(60000);
        }

        public void Start()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Information($"{_productName} Windows Service Starting...");

                _timer.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
                _timer.AutoReset = true;
                _timer.Enabled = true;
                Logger.ConsoleOut("Threads started");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Information($"{_productName} Exception in OnStart task.");
                Log.Error(ex, ex.Message);
                Logger.ConsoleOut(ex.Message);
            }
        }

        public new void Stop()
        {
            OnStop();
        }

        protected override void OnStop()
        {
            Log.Information($"{_productName} Windows Service Stopped...");
            _timer.AutoReset = false;
            _timer.Enabled = false;
            _timer.Dispose();
            _featureFlagService?.Dispose();
            Logger.ConsoleOut("Threads stopped");
        }

        private void SendRegularUserNotification()
        {
            if (_featureFlagService.GetBoolFlag(UseCoreApiV2FlagKey))
            {
                Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "SendRegularUserNotification", $"Skipping: '{UseCoreApiV2FlagKey}' flag is enabled. Core API v2 handles this process." });
                return;
            }

            string correlationId = Guid.NewGuid().ToString();
            try
            {
                Log.Information($"SendRegularUserNotification - Getting user list to process. CorrelationId = " + correlationId);

                // Get Db data by batchSize
                var repository = new UserListRepository();
                var usersList = repository.GetFutureUsersToProcess(_batchSize);

                if (usersList == null || usersList.Count <= 0)
                {
                    Logger.ConsoleOut($"{_productName} - No users to process.");
                    Log.Information($"SendRegularUserNotification - No Users to process in the batch. CorrelationId = " + correlationId);
                    return;
                }

                Logger.ConsoleOut($"SendRegularUserNotification - Launching threads to process {usersList.Count} users.");
                Log.Information($"SendRegularUserNotification - Launching threads to process {usersList.Count} users. CorrelationId =" + correlationId);

                var splitUserList = SplitList<ProcessUserLogin>(usersList, 20);

                // Launch threads
                Parallel.ForEach(splitUserList, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, CallApiToSendNotification);

                Log.Information($"SendRegularUserNotification - All threads processed. CorrelationId = " + correlationId);
                Logger.ConsoleOut($"{_productName} - All threads processed.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Information($"SendRegularUserNotification - Exception in main task. CorrelationId = " + correlationId);
                Log.Error(ex, ex.Message + ". CorrelationId = " + correlationId);

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"SendRegularUserNotification - {ex.InnerException.Message}"
                    : $"SendRegularUserNotification - {ex.Message}");
            }
        }

        private void ProcessPendingUsers()
        {
            if (_featureFlagService.GetBoolFlag(UseCoreApiV2FlagKey))
            {
                Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "ProcessPendingUsers", $"Skipping: '{UseCoreApiV2FlagKey}' flag is enabled. Core API v2 handles this process." });
                return;
            }

            string correlationId = Guid.NewGuid().ToString();
            try
            {
                Log.Information("ProcessPendingUsers - Getting user list to  process. CorrelationId = " + correlationId);

                // Get Db data by batchSize
                var repository = new UserListRepository();
                var usersList = repository.GetPendingUsersToProcess();

                if (usersList?.Count == 0)
                {
                    Logger.ConsoleOut("ProcessPendingUsers - No users to process.");
                    Log.Information("ProcessPendingUsers - No Users to process in the batch. CorrelationId = " + correlationId);
                    return;
                }

                Logger.ConsoleOut($"ProcessPendingUsers - Launching threads to process {usersList.Count} users.");
                Log.Information($"ProcessPendingUsers - Launching threads to process {usersList.Count} users. CorrelationId = " + correlationId);

                var splitUserList = SplitList(usersList, 50);

                // Launch threads
                Parallel.ForEach(splitUserList, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, CallApiToSetPendingToExpireUserStatus);

                Log.Information("ProcessPendingUsers - All threads processed. CorrelationId = " + correlationId);
                Logger.ConsoleOut("ProcessPendingUsers - All threads processed.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Information("ProcessPendingUsers - Exception in main task. CorrelationId = " + correlationId);
                Log.Error(ex, ex.Message + ". CorrelationId = " + correlationId);

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"ProcessPendingUsers - {ex.InnerException.Message}"
                    : $"ProcessPendingUsers - {ex.Message}");
            }
        }

        private void ProcessDisableUsersinProducts()
        {
            if (_featureFlagService.GetBoolFlag(UseCoreApiV2FlagKey))
            {
                Log.Debug("{ActionName} - {state}", propertyValues: new object[] { "ProcessDisableUsersinProducts", $"Skipping: '{UseCoreApiV2FlagKey}' flag is enabled. Core API v2 handles this process." });
                return;
            }

            string correlationId = Guid.NewGuid().ToString();
            try
            {
                Log.Information($"ProcessDisableUsersinProducts - Getting user list to process. CorrelationId = " + correlationId);

                // Get Db data by batchSize
                var repository = new UserListRepository();
                var usersList = repository.GetExpiredUsersToProcess();

                if (usersList?.Count == 0)
                {
                    Logger.ConsoleOut($"ProcessDisableUsersinProducts - No users to process.");
                    Log.Information($"ProcessDisableUsersinProducts - No Users to process in the batch. CorrelationId = " + correlationId);
                    return;
                }

                Logger.ConsoleOut($"ProcessDisableUsersinProducts - Launching threads to process {usersList.Count} users.");
                Log.Information($"ProcessDisableUsersinProducts - Launching threads to process {usersList.Count} users. CorrelationId = " + correlationId);

                var splitUserList = SplitList(usersList, 20);

                // Launch threads
                Parallel.ForEach(splitUserList, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, CallApiToDisableUsers);

                Log.Information($"ProcessDisableUsersinProducts - All threads processed. CorrelationId = " + correlationId);
                Logger.ConsoleOut($"ProcessDisableUsersinProducts - All threads processed.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Information($"ProcessDisableUsersinProducts - Exception in main task. CorrelationId = " + correlationId);
                Log.Error(ex, ex.Message + ". CorrelationId = " + correlationId);

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"ProcessDisableUsersinProducts - {ex.InnerException.Message}"
                    : $"ProcessDisableUsersinProducts - {ex.Message}");
            }
        }

        private void CallApiToSendNotification(List<ProcessUserLogin> userList)
        {
            string correlationId = Guid.NewGuid().ToString();
            var additionalInfo = new Dictionary<string, object>
            {
                {"userList", userList},
                {"userListHash", userList.GetHashCode()}
            };

            try
            {
                var logger = Log.Logger;
				logger = logger.ForContext($"AdditionalInfo", additionalInfo, true);
				logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);
                logger.Information($"CallApiToSendNotification - Working to send notification to {userList.Count} users hashCode: {userList.GetHashCode()}" + ". CorrelationId = " + correlationId);

                var notificationApiCaller = new UserApiCaller();
                var result = notificationApiCaller.ProcessUserLogins(userList);

                logger.Information($"CallApiToSendNotification - Result received for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}. CorrelationId = " + correlationId);

                Logger.ConsoleOut($"CallApiToSendNotification - Calling API Completed for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Information(
                    $"CallApiToSendNotification - Exception while calling API for product hashCode: {userList.GetHashCode()}" + ". CorrelationId = " + correlationId,
                    additionalInfo);

                //Log.ForContext("AdditionalInfo", additionalInfo).Write(LogEventLevel.Error, ex, ex.Message + ".CorrelationId = " + correlationId);
                var logger = Log.Logger;
				if (additionalInfo?.Keys != null)
				{
					logger = logger.ForContext($"AdditionalInfo", additionalInfo, true);
				}
				logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);
                logger.Write(LogEventLevel.Error, ex, ex.Message + ".CorrelationId = " + correlationId);

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"CallApiToSendNotification - {ex.InnerException.Message}"
                    : $"CallApiToSendNotification - {ex.Message}");
            }
        }

        private void CallApiToSetPendingToExpireUserStatus(List<ProcessUserLogin> userList)
        {
            string correlationId = Guid.NewGuid().ToString();
            var logger = Log.Logger;

            var additionalInfo = new Dictionary<string, object>
            {
                {"userList", userList},
                {"userListHash", userList.GetHashCode()}
            };

            try
            {
                logger = logger.ForContext("AdditionalInfo", additionalInfo);
                logger.Information($"CallApiToSetPendingToExpireUserStatus - Working to set status to {userList.Count} users hashCode: {userList.GetHashCode()}" + ". CorrelationId = " + correlationId, additionalInfo);

                var apiCaller = new UserApiCaller();
                var result = apiCaller.ProcessUserLogins(userList);

                if (result.Result == null)
                {
                    throw new Exception("CallApiToSetPendingToExpireUserStatus - Null response posting to API");
                }

                logger.Information($"CallApiToSetPendingToExpireUserStatus - Result received for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}" + ".CorrelationId = " + correlationId, additionalInfo);

                Logger.ConsoleOut($"CallApiToSetPendingToExpireUserStatus - Calling API Completed for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                
                logger = logger.ForContext("CorrelationId", correlationId);
                logger.Write(LogEventLevel.Error, ex, ex.Message);
                
                logger.Information($"CallApiToSetPendingToExpireUserStatus - Exception while calling API for product hashCode: {userList.GetHashCode()}" + ".CorrelationId = " + correlationId);
                Logger.ConsoleOut(ex.InnerException != null
                    ? $"CallApiToSetPendingToExpireUserStatus - {ex.InnerException.Message}"
                    : $"CallApiToSetPendingToExpireUserStatus - {ex.Message}");
            }
        }

        private void CallApiToDisableUsers(List<ProcessUserLogin> userList)
        {
            string correlationId = Guid.NewGuid().ToString();
            var additionalInfo = new Dictionary<string, object>
            {
                {"userList", userList},
                {"userListHash", userList.GetHashCode()}
            };

            try
            {
                Log.Information($"CallApiToDisableUsers - Working to disable user to {userList.Count} users hashCode: {userList.GetHashCode()}"+ ".CorrelationId = " + correlationId, additionalInfo);

                var disableUserApiCaller = new UserApiCaller();
                var result = disableUserApiCaller.DisableExpiredUsers(userList);

                Log.Information($"CallApiToDisableUsers - Result received for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}" + ".CorrelationId = " + correlationId);

                Logger.ConsoleOut($"CallApiToDisableUsers - Calling API Completed for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Information("CallApiToDisableUsers - Exception while calling API for product {}" + ".CorrelationId = " + correlationId, additionalInfo);

                Log.ForContext("AdditionalInfo", additionalInfo).Write(LogEventLevel.Error, ex, ex.Message + ".CorrelationId = " + correlationId);

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"CallApiToDisableUsers - {ex.InnerException.Message}"
                    : $"CallApiToDisableUsers - {ex.Message}");
            }
        }

        private void ServiceTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Information($"{_productName} process started..." );
            string _CurrentTime = String.Format("{0:t}", DateTime.UtcNow);

            if (Environment.UserInteractive || DateTime.Now.Minute % 15 == 0)
            {
                SendRegularUserNotification();
                ProcessPendingUsers();
            }

            if (Environment.UserInteractive || _CurrentTime == _ScheduledTime)
            {
                ProcessDisableUsersinProducts();
            }


            _timer.Stop();
            SetTimer();
        }

        private void SetTimer()
        {
            try
            {
                _timer.Interval = _interval;
                _timer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
