using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.ServiceProcess;
using System.Text;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public partial class ServiceCheck : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}


		public Dictionary<string, string> CheckServices()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			if (ConfigurationManager.AppSettings["StatusCheck"] != null)
			{
				string config = Encoding.UTF8.GetString(Convert.FromBase64String(ConfigurationManager.AppSettings["StatusCheck"]));
				if (!string.IsNullOrEmpty(config))
				{
					try
					{
						StatusCheckServerSettings setting = JsonConvert.DeserializeObject<StatusCheckServerSettings>(config);
						ServiceController[] services = ServiceController.GetServices();
						foreach (Dictionary<string, string> service in setting.Services)
						{
							if (services.Any(p => p.ServiceName.ToUpper() == service["name"].ToUpper()))
							{
								ServiceController sc = services.FirstOrDefault(p => p.ServiceName.ToUpper() == service["name"].ToUpper());
								result.Add(service["name"].ToString(), sc.Status.ToString());
							}
						}
					}
					catch (Exception ex)
					{
					}
				}
			}
			return result;
		}

		public Dictionary<string, string> CheckMSMQ()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			
			try
			{
				using (var queue = new MessageQueue(@".\private$\greenbook_activity"))
				{
					int count = 0;
					System.Messaging.MessageEnumerator me = queue.GetMessageEnumerator2();
					while (me.MoveNext(new TimeSpan(0, 0, 0)))
					{
						count++;
					}

					result.Add(@"greenbook_activity", count.ToString());
				}
			}
			catch (Exception ex)
			{
			}

			return result;
		}
	}
}