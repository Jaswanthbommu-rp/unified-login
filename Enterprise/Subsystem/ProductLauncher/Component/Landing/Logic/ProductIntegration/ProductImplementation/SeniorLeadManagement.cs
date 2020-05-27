using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.SeniorLeadManagement;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Lead2Lease;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;

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
                CanReceiveMonthlyReport = userRolePropertiesRegion.CanReceiveMonthlyReport,
                PhoneNumbers = SubjectUserDetails.PhoneNumbers,
                OneSiteUserInfo = userRolePropertiesRegion.OneSiteUserInfo
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

                var roleList = GetResultFromApi<IList<Model.ProductRole>>(baseUrlAndQuery);

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
                var rolesRights = GetResultFromApi<IList<Model.ProductRole>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                  $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {rolesRights?.Count}");


                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Get All Rights method.");

                //Get all rights by company
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRightEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                var allRights = GetResultFromApi<IList<Model.ProductRight>>(baseUrlAndQuery);


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

            productUserProfile.PhoneNumbers = _dataCollector.GetUserDetailsByPersona(_userClaims.PersonaId, ProductId).PhoneNumbers;

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

        /// <summary>
        /// Create or update product user
        /// Gets called from Product-Batch
        /// </summary> 
        public override string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            string result;

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method.");

            if (SubjectUserDetails.UserRoleTypeId != (int)UserRoleType.SuperUser)
            {
                userRolePropertiesRegion.OneSiteUserInfo = GetOneSiteUserInfo(userRolePropertiesRegion.PropertyList);
            }

            // Get product user object 
            var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

            if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser.");

                // Get User & check if already exist 
                bool isUserExistInProduct = CheckUserExistInProduct(newProductUser.LoginName);
                if (isUserExistInProduct)
                {
                    WriteToErrorLog(
                        $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Product User {newProductUser.LoginName} already exist.");

                    return $"{newProductUser.LoginName} already exist in the product {ProductType}.";
                }

                // Create User
                result = CreateUser(newProductUser);
            }
            else
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser.");
                // Update user with Id/Login from product
                newProductUser.UserId = SubjectUserDetails.ProductUserId;
                newProductUser.LoginName = SubjectUserDetails.ProductUserName;

                result = UpdateUser(newProductUser, batchProcessType);
            }

            return result;
        }

        #endregion

        #region "Private Methods"

        /// <summary>
        /// Assign the rolesid to the rights
        /// </summary>
        /// <param name="roles">Roles collection</param>
        /// <param name="rights">Rights collection</param>
        /// <returns>A dictionary with all rolesid</returns>
        private Dictionary<string, object> AddRolesToRights(IList<Model.ProductRole> roles, IList<Model.ProductRight> rights)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            List<Preset> presets = new List<Preset>();

            foreach (Model.ProductRight right in rights)
            {
                List<int> rolesId = new List<int>();

                foreach (Model.ProductRole rol in roles)
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

            result.Add("Presets", presets.OrderBy(p => p.Id).ToList());

            return result;
        }

        /// <summary>
        /// Get the information of properties
        /// </summary>
        /// <param name="propertyList">Property list of SLM from UI</param>
        /// <returns>A onesiteuserinfo entity</returns>
        private OneSiteUserInfo GetOneSiteUserInfo(List<string> propertyList)
        {
            OneSiteUserInfo oneSiteUserInfo = new OneSiteUserInfo();
            oneSiteUserInfo.Properties = new List<string>();

            IManagePerson _managePerson = new ManagePerson();
            IManagePersona _managePersona = new ManagePersona();

            Persona userPersona = _managePersona.GetPersona(SubjectUserDetails.PersonaId);
            Guid realPageId = userPersona.RealPageId;

            var person = _managePerson.GetPerson(realPageId);

            //Override GetProductProperties 
            IList<ProductPropertiesSLM> allPropertyList = this.GetAllProductProperties();

            // walk the list of properties sent to be saved to the user 
            foreach (string prptyId in propertyList)
            {
                ////if (isSuperUser)
                ////{
                ////    propertyListToSave.Add(new Property() { PropertyId = Convert.ToInt32(prptyId) });
                ////    continue;
                ////}

                // find the property being added in the main list and see if it has a OneSite ID associated to it
                if (allPropertyList.Any(a => a.GetPropertyId.ToString() == prptyId))
                {
                    ProductPropertiesSLM p = (from a in allPropertyList
                                              where a.GetPropertyId.ToString() == prptyId
                                              select a).FirstOrDefault();
                    if (p != null)
                    {
                        //Exists
                        if (string.IsNullOrWhiteSpace(p.OneSitePropertyId))
                        {
                            oneSiteUserInfo.Properties.Add(p.OneSitePropertyId);
                        }

                        //ProductPropertiesSLM toAdd = new ProductPropertiesSLM() { PropertyId = p.PropertyId, PMSystemID = p.PMSystemID };
                        //if (!string.IsNullOrEmpty(p.PMSystemID))
                        //{
                        //    checkOneSiteUserInfo = true;
                        //}
                        //propertyListToSave.Add(toAdd);
                    }
                }
            }

            // OneSite super users aren't assigned the Leasing Consultant right so no need to check for the right for a GB Super User
            if (oneSiteUserInfo.Properties.Any())
            {
                SamlRepository _samlRepository = new SamlRepository();

                // See if the L2L user is also a OneSite user
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(SubjectUserDetails.PersonaId, (int)ProductEnum.OneSite);

                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    var oneSiteSystemIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();

                    var _mpOneSite = new ManageProductOneSite(_userClaims);

                    var OSUser = _mpOneSite.GetOneSiteUserInfo(oneSiteSystemIdentifier);

                    var response = _mpOneSite.GetOneSitePropertyList(EditorUserDetails.PersonaId, SubjectUserDetails.PersonaId, true, null);
                    var osPropertyList = response.Records.Cast<ProductProperty>().ToList();
                    bool isLeasingAgentInOneSite = false;
                    bool didLeasingAgentOneSiteCheck = false;

                    foreach (string p in oneSiteUserInfo.Properties)
                    {
                        //if (!string.IsNullOrEmpty(p.PMSystemID))
                        //{
                        if (osPropertyList.Any(a => a.ID == p))
                        {
                            // the L2L system id appears to be a OneSite site id, so see if this user has the Leasing Consultant right
                            if (!didLeasingAgentOneSiteCheck)
                            {
                                isLeasingAgentInOneSite = _mpOneSite.UserInLeasingAgentList(EditorUserDetails.PersonaId, SubjectUserDetails.PersonaId, Convert.ToInt32(p));

                                didLeasingAgentOneSiteCheck = true;
                            }
                            if (isLeasingAgentInOneSite)
                            {
                                oneSiteUserInfo.UserId = Convert.ToInt32(OSUser.UserId);
                                oneSiteUserInfo.FirstName = person.FirstName;
                                oneSiteUserInfo.LastName = person.LastName;
                                break;
                            }
                            else
                            {
                                //p.PMSystemID = null;
                            }
                            //}
                        }
                    }
                }
            }

            return oneSiteUserInfo;
        }

        /// <summary>
        /// Returns Product Properties
        /// </summary> 
        private IList<ProductPropertiesSLM> GetAllProductProperties(string baseUrlAndQuery = null)
        {
            IList<ProductPropertiesSLM> propertyList = null;

            try
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                }

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                propertyList = GetResultFromApi<IList<ProductPropertiesSLM>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}");

                if (propertyList == null)
                    throw new Exception("Null Property List.");

            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
            }

            return propertyList;
        }

        #endregion

    }
}
