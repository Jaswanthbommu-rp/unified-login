using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public partial class StatusCheck : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

		}

        public Dictionary<string, Dictionary<string, string>> checkService(string serverName, StatusCheckServerSetting serverSetting)
        {
			Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
			try
			{
				using (HttpClient client = new HttpClient())
				{
					client.DefaultRequestHeaders.Host = serverSetting.DashboardHost;
					string protocol = "http";
					if (serverSetting.DashboardHost.ToUpper().Contains("LOCAL."))
					{
						protocol = "https";
					}
					// need to hit the server directly over port 80, so it doesn't go through bigip
					client.BaseAddress = new Uri(protocol + "://" + serverName);
					string uriExtra = "";
					if (!serverSetting.DashboardHost.Equals(serverSetting.DashboardUrl, StringComparison.OrdinalIgnoreCase))
					{
						uriExtra = serverSetting.DashboardUrl.Replace(serverSetting.DashboardHost, "");
					}
					var response = client.GetAsync($"{uriExtra}/servicecheck.aspx").Result;
					if (response.IsSuccessStatusCode)
					{
						result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(response.Content.ReadAsStringAsync().Result);
					}

                    if (result.ContainsKey("logmessages") && result["logmessages"].ContainsKey("greenbook_activity"))
                    {
                        int messageCount = Convert.ToInt32(result["logmessages"]["greenbook_activity"]);
                        if (messageCount > 0)
                        {
                            // restart commandreader
                            using (HttpClient msmq = new HttpClient())
                            {
                                if (serverSetting.Name.Equals("EUSAT", StringComparison.OrdinalIgnoreCase) || serverSetting.Name.Equals("EUPROD", StringComparison.OrdinalIgnoreCase))
                                {
                                    msmq.DefaultRequestHeaders.Host = $"mylogcommand{serverSetting.Name.ToLower()}.realpage.co.uk";
								}
                                else
                                {
                                    msmq.DefaultRequestHeaders.Host = $"mylogcommand{serverSetting.Name.ToLower()}.realpage.com";
								}
                                
                                msmq.BaseAddress = client.BaseAddress;
                                var msmqResponse = msmq.GetAsync($"{uriExtra}/Writer.svc").Result;
                            }
                        }
                    }
                    
                }
			}catch (Exception ex) { }

			return result;
		}


		public string checkIdentityServer(string serverName, string hostName, string hostUrl)
		{
			try
			{
				using (HttpClient client = new HttpClient())
				{
					client.DefaultRequestHeaders.Host = hostName;
					string protocol = "http";
					if (hostName.ToUpper().Contains("LOCAL."))
					{
						protocol = "https";
					}

					// need to hit the server directly over port 80, so it doesn't go through bigip
					client.BaseAddress = new Uri(protocol + "://" + serverName);
					string uriExtra = "";
					if (!hostName.Equals(hostUrl, StringComparison.OrdinalIgnoreCase))
					{
						uriExtra = hostUrl.Replace(hostName, "");
					}

					var response = client.GetAsync( $"{uriExtra}/identity/.well-known/openid-configuration").Result;
					if (response.IsSuccessStatusCode)
					{
						var result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
						try
						{
							string issuer = result.issuer;
							if (issuer.Equals("https://" + hostUrl+ "/identity", StringComparison.OrdinalIgnoreCase))
							{
								return "online";
							}
						}
						catch (Exception ex)
						{
							return "error";
						}

						return "offline";
					}
				}
			}
			catch (Exception ex)
			{
				return "error";
			}

			return "offline";
		}

        public string checkApi(string serverName, string hostName, string apiHealthUrl)
        {
			if (hostName == null)
			{
				return "missing";
			}

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Host = hostName;
                    string protocol = "http";
                    if (hostName.ToUpper().Contains("LOCAL.") || hostName.ToUpper().Contains("LOCAL2."))
                    {
                        protocol = "https";
                    }

                    // need to hit the server directly over port 80, so it doesn't go through bigip
                    client.BaseAddress = new Uri(protocol + "://" + serverName);
                    
                    var response = client.GetAsync(apiHealthUrl).Result;
					if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						return "running";
					}
                }
            }
            catch 
            {
                return "error";
            }

            return "offline";
        }

        protected static string MaskServerName(KeyValuePair<string, string> server)
        {
			var serverName = server.Key.Split('.')[0];
            serverName = new string('X', serverName.Length - 5) + serverName.Substring(serverName.Length - 5);
            return serverName;
        }
    }

	public class StatusCheckServerSettings
	{
		public Dictionary<string, string> Kibana { get; set; }
		public List<StatusCheckServerSetting> Env { get; set; }
		public List<Dictionary<string, string>> Services { get; set; }
        public List<ApiInformation> Apis { get; set; }
    }

	public class StatusCheckServerSetting
	{
		public string Name { get; set; }
		public string LoginHost { get; set; }
		public string LoginUrl { get; set; }
		public string ApiUrl { get; set; }
		public string DashboardHost { get; set; }
		public string DashboardUrl { get; set; }
		public List<string> Servers { get; set; }
	}

    public class ApiInformation
    {
		public string Name { get; set; }
		public string Route { get; set; }
    }
}