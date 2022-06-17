<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StatusCheck.aspx.cs" Inherits="RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.StatusCheck" %>
<%@ Import Namespace="System.Collections.Concurrent" %>
<%@ Import Namespace="System.Threading.Tasks" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper" %>
<%@ Import Namespace="RP.Enterprise.Subsystem.ProductLauncher.Web.Landing" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Status</title>
	<meta http-equiv="Refresh" content="30"/>
	<meta charset="utf-8">
	<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css" integrity="sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO" crossorigin="anonymous">
	<script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
	<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js" integrity="sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q" crossorigin="anonymous"></script>
	<script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>
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

	ConcurrentDictionary<string, object> envStatus = new ConcurrentDictionary<string, object>();
	ConcurrentDictionary<string, Dictionary<string, Dictionary<string, string>>> serviceStatus = new ConcurrentDictionary<string, Dictionary<string, Dictionary<string, string>>>();
	var sortedServerList = new List<string>();
	string collapseSetting = "";

	var watch = System.Diagnostics.Stopwatch.StartNew();

	if (ConfigurationManager.AppSettings["StatusCheck"] != null)
	{
		string config = Encoding.UTF8.GetString(Convert.FromBase64String(ConfigurationManager.AppSettings["StatusCheck"]));

		if (!string.IsNullOrEmpty(config))
		{
			try
			{
				StatusCheckServerSettings setting = JsonConvert.DeserializeObject<StatusCheckServerSettings>(config);
				string kibanaUrl = setting.Kibana.FirstOrDefault(p => p.Key.Equals("dashboardUrl", StringComparison.OrdinalIgnoreCase)).Value;
				Response.Write($"<a target='_blank' href='{kibanaUrl}'>Kibana errors</a>");

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
						{
							string res = checkServer(serverName, server.LoginHost, server.LoginUrl);
							serverStatus.TryAdd(serverName.ToLower(), res);
						}
						Dictionary<string, Dictionary<string, string>> serviceResult = checkService(serverName, server);
						if (serviceResult.Count > 0)
						{
							{
								serviceStatus.TryAdd(serverName.ToLower(), serviceResult);
							}
						}
					});
					envStatus.TryAdd(server.Name, serverStatus);
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
	List<string> serviceListByEnvironment = new List<string>();


	//foreach (KeyValuePair<string, object> environment in envStatus)
	foreach (var sortedEnv in sortedServerList)
	{
		KeyValuePair<string, object> environment = envStatus.FirstOrDefault(p => p.Key.Equals(sortedEnv, StringComparison.OrdinalIgnoreCase));
		somethingDown = false;
		serviceListByEnvironment = new List<string>();

		string status = "envUp";
		foreach (var server in environment.Value as ConcurrentDictionary<string, string>)
		{
			if (!server.Value.Equals("ONLINE", StringComparison.OrdinalIgnoreCase))
			{
				status = "envDown";
				if ((environment.Key as string).Equals("LOCALHOST", StringComparison.OrdinalIgnoreCase))
				{
					somethingDown = true;
				}
			}

			KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> env2 = serviceStatus.FirstOrDefault(p => p.Key.Equals(server.Key, StringComparison.OrdinalIgnoreCase));
			if (env2.Key != null && server.Key.Equals(env2.Key, StringComparison.OrdinalIgnoreCase))
			{
				var serverDetails = env2.Value;
				if (serverDetails.ContainsKey("services"))
				{
					foreach (KeyValuePair<string, string> serverServiceList in serverDetails["services"])
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
		Response.Write($"<h5 class='mb-0'>" + Environment.NewLine);
		Response.Write($"<button class='btn btn-lg btn-block btn-light' data-toggle='collapse' data-target='#collapse{cardNumber}' aria-expanded='true' aria-controls='collapse{cardNumber}'>" + Environment.NewLine);
		Response.Write($"<span class='envName {status}'>{environment.Key}</span>" + Environment.NewLine);
		Response.Write($"</button>" + Environment.NewLine);
		Response.Write($"</h5>" + Environment.NewLine);
		Response.Write($"</div>" + Environment.NewLine);

		Response.Write($"<div id='collapse{cardNumber}' class='collapse {collapseSetting}' aria-labelledby='heading{cardNumber}' data-parent='#accordion'>" + Environment.NewLine);
		Response.Write($"<div class='card-body'>" + Environment.NewLine);

		Response.Write($"<table class='table'>" + Environment.NewLine);
		Response.Write($"<thead>" + Environment.NewLine);
		Response.Write($"<th>Server</th>" + Environment.NewLine);
		Response.Write($"<th>Status</th>" + Environment.NewLine);
		Response.Write($"<th>Actions not logged</th>" + Environment.NewLine);
		foreach (var serviceName in serviceListByEnvironment.Distinct())
		{
			Response.Write($"<th>{serviceName}</th>" + Environment.NewLine);
		}

		Response.Write("</thead>" + Environment.NewLine);
		Response.Write("<tbody>" + Environment.NewLine);

		foreach (var server in environment.Value as ConcurrentDictionary<string, string>)
		{
		    var serverName = MaskServerName(server);
		    
			string serverStatus = "success";
			if (!server.Value.Equals("ONLINE", StringComparison.OrdinalIgnoreCase))
			{
				serverStatus = "danger";
			}

			Response.Write("<tr>" + Environment.NewLine);
			Response.Write("<td>" + serverName + "</td>" + Environment.NewLine);
			Response.Write("<td><span class='badge badge-" + serverStatus + "'>" + server.Value + "</span></td>" + Environment.NewLine);

			var env2 = serviceStatus.FirstOrDefault(p => p.Key.Equals(server.Key, StringComparison.OrdinalIgnoreCase));
			//foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> env2 in serviceStatus)
			{
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
								Response.Write("<span class='badge badge-" + serverLogStatus + "'>" + serverLog.Value + "</span>");
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
								Response.Write($"<span class='badge badge-{serverServiceStatus}'>{serviceDetail.Value.ToLower()}</span>");
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
					Response.Write("<span class='badge badge-danger'>Services call failed</span>");
				}
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
</body>
</html>
