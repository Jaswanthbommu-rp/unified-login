using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web.Http.Filters;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;

namespace RP.Enterprise.Foundation.Audit.Core.Component
{
    public class PerformanceLogger
    {
        private readonly Stopwatch _sw;
        private DateTime _beginTime;
        private readonly Dictionary<string, object> _details;

        public string PerfName { get; set; }

        public PerformanceLogger(string name)
        {
            _sw = Stopwatch.StartNew();
            PerfName = name;
            _beginTime = DateTime.Now;
            _details = new Dictionary<string, object>();
        }
        public PerformanceLogger(string name, Dictionary<string, object> perfParams) : this(name)
        {
            foreach (var item in perfParams)
                _details.Add("input-" + item.Key, item.Value);
        }

        public void Stop(HttpActionExecutedContext actionExecutedContext)
        {
            _sw.Stop();

            if (!_details.ContainsKey("Started"))
                _details.Add("Started", _beginTime.ToString(CultureInfo.InvariantCulture));
            else
                _details["Started"] = _beginTime.ToString(CultureInfo.InvariantCulture);

            if (!_details.ContainsKey("ElapsedMilliseconds"))
                _details.Add("ElapsedMilliseconds", _sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
            else
                _details["ElapsedMilliseconds"] = _sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);

            if (!_details.ContainsKey("PerfName"))
                _details.Add("PerfName", PerfName);
            else
                _details["PerfName"] = PerfName;


            var logDetails = new LogDetails
            {
                Message = $"Performance captured for {PerfName} ",
                ElapsedMilliseconds = Convert.ToInt64(_details["ElapsedMilliseconds"]),
                CorrelationId = Guid.NewGuid().ToString(),
                AdditionalInfo = _details,
                ServerName = Environment.MachineName

            };

            var user = actionExecutedContext.ActionContext.RequestContext.Principal as ClaimsPrincipal;
            if (user != null)
            {
                var userNmClaim = user.Claims.FirstOrDefault(a => a.Type == "loginName");
                if (userNmClaim != null) logDetails.UserName = userNmClaim.Value;

                var orgIdClaim = user.Claims.FirstOrDefault(a => a.Type == "orgPartyId");
                if (orgIdClaim != null) logDetails.PmcId = orgIdClaim.Value;

                var orgNameClaim = user.Claims.FirstOrDefault(a => a.Type == "orgName");
                if (orgNameClaim != null) logDetails.PmcName = orgNameClaim.Value;

                var realPageClaim = user.Claims.FirstOrDefault(a => a.Type == "realPageId");
                if (realPageClaim != null && !_details.ContainsKey("realPageId")) _details.Add("realPageId", realPageClaim.Value);
            }

            Log.Write(LogType.Performance, logDetails);
        }

        public void Stop()
        {
            _sw.Stop();

            if (!_details.ContainsKey("Started"))
                _details.Add("Started", _beginTime.ToString(CultureInfo.InvariantCulture));
            else
                _details["Started"] = _beginTime.ToString(CultureInfo.InvariantCulture);

            if (!_details.ContainsKey("ElapsedMilliseconds"))
                _details.Add("ElapsedMilliseconds", _sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
            else
                _details["ElapsedMilliseconds"] = _sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);


            Log.Write(LogType.Performance,
                new LogDetails
                {
                    Message = $"Performance captured for {PerfName} ",
                    ElapsedMilliseconds = Convert.ToInt64(_details["ElapsedMilliseconds"]),
                    CorrelationId = Guid.NewGuid().ToString(),
                    AdditionalInfo = _details,
                    ServerName = Environment.MachineName
                    //        //TODO: Get Organization & other info
                    //        var user = context.RequestContext.Principal as ClaimsPrincipal;
                    //if (user != null)
                    //{
                    //    var organizationNameClaim = user.Claims.FirstOrDefault(a => a.Type == RealPageClaims.OrganizationName);
                    //    if (organizationNameClaim != null && !dict.ContainsKey("OrganizationName"))
                    //        dict.Add("OrganizationName", organizationNameClaim.Value);
                    //}
                });
        }
    }
}