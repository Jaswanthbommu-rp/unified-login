<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StatusCheck.aspx.cs" Inherits="RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.StatusCheck" %>
<%@ Import Namespace="System.Collections.Concurrent" %>
<%@ Import Namespace="System.Threading.Tasks" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="RP.Enterprise.Subsystem.ProductLauncher.Web.Landing" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Status</title>
	<meta http-equiv="Refresh" content="30"/>
	<meta charset="utf-8">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-gH2yIJqKdNHPEq0n4Mqa/HGKIhSkIHeL5AyhkYV8i59U5AR6csBvApHHNl/vI1Bx" crossorigin="anonymous">
    <script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.5/dist/umd/popper.min.js" integrity="sha384-Xe+8cL9oJa6tN/veChSP7q+mnSPaj5Bcu9mPX5F5xIGE0DVittaqT5lorf0EI7Vk" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/js/bootstrap.min.js" integrity="sha384-ODmDIVzN+pFdexxHEHFBQH3/9/vQ9uori45z4JjnFsRydbmQbmL5t1tQ0culUzyK" crossorigin="anonymous"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
 
	<style>
		@import url("https://fonts.googleapis.com/css?family=Roboto:400,100,300,500,700,900");
		body {
			font-family: "Roboto", "Helvetica Neue", Helvetica, Arial, sans-serif;
			text-align: center;
			font-size: 16px;
		}

		.envLabel {
			text-align: center;
			font-size: 18px;
			padding-bottom: 5px;
			padding-top: 10px;
		}

		.envName {
			font-weight: bold;
		}

		.envUp {
			color: green;
		}

		.envDown {
			color: red;
		}

		.envTable {
			margin-left: auto;
			margin-right: auto;
			width: 450px;
			padding: 2px;
			xborder-spacing: 2px;
			font-size: 16px;
		}

		.envTable tr:hover {
			background-color: #f2f2f2;
		}

		.envTable td {
			border-bottom: 0px solid black;
			border-color: #ddd;
		}

		.online {
			color: green;
			font-weight: bold;
		}

		.offline {
			color: red;
			font-weight: bold;
		}

		.downHide {
			display: none;
		}
	</style>
</head>
<body>
<%
    bool somethingDown = false;
    Response.Write("<h2>Updated <span>" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "</span></h2>" + Environment.NewLine);
    Response.Write("<h1><span class='downHide' id='somethingDown'>SOMETHING IS DOWN!</span></h1>" + Environment.NewLine);

    ConcurrentDictionary<string, object> identityServerStatus = new ConcurrentDictionary<string, object>();
    var serviceStatus = new ConcurrentDictionary<string, Dictionary<string, Dictionary<string, string>>>();
    var apiStatus = new ConcurrentDictionary<string, Dictionary<string, string>>();
    var sortedServerList = new List<string>();
    string collapseSetting = "";
    StatusCheckServerSettings setting = null;
    var watch = System.Diagnostics.Stopwatch.StartNew();

    if (ConfigurationManager.AppSettings["StatusCheck"] != null)
    {
        string config = Encoding.UTF8.GetString(Convert.FromBase64String(ConfigurationManager.AppSettings["StatusCheck"]));

        if (!string.IsNullOrEmpty(config))
        {
            try
            {
                setting = JsonConvert.DeserializeObject<StatusCheckServerSettings>(config);
                string kibanaUrl = setting.Kibana.FirstOrDefault(p => p.Key.Equals("dashboardUrl", StringComparison.OrdinalIgnoreCase)).Value;
                Response.Write($"<a target='_blank' href='{kibanaUrl}'>Kibana errors</a>&nbsp;&nbsp;Refresh In&nbsp;");
%><span id="timerLabel" runat="server">30</span>
<%
                foreach (StatusCheckServerSetting server in setting.Env)
                {
                    // keep the order of the servers based on the json order
                    sortedServerList.Add(server.Name);
                }

                Parallel.ForEach(setting.Env, server =>
                //foreach (StatusCheckServerSetting server in setting.Env)
                {
                    ConcurrentDictionary<string, string> serverStatus = new ConcurrentDictionary<string, string>();
                    Parallel.ForEach(server.Servers, serverName =>
                    {
                        string res = checkIdentityServer(serverName, server.LoginHost, server.LoginUrl);
                        serverStatus.TryAdd(serverName.ToLower(), res);

                        var apiResultDict = new Dictionary<string, string>();

                        for (var x = 0; x < setting.Apis.Count; x++)
                        {
                            var apiResult = checkApi(serverName, server.ApiUrl, setting.Apis[x].Route);
                            apiResultDict.Add(setting.Apis[x].Name, apiResult);
                        }

                        //string apiResultText = checkApi(serverName, server.ApiUrl, "apicore/health");
                        //apiResultDict.Add("apicore", apiResultText);
                        //
                        //apiResultText = checkApi(serverName, server.ApiUrl, "apicoreenterprise/health");
                        //apiResultDict.Add("apicoreent", apiResultText);
                        //
                        //apiResultText = checkApi(serverName, server.ApiUrl, "api/test/testapi");
                        //apiResultDict.Add("api", apiResultText);
                        //
                        //apiResultText = checkApi(serverName, server.ApiUrl, "apienterprise/test/testapi");
                        //apiResultDict.Add("api", apiResultText);

                        apiStatus.TryAdd(serverName.ToLower(), apiResultDict);

                        var serviceResult = checkService(serverName, server);
                        if (serviceResult.Count > 0)
                        {
                            serviceStatus.TryAdd(serverName.ToLower(), serviceResult);
                        }
                    });
                    identityServerStatus.TryAdd(server.Name, serverStatus);
                });


                //foreach (StatusCheckServerSetting server in setting.Env)
                //{
                //	ConcurrentDictionary<string, string> serverStatus = new ConcurrentDictionary<string, string>();

                //	foreach (string serverName in server.Servers)
                //	{
                //		serverStatus.TryAdd(serverName.ToLower(), checkServer(serverName, server.LoginHost));
                //		Dictionary<string, Dictionary<string, string>> serviceResult = checkService(serverName, server.DashboardHost);
                //		if (serviceResult.Count > 0)
                //		{
                //			serviceStatus.TryAdd(serverName.ToLower(), serviceResult);
                //		}
                //	}
                //	envStatus.TryAdd(server.Name, serverStatus);
                //}

            }
            catch (Exception ex)
            {
            }
        }
    }
    watch.Stop();
    Response.Write($"<!--elapsed: {watch.ElapsedMilliseconds}-->"+ Environment.NewLine);
    Response.Write("<div id='accordion'>"+ Environment.NewLine);
    int cardNumber = 0;
    var serviceListByEnvironment = new List<string>();


    //foreach (KeyValuePair<string, object> environment in envStatus)
    foreach (var sortedEnv in sortedServerList)
    {
        var id4Status = identityServerStatus.FirstOrDefault(p => p.Key.Equals(sortedEnv, StringComparison.OrdinalIgnoreCase));
        somethingDown = false;
        serviceListByEnvironment = new List<string>();

        string status = "envUp";
        foreach (var server in id4Status.Value as ConcurrentDictionary<string, string>)
        {
            if (!server.Value.Equals("ONLINE", StringComparison.OrdinalIgnoreCase))
            {
                status = "envDown";
                if ((id4Status.Key as string).Equals("LOCALHOST", StringComparison.OrdinalIgnoreCase))
                {
                    somethingDown = true;
                }
            }
            //var coreApis = apiStatus.FirstOrDefault(p => p.Key.Equals(server.Key, StringComparison.OrdinalIgnoreCase));
            //string apiCoreValue = coreApis.Value.FirstOrDefault(p => p.Key.Equals("apicore", StringComparison.OrdinalIgnoreCase)).Value;
            //string apiCoreStatus = apiCoreValue.Equals("error", StringComparison.OrdinalIgnoreCase) || apiCoreValue.Equals("offline", StringComparison.OrdinalIgnoreCase) ?  "danger" : "success";
            //if (apiCoreStatus.Equals("danger", StringComparison.OrdinalIgnoreCase))
            //{
            //    status = "envDown";
            //}
            //
            //var apiCoreEntValue = coreApis.Value.FirstOrDefault(p => p.Key.Equals("apicoreent", StringComparison.OrdinalIgnoreCase)).Value;
            //var apiCoreEntStatus = apiCoreEntValue.Equals("error", StringComparison.OrdinalIgnoreCase) || apiCoreEntValue.Equals("offline", StringComparison.OrdinalIgnoreCase) ?  "danger" : "success";
            //if (apiCoreEntStatus.Equals("danger", StringComparison.OrdinalIgnoreCase))
            //{
            //    status = "envDown";
            //}
            var apiDetails = apiStatus.FirstOrDefault(p => p.Key.Equals(server.Key, StringComparison.OrdinalIgnoreCase)).Value;

            foreach (var keyValuePair in apiDetails)
            {
                if (keyValuePair.Value.Equals("error", StringComparison.OrdinalIgnoreCase) || keyValuePair.Value.Equals("offline", StringComparison.OrdinalIgnoreCase))
                {
                    status = "envDown";
                    break;
                }
            }

            var env2 = serviceStatus.FirstOrDefault(p => p.Key.Equals(server.Key, StringComparison.OrdinalIgnoreCase));
            if (env2.Key != null && server.Key.Equals(env2.Key, StringComparison.OrdinalIgnoreCase))
            {
                var serverDetails = env2.Value;
                if (serverDetails.ContainsKey("services"))
                {
                    foreach (var serverServiceList in serverDetails["services"])
                    {
                        serviceListByEnvironment.Add(serverServiceList.Key);
                        if (!serverServiceList.Value.Equals("RUNNING", StringComparison.OrdinalIgnoreCase))
                        {
                            status = "envDown";
                        }
                    }
                }
                if (serverDetails.ContainsKey("logmessages"))
                {
                    foreach (KeyValuePair<string, string> serverLogs in serverDetails["logmessages"])
                    {
                        if (!serverLogs.Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                        {
                            status = "envDown";
                        }
                    }
                }
                //break;
            }
            else
            {
                status = "envDown";
            }
        }
        serviceListByEnvironment = serviceListByEnvironment.OrderBy(p => p).ToList();
        collapseSetting = "";
        if (status.Equals("envDown"))
        {
            collapseSetting = "show";
        }

        cardNumber++;

        Response.Write($"<div class='card'>" + Environment.NewLine);
        Response.Write($"<div class='card-header' id='heading{cardNumber}'>" + Environment.NewLine);
        Response.Write($"<h5 class='d-grid gap-2'>" + Environment.NewLine);
        Response.Write($"<button class='btn btn-lg btn-block btn-light' data-bs-toggle='collapse' data-bs-target='#collapse{cardNumber}' aria-expanded='true' aria-controls='collapse{cardNumber}'>" + Environment.NewLine);
        Response.Write($"<span class='envName {status}'>{id4Status.Key}</span>" + Environment.NewLine);
        Response.Write($"</button>" + Environment.NewLine);
        Response.Write($"</h5>" + Environment.NewLine);
        Response.Write($"</div>" + Environment.NewLine);

        Response.Write($"<div id='collapse{cardNumber}' class='collapse {collapseSetting}' aria-labelledby='heading{cardNumber}' data-parent='#accordion'>" + Environment.NewLine);
        Response.Write($"<div class='card-body'>" + Environment.NewLine);

        Response.Write($"<table class='table'>" + Environment.NewLine);
        Response.Write($"<thead>" + Environment.NewLine);
        Response.Write($"<th>Server</th>" + Environment.NewLine);
        Response.Write($"<th>ID4</th>" + Environment.NewLine);
        for (var x = 0; x < setting.Apis.Count; x++)
        {
            Response.Write($"<th>{setting.Apis[x].Name}</th>" + Environment.NewLine);
        }
        
        Response.Write($"<th>Queued Activities</th>" + Environment.NewLine);
        foreach (var serviceName in serviceListByEnvironment.Distinct())
        {
            Response.Write($"<th>{serviceName}</th>" + Environment.NewLine);
        }

        Response.Write("</thead>" + Environment.NewLine);
        Response.Write("<tbody>" + Environment.NewLine);

        foreach (var server in id4Status.Value as ConcurrentDictionary<string, string>)
        {
            var serverName = MaskServerName(server);

            string serverStatus = "success";
            if (!server.Value.Equals("ONLINE", StringComparison.OrdinalIgnoreCase))
            {
                serverStatus = "danger";
            }

            Response.Write("<tr>" + Environment.NewLine);
            Response.Write($"<td>{serverName}</td>" + Environment.NewLine);
            Response.Write($"<td><span class='badge text-bg-{serverStatus}'>{server.Value}</span></td>" + Environment.NewLine);

            var apiResults = apiStatus.FirstOrDefault(p => p.Key.Equals(server.Key, StringComparison.OrdinalIgnoreCase));
            for (var x = 0; x < setting.Apis.Count; x++)
            {
                var currentApiName = setting.Apis[x].Name;
                if (apiResults.Value.ContainsKey(currentApiName))
                {
                    Response.Write("<td>");
                    var apiCoreValue = apiResults.Value.FirstOrDefault(p => p.Key.Equals(currentApiName, StringComparison.OrdinalIgnoreCase)).Value;
                    var apiCoreStatus = apiCoreValue.Equals("error", StringComparison.OrdinalIgnoreCase) || apiCoreValue.Equals("offline", StringComparison.OrdinalIgnoreCase) ?  "danger" : "success";
                    Response.Write($"<span class='badge text-bg-{apiCoreStatus}'>{apiCoreValue}</span>");
                    Response.Write("</td>" + Environment.NewLine);
                }
            }
            
            //Response.Write("<td>");
            //var apiCoreValue = coreApis.Value.FirstOrDefault(p => p.Key.Equals("apicore", StringComparison.OrdinalIgnoreCase)).Value;
            //var apiCoreStatus = apiCoreValue.Equals("error", StringComparison.OrdinalIgnoreCase) || apiCoreValue.Equals("offline", StringComparison.OrdinalIgnoreCase) ?  "danger" : "success";
            //Response.Write($"<span class='badge text-bg-{apiCoreStatus}'>{apiCoreValue}</span>");
            //Response.Write("</td>" + Environment.NewLine);
            //Response.Write("<td>");
            //var apiCoreEntValue = coreApis.Value.FirstOrDefault(p => p.Key.Equals("apicoreent", StringComparison.OrdinalIgnoreCase)).Value;
            //var apiCoreEntStatus = apiCoreEntValue.Equals("error", StringComparison.OrdinalIgnoreCase) || apiCoreEntValue.Equals("offline", StringComparison.OrdinalIgnoreCase) ?  "danger" : "success";
            //Response.Write($"<span class='badge text-bg-{apiCoreEntStatus}'>{apiCoreEntValue}</span>");
            //Response.Write("</td>" + Environment.NewLine);


            var env2 = serviceStatus.FirstOrDefault(p => p.Key.Equals(server.Key, StringComparison.OrdinalIgnoreCase));
            if (env2.Key != null)
            {
                string statusServices = "envUp";
                Dictionary<string, Dictionary<string, string>> resultList = env2.Value as Dictionary<string, Dictionary<string, string>>;

                if (resultList.ContainsKey("services"))
                {
                    foreach (KeyValuePair<string, string> serverService in resultList["services"] as Dictionary<string, string>)
                    {
                        if (!serverService.Value.Equals("RUNNING", StringComparison.OrdinalIgnoreCase))
                        {
                            statusServices = "envDown";
                            if (!(env2.Key as string).Equals("LOCALHOST", StringComparison.OrdinalIgnoreCase))
                            {
                                somethingDown = true;
                            }
                        }
                    }
                }
                if (resultList.ContainsKey("logmessages"))
                {
                    foreach (KeyValuePair<string, string> serverLogs in resultList["logmessages"] as Dictionary<string, string>)
                    {
                        if (!serverLogs.Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                        {
                            statusServices = "envDown";
                            if (!(env2.Key as string).Equals("LOCALHOST", StringComparison.OrdinalIgnoreCase))
                            {
                                somethingDown = true;
                            }
                        }
                    }
                }

                #region msmq

                // write the greenbook_activity result
                Response.Write("<td>");
                if (resultList.ContainsKey("logmessages"))
                {
                    foreach (KeyValuePair<string, string> serverLog in resultList["logmessages"] as Dictionary<string, string>)
                    {
                        if (serverLog.Key.Equals("GREENBOOK_ACTIVITY", StringComparison.OrdinalIgnoreCase))
                        {
                            string serverLogStatus = "success";
                            if (!serverLog.Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                            {
                                serverLogStatus = "danger";
                            }
                            Response.Write("<span class='badge text-bg-" + serverLogStatus + "'>" + serverLog.Value + "</span>");
                            break;
                        }
                    }
                }
                Response.Write("</td>" + Environment.NewLine);
                #endregion


                #region services
                foreach (string serviceName in serviceListByEnvironment.Distinct())
                {
                    Response.Write("<td>");
                    if (resultList.ContainsKey("services"))
                    {
                        KeyValuePair<string, string> serviceDetail = resultList["services"].FirstOrDefault(p => p.Key.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                        if (serviceDetail.Key != null)
                        {
                            string serverServiceStatus = "success";

                            if (!serviceDetail.Value.Equals("RUNNING", StringComparison.OrdinalIgnoreCase))
                            {
                                serverServiceStatus = "danger";
                            }
                            Response.Write($"<span class='badge text-bg-{serverServiceStatus}'>{serviceDetail.Value.ToLower()}</span>");
                        }
                    }
                    else
                    {
                        Response.Write("&nbsp;");
                    }
                    Response.Write("</td>");
                }

                #endregion

                Response.Write("</tr>" + Environment.NewLine);
                //break;
            }
            else
            {
                Response.Write("<span class='badge text-bg-danger'>Services call failed</span>");
            }

        }
        Response.Write("</tbody>" + Environment.NewLine);
        Response.Write("</table>" + Environment.NewLine);
        Response.Write("</div>" + Environment.NewLine);
        Response.Write("</div>" + Environment.NewLine);
        Response.Write("</div>" + Environment.NewLine);
        //Response.Write("<br />" + Environment.NewLine);

    }

    Response.Write("</div>");
    if (somethingDown)
    {
        Response.Write("<script type='text/javascript'>parent.document.getElementById('somethingDown').className = 'offline';</script>");
    }
%>
<script type="text/javascript">

    function countdown() {
        seconds = document.getElementById("timerLabel").innerHTML;
        if (seconds > 0) {
            document.getElementById("timerLabel").innerHTML = seconds - 1;
            setTimeout("countdown()", 1000);
        }
    }

    setTimeout("countdown()", 1000);

</script>
</body>
</html>
