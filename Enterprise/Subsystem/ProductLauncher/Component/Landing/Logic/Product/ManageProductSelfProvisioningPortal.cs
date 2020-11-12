using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.SelfProvisioningPortal;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
	/// <summary>
	/// 
	/// </summary>
	public class ManageProductSelfProvisioningPortal : ManageProductBase, IManageProductSelfProvisioningPortal
    {
        #region Private Variables
        private string _selfProvisioningPortaleUrl;
        private long _companyInstanceId;

        private IList<ProductSettingList> _userProductSettings = new List<ProductSettingList>();
        private SelfProvisioningPortal _selfProvisioningPortal = new SelfProvisioningPortal(); 

        #endregion

        #region Constructor
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="editorRealPageId">The RealPageId of the editor</param>
        public ManageProductSelfProvisioningPortal(DefaultUserClaim userClaims) : base((int)ProductEnum.SelfProvisioningPortal, userClaims, null, null)
        {
            _productId = (int)ProductEnum.SelfProvisioningPortal;
            _editorRealPageId = userClaims.UserRealPageGuid;

            _selfProvisioningPortaleUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "PRODUCTURL").Value;
        }

        /// <summary>
        /// Unit test constructor to test list roles
        /// </summary>
        /// <param name="editorRealPageId">The RealPageId of the editor</param>
        /// <param name="selfProvisioningPortal">self-Provisioning Portal User object</param>
        /// <param name="samlRepository">SAML Repository</param>
        /// <param name="managePersona">Persona business logic</param>
        /// <param name="manageBlueBook">BlueBook business logic</param>
        /// <param name="productRepository">Product Repository</param>
        /// <param name="productInternalSettingRepository">Product Internal Setting Repository</param>
        /// <param name="managePerson">Person business logic</param>
        /// <param name="manageUserLogin">UserLogin business logic</param>
        /// <param name="managePartyRelationship">Party Relationship business logic</param>
        public ManageProductSelfProvisioningPortal(Guid editorRealPageId, SelfProvisioningPortal selfProvisioningPortal, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship) : base((int)ProductEnum.SelfProvisioningPortal, productInternalSettingRepository, productRepository)
        {
            _editorRealPageId = editorRealPageId;
            _selfProvisioningPortal = selfProvisioningPortal;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _managePerson = managePerson;
            _manageUserLogin = manageUserLogin;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
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
        public ManageProductSelfProvisioningPortal(Guid editorRealPageId, long companyInstanceId, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship) : base((int)ProductEnum.SelfProvisioningPortal, productInternalSettingRepository,productRepository)
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
        /// Assign User in GreenBook access to Self Provisioning Portal
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="selfProvisioningPortal">Used to grant access to Self Provisioning Portal Product.</param>
        /// <returns>ObjectOutput object</returns>
        public ObjectOutput<ISelfProvisioningPortal, IErrorData> ManageSelfProvisioningPortalUser(long editorPersonaId, long userPersonaId, SelfProvisioningPortal selfProvisioningPortal)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            ObjectOutput<ISelfProvisioningPortal, IErrorData> output = new ObjectOutput<ISelfProvisioningPortal, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                errorStatus.Success = false;
                errorStatus.ErrorMsg = listResponse.ErrorReason;
                output.Status = errorStatus;
                return output;
            }

            WriteToDiagnosticLog("ManageProductSelfProvisioningPortal.ManageSelfProvisioningPortalUser - Setting product result to success");
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

            // log product user updated activity  
            WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                "User {0} {1} access is granted for product {2} by user {3} {4}.");

            errorStatus.Success = true;
            errorStatus.ErrorMsg = "";
            output.Status = errorStatus;
            output.obj = selfProvisioningPortal;
            return output;
        }

        /// <summary>
        /// Unassign User in GreenBook from Self Provisioning Portal
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="selfProvisioningPortal">Used to remove access to Self Provisioning Portal Product.</param>
        /// <returns>ObjectOutput object</returns>
        public ObjectOutput<ISelfProvisioningPortal, IErrorData> UnassignSelfProvisioningPortalUser(long editorPersonaId, long userPersonaId, SelfProvisioningPortal selfProvisioningPortal)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            ObjectOutput<ISelfProvisioningPortal, IErrorData> output = new ObjectOutput<ISelfProvisioningPortal, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                errorStatus.Success = false;
                errorStatus.ErrorMsg = listResponse.ErrorReason;
                output.Status = errorStatus;
                return output;
            }

            WriteToDiagnosticLog($"ManageProductSelfProvisioningPortal.UnassignSelfProvisioningPortalUser userPersonaId: {userPersonaId}");
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

            // log product user updated activity  
            WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                "User {0} {1} access is removed for product {2} by user {3} {4}.");

            errorStatus.Success = true;
            errorStatus.ErrorMsg = "";
            output.Status = errorStatus;
            output.obj = selfProvisioningPortal;
            return output;
        }
        #endregion
    }
}