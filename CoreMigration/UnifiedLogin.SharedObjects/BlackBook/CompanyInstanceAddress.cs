namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// The Company instance address details
    /// </summary>
    public class CompanyInstanceAddress
    {
        private string _Country;

        /// <summary>
        /// The instance id for the company
        /// </summary>
        public int CompanyInstanceId { get; set; }
        /// <summary>
        /// Address
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// State
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Country
        /// </summary>
        public string Country
        {
            get
            {
                return string.IsNullOrEmpty(_Country) ? "United States of America" : _Country;
            }
            set
            {
                _Country = value;
            }
        }
        /// <summary>
        /// County
        /// </summary>
        public string County { get; set; }
        /// <summary>
        /// Postal Code
        /// </summary>
        public string PostalCode { get; set; }
        public bool IsPrimary { get; set; }
        public string LocationType { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
