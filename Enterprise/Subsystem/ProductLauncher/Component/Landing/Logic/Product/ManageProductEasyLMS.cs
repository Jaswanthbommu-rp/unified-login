using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
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
		/// <param name="editorRealPageId">The RealPageId of the editor</param>
		public ManageProductEasyLMS(DefaultUserClaim userClaims) : base((int)ProductEnum.EasyLMS, userClaims, null, null)
		{
			_productId = (int)ProductEnum.EasyLMS;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_blueBook = new Logic.ManageBlueBook();
		}

		/// <summary>
		/// Unit test constructor to test Levels, Notifications, and MessageGroups
		/// </summary>
		/// <param name="editorRealPageId">The RealPageId of the editor</param>
		/// <param name="samlRepository">SAML Repository</param>
		/// <param name="managePersona">Persona business logic</param>
		/// <param name="manageBlueBook">BlueBook business logic</param>
		/// <param name="productRepository">Product Repository</param>
		/// <param name="productInternalSettingRepository">Product Internal Setting Repository</param>
		/// <param name="managePerson">Person business logic</param>
		/// <param name="manageUserLogin">UserLogin business logic</param>
		/// <param name="managePartyRelationship">Party Relationship business logic</param>
		/// <param name="manageElectronicAddress">Electronic Address business logic</param>
		public ManageProductEasyLMS(Guid editorRealPageId, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship, IManageElectronicAddress manageElectronicAddress) : base((int)ProductEnum.ResidentPortal, productInternalSettingRepository, productRepository)
		{
			_editorRealPageId = editorRealPageId;
			_samlRepository = samlRepository;
			_managePersona = managePersona;
			_managePerson = managePerson;
			_manageUserLogin = manageUserLogin;
			_blueBook = manageBlueBook;
			_productRepository = productRepository;
			_productInternalSettingRepository = productInternalSettingRepository;
			_managePartyRelationship = managePartyRelationship;
			_manageElectronicAddress = manageElectronicAddress;
		}

		/// <summary>
		/// Unit test constructor to test list properties
		/// </summary>
		/// <param name="editorRealPageId">The RealPageId of the editor</param>
		/// <param name="companyInstanceId">Company Id</param>
		/// <param name="samlRepository">SAML Repository</param>
		/// <param name="managePersona">Persona business logic</param>
		/// <param name="manageBlueBook">BlueBook business logic</param>
		/// <param name="productRepository">Product Repository</param>
		/// <param name="productInternalSettingRepository">Product Internal Setting Repository</param>
		/// <param name="managePerson">Person business logic</param>
		/// <param name="manageUserLogin">UserLogin business logic</param>
		/// <param name="managePartyRelationship">Party Relationship business logic</param>
		public ManageProductEasyLMS(Guid editorRealPageId, long companyInstanceId, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship) : base((int)ProductEnum.ResidentPortal, productInternalSettingRepository, productRepository)
		{
			_editorRealPageId = editorRealPageId;
			_companyInstanceId = companyInstanceId;
			_samlRepository = samlRepository;
			_managePersona = managePersona;
			_managePerson = managePerson;
			_manageUserLogin = manageUserLogin;
			_blueBook = manageBlueBook;
			_productRepository = productRepository;
			_productInternalSettingRepository = productInternalSettingRepository;
			_managePartyRelationship = managePartyRelationship;
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