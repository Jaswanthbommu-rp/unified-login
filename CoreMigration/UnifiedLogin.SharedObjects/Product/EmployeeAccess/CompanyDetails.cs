using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.EmployeeAccess
{
    public class CompanyDetails
    {
        private string _Country;

        /// <summary>
        /// The PartyId of the company
        /// </summary>
        public long PartyId { get; set; }

        /// <summary>
        /// Company name
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Phone Number
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Is the Company active
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Company RealPage Id
        /// </summary>
        public string CompanyRealPageId { get; set; }
        /// <summary>
        /// Company Id mapped to BlueBook
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// User RealPage Id
        /// </summary>
        public string UserRealPageId { get; set; }

        /// <summary>
        /// User Login As
        /// </summary>
        public string UserLoginAs { get; set; }

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
    }
}
