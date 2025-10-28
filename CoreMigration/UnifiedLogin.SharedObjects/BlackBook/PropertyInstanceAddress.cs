namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// The Company instance address details
    /// </summary>
    public class PropertyInstanceAddress
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

        /// <summary>
        /// Latitude
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public decimal Longitude { get; set; }
    }
}
