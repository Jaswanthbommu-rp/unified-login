using System;
using System.IO;
using System.Net;
using System.Text;

namespace RPBooksApiTestingFramework.Controllers
{
    public enum HttpVerb
    {
        Get,
        Post,
        Put,
        Delete,
		Patch // Added for Green Book
    }
    public class RestController
	{
		public string EndPoint { get; set; }
		public HttpVerb Method { get; set; }
		public string ContentType { get; set; }
		public string Accept { get; set; }
		public string Authorization { get; set; }
		public string PostData { get; set; }
		public RestController(string endpoint, HttpVerb method, string contentType, string accept, string postData, string authorization)
		{
			EndPoint = endpoint;
			Method = method;
			ContentType = contentType;
			Accept = accept;
			Authorization = authorization;
			PostData = postData;
		}

        public RestController(string endpoint, HttpVerb method, string contentType, string accept, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            Accept = accept;
            PostData = postData;
        }

        public Tuple<HttpStatusCode, string> MakeRequestHttpResponseIdentity()
        {
            HttpWebResponse responseValue = (HttpWebResponse)(new WebException()).Response;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(EndPoint);
                request.Method = Method.ToString();
                request.ContentLength = 0;
                request.ContentType = ContentType;
                request.Accept = Accept;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36";
                //ToDo : updated as soon as we have auth
                //if (Authorization != null) request.Headers.Add(Authorization);
                //request.Headers.Add("Authorization", Authorization);

                if (!string.IsNullOrEmpty(PostData) && (Method == HttpVerb.Post || Method == HttpVerb.Put
                    || Method == HttpVerb.Patch)) // Added for Green Book
                {
                    //var encoding = new UTF8Encoding();
                    var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                    request.ContentLength = bytes.Length;
                    request.ContentLength = PostData.Length;

                    using (var writeStream = request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }

                request.KeepAlive = false; // Avoids "The server committed a protocol violation. Section=ResponseStatusLine" ERROR.
                responseValue = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                String baseExcep = e.GetBaseException().GetType().FullName;

                if (baseExcep != null && baseExcep.Equals("System.Net.WebException"))
                {
                    responseValue = (HttpWebResponse)((WebException)e).Response;
                }
            }

            string response = "";
            var responseStream = responseValue.GetResponseStream();
            if (responseStream != null)
                using (var reader = new StreamReader(responseStream))
                {
                    response = reader.ReadToEnd();
                }
            responseValue.Close();
            return Tuple.Create(responseValue.StatusCode, response);
        }




        public Tuple<HttpStatusCode, string> MakeRequestHttpResponse()
        {
			HttpWebResponse responseValue = (HttpWebResponse)(new WebException()).Response;
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(EndPoint);
				request.Method = Method.ToString();
				request.ContentLength = 0;
				request.ContentType = ContentType;
				request.Accept = Accept;
				request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36";
				//ToDo : updated as soon as we have auth
				//if (Authorization != null) request.Headers.Add(Authorization);
				request.Headers.Add("Authorization", Authorization);

				if (!string.IsNullOrEmpty(PostData) && (Method == HttpVerb.Post || Method == HttpVerb.Put
					|| Method == HttpVerb.Patch)) // Added for Green Book
				{
					//var encoding = new UTF8Encoding();
					var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
					request.ContentLength = bytes.Length;
					request.ContentLength = PostData.Length;

					using (var writeStream = request.GetRequestStream())
					{
						writeStream.Write(bytes, 0, bytes.Length);
					}
				}

				request.KeepAlive = false; // Avoids "The server committed a protocol violation. Section=ResponseStatusLine" ERROR.
				responseValue = (HttpWebResponse)request.GetResponse();
			}
			catch (Exception e)
			{
				String baseExcep = e.GetBaseException().GetType().FullName;

				if (baseExcep != null && baseExcep.Equals("System.Net.WebException"))
				{
					responseValue = (HttpWebResponse)((WebException)e).Response;
				}
			}

			string response = "";
			var responseStream = responseValue.GetResponseStream();
			if (responseStream != null)
				using (var reader = new StreamReader(responseStream))
				{
					response = reader.ReadToEnd();
				}
			responseValue.Close();
			return Tuple.Create(responseValue.StatusCode, response);
        }
    }
}
