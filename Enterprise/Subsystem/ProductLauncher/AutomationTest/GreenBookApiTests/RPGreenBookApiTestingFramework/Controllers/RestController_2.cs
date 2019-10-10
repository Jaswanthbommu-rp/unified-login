using System.Net;
using System.Text;

namespace RPBooksApiTestingFramework.Controllers
{
    public enum HttpVerb_2
    {
        Get,
        Post,
        Put,
        Delete,
		Patch // Added for Green Book
    }
    public class RestController_2
    {
        public string EndPoint { get; set; }
        public HttpVerb Method { get; set; }
        public string ContentType { get; set; }
        public string Accept { get; set; }
        public string Authorization { get; set; }
        public string PostData { get; set; }
        public string AccessToken  { get; set; }

        public RestController_2()
        {
            EndPoint = "";
            Method = HttpVerb.Get;
            ContentType = "application/json";
            Accept = "application/json";
            PostData = "";
        }
  
        public RestController_2(string endpoint, HttpVerb method, string contentType, string accept, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            Accept = accept;
            PostData = postData;
        }
        public RestController_2(string endpoint, HttpVerb method, string contentType, string accept, string authorization, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            Accept = accept;
            Authorization = authorization;
            PostData = postData;
        }

        public HttpWebResponse MakeRequestHttpResponse()
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPoint);
            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;
            request.Accept = Accept;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36";
            //ToDo : updated as soon as we have auth
            //if (Authorization != null) request.Headers.Add(Authorization);
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

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

            var responseValue = (HttpWebResponse)request.GetResponse();

            return responseValue;
        }


    }
}
