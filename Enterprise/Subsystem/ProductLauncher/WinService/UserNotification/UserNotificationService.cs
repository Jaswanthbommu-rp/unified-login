using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Model;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Repository;
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

        static readonly string timeString;
        private System.ComponentModel.IContainer components;
        private Timer _timer;

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
                Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} Windows Service Starting..."});

                _timer.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
                _timer.AutoReset = true;
                _timer.Enabled = true;
                Logger.ConsoleOut("Threads started");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} Exception in OnStart task."});
                Log.Write(LogType.Error, new LogDetails {Exception = ex});
                Logger.ConsoleOut(ex.Message);
            }
        }

        public new void Stop()
        {
            OnStop();
        }

        protected override void OnStop()
        {
            Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} Windows Service Stopped..."});
            _timer.AutoReset = false;
            _timer.Enabled = false;
            _timer.Dispose();
            Logger.ConsoleOut("Threads stopped");
        }

        private void SendRegularUserNotification()
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                Log.Write(LogType.Information, new LogDetails {Message = $"SendRegularUserNotification - Getting user list to process.", CorrelationId = correlationId});

                // Get Db data by batchSize
                var repository = new UserListRepository();
                var usersList = repository.GetFutureUsersToProcess(_batchSize);

                if (usersList == null || usersList.Count <= 0)
                {
                    Logger.ConsoleOut($"{_productName} - No users to process.");
                    Log.Write(LogType.Information, new LogDetails {Message = $"SendRegularUserNotification - No Users to process in the batch.", CorrelationId = correlationId});
                    return;
                }

                Logger.ConsoleOut($"SendRegularUserNotification - Launching threads to process {usersList.Count} users.");
                Log.Write(LogType.Information, new LogDetails {Message = $"SendRegularUserNotification - Launching threads to process {usersList.Count} users.", CorrelationId = correlationId});

                var splitUserList = SplitList<ProcessUserLogin>(usersList, 20);

                // Launch threads
                Parallel.ForEach(splitUserList, new ParallelOptions {MaxDegreeOfParallelism = _threadCount}, CallApiToSendNotification);

                Log.Write(LogType.Information, new LogDetails {Message = $"SendRegularUserNotification - All threads processed.", CorrelationId = correlationId});
                Logger.ConsoleOut($"{_productName} - All threads processed.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails {Message = $"SendRegularUserNotification - Exception in main task.", CorrelationId = correlationId});
                Log.Write(LogType.Error, new LogDetails {Exception = ex, CorrelationId = correlationId});

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"SendRegularUserNotification - {ex.InnerException.Message}"
                    : $"SendRegularUserNotification - {ex.Message}");
            }
        }

        private void ProcessPendingUsers()
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                Log.Write(LogType.Information, new LogDetails {Message = "ProcessPendingUsers - Getting user list to  process.", CorrelationId = correlationId});

                // Get Db data by batchSize
                var repository = new UserListRepository();
                var usersList = repository.GetPendingUsersToProcess();

                if (usersList?.Count == 0)
                {
                    Logger.ConsoleOut("ProcessPendingUsers - No users to process.");
                    Log.Write(LogType.Information, new LogDetails {Message = "ProcessPendingUsers - No Users to process in the batch.", CorrelationId = correlationId });
                    return;
                }

                Logger.ConsoleOut($"ProcessPendingUsers - Launching threads to process {usersList.Count} users.");
                Log.Write(LogType.Information, new LogDetails {Message = $"ProcessPendingUsers - Launching threads to process {usersList.Count} users.", CorrelationId = correlationId });

                var splitUserList = SplitList(usersList, 50);

                // Launch threads
                Parallel.ForEach(splitUserList, new ParallelOptions {MaxDegreeOfParallelism = _threadCount}, CallApiToSetPendingToExpireUserStatus);

                Log.Write(LogType.Information, new LogDetails {Message = "ProcessPendingUsers - All threads processed.", CorrelationId = correlationId });
                Logger.ConsoleOut("ProcessPendingUsers - All threads processed.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails {Message = "ProcessPendingUsers - Exception in main task.", CorrelationId = correlationId });
                Log.Write(LogType.Error, new LogDetails {Exception = ex, CorrelationId = correlationId });

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"ProcessPendingUsers - {ex.InnerException.Message}"
                    : $"ProcessPendingUsers - {ex.Message}");
            }
        }
        
        private void ProcessDisableUsersinProducts()
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                Log.Write(LogType.Information, new LogDetails { Message = $"ProcessDisableUsersinProducts - Getting user list to process.", CorrelationId = correlationId });

                // Get Db data by batchSize
                var repository = new UserListRepository();
                var usersList = repository.GetExpiredUsersToProcess();

                if (usersList?.Count == 0)
                {
                    Logger.ConsoleOut($"ProcessDisableUsersinProducts - No users to process.");
                    Log.Write(LogType.Information, new LogDetails { Message = $"ProcessDisableUsersinProducts - No Users to process in the batch.", CorrelationId = correlationId });
                    return;
                }

                Logger.ConsoleOut($"ProcessDisableUsersinProducts - Launching threads to process {usersList.Count} users.");
                Log.Write(LogType.Information, new LogDetails { Message = $"ProcessDisableUsersinProducts - Launching threads to process {usersList.Count} users.", CorrelationId = correlationId });

                var splitUserList = SplitList(usersList, 20);

                // Launch threads
                Parallel.ForEach(splitUserList, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, CallApiToDisableUsers);

                Log.Write(LogType.Information, new LogDetails { Message = $"ProcessDisableUsersinProducts - All threads processed.", CorrelationId = correlationId });
                Logger.ConsoleOut($"ProcessDisableUsersinProducts - All threads processed.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails { Message = $"ProcessDisableUsersinProducts - Exception in main task.", CorrelationId = correlationId });
                Log.Write(LogType.Error, new LogDetails { Exception = ex, CorrelationId = correlationId });

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
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToSendNotification - Working to send notification to {userList.Count} users hashCode: {userList.GetHashCode()}",
                    CorrelationId = correlationId,
                    AdditionalInfo = additionalInfo
                });

                var notificationApiCaller = new UserApiCaller();
                var result = notificationApiCaller.ProcessUserLogins(userList);

                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToSendNotification - Result received for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.",
                    CorrelationId = correlationId
                });

                Logger.ConsoleOut($"CallApiToSendNotification - Calling API Completed for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToSendNotification - Exception while calling API for product hashCode: {userList.GetHashCode()}.",
                    CorrelationId = correlationId,
                    AdditionalInfo = additionalInfo
                });
                Log.Write(LogType.Error, new LogDetails
                {
                    Exception = ex,
                    CorrelationId = correlationId,
                    AdditionalInfo = additionalInfo
                });
                Logger.ConsoleOut(ex.InnerException != null
                    ? $"CallApiToSendNotification - {ex.InnerException.Message}"
                    : $"CallApiToSendNotification - {ex.Message}");
            }
        }

        private void CallApiToSetPendingToExpireUserStatus(List<ProcessUserLogin> userList)
        {
            string correlationId = Guid.NewGuid().ToString();
            var additionalInfo = new Dictionary<string, object>
            {
                {"userList", userList},
                {"userListHash", userList.GetHashCode()}
            };

            try
            {
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToSetPendingToExpireUserStatus - Working to set status to {userList.Count} users hashCode: {userList.GetHashCode()}",
                    AdditionalInfo = additionalInfo,
                    CorrelationId = correlationId
                });

                var apiCaller = new UserApiCaller();
                var result = apiCaller.ProcessUserLogins(userList);

                if (result.Result == null)
                {
                    throw new Exception("CallApiToSetPendingToExpireUserStatus - Null response posting to API");
                }

                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToSetPendingToExpireUserStatus - Result received for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.",
                    CorrelationId = correlationId
                });

                Logger.ConsoleOut($"CallApiToSetPendingToExpireUserStatus - Calling API Completed for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToSetPendingToExpireUserStatus - Exception while calling API for product hashCode: {userList.GetHashCode()}.",
                    AdditionalInfo = additionalInfo,
                    CorrelationId = correlationId
                });
                Log.Write(LogType.Error, new LogDetails
                {
                    Exception = ex,
                    AdditionalInfo = additionalInfo,
                    CorrelationId = correlationId
                });
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
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToDisableUsers - Working to disable user to {userList.Count} users hashCode: {userList.GetHashCode()}",
                    CorrelationId = correlationId,
                    AdditionalInfo = additionalInfo
                });

                var disableUserApiCaller = new UserApiCaller();
                var result = disableUserApiCaller.DisableExpiredUsers(userList);

                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToDisableUsers - Result received for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.",
                    CorrelationId = correlationId
                });

                Logger.ConsoleOut($"CallApiToDisableUsers - Calling API Completed for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = "CallApiToDisableUsers - Exception while calling API for product {}.",
                    CorrelationId = correlationId,
                    AdditionalInfo = additionalInfo
                });
                Log.Write(LogType.Error, new LogDetails
                {
                    Exception = ex,
                    AdditionalInfo = additionalInfo,
                    CorrelationId = correlationId
                });
                Logger.ConsoleOut(ex.InnerException != null
                    ? $"CallApiToDisableUsers - {ex.InnerException.Message}"
                    : $"CallApiToDisableUsers - {ex.Message}");
            }
        }

        private void ServiceTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} process started..."});
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
                Log.Write(LogType.Error, new LogDetails {Exception = ex});
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
