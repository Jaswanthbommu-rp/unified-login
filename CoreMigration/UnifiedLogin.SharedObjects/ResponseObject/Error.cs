namespace UnifiedLogin.SharedObjects.ResponseObject
{
    public class Error
    {
        public string StatusCode { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
    }
}