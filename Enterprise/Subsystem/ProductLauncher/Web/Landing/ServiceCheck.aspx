<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ServiceCheck.aspx.cs" Inherits="RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.ServiceCheck" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%
	Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();

	Dictionary<string, string> serviceResult = CheckServices();
	if (serviceResult.Count > 0)
	{
		result.Add("services", serviceResult);
	}
	Dictionary<string, string> msmqResult = CheckMSMQ();
	if (msmqResult.Count > 0)
	{
		result.Add("logmessages", msmqResult);
	}
	Response.Write(JsonConvert.SerializeObject(result));
%>
