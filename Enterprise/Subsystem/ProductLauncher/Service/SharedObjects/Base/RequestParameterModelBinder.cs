using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base
{
	/// <summary>
	/// New model binder used to map the RequestParameter object to an incoming query string object that contains json data
	/// </summary>
	public class RequestParameterModelBinder : IModelBinder
	{

		/// <summary>
		/// Used to allow the usage of json data types for the sortby and filterby parameters of the RequestParameter object
		/// when it is being created in a webapi GET request from the query string
		/// </summary>
		/// <param name="actionContext"></param>
		/// <param name="bindingContext"></param>
		/// <returns></returns>
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof(RequestParameter))
			{
				return false;
			}

			// see if any of the properties are in use in the query string
			if (!FoundAttributes(bindingContext))
			{
				return false;
			}

			RequestParameter resultRequestParameter = new RequestParameter();
			bindingContext.Model = resultRequestParameter;

			bool useDataFilterObject = false;
			bool useResultsPerPage = false;
			bool useStartRow = false;
			bool useSortBy = false;
			bool useFilterBy = false;

			useDataFilterObject = processObject(bindingContext, "");
			useResultsPerPage = processInt(bindingContext, ".pages.resultsperpage");
			useStartRow = processInt(bindingContext, ".pages.startrow");
			useSortBy = processObject(bindingContext, ".sortby");
			useFilterBy = processObject(bindingContext, ".filterby");
			if (useDataFilterObject || useResultsPerPage || useStartRow || useSortBy || useFilterBy)
			{
				return true;
			}
			else
			{
				// return an empty object if nothing was found to bind to
				bindingContext.Model = null;
				return false;
			}
		}

		/// <summary>
		/// Used to see if any of the RequestParameter objects exist in the query string
		/// </summary>
		/// <param name="bindingContext"></param>
		/// <returns></returns>
		private bool FoundAttributes(ModelBindingContext bindingContext)
		{
			List<string> attributes = new List<string>() { "", ".filterby", ".sortby", ".pages.startrow", ".pages.resultsperpage" };
			foreach (string attr in attributes)
			{
				if (bindingContext.ValueProvider.GetValue(bindingContext.ModelName + attr) != null)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Used to bind the int data types to the RequestParameter object if they exist in the input
		/// </summary>
		/// <param name="bindingContext"></param>
		/// <param name="attributeName"></param>
		/// <returns></returns>
		private bool processInt(ModelBindingContext bindingContext, string attributeName)
		{
			RequestParameter parm = bindingContext.Model as RequestParameter;
			ValueProviderResult val;
			try
			{
				//string processString = val.RawValue as string;
				switch (attributeName)
				{
					case ".pages.resultsperpage":
						val = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + attributeName);
						if (val != null)
						{
							parm.Pages.ResultsPerPage = Convert.ToInt16(val.RawValue);
							return true;
						}
						break;
					case ".pages.startrow":
						val = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + attributeName);
						if (val != null)
						{
							parm.Pages.StartRow = Convert.ToInt16(val.RawValue);
							return true;
						}
						break;
				}
				return false;
			}
			catch (Exception)
			{
				bindingContext.ModelState.AddModelError(
				bindingContext.ModelName, "Cannot convert " + attributeName + " value to RequestParameter");
			}
			return false;
		}

		/// <summary>
		/// used to process the Dictionary data types
		/// </summary>
		/// <param name="bindingContext"></param>
		/// <param name="attributeName"></param>
		/// <returns></returns>
		private bool processObject(ModelBindingContext bindingContext, string attributeName)
		{
			RequestParameter parm = bindingContext.Model as RequestParameter;
			// exit out if the key isn't in the collection
			ValueProviderResult val = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + attributeName);
			if (val != null)
			{
				try
				{
					string processString = val.RawValue as string;
					// check the length to see if it looks like a base64 string
					try
					{
						if (processString.Replace(" ", "").Length % 4 == 0)
						{
							{
								byte[] processArray = Convert.FromBase64String(processString);
								processString = System.Text.Encoding.UTF8.GetString(processArray, 0, processArray.Length);
							}
						}
					}
					catch (Exception)
					{
						// if it throws an exception then it may not be base64 encoded. Try to process it as a string instead
					}
					// now attempt to get the dictionary out of the json object
					Dictionary<string, string> dictionary;
					switch (attributeName)
					{
						case "":
							bindingContext.Model = JsonConvert.DeserializeObject<RequestParameter>(processString);
							return true;
						case ".filterby":
							dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(processString);
							if (dictionary.Count > 0)
							{
								parm.FilterBy = dictionary;
								return true;
							}
							break;
						case ".sortby":
							dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(processString);
							if (dictionary.Count > 0)
							{
								parm.SortBy = dictionary;
								return true;
							}
							break;
					}
					return false;
				}
				catch (Newtonsoft.Json.JsonReaderException x)
				{
					bindingContext.ModelState.AddModelError(
						bindingContext.ModelName, "Invalid json was passed for " + attributeName + " value to RequestParameter");
				}
				catch (Exception)
				{
					bindingContext.ModelState.AddModelError(
						bindingContext.ModelName, "Cannot convert " + attributeName + " value to RequestParameter");
				}
			}
			return false;
		}
	}
}
