using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    /// Used to Get EasyLMS Company API Code and Key from BlueBook
    /// </summary>
    public class ManageProductEasyLMS : ManageProductBase, IManageProductEasyLMS
	{
		#region Private Variables
		private long _companyInstanceId;
		private ListResponse _listResponse = new ListResponse();
        #endregion

        #region Constructor
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="userClaims">The claims of the editor</param>
        public ManageProductEasyLMS(DefaultUserClaim userClaims) : base((int)ProductEnum.EasyLMS, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
			_productId = (int)ProductEnum.EasyLMS;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_blueBook = new Logic.ManageBlueBook(userClaims);
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Get EasyLMS Company InstanceId from BlueBook
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>CompanyMap object</returns>
		public CustomerCompanyMap GetCompanyAPICodeAndKey(long editorPersonaId, long userPersonaId)
		{
			_listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			return GetEasyLMSCompanyInstanceId();
		}

		/// <summary>
		/// Get Productname
		/// </summary>
		/// <param name="productId">Product Id</param>
		/// <returns>string ProductName</returns>
		public string getProductName(ProductEnum productId)
		{
			string productName = string.Empty;
			GbProductMap booksProductDetail = _productRepository.GetBooksMasterProductDetail((int)productId);
			if (booksProductDetail != null)
			{
				productName = booksProductDetail.Name;
			}
			return productName;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Get EasyLMS Company InstanceId from BlueBook
		/// </summary>
		/// <returns>CompanyMap object</returns>
		private CustomerCompanyMap GetEasyLMSCompanyInstanceId()
		{
			var udmSourceCode = _productDetails.UDMSourceCode?.Length > 0 ? _productDetails.UDMSourceCode : _productDetails.BooksProductCode;
			return GetProductCompanyInstanceId(udmSourceCode, null, useTranslate: false);
		}
		#endregion
	}
}