using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Model;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Repository;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser
{
    public partial class DisableUserService : ServiceBase
    {
        static readonly string _productName = "Disable Users Multi Company";
        static readonly int _threadCount = int.Parse(ConfigReader.GetThreadCount);
        static readonly string _ScheduledTime = ConfigReader.GetScheduledTime;
        static readonly int _intervel = int.Parse(ConfigReader.GetCallDuration);
        private Timer _timer;

        public DisableUserService()
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
                Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} - Exception in OnStart task."});
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

        private void ProcessDisableUsersinProducts()
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} - Getting user list to process.", CorrelationId = correlationId});

                // Get Db data by batchSize
                var repository = new UserListRepository();
                var usersList = repository.GetExpiredUsersToProcess();

                if (usersList?.Count == 0)
                {
                    Logger.ConsoleOut($"{_productName} - No users to process.");
                    Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} - No Users to process in the batch.", CorrelationId = correlationId});
                    return;
                }

                Logger.ConsoleOut($"{_productName} - Launching threads to process {usersList.Count} users.");
                Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} - Launching threads to process {usersList.Count} users.", CorrelationId = correlationId});

                var splitUserList = SplitList(usersList, 20);

                // Launch threads
                Parallel.ForEach(splitUserList, new ParallelOptions {MaxDegreeOfParallelism = _threadCount}, CallApiToDisableUsers);

                Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} - All threads processed.", CorrelationId = correlationId});
                Logger.ConsoleOut($"{_productName} - All threads processed.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} - Exception in main task.", CorrelationId = correlationId});
                Log.Write(LogType.Error, new LogDetails {Exception = ex, CorrelationId = correlationId});

                Logger.ConsoleOut(ex.InnerException != null
                    ? $"{_productName} - {ex.InnerException.Message}"
                    : $"{_productName} - {ex.Message}");
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
                    Message = $"CallApiToDisableUsers-Working to disable user to {userList.Count} users hashCode: {userList.GetHashCode()}",
                    CorrelationId = correlationId,
                    AdditionalInfo = additionalInfo
                });

                var disableUserApiCaller = new DisableUserApiCaller();
                var result = disableUserApiCaller.DisableExpiredUsers(userList);

                Log.Write(LogType.Information, new LogDetails
                {
                    Message = $"CallApiToDisableUsers-Result received for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.",
                    CorrelationId = correlationId
                });

                Logger.ConsoleOut($"CallApiToDisableUsers-Calling API Completed for {userList.Count} users hashCode: {userList.GetHashCode()} - {result.Result}.");
            }
            catch (Exception ex)
            {
                // Log the exception.
                Log.Write(LogType.Information, new LogDetails
                {
                    Message = "Exception while calling API for product {}.",
                    CorrelationId = correlationId,
                    AdditionalInfo = additionalInfo
                });
                Log.Write(LogType.Error, new LogDetails
                {
                    Exception = ex, AdditionalInfo = additionalInfo,
                    CorrelationId = correlationId
                });
                Logger.ConsoleOut(ex.InnerException != null
                    ? $"CallApiToDisableUsers - {ex.InnerException.Message}"
                    : $"CallApiToDisableUsers - {ex.Message}");
            }
        }

        private void ServiceTimer_Tick(object sender, ElapsedEventArgs e)
        {
            Log.Write(LogType.Information, new LogDetails {Message = $"{_productName} - Disable Expired User process started..."});

            string _CurrentTime = String.Format("{0:t}", DateTime.UtcNow);

            //if (_CurrentTime == _ScheduledTime)
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
                _timer.Interval = _intervel;
                _timer.Start();
            }
            catch (Exception ex)
            {
                Log.Write(LogType.Error, new LogDetails {Exception = ex});
            }
        }

        /// <summary>
        /// Used to split a list into multiple smaller lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="locations"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
