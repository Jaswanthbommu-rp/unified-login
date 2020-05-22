using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.SeniorLeadManagement;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Lead2Lease;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
    public sealed class SeniorLeadManagement : ManageProductInvokerBase, IManageProductIntegration
    {
        #region "Constructors"

        public SeniorLeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
        { }

        public SeniorLeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
            base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
        { }

        #endregion

        #region "Public Methods"

        protected override bool CheckUserExistInProduct(string loginNameToCheck, string baseUrlAndQuery = null)
        {
            if (baseUrlAndQuery == null)
                baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserExistEndpoint), loginNameToCheck);

            var response = GetResultFromApi<SLMUserExist>(baseUrlAndQuery, false);
            if (response != null && response.Message.Equals("User Not exists", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        protected override IntegrationProductUser GenerateProductUserObject(ProductUserRolePropertiesGroups userRolePropertiesRegion)
        {
            // Map user info
            var productUser = new IntegrationProductUser
            {
                UserId = string.IsNullOrEmpty(SubjectUserDetails.ProductUserId) ? "0" : SubjectUserDetails.ProductUserId,
                LoginName = string.IsNullOrEmpty(SubjectUserDetails.LoginName) ? SubjectUserDetails.LoginName : GetUniqueProductLogin(SubjectUserDetails.LoginName),
                CompanyId = CompanyInstanceSourceId,
                FirstName = SubjectUserDetails.FirstName,
                LastName = SubjectUserDetails.LastName,
                Email = SubjectUserDetails.Email,
                Phone = SubjectUserDetails.PhoneNumber,
                IsActive = true,
                IsMigratedUser = true,
                PropertyGroups = (userRolePropertiesRegion.PropertyGroupList == null) ? new List<string>() : userRolePropertiesRegion.PropertyGroupList,
                Properties = userRolePropertiesRegion.PropertyList,
                Roles = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
                PropertyRoles = userRolePropertiesRegion.PropertyRoleList,
                OrganizationRoles = userRolePropertiesRegion.OrganizationRoleList,
                CanReceiveMonthlyReport = userRolePropertiesRegion.CanReceiveMonthlyReport
            };

            if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.SuperUser)
            {
                ApplySuperUserData(productUser);
            }

            return productUser;
        }

        /// <summary>
        /// Returns Product Roles
        /// </summary>
        public override ListResponse GetProductRoles(RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
                if (isCompanyIdRequiredToQuery)
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "true");

                var roleList = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {roleList?.Count}");

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");
                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

                        var userRoles = user.RoleList;
                        this.MergeUserRoles(roleList, userRoles);
                    }
                }

                if (roleList == null)
                    throw new Exception("Null Role List.");

                return new ListResponse
                {
                    Records = roleList.Cast<object>().ToList(),
                    TotalRows = roleList.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }

        /// <summary>
        /// Returns Product Rights for a Company 
        /// </summary>
        /// <param name="dataFilter">Request parameters</param>
        /// <param name="baseUrlAndQuery">Base url</param>
        /// <returns>A response list</returns>
        public override ListResponse GetAllRights(RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Get All Roles method.");

                //Get all roles with the rights
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, true);
                var rolesRights = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                  $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {rolesRights?.Count}");


                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Get All Rights method.");

                //Get all rights by company
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRightEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                var allRights = GetResultFromApi<IList<ProductRight>>(baseUrlAndQuery);


                WriteToDiagnosticLog(
                  $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received all the rights with count = {allRights?.Count}");


                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");
                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

                        var userRoles = user.Roles;
                    }
                }

                if (allRights == null)
                    throw new Exception("Null Right List.");

                return new ListResponse
                {
                    Records = allRights.Cast<object>().ToList(),
                    TotalRows = allRights.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1,
                    Additional = AddRolesToRights(rolesRights, allRights)
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }

        #endregion

        #region "Private Methods"

        /// <summary>
        /// Assign the rolesid to the rights
        /// </summary>
        /// <param name="roles">Roles collection</param>
        /// <param name="rights">Rights collection</param>
        /// <returns>A dictionary with all rolesid</returns>
        private Dictionary<string, object> AddRolesToRights(IList<ProductRole> roles, IList<ProductRight> rights)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            List<Preset> presets = new List<Preset>();

            foreach (ProductRight right in rights)
            {
                List<int> rolesId = new List<int>();

                foreach (ProductRole rol in roles)
                {
                    if (rol.Rights.Any((p) => p.RightId.ToString() == right.GetRightId))
                    {
                        if (!rolesId.Contains(Convert.ToInt32(rol.GetRoleId)))
                        {
                            rolesId.Add(Convert.ToInt32(rol.GetRoleId));
                        }
                    }
                }

                if (!presets.Any(p => p.Id == Convert.ToInt32(right.GetRightId)))
                {
                    Preset preset = new Preset();

                    preset.Id = Convert.ToInt32(right.GetRightId);
                    preset.Name = right.GetName;
                    preset.RoleIds = rolesId;

                    presets.Add(preset);
                }
            }

            result.Add("Presets", presets.OrderBy(p=>p.Id).ToList());

            return result;
        }

        /// <summary>
        /// Direct call to product to change profile including isActive (mainly used to activate-deactivate from Migration tool)
        /// </summary>
        /// <param name="productUserProfile">Product user information</param>
        /// <returns>string.Empty if success else response contents.</returns>
        public override bool ExternalProductUserProfileChange(ProductUserProfile productUserProfile)
        {
            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.ExternalProductUserProfileChange - Product {ProductType} " +
                $"editorPersona id - {EditorUserDetails.PersonaId}, productUserProfile.UserId - {productUserProfile.UserId}. At beginning of the method.");

            switch ((ProductEnum)ProductId)
            {
                case ProductEnum.SeniorLeadManagement:

                    var userAux = _dataCollector.GetUserDetailsByPersona(long.Parse(productUserProfile.UserId), ProductId);

                    productUserProfile.PhoneNumbers = userAux.PhoneNumbers;
                    break;

                default:
                    break;
            }

            // used from external source (migration tool) so no activity logging required
            var result = ProductUserProfileChange(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }

            // log exception details from result
            WriteToErrorLog(
                $"ManageProductInvokerBase.ExternalProductUserProfileChange - Product {ProductType} " +
                $"editorPersona id - {EditorUserDetails.PersonaId} productUserProfile.UserId - {productUserProfile.UserId}. Result received - {result}.");

            return false;
        }

        #endregion 
    }
}
