using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.SelfProvisioningPortal;

namespace UnifiedLogin.BusinessLogic.Logic.Product
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
        public ManageProductSelfProvisioningPortal(DefaultUserClaim userClaims) : base((int)ProductEnum.SelfProvisioningPortal, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _productId = (int)ProductEnum.SelfProvisioningPortal;
            _editorRealPageId = userClaims.UserRealPageGuid;

            _selfProvisioningPortaleUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "PRODUCTURL").Value;
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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageSelfProvisioningPortalUser", $"Setting product status to success. userPersonaId {userPersonaId}" });
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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSelfProvisioningPortalUser", $"Setting product status to deleted. userPersonaId {userPersonaId}" });
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