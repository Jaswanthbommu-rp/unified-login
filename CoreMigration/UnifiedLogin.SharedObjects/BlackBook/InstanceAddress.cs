namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// The address details
    /// </summary>
    public class InstanceAddress
    {
        private string _Country;

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
        //public long Latitude { get; set; }
        //public long Longitude { get; set; }
    }
}
