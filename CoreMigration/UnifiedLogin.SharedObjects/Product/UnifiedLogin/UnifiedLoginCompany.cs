namespace UnifiedLogin.SharedObjects.Product.UnifiedLogin
{
    public class UnifiedLoginCompany
    {
        /// <summary>
        /// The PartyId of the company
        /// </summary>
        public long PartyId { get; set; }

        /// <summary>
        /// Company name
        /// </summary>
        public string CompanyName { get; set; }

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
        /// Used to store the BlackBook Company master id for the organization - RPUP id
        /// </summary>
        public long BooksCustomerMasterId { get; set; }

        /// <summary>
        /// User RealPage Id
        /// </summary>
        public string UserRealPageId { get; set; }

        /// <summary>
        /// User Login As
        /// </summary>
        public string UserLoginAs { get; set; }

        /// <summary>
        /// The domain of the company
        /// </summary>
        public string Domain { get; set; }
    }
}
