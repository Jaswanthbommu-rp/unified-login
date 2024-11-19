using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.SeniorLeadManagement;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Lead2Lease;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
    public sealed class SeniorLeadManagement : StandardV1ProductIntegration, IManageProductIntegration
    {
        #region "Constructors"

        public SeniorLeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base((int)productType, editorPersonaId, subjectPersonaId, userClaims)
        { }

        public SeniorLeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
            base((int)productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
        { }

        #endregion

        #region "Public Methods"

        protected override void ApplyApiSecurity()
        {
            string apiKey = ProductInternalSettingList.First(a => a.Name.ToUpper() == "APIKEY").Value;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-ExternalClientId", apiKey);
        }

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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}" });

                bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
                if (isCompanyIdRequiredToQuery)
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "true");

                var roleList = GetResultFromApi<IList<Model.ProductRole>>(baseUrlAndQuery);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {roleList?.Count}" });

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });
                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" }, exception: ex);
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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });
                
                //Get all roles with the rights
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, true);
                var rolesRights = GetResultFromApi<IList<Model.ProductRole>>(baseUrlAndQuery);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {rolesRights?.Count}" });

                if (rolesRights == null)
                    throw new Exception("Null Right List.");

                List<string> slmRights = new List<string>();

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });
                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

                        slmRights = user.Roles;
                    }
                }

                List<ProductRightRole> records = AddRightsRole(rolesRights,slmRights);
                Dictionary<string, object> additional = AddRolesToRights(rolesRights);

                return new ListResponse
                {
                    Records = records.Cast<object>().ToList(),
                    TotalRows = records.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1,
                    Additional = additional
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" }, exception: ex);
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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ExternalProductUserProfileChange", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}, productUserProfile.UserId - {productUserProfile.UserId}. At beginning of the method." });

            productUserProfile.PhoneNumbers = _dataCollector.GetUserDetailsByPersona(_userClaims.PersonaId, ProductId).PhoneNumbers;

            // used from external source (migration tool) so no activity logging required
            var result = ProductUserProfileChange(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }

            // log exception details from result
            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ExternalProductUserProfileChange", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId} productUserProfile.UserId - {productUserProfile.UserId}. Result received - {result}." });

            return false;
        }

        /// <summary>
        /// Create or update product user
        /// Gets called from Product-Batch
        /// </summary> 
        public override string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            string result;

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method." });

            if (SubjectUserDetails.UserRoleTypeId != (int)UserRoleType.SuperUser)
            {
                userRolePropertiesRegion.OneSiteUserInfo = GetOneSiteUserInfo(userRolePropertiesRegion.PropertyList);
            }

            // Get product user object 
            var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

            if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser." });

                // get a login name that isn't in use for the new user
                bool foundUserName = false;
                int incrementor = 0;
                string lastNameNoWhiteSpace = SubjectUserDetails.LastName.TrimWhiteSpace();
                string newproductUsername = (SubjectUserDetails.FirstName.TrimWhiteSpace().Substring(0, 1) + lastNameNoWhiteSpace.Substring(0, (lastNameNoWhiteSpace.Length >= 19 ? 19 : lastNameNoWhiteSpace.Length))).ToLower();
                string accountingLoginName = newproductUsername;

                // give up after 10 tries
                while (!foundUserName)
                {
                    if (CheckUserExistInProduct(accountingLoginName))
                    {
                        incrementor++;
                        accountingLoginName = newproductUsername.Substring(0, (newproductUsername.Length >= 48 ? 48 : newproductUsername.Length)) + incrementor.ToString();
                    }
                    else
                    {
                        foundUserName = true;
                        newProductUser.LoginName = accountingLoginName;

                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Generated accountingLoginName = {accountingLoginName}" });
                    }
                }

                // Create User
                result = CreateUser(newProductUser, out additionalParameters);
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser." });
                // Update user with Id/Login from product
                newProductUser.UserId = SubjectUserDetails.ProductUserId;
                newProductUser.LoginName = SubjectUserDetails.ProductUserName;

                result = UpdateUser(newProductUser, batchProcessType, out additionalParameters);
            }

            return result;
        }

        #endregion

        #region "Private Methods"

        /// <summary>
        /// Assign the rolesid to the rights
        /// </summary>
        /// <param name="roles">Roles collection</param>
        /// <returns>A dictionary with all rolesid</returns>
        private Dictionary<string, object> AddRolesToRights(IList<Model.ProductRole> roles)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            List<Preset> presets = new List<Preset>();
            List<Right> finalRights = new List<Right>();

            foreach (Model.ProductRole rol in roles)
            {
                if (!presets.Any(p => p.Id == Convert.ToInt32(rol.GetRoleId)))
                {
                    Preset preset = new Preset();
                    List<int> rolesId = new List<int>();

                    preset.Id = Convert.ToInt32(rol.GetRoleId);
                    preset.Name = rol.GetName;
                    
                    foreach (Right right in rol.Rights)
                    {
                        if (!rolesId.Contains(right.RightId))
                        {
                            rolesId.Add(right.RightId);
                        }
                    }

                    preset.RoleIds = rolesId.OrderBy(p=>p).ToList();

                    presets.Add(preset);
                }
            }

            result.Add("Presets", presets.OrderBy(p => p.Id).ToList());

            return result;
        }

        /// <summary>
        /// Add the info to rigths depending of roles
        /// </summary>
        /// <param name="roles">Product role data</param>
        /// <param name="slmRights">Slm rights</param>
        /// <returns>A right role list</returns>
        private List<ProductRightRole> AddRightsRole(IList<Model.ProductRole> roles, List<string> slmRights)
        {
            List<ProductRightRole> result = new List<ProductRightRole>();

            foreach (Model.ProductRole rol in roles)
            {
                foreach (Right right in rol.Rights)
                {
                    if (!result.Any(p => p.RightId == right.RightId.ToString()))
                    {
                        result.Add(new ProductRightRole()
                        {
                            RightId = right.RightId.ToString(),
                            RightName = right.RightName,
                            RoleType = ""
                        });
                    }
                }
            }

            foreach (ProductRightRole right in result)
            {
                if (slmRights.Contains(right.RightId))
                {
                    right.IsAssigned = true;
                }
            }

            return result.OrderBy(p=> p.RightName).ToList();
        }

        /// <summary>
        /// Get the information of properties
        /// </summary>
        /// <param name="propertiesListSLM">Property list of SLM from UI</param>
        /// <returns>A onesiteuserinfo entity</returns>
        private OneSiteUserInfo GetOneSiteUserInfo(List<string> propertiesListSLM)
        {
            OneSiteUserInfo oneSiteUserInfo = new OneSiteUserInfo();
            oneSiteUserInfo.Properties = new List<string>();

            IManagePerson _managePerson = new ManagePerson();
            IManagePersona _managePersona = new ManagePersona();

            Persona userPersona = _managePersona.GetPersona(SubjectUserDetails.PersonaId);
            Guid realPageId = userPersona.RealPageId;

            var person = _managePerson.GetPerson(realPageId);

            //Override GetProductProperties 
            IList<ProductPropertiesSLM> allPropertyListSLM = this.GetAllProductProperties();

            // walk the list of properties sent to be saved to the user 
            foreach (string propertyIdSLM in propertiesListSLM)
            {
                // find the property being added in the main list and see if it has a OneSite ID associated to it
                if (allPropertyListSLM.Any(a => a.GetPropertyId.ToString() == propertyIdSLM))
                {
                    ProductPropertiesSLM productPropertySLM = (from a in allPropertyListSLM
                                                               where a.GetPropertyId.ToString() == propertyIdSLM
                                                               select a).FirstOrDefault();
                    if (productPropertySLM != null)
                    {
                        //Exists
                        if (!string.IsNullOrWhiteSpace(productPropertySLM.OneSitePropertyId))
                        {
                            oneSiteUserInfo.Properties.Add(productPropertySLM.OneSitePropertyId);
                        }
                    }
                }
            }

            bool isLeasingAgentInOneSite = false;

            // OneSite super users aren't assigned the Leasing Consultant right so no need to check for the right for a GB Super User
            if (oneSiteUserInfo.Properties.Any())
            {
                SamlRepository _samlRepository = new SamlRepository();

                // 01. See if the L2L user is also a OneSite user
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(SubjectUserDetails.PersonaId, (int)ProductEnum.OneSite);

                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    var oneSiteSystemIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();

                    var _mpOneSite = new ManageProductOneSite(_userClaims);

                    var OSUser = _mpOneSite.GetOneSiteUserInfo(oneSiteSystemIdentifier);

                    //02. We get information about the OneSite user including the list of properties for that user
                    var response = _mpOneSite.GetOneSitePropertyList(EditorUserDetails.PersonaId, SubjectUserDetails.PersonaId, true, null);
                    var osPropertyList = response.Records.Cast<ProductProperty>().ToList();
                    bool didLeasingAgentOneSiteCheck = false;

                    //03. For each of the properties that we are saving for SLM
                    //We check to see if the property has the "PMSystemID", which is the property id in OneSite                    
                    foreach (string oneSiteUserInfoProperty in oneSiteUserInfo.Properties)
                    {
                        if (osPropertyList.Any(a => a.ID == oneSiteUserInfoProperty))
                        {
                            // the SLM system id appears to be a OneSite site id, so see if this user has the Leasing Consultant right
                            if (!didLeasingAgentOneSiteCheck)
                            {
                                isLeasingAgentInOneSite = _mpOneSite.UserInLeasingAgentList(EditorUserDetails.PersonaId, SubjectUserDetails.PersonaId, Convert.ToInt32(oneSiteUserInfoProperty));

                                didLeasingAgentOneSiteCheck = true;
                            }
                            if (isLeasingAgentInOneSite)
                            {
                                oneSiteUserInfo.UserId = Convert.ToInt32(OSUser.UserId);
                                oneSiteUserInfo.FirstName = person.FirstName;
                                oneSiteUserInfo.LastName = person.LastName;
                                break;
                            }
                        }
                    }
                }
            }

            if (!isLeasingAgentInOneSite)
            {
                return null;
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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}" });

                propertyList = GetResultFromApi<IList<ProductPropertiesSLM>>(baseUrlAndQuery);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}" });

                if (propertyList == null)
                    throw new Exception("Null Property List.");

            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" }, exception: ex);
            }

            return propertyList;
        }

        #endregion

    }
}
