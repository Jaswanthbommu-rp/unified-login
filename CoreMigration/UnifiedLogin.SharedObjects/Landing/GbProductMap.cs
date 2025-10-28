 namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Mapping class for GB product Id and Bluebook product code
    /// This class can be removed once all GB starts supporting Bluebook based Ids or Codes
    /// </summary>
    public class GbProductMap
    {
        /// <summary>
        /// GB ProductId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// GB Product Name - will get replaced with Bluebook product Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Bluebook product Code
        /// </summary>
        public string BooksProductCode { get; set; }

        /// <summary>
        /// UDM Source Code
        /// </summary>
        public string UDMSourceCode { get; set; }
    }
}
