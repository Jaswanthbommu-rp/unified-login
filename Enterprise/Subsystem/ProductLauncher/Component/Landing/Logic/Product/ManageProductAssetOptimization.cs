using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// 
    /// </summary>
    public class ManageProductAssetOptimization : ManageProductBase, IManageProductAssetOptimization
    {
        #region Private members

        private IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly string _apiUser;
        private readonly string _apiPassword;
        private readonly string _apiEndPoint;
        private readonly string _aoSuperUser;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="editorRealPageId">Real page Id of user</param>
        public ManageProductAssetOptimization(Guid editorRealPageId) : base((int) ProductEnum.AssetOptimizer, null)
        {
            WriteToDiagnosticLog("ManageProductAssetOptimization.Ctor - Getting Product settings.");
            _productId = (int) ProductEnum.AssetOptimizer;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _editorRealPageId = editorRealPageId;

            _blueBook = new ManageBlueBook();

            _apiEndPoint = _productInternalSettingList.First(a => a.Name.Equals("APIEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            _apiUser = _productInternalSettingList.First(a => a.Name.Equals("APIUserName", StringComparison.OrdinalIgnoreCase)).Value;
            _apiPassword =
                Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        _productInternalSettingList.First(a => a.Name.Equals("APIPassword", StringComparison.OrdinalIgnoreCase)).Value));
            _aoSuperUser = _productInternalSettingList.First(a => a.Name.Equals("ProductSuperUserLoginName", StringComparison.OrdinalIgnoreCase)).Value;

            WriteToDiagnosticLog("ManageProductAssetOptimization.Ctor - Received Product settings.");
        }

        #endregion

        #region Public Methods
        /// <summary>
        ///Get companies
        /// </summary>
        public ListResponse GetCompanies(long editorPersonaId, long userPersonaId, string productName,
            RequestParameter datafilter)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetCompanies at beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                // to get _editorProductUserId

                if (result.IsError)
                {
                    WriteToErrorLog(
                        "ManageProductAssetOptimization.GetCompanies.GetCompanyEditorAndUserDetails error for user " +
                        $"with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName} - {result.ErrorReason}");
                    return result;
                }

                var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/";
                var editorUserProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);
                var productDivisionName =
                    ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));

                var ac = editorUserProfile.Divisions.Where(x => x.Division == productDivisionName).ToList();
                var allCompanies = ac.SelectMany(f => f.Companies).ToList();

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                {
                    productUserProfileApiUrl =
                        $"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/{_productUserId.ToLower()}/";
                    var productUserProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

                    var productUserComp =
                        productUserProfile.Divisions.Where(x => x.Division == productDivisionName).ToList();
                    var productUserCompanies = productUserComp.SelectMany(f => f.Companies).ToList();

                    allCompanies = FilterAssignedCompanies(allCompanies, productUserCompanies);
                }

                allCompanies = allCompanies.OrderBy(x => x.CompanyName).ToList();

                response = new ListResponse()
                {
                    Records = allCompanies.Cast<object>().ToList(),
                    TotalRows = allCompanies.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog(
                    $"Exiting ManageProductAssetOptimization.GetCompanies method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}" +
                    $"and userPersonaId {userPersonaId}.");
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the Companies.";
                WriteToErrorLog(
                    $"ManageProductAssetOptimization.GetCompanies Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} " +
                    $"for product {productName} ", exception: ex);
            }

            return response;
        }

        /// <summary>
        ///Get companies and roles
        /// </summary>
        public ListResponse GetCompaniesWithRoles(long editorPersonaId, long userPersonaId, string productName,
            RequestParameter datafilter)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetCompaniesWithRoles at beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}");

            var response = new ListResponse();
            try
            {
                var allCompanies =
                    GetCompanies(editorPersonaId, userPersonaId, productName, datafilter).Records.Cast<AoCompany>();

                IList<AoCompanyRoles> companyRoles = new List<AoCompanyRoles>();
                // for each company get roles
                foreach (var company in allCompanies)
                {
                    List<AORoles> roles = GetRoles(company.CompanyId, productName).ToList();
                    roles = roles.OrderBy(x => x.DisplayName).ToList();

                    companyRoles.Add(new AoCompanyRoles
                    {
                        CompanyId = company.CompanyId,
                        CompanyName = company.CompanyName,
                        IsAssigned = company.IsAssigned,
                        Status = company.Status,
                        Roles = roles,
                    });
                }

                response = new ListResponse()
                {
                    Records = companyRoles.Cast<object>().ToList(),
                    TotalRows = companyRoles.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog(
                    $"Exiting ManageProductAssetOptimization.GetCompaniesWithRoles method with total rows - {response.TotalRows} " +
                    $"for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}.");
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the GetCompaniesWithRoles.";
                WriteToErrorLog($"ManageProductAssetOptimization.GetCompaniesWithRoles Error for user with " +
                                $"editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}",
                    exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get Companies With Properties
        /// </summary>
        public ListResponse GetCompaniesWithProperties(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetCompaniesWithProperties at beginning of method for user with editorPersona id - {editorPersonaId} " +
                $"and userPersonaId {userPersonaId} for product {productName}.");

            var response = new ListResponse();
            try
            {
                var allCompanies =
                    GetCompanies(editorPersonaId, userPersonaId, productName, datafilter).Records.Cast<AoCompany>();

                IList<AoCompanyProperties> companyProperties = new List<AoCompanyProperties>();
                // for each compnay get properties
                foreach (var company in allCompanies)
                {
                    List<AoProperty> properties = GetProperties(company.CompanyId, productName).ToList();
                    properties = properties.OrderBy(x => x.PropertyName).ToList();

                    if (properties != null)
                    {
                        string assignedCount = $"{properties.Count(p => p.IsAssigned)} of {properties.Count}";

                        companyProperties.Add(new AoCompanyProperties
                        {
                            CompanyId = company.CompanyId,
                            CompanyName = company.CompanyName,
                            IsAssigned = company.IsAssigned,
                            Status = company.Status,
                            AssignedProperties = assignedCount,
                            Properties = properties
                        });
                    }
                }

                response = new ListResponse()
                {
                    Records = companyProperties.Cast<object>().ToList(),
                    TotalRows = companyProperties.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog(
                    $"Exiting ManageProductAssetOptimization.GetCompaniesWithProperties method with total rows - {response.TotalRows} for user" +
                    $" with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}.");
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the GetCompaniesWithProperties.";
                WriteToErrorLog(
                    $"ManageProductAssetOptimization.GetCompaniesWithProperties Error for user with editorPersona id - {editorPersonaId} " +
                    $"and userPersonaId {userPersonaId} for product {productName}.", exception: ex);
            }

            return response;
        }

        #region Migration

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter)
        {
            var response = new ListResponse()
            {
                IsError = true,
                ErrorReason = "No Users."
            };
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError)
            {
                response.ErrorReason = claimResposnse.ErrorReason;
                return response;
            }

            var filter = false;
            var startRow = 0;
            var resultPerRow = 1000;
            if (datafilter != null)
            {
                if (datafilter.FilterBy.ContainsKey("filter"))
                {
                    filter = datafilter.FilterBy["filter"].ToLower() == "migrated" ? true : false;
                }

                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage;
                }
            }

            var productUserProfileApiUrl = $"{_apiEndPoint}unity/migration/users/{_editorProductUserId.ToLower()}/";
            var migrationResponse = GetResultFromApi<IList<AssetOptimizationMigrationUser>>(productUserProfileApiUrl);
            if (migrationResponse == null)
            {
                WriteToErrorLog($"ManageProductAssetOptimization.GetMigrationUsers-no users received from product for user with editorPersona id - {editorPersonaId}.");
                return response;
            }

            var migrationUsers = new List<MigrationUser>();
            foreach (var user in migrationResponse)
            {
                var migrationUser = new MigrationUser
                {
                    CompanyInstanceSourceId = user.CompanySourceInstanceId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserId = user.UserId,
                    Username = user.UserName,
                    Email = user.Email,
                    LastActivity = user.Activity.ToString(),
                    Extra = string.Join("|", user.Products),
                    Status = (string.IsNullOrWhiteSpace(user.Status) || user.Status.ToLower() == "active") ? "Active" : "Disabled"
                };
                migrationUsers.Add(migrationUser);
            }

            WriteToDiagnosticLog($"ManageProductAssetOptimization.GetMigrationUsers - Received users from product for user with editorPersona id - {editorPersonaId}.");
            response.RowsPerPage = migrationResponse.Count;
            response.ErrorReason = string.Empty;
            response.IsError = false;
            response.TotalPages = 1;
            response.Records = migrationUsers.Cast<object>().ToList();
            response.TotalRows = migrationResponse.Count;
            return response;
        }

        /// <summary>
        /// Update the users migration status
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="migrateUsers"></param>
        /// <returns></returns>
        public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
        {
            var migrateResponse = new MigrateResponse()
            {
                Status = false
            };

            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError)
            {
                migrateResponse.Message = claimResposnse.ErrorReason;
                return migrateResponse;
            }

            var url = $"{_apiEndPoint}unity/migration/users";
            var userIds = migrateUsers.Select(x => x.UserId).ToList();

            var response = PutApi(url, userIds);
            var responseContent = response;

            var logData = new Dictionary<string, object>
            {
                {"Url", url},
                {"Response", responseContent},
                {"EditorPersonaId", editorPersonaId},
                {"MigratedUser", migrateUsers}
            };

            if (!string.IsNullOrWhiteSpace(response))
            {
                try
                {
                    var migrationResponse = JsonConvert.DeserializeObject<IList<AOMigrateResponse>>(responseContent);
                    WriteToDiagnosticLog("ManageAssetOptimization.UpdateUsersMigrationStatus.PutAsJsonAsync", logData);
                    if (!(migrationResponse.Select(x => x.Status).Any() == false))
                    {
                        migrateResponse.Status = true;
                        migrateResponse.Message = "success";
                    }
                }
                catch
                {
                    migrateResponse.Message = responseContent;
                }

                return migrateResponse;
            }
            else
            {
                WriteToErrorLog($"ManageAssetOptimizer.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
                migrateResponse.Message = "Cannot update user status to migrated.";

                return migrateResponse;
            }
        }

        #endregion

        /// <summary>
        /// Change Asset Optimization Product UserType
        /// </summary>
        public string ChangeAssetOptimizationProductUserType(long createUserPersonaId, long assignUserPersonaId,
            IList<AoUserCompanyPropertyRoleDetail> rolePropAoUserCompanyPropertyRoleDetailList, BatchProcessType batchProcessType)
        {
            return ManageAssetOptimizationUser(createUserPersonaId, assignUserPersonaId, rolePropAoUserCompanyPropertyRoleDetailList, batchProcessType);
        }

        /// <summary>
        /// Create/update a user in AO-BI
        /// </summary>
        public string ManageAssetOptimizationUser(long editorPersonaId, long productUserPersonaId, IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.ManageAssetOptimizationUser - Begin create/update user for user with editorPersona id - {editorPersonaId}." +
                $"and userPersonaId {productUserPersonaId}.");

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, productUserPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductAssetOptimization.ManageAssetOptimizationUser - Error for user with editorPersona id - {editorPersonaId} " +
                        $"and userPersonaId {productUserPersonaId}. Error - {listResponse.ErrorReason}");
                    return listResponse.ErrorReason;
                }

                var persona = _managePersona.GetPersona(productUserPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var productUserGbLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                if (productUserGbLogin == null)
                {
                    WriteToErrorLog(
                        $"ManageProductAssetOptimization.ManageAssetOptimizationUser - User Login Name not exist in greenbook for editorPersonaId - {editorPersonaId}.");

                    return "User Login Name not exists in greenbook.";
                }

                GetProductCompanyInstanceId(BlueBookProductConstants.AssetOptimizer);

                var aoUser = new AOUser
                {
                    IsInternalUser = false, // Initial release is w/o internal user
                    IsEnabled = true,
                    IsSuperUser = false,
                    Email = productUserGbLogin.LoginName.ToLower(),

                    Login = productUserGbLogin.LoginName.ToLower(),
                    OldUserId = string.Empty,
                    UserId = string.Empty,

                    FirstName = person.FirstName,
                    LastName = person.LastName,
                };

                // Check if GB super user
                if (IsSuperUser(productUserPersonaId))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductAssetOptimization.ManageAssetOptimizationUser user is super user with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}.");

                    aoGbUserCompanyPropertyRoleDetails = CopyEditorUserToCreateSuperUser(editorPersonaId);

                    try
                    {
                        // For Investment Analytics (MA) assign US market to super user
                        var allGroups = GetAllPropertyGroups();
                        var usGroupId = allGroups.Groups.FirstOrDefault(x => x.GroupName == "US")?.GroupId;

                        if (usGroupId != null && usGroupId != 0)
                        {
                            var ss =
                                aoGbUserCompanyPropertyRoleDetails.Where(x => x.ProductName == "MA")
                                    .SelectMany(c => c.PropertyGroups)
                                    .ToList();
                            if (!ss.Contains(usGroupId.Value))
                            {
                                // aoGbUserCompanyPropertyRoleDetails.Where(x => x.ProductName == "MA").SelectMany(c => c.PropertyGroups).ToList().Add(usGroupId.Value);
                                foreach (var c in aoGbUserCompanyPropertyRoleDetails)
                                {
                                    if (c.ProductName == "MA")
                                    {
                                        c.PropertyGroups.Add(usGroupId.Value);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(_productUsername))
                {
                    aoUser.GroupsModel = GetBundledGroups(aoGbUserCompanyPropertyRoleDetails);
                    aoUser.Divisions = new List<Divisions>();
                    aoUser.Model = GetModel(aoGbUserCompanyPropertyRoleDetails);

                    // New User logic - Check if loginName already Exists
                    if (CheckUniqueAOUserName(productUserGbLogin.LoginName.ToLower()))
                    {
                        // user already exist in AO - create GB association 
                        WriteToErrorLog(
                            $"ManageProductAssetOptimization.ManageAssetOptimizationUser - User Login Name {productUserGbLogin.LoginName} already exists in product. Associating GB User with Product AO User.");

                        AssociateAoUserWithGb(editorPersonaId, productUserPersonaId, productUserGbLogin.LoginName);

                        // Call AO to indicate user is associated (use migration API)
                        var result = PutApi($"{_apiEndPoint}unity/migration/users",
                            new List<string> {productUserGbLogin.LoginName.ToLower()});

                        try
                        {
                            var jsObj = JsonConvert.DeserializeObject<dynamic>(result)[0];
                            if (jsObj.success == true)
                                return "";

                            return
                                $"False result for user {productUserGbLogin.LoginName.ToLower()} while parsing AO migration API response. Result from API {_apiEndPoint}unity/migration/users is {result}";
                        }
                        catch (Exception ex)
                        {
                            WriteToErrorLog(
                                $"ManageProductAssetOptimization ManageAssetOptimizationUser - ERROR for user {productUserGbLogin.LoginName.ToLower()} while parsing AO migration API response. Result from API {_apiEndPoint}unity/migration/users is {result}");
                            return result;
                        }
                    }

                    var createResult =
                        PostApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);

                    if (string.IsNullOrEmpty(createResult))
                    {
                        // Create GB Product association - for new user insert record
                        var productList = (from x in aoUser.Model select x.Product).Distinct().ToList();

                        CreateProductUserInGreenBook(editorPersonaId, productUserPersonaId, productList,
                            productUserGbLogin.LoginName.ToLower());
                    }

                    WriteToDiagnosticLog(
                        $"ManageProductAssetOptimization.ManageAssetOptimizationUser completed user creation process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}.");

                    return createResult;
                }

                // Update User logic
                // Get Copy of User from AO
                var copiedAoUserCompanyPropertyRoleDetails = CopyRegularUser(editorPersonaId, productUserPersonaId);

                // store existing assigned products
                var existingAoProducts = copiedAoUserCompanyPropertyRoleDetails;

                UpdateProductRolePropertyDetails(aoGbUserCompanyPropertyRoleDetails,
                    copiedAoUserCompanyPropertyRoleDetails);

                aoUser.GroupsModel = GetBundledGroups(copiedAoUserCompanyPropertyRoleDetails);
                aoUser.Divisions = new List<Divisions>();
                aoUser.Model = GetModel(copiedAoUserCompanyPropertyRoleDetails);

                aoUser.UserId = _productUserId.ToLower();
                aoUser.Login = _productUsername.ToLower();
                aoUser.OldUserId = _productUserId.ToLower();
                aoUser.Email = _productUsername.ToLower();

                var updateResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);

                if (string.IsNullOrEmpty(updateResult))
                {
                    UpdateProductUserInGreenBook(editorPersonaId, productUserPersonaId, productUserGbLogin.LoginName.ToLower(), existingAoProducts.Count, aoGbUserCompanyPropertyRoleDetails);
                }
                else
                {
                    // check if error is because of removing all products
                    try
                    {
                        var jsObj = JsonConvert.DeserializeObject<dynamic>(updateResult);
                        if (
                            jsObj.errorResults[0].message.Value.Equals(
                                "A user must be attached to at least one company and one role",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            // keep old products to avoid API error
                            copiedAoUserCompanyPropertyRoleDetails = CopyRegularUser(editorPersonaId, productUserPersonaId);
                            // store existing assigned products
                            existingAoProducts = copiedAoUserCompanyPropertyRoleDetails;

                            // get existing AP details
                            aoUser.GroupsModel = GetBundledGroups(copiedAoUserCompanyPropertyRoleDetails);
                            aoUser.Divisions = new List<Divisions>();
                            aoUser.Model = GetModel(copiedAoUserCompanyPropertyRoleDetails);
                            // disable user
                            aoUser.IsEnabled = false;
                            var disableUserResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/",
                                aoUser);
                            if (string.IsNullOrEmpty(disableUserResult))
                            {
                                // Disable products from GB
                                UpdateProductUserInGreenBook(editorPersonaId, productUserPersonaId,
                                    productUserGbLogin.LoginName.ToLower(), existingAoProducts.Count,
                                    aoGbUserCompanyPropertyRoleDetails);
                                return string.Empty;
                            }

                            return
                                $"Error while setting disable flag on user {aoUser.Login} API -{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()} , disableUserResult - {disableUserResult}";
                        }

                        return updateResult;
                    }
                    catch (Exception ex)
                    {
                        WriteToErrorLog(
                            $"ManageProductAssetOptimization ManageAssetOptimizationUser - ERROR for user {productUserGbLogin.LoginName.ToLower()} while parsing AO PUT API response to cheeck condition if all products removed. Result from API {_apiEndPoint}user/profile/{_editorProductUserId.ToLower()} is {updateResult}");
                        return updateResult;
                    }
                }

                return updateResult;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(
                    $"ManageProductAssetOptimization.ManageAssetOptimizationUser - Exception during user creation process " +
                    $"with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}.", exception: ex);

                return ex.Message;
            }
        }

        /// <summary>
        /// CopyRegularUser
        /// </summary>
        public IList<AoUserCompanyPropertyRoleDetail> CopyRegularUser(long editorUserPersonaId, long subjectUserPersonaId, string productUserName = "")
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.CopyRegularUser - Begin - editorPersona id - {editorUserPersonaId}. sourceUserPersonaId {subjectUserPersonaId}");

            var aoUserCompanyPropertyRoleDetails = new List<AoUserCompanyPropertyRoleDetail>();

            var samlEditorProductUserName = GetSamlProductUserName(editorUserPersonaId).ToLower();
            var samlSubjectProductUserName = "";
            if (productUserName == "")
            {
                samlSubjectProductUserName = GetSamlProductUserName(subjectUserPersonaId).ToLower();
            }
            else
            {
                samlSubjectProductUserName = productUserName;
            }

            if (string.IsNullOrEmpty(samlEditorProductUserName) || string.IsNullOrEmpty(samlSubjectProductUserName))
            {
                WriteToErrorLog(
                    $"ManageProductAssetOptimization.GetGbSupportedAoUserProductsToAssign - Error -unable to find product User name with editorUserPersonaId   - {editorUserPersonaId}, subjectUserPersonaId {subjectUserPersonaId} .");

                throw new Exception($"Error - unable to find product User name with editorUserPersonaId   - {editorUserPersonaId}, subjectUserPersonaId {subjectUserPersonaId}");
            }

            string productUserProfileApiUrl = $"{_apiEndPoint}user/active-authorities/{samlEditorProductUserName}/{samlSubjectProductUserName}/";
            var aoActiveAuthorities = GetResultFromApi<IList<AoActiveAuthorities>>(productUserProfileApiUrl);

            IList<string> aoProductsAvailableToAssign = GetGbSupportedAoSubjectProductsAssigned(aoActiveAuthorities);

            // remove Axiometrics product
            aoProductsAvailableToAssign.Remove("AX");
            var allGroupsResponse = GetSubjectUserAssignedPropertyGroups(samlEditorProductUserName, samlSubjectProductUserName);

            foreach (var aoProduct in aoProductsAvailableToAssign)
            {
                // Get Assigned companies for product
                var companyIdList = GetSubjectUserAssignedCompaniesForProduct(aoActiveAuthorities, aoProduct).Distinct();

                var propertyGroupList = new List<int>();

                // assigned groups
                var productGroups = allGroupsResponse.Where(x => x.Assignments.Contains(aoProduct));
                var grups = (from i in productGroups select i.GroupId).ToList();
                propertyGroupList.AddRange(grups);

                foreach (var companyId in companyIdList)
                {
                    // assign active roles of the user
                    var userAuths = aoActiveAuthorities.Where(x => x.Products != null).SelectMany(s => s.Products).Where(z => z.Product == aoProduct && z.CompanyId == companyId);
                    var roleNames = userAuths.Select(x => x.AuthortyName).ToList();

                    // assign active properties of the user
                    var props = GetActiveProperties(samlEditorProductUserName, samlSubjectProductUserName, aoProduct, companyId);

                    // get division
                    var divisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(aoProduct));

                    aoUserCompanyPropertyRoleDetails.Add(new AoUserCompanyPropertyRoleDetail
                    {
                        CompanyId = companyId,
                        DivisionName = divisionName,
                        ProductName = aoProduct,
                        PropertyGroups = propertyGroupList,
                        SelectedPortfolioValues = props,
                        SelectedRoleValues = roleNames,
                        IsAssigned = true
                    });
                }
            }

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.CopyRegularUser - End - editorPersona id - {editorUserPersonaId}. sourceUserPersonaId {subjectUserPersonaId}");

            return aoUserCompanyPropertyRoleDetails;
        }

        /// <summary>
        /// Get Properties assigned to Group
        /// </summary> 
        public ListResponse GetPropertiesInGroup(long editorPersonaId, long userPersonaId, int propertyGroupId)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetPropertiesInGroup - Begin with editorPersona id - {editorPersonaId}.");

            var response = new ListResponse();

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (listResponse.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductAssetOptimization.GetPropertiesInGroup - Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}");
                    return listResponse;
                }

                if (string.IsNullOrEmpty(_editorProductUserId))
                {
                    response = new ListResponse()
                    {
                        Records = null,
                        TotalRows = 0,
                        RowsPerPage = 9999,
                        ErrorReason = $"User is not exisist in AO product with editorPersonaId {editorPersonaId}.",
                        TotalPages = 1
                    };

                    return response;
                }

                var props = GetPropertiesInGroups(propertyGroupId);
                props = props.OrderBy(x => x.PropertyName).ToList();

                if (props != null && props.Count > 0)
                {
                    response = new ListResponse()
                    {
                        Records = props.Cast<object>().ToList(),
                        TotalRows = props.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }
                else
                {
                    response = new ListResponse()
                    {
                        Records = null,
                        TotalRows = 0,
                        RowsPerPage = 9999,
                        ErrorReason = $"Received null or empty products for AO user {_editorProductUserId}",
                        TotalPages = 1
                    };
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the AO products.";
                WriteToErrorLog($"ManageProductAssetOptimization.GetPropertiesInGroup. Error for user with editor AO user Id - {_editorProductUserId} ", exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get Property Groups
        /// </summary>
        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId,
            string productName, IList<int> selectedCompanies)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetPropertyGroups at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                IList<AoPropertyGroups> propertyGroups = new List<AoPropertyGroups>();

                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductAssetOptimization.GetPropertyGroups.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                if (selectedCompanies == null || selectedCompanies.Count == 0)
                {
                    if (productName == "MA" || productName == "AX")
                    {
                        // return all groups
                        var groups = GetAllPropertyGroups().Groups;

                        foreach (var grp in groups)
                        {
                            propertyGroups.Add(new AoPropertyGroups {GroupId = grp.GroupId, GroupName = grp.GroupName});
                        }

                        WriteToDiagnosticLog(
                            $"ManageProductAssetOptimization.GetPropertyGroups-Received {groups.Count} groups for existing user.");

                    }
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    // existing user
                    var assgnPropertGroups = GetAssignablePropertyGroups(productName, selectedCompanies);

                    var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/{_productUserId.ToLower()}/";
                    var userProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);
                    propertyGroups = GetPropertyGroupsForExistingUser(assgnPropertGroups, userProfile, productName);
                }
                else
                {
                    // return all groups for new user
                    var assgnPropertGroups = GetAssignablePropertyGroups(productName, selectedCompanies);
                    propertyGroups = GetPropertyGroupsForNewUser(assgnPropertGroups);
                }

                if (propertyGroups == null || propertyGroups.Count == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductAssetOptimization.GetPropertyGroups-no groups received from product for user with editorPersona id - {editorPersonaId}.");

                    response.IsError = true;
                    response.ErrorReason = "No groups received from product.";
                    return response;
                }

                WriteToDiagnosticLog(
                    $"ManageProductAssetOptimization.GetPropertyGroups-Received {propertyGroups.Count} groups for existing user with editorPersona id - {editorPersonaId} & userPersonaId {userPersonaId}");

                propertyGroups = propertyGroups.OrderBy(x => x.GroupName).ToList();

                response = new ListResponse()
                {
                    Records = propertyGroups.Cast<object>().ToList(),
                    TotalRows = propertyGroups.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog(
                    $"Exiting ManageProductAssetOptimization.GetPropertyGroups method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}.");
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the groups.";
                WriteToErrorLog($"ManageProductAssetOptimization.GetPropertyGroups Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Gets products available for assigning
        /// </summary> 
        public IList<string> GetGbSupportedAoEditorUserProductsToAssign(long userPersonaId)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetGbSupportedAoUserProductsToAssign - Begin with editorPersona id - {userPersonaId}.");

            var products = new List<string>();

            var samlProductUserName = GetSamlProductUserName(userPersonaId).ToLower();

            if (string.IsNullOrEmpty(samlProductUserName))
            {
                WriteToErrorLog(
                    $"ManageProductAssetOptimization.GetGbSupportedAoUserProductsToAssign - Error -unable to find product User name with persona id - {userPersonaId}.");

                return products;
            }

            string productUserProfileApiUrl = $"{_apiEndPoint}user/divisions/{samlProductUserName}/";
            var aoDivisionProduct = GetResultFromApi<IList<AoDivisionProduct>>(productUserProfileApiUrl);

            if (aoDivisionProduct != null && aoDivisionProduct.Count > 0)
            {
                var aoUserProducts = aoDivisionProduct.SelectMany(c => c.Products).Select(s => s.Product).ToList();

                if (aoUserProducts.Count > 0)
                {
                    foreach (var product in aoUserProducts)
                    {
                        if (ProductEnumHelper.CheckAoProductSupportedByGreenBook(product))
                        {
                            products.Add(product);
                        }
                    }
                }
            }

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetGbSupportedAoUserProductsToAssign at end of method for user with " +
                $"editorPersona id - {userPersonaId} samlProductUserName - {samlProductUserName}. productUserProfileApiUrl {productUserProfileApiUrl}, product count {products.Count}");

            return products;
        }

        #endregion

        #region user Status

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userName"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, string userName, string firstName, string lastName, bool isActive = false)
        {
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError)
            {
                return false;
            }

            var aoUser = new AOUser
            {
                IsInternalUser = false, // Initial release is w/o internal user
                IsEnabled = isActive,
                IsSuperUser = false,
                Email = userName,

                Login = userName,
                OldUserId = userName,
                UserId = userName,

                FirstName = firstName,
                LastName = lastName
            };
            var copiedAoUserCompanyPropertyRoleDetails = CopyRegularUser(editorPersonaId, 0, userName);
            // store existing assigned products
            var existingAoProducts = copiedAoUserCompanyPropertyRoleDetails;
            // get existing AP details
            aoUser.GroupsModel = GetBundledGroups(copiedAoUserCompanyPropertyRoleDetails);
            aoUser.Divisions = new List<Divisions>();
            aoUser.Model = GetModel(copiedAoUserCompanyPropertyRoleDetails);
            var disableUserResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);
            if (string.IsNullOrEmpty(disableUserResult))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Used to get login name from SAML attribute
        /// </summary>
        /// <param name="userPersonaId"></param>
        private string GetSamlProductUserName(long userPersonaId)
        {
            string userName = string.Empty;

            if (userPersonaId != 0)
            {
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, (int) ProductEnum.AssetOptimizer);

                if (productAttributes.Any(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase)))
                {
                    userName = (from a in productAttributes where a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
                }
            }

            return userName;
        }

        #region Used to create Super user

        private IList<AoCompany> GetEditorUserAssignedCompaniesForProduct(long personaId, string productName)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetEditorUserAssignedCompanies at beginning of method for user with editorPersona id - {personaId}");

            var samlProductUserName = GetSamlProductUserName(personaId).ToLower();

            if (string.IsNullOrEmpty(samlProductUserName))
            {
                WriteToErrorLog(
                    $"ManageProductAssetOptimization.GetEditorUserAssignedCompaniesForProduct - Error -unable to find product User name with persona id - {personaId}.");
                return null;
            }

            var productDivisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));

            var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{samlProductUserName}/";
            var productUserProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

            var productUserComp = productUserProfile.Divisions.Where(x => x.Division == productDivisionName).ToList();

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetEditorUserAssignedCompanies at end of method for user with editorPersona id - {personaId} editorSamlProductUserName - {samlProductUserName} productName {productName}");

            return productUserComp.SelectMany(f => f.Companies).ToList();
        }

        private IList<Groups> GetEditorUserAssignedPropertyGroups(long editorPersonaId)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetEditorUserAssignedPropertyGroups at beginning of method for user with editorPersona id - {editorPersonaId}");

            var editorSamlProductUserName = GetSamlProductUserName(editorPersonaId).ToLower();

            if (string.IsNullOrEmpty(editorSamlProductUserName))
            {
                WriteToErrorLog(
                    $"ManageProductAssetOptimization.GetEditorUserAssignedCompaniesForProduct - Error -unable to find product User name with persona id - {editorPersonaId}.");
                return null;
            }

            var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{editorSamlProductUserName}/{editorSamlProductUserName}/";
            var userProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

            return userProfile.Divisions.Where(r => r.Groups != null).SelectMany(x => x.Groups).ToList();
        }

        #endregion

        private IList<Groups> GetSubjectUserAssignedPropertyGroups(string editorProductUserId, string productUserId)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetEditorUserAssignedPropertyGroups at beginning of method for user with editorProductUserId - {editorProductUserId} productUserId {productUserId}");

            var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{editorProductUserId.ToLower()}/{productUserId.ToLower()}/";
            var userProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

            return userProfile.Divisions.Where(r => r.Groups != null).SelectMany(x => x.Groups).ToList();
        }

        private List<AoCompany> FilterAssignedCompanies(List<AoCompany> allCompanies, List<AoCompany> productUserCompanies)
        {
            if (productUserCompanies != null && productUserCompanies.Count > 0)
            {
                foreach (var productUserCompany in productUserCompanies)
                {
                    foreach (var allComp in allCompanies)
                    {
                        if (productUserCompany.CompanyId == allComp.CompanyId)
                        {
                            allComp.IsAssigned = true;
                        }
                    }
                }
            }

            return allCompanies;
        }

        private IList<AoProperty> GetPropertiesInGroups(int propertyGroupId)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetPropertiesInGroups at beginning of method.");

            var response = new List<AoProperty>();

            AoVisiblePropertyGroups visiblePropertyGroups = GetAllPropertyGroups();

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetPropertiesInGroups-Received {visiblePropertyGroups.Groups.Count} groups for existing user.");

            foreach (var x in visiblePropertyGroups.Groups.Where(z => z.Properties != null && z.GroupId == propertyGroupId).SelectMany(s => s.Properties))
            {
                response.Add(new AoProperty {PropertyId = x.PropertyId, PropertyName = x.PropertyName});
            }

            return response;
        }

        private AoVisiblePropertyGroups GetAllPropertyGroups()
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetAllPropertyGroups at beginning of method.");

            ObjectCache groupCache = MemoryCache.Default;

            // Get token values from cache
            var propertyGroups = groupCache["propertyGroups_AO"] as AoVisiblePropertyGroups;

            if (propertyGroups == null)
            {
                WriteToDiagnosticLog("ManageProductAssetOptimization.GetAllPropertyGroups- Null cache value. Getting new Groups.");

                var groupApiUrl =
                    $"{_apiEndPoint}user/groups/visible/{_aoSuperUser.ToLower()}/{_editorProductUserId.ToLower()}/";
                propertyGroups = GetResultFromApi<AoVisiblePropertyGroups>(groupApiUrl);

                var cachePolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60)
                };

                groupCache.Set("propertyGroups_AO", propertyGroups, cachePolicy);
            }
            else
            {
                WriteToDiagnosticLog("ManageProductAssetOptimization.GetAllPropertyGroups- Received cache value for Groups.");
            }

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetAllPropertyGroups-Received {propertyGroups.Groups.Count} groups for existing user.");

            return propertyGroups;
        }

        private IList<AoAssignableDivisionGroups> GetAssignablePropertyGroups(string productName, IList<int> selectedCompanies)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetAssignablePropertyGroups at beginning of method.");

            // existing user
            var groupApiUrl = $"{_apiEndPoint}user/groups/assignablepropertygroups/{_editorProductUserId.ToLower()}/{GetProductCompanyParam(selectedCompanies, productName)}";
            IList<AoAssignableDivisionGroups> assignableDivisionGroups = GetResultFromApi<IList<AoAssignableDivisionGroups>>(groupApiUrl);
            //https://aodev.realpage.com/ysconfig/ws/user/groups/assignablepropertygroups/jwun/BI?companies=1661|BI 

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetAssignablePropertyGroups-Received {assignableDivisionGroups.Count} groups for existing user.");

            return assignableDivisionGroups;
        }

        private IList<AoProperty> GetPropertiesForNewUser(string productPropertyApiUrl)
        {
            var aoProps = GetResultFromApi<IList<AoProperties>>(productPropertyApiUrl);
            return aoProps?.FirstOrDefault()?.Properties;
        }

        private IList<AoProperty> GetPropertiesForExistingProductUser(IList<AoProperty> allPropList, string productPropertyApiUrl, string productName)
        {
            var aoUserProps = GetResultFromApi<IList<AoProperties>>(productPropertyApiUrl);

            if (aoUserProps != null)
            {
                var props = aoUserProps.SelectMany(x => x.Properties).ToList();

                var assignedPros = props.Where(x => x.Products.Any(d => d.Product == productName && d.IsEnabled)).ToList();

                // Mark selected properties
                foreach (var assignedProp in assignedPros)
                {
                    foreach (var allProp in allPropList)
                    {
                        if (assignedProp.PropertyId == allProp.PropertyId)
                        {
                            allProp.IsAssigned = true;
                        }
                    }
                }
            }

            return allPropList;
        }

        private T GetResultFromApi<T>(string baseUrlAndQuery) where T : class
        {
            T results = null;
            using (var client = new HttpClient())
            {
                client.SetBasicAuthentication(_apiUser, _apiPassword);
                var response = client.GetAsync(baseUrlAndQuery).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
                }
                else
                {
                    WriteToErrorLog(
                        $"Error - Response is not 200. ManageProductAssetOptimization.GetResultFromApi, baseUrlAndQuery {baseUrlAndQuery}, StatusCode - {response.StatusCode}");
                }
            }

            return results;
        }

        private string PostApi(string baseUrlAndQuery, object inputObject)
        {
            string result = string.Empty;

            // dump diagnostic info
            DumpApiCallInfoToDiagnosticLog($"ManageProductAssetOptimization.PostApi - API Url - {baseUrlAndQuery}",
                inputObject);

            using (var client = new HttpClient())
            {
                client.SetBasicAuthentication(_apiUser, _apiPassword);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync(baseUrlAndQuery, inputObject).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        result = userResult.ToString();
                    }
                }
                else
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic errorResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (errorResult != null)
                    {
                        result = "Error -" + errorResult.ToString();
                    }

                    WriteToErrorLog(
                        $"Error - Response is not 200. ManageProductAssetOptimization.PostApi, baseUrlAndQuery {baseUrlAndQuery}, StatusCode - {response.StatusCode}, jsonContent {jsonContent}, errorResult {result}");
                }
            }

            return result;
        }

        private string PutApi(string baseUrlAndQuery, object inputObject)
        {
            string result = string.Empty;

            using (var client = new HttpClient())
            {
                client.SetBasicAuthentication(_apiUser, _apiPassword);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // dump diagnostic info
                DumpApiCallInfoToDiagnosticLog($"ManageProductAssetOptimization.PutApi - API Url - {baseUrlAndQuery}",
                    inputObject);

                using (var response = client.PutAsJsonAsync(baseUrlAndQuery, inputObject).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                        if (userResult != null)
                        {
                            result = userResult.ToString();
                        }
                    }
                    else
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        dynamic errorResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                        if (errorResult != null)
                        {
                            result = errorResult.ToString();
                        }

                        WriteToErrorLog(
                            $"Error - Response is not 200. ManageProductAssetOptimization.PutApi, baseUrlAndQuery {baseUrlAndQuery}, StatusCode - {response.StatusCode}, jsonContent {jsonContent}, result {result}");
                    }
                }
            }

            return result;
        }

        private bool CheckUniqueAOUserName(string loginName)
        {
            //need super user else won't return user info for other companies that are not associated with editor User
            string productUserProfileApiUrl = $"{_apiEndPoint}users/{loginName}/validation";
            var validationResult = GetResultFromApi<dynamic>(productUserProfileApiUrl);

            if (validationResult != null)
                return validationResult.exists;

            throw new Exception($"CheckUniqueAOUserName returned invalid results - URL {productUserProfileApiUrl}");
        }

        private void CreateProductUserInGreenBook(long editorPersonaId, long userPersonaId, IList<string> aoProductList, string productLoginName)
        {
            // Default AO record
            WriteToDiagnosticLog($"ManageProductAssetOptimization.CreateProductUserInGreenBook - Inserting in GB -productUsername -{productLoginName} for AO user..");
            _samlRepository.CreateSamlUserAttribute(userPersonaId, (int) ProductEnum.AssetOptimizer, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, (int) ProductEnum.AssetOptimizer, SamlAttributeEnum.UserId, productLoginName);
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductEnum.AssetOptimizer, (int) ProductBatchStatusType.Success);

            // add activity log
            WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "User account for {0} {1} is created in product {2} by user {3} {4}.");

            // AoDivisionType
            foreach (var product in aoProductList)
            {
                WriteToDiagnosticLog($"ManageProductAssetOptimization.CreateProductUserInGreenBook - Inserting in GB -productUsername -{productLoginName}, product - {product}.");
                _samlRepository.CreateSamlUserAttribute(userPersonaId, (int) ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.productUsername, productLoginName);
                _samlRepository.CreateSamlUserAttribute(userPersonaId, (int) ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.UserId, productLoginName);

                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductEnumHelper.GetAoProductEnum(product), (int) ProductBatchStatusType.Success);

                // add activity log
                WriteActivityLogWithMessageByProduct(editorPersonaId, userPersonaId, (int) ProductEnumHelper.GetAoProductEnum(product), "User {0} {1} assigned product {2} by user {3} {4}.");

                WriteToDiagnosticLog($"ManageProductAssetOptimization.CreateProductUserInGreenBook - Create user Success. Set product status to Success. productUsername -{productLoginName}, product - {product}");
            }
        }

        private void UpdateProductUserInGreenBook(long editorPersonaId,
            long userPersonaId,
            string productLoginName,
            int existingAssignedProductCount,
            IList<AoUserCompanyPropertyRoleDetail> aoUserCompanyPropertyRoleDetails)
        {
            var productAssigned = new List<string>();
            var productUnAssigned = new List<string>();

            foreach (var aoUserCompanyPropertyRoleDetail in aoUserCompanyPropertyRoleDetails)
            {
                if (aoUserCompanyPropertyRoleDetail.IsAssigned)
                {
                    productAssigned.Add(aoUserCompanyPropertyRoleDetail.ProductName);
                }
                else
                {
                    productUnAssigned.Add(aoUserCompanyPropertyRoleDetail.ProductName);
                }
            }

            // select distinct products for multiple companies
            productAssigned = productAssigned.Distinct<string>().ToList();
            productUnAssigned = productUnAssigned.Distinct<string>().ToList();

            //set delete status if all products are unassigned.
            if (existingAssignedProductCount == productUnAssigned.Count)
            {
                // remove all association from GB
                UpdateProductSettingProductStatus(userPersonaId,
                    _productSettingType_ProductStatus, (int) ProductEnum.AssetOptimizer, (int) ProductBatchStatusType.Deleted);

                // add activity log
                WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "User {0} {1} account is disabled in product {2} by user {3} {4}.");

                foreach (var item in aoUserCompanyPropertyRoleDetails)
                {
                    UpdateProductSettingProductStatus(userPersonaId,
                        _productSettingType_ProductStatus, (int) ProductEnumHelper.GetAoProductEnum(item.ProductName), (int) ProductBatchStatusType.Deleted);

                    // add activity log
                    WriteActivityLogWithMessageByProduct(editorPersonaId, userPersonaId, (int) ProductEnumHelper.GetAoProductEnum(item.ProductName),
                        "User {0} {1} access is removed for product {2} by user {3} {4}.");
                }
            }
            else
            {
                if (productAssigned.Any())
                {
                    // First check default AO record exists 
                    WriteToDiagnosticLog($"ManageProductAssetOptimization.UpdateProductUserInGreenBook - Checking AO record in GB -productUsername -{productLoginName} for AO user..");
                    var samlUserDetails = _samlRepository.GetProductSamlDetails(userPersonaId, (int) ProductEnum.AssetOptimizer);
                    if (!samlUserDetails.Any())
                    {
                        WriteToDiagnosticLog($"ManageProductAssetOptimization.UpdateProductUserInGreenBook - No AO record found in GB for AO user -{productLoginName}. Creating new one.");
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, (int) ProductEnum.AssetOptimizer,
                            SamlAttributeEnum.productUsername, productLoginName);
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, (int) ProductEnum.AssetOptimizer,
                            SamlAttributeEnum.UserId, productLoginName);

                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductEnum.AssetOptimizer, (int) ProductBatchStatusType.Success);

                        // add activity log
                        WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "User {0} {1} account is updated for product {2} by user {3} {4}.");
                    }

                    //if product is assigned
                    foreach (var product in productAssigned)
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductAssetOptimization.UpdateProductUserInGreenBook - Checking if product {product} esists in GB for AO user - {productLoginName}");
                        samlUserDetails = _samlRepository.GetProductSamlDetails(userPersonaId,
                            (int) ProductEnumHelper.GetAoProductEnum(product));

                        if (!samlUserDetails.Any())
                        {
                            WriteToDiagnosticLog($"ManageProductAssetOptimization.UpdateProductUserInGreenBook - No {product} record found in GB for AO user -{productLoginName}. Creating new one.");

                            _samlRepository.CreateSamlUserAttribute(userPersonaId,
                                (int) ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.productUsername,
                                productLoginName);
                            _samlRepository.CreateSamlUserAttribute(userPersonaId,
                                (int) ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.UserId, productLoginName);

                            // add activity log
                            WriteActivityLogWithMessageByProduct(editorPersonaId, userPersonaId, (int) ProductEnumHelper.GetAoProductEnum(product), "User {0} {1} assigned for product {2} by user {3} {4}.");
                        }

                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductEnumHelper.GetAoProductEnum(product), (int) ProductBatchStatusType.Success);

                        WriteToDiagnosticLog($"ManageProductAssetOptimization.UpdateProductUserInGreenBook -{product} record updated in GB for AO user -{productLoginName}. (UpdateProductSettingProductStatus)");
                    }
                }

                if (productUnAssigned.Any())
                {
                    //if product is un-assigned then remove product from GB
                    foreach (var product in productUnAssigned)
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductAssetOptimization.UpdateProductUserInGreenBook - Checking if product {product} esists in GB for AO user - {productLoginName}");

                        var samlUserDetails = _samlRepository.GetProductSamlDetails(userPersonaId, (int) ProductEnumHelper.GetAoProductEnum(product));

                        if (samlUserDetails.Any())
                        {
                            WriteToDiagnosticLog($"ManageProductAssetOptimization.UpdateProductUserInGreenBook - {product} record found in GB for AO user -{productLoginName}. Removing.");

                            UpdateProductSettingProductStatus(userPersonaId,
                                _productSettingType_ProductStatus, (int) ProductEnumHelper.GetAoProductEnum(product), (int) ProductBatchStatusType.Deleted);

                            WriteToDiagnosticLog($"ManageProductAssetOptimization.UpdateProductUserInGreenBook -{product} record removed from GB for AO user -{productLoginName}.");

                            // add activity log
                            WriteActivityLogWithMessageByProduct(editorPersonaId, userPersonaId, (int) ProductEnumHelper.GetAoProductEnum(product), "User {0} {1} access is removed for product {2} by user {3} {4}.");
                        }
                    }
                }
            }
        }

        private IList<Model> GetModel(IList<AoUserCompanyPropertyRoleDetail> aoUserCompanyPropertyRoleDetails)
        {
            IList<Model> models = new List<Model>();

            foreach (var aoUserCompanyPropertyRoleDetail in aoUserCompanyPropertyRoleDetails)
            {
                if (aoUserCompanyPropertyRoleDetail.IsAssigned)
                {
                    var model = new Model
                    {
                        CompanyId = aoUserCompanyPropertyRoleDetail.CompanyId,
                        DivisionName = aoUserCompanyPropertyRoleDetail.DivisionName,
                        Product = aoUserCompanyPropertyRoleDetail.ProductName,
                        SelectedPortfolioValues = aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues ?? new List<int>(),
                        SelectedRoleValues = aoUserCompanyPropertyRoleDetail.SelectedRoleValues ?? new List<string>()
                    };

                    models.Add(model);
                }
            }

            return models;
        }

        private IList<GroupModel> GetBundledGroups(IList<AoUserCompanyPropertyRoleDetail> aoUserCompanyPropertyRoleDetails)
        {
            IList<GroupModel> groupModelList = new List<GroupModel>();
            foreach (var aoProductPropertyGroup in aoUserCompanyPropertyRoleDetails)
            {
                if (aoProductPropertyGroup.PropertyGroups != null && aoProductPropertyGroup.PropertyGroups.Any())
                {

                    var division = aoProductPropertyGroup.DivisionName;
                    var productId = aoProductPropertyGroup.ProductName;

                    foreach (var groupId in aoProductPropertyGroup.PropertyGroups)
                    {
                        var groupModel = new GroupModel
                        {
                            Division = division,
                            GroupId = groupId,
                            ProductName = productId,
                            IsEnabled = true
                        };

                        groupModelList.Add(groupModel);
                    }
                }
            }

            return
                groupModelList.GroupBy(o => new {o.Division, o.GroupId, o.IsEnabled, o.ProductName})
                    .Select(o => o.FirstOrDefault())
                    .ToList();
        }

        private IList<AoPropertyGroups> GetPropertyGroupsForNewUser(IList<AoAssignableDivisionGroups> assignPropertyGroups)
        {
            IList<AoPropertyGroups> response = new List<AoPropertyGroups>();
            foreach (var grp in assignPropertyGroups)
            {
                foreach (var gp in grp.Groups)
                {
                    response.Add(new AoPropertyGroups {GroupId = gp.PropertyGroupId, GroupName = gp.GroupName});
                }
            }

            return response;
        }

        private IList<AoPropertyGroups> GetPropertyGroupsForExistingUser(IList<AoAssignableDivisionGroups> assignPropertyGroups, AOUser userProfile, string productName)
        {
            IList<AoPropertyGroups> response = new List<AoPropertyGroups>();
            foreach (var grp in assignPropertyGroups)
            {
                foreach (var gp in grp.Groups)
                {
                    if (gp.Products.Any(x => x.Product == productName))
                    {
                        response.Add(new AoPropertyGroups {GroupId = gp.PropertyGroupId, GroupName = gp.GroupName});
                    }
                }
            }

            var groups = userProfile.Divisions.Where(r => r.Groups != null).SelectMany(x => x.Groups);
            var productGropus = groups.Where(x => x.Assignments.Any(f => f.Contains(productName))).ToList();

            foreach (var item in productGropus)
            {
                foreach (var gp in response)
                {
                    if (gp.GroupId == item.GroupId)
                    {
                        gp.IsAssigned = true;
                    }
                }
            }

            return response;
        }

        private string GetProductCompanyParam(IList<int> selectedCompanies, string productName)
        {
            //BI?companies=1661|BI
            var result = new List<string>();
            var divisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));
            foreach (var selectedCompany in selectedCompanies)
            {
                result.Add($"{selectedCompany}|{productName}");
            }

            return $"{divisionName}?companies={string.Join<string>(",", result)}";
        }

        private IList<AORoles> CheckAuthorities(IList<AORoles> allRoles, IList<AoActiveAuthorities> activeAuthorities, string productName, int companyId)
        {
            var userAuths = activeAuthorities.Where(x => x.Products != null).SelectMany(s => s.Products).Where(z => z.Product == productName && z.CompanyId == companyId);
            foreach (var auth in userAuths)
            {
                foreach (var role in allRoles)
                {
                    if (auth.AuthortyName.ToLower() == role.Name.ToLower())
                    {
                        role.IsAssigned = true;
                    }
                }
            }

            return allRoles;
        }

        private void AssociateAoUserWithGb(long editorPersonaId, long productUserPersonaId, string loginName)
        {
            IList<string> products = GetAoProductsForUser(loginName);

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.AssociateAoUserWithGb with editorPersona id - {editorPersonaId} productUserPersonaId {productUserPersonaId}, Products - {string.Join<string>(",", products)}");

            CreateProductUserInGreenBook(editorPersonaId, productUserPersonaId, products, loginName);
        }

        private IList<string> GetAoProductsForUser(string loginName)
        {
            WriteToDiagnosticLog($"ManageProductAssetOptimization.GetAoProductsForUser with loginName {loginName}");

            var productUserProfileApiUrl = $"{_apiEndPoint}user/divisions/{loginName.ToLower()}/";
            var aoDivisionProduct = GetResultFromApi<IList<AoDivisionProduct>>(productUserProfileApiUrl);
            IList<string> products = new List<string>();

            if (aoDivisionProduct != null && aoDivisionProduct.Count > 0)
            {
                var aoUserProducts =
                    aoDivisionProduct.SelectMany(c => c.Products).Select(s => s.Product).ToList();

                if (aoUserProducts.Count > 0)
                {
                    foreach (var product in aoUserProducts)
                    {
                        if (ProductEnumHelper.CheckAoProductSupportedByGreenBook(product))
                        {
                            products.Add(product);
                        }
                    }
                }
            }

            WriteToDiagnosticLog($"ManageProductAssetOptimization.GetAoProductsForUser-Received products {products.Count} for user with loginName {loginName} API-URL {productUserProfileApiUrl}");

            return products;
        }

        private IList<AORoles> GetRoles(int companyId, string productName)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetRoles at beginning of method for user with _editorProductUserId{_editorProductUserId} _productUserId {_productUserId} companyId - {companyId} productName {productName}");

            string roleApiUrl;
            if (!string.IsNullOrEmpty(_productUserId))
            {
                // Called during updating Existing User
                roleApiUrl = $"{_apiEndPoint}user/roles/available/{_editorProductUserId.ToLower()}/{_productUserId.ToLower()}/{companyId}/{productName}";
            }
            else
            {
                // new user
                roleApiUrl = $"{_apiEndPoint}user/roles/available/{_editorProductUserId.ToLower()}/{companyId}/{productName}";
            }

            // get all roles
            var allRoles = GetResultFromApi<IList<AORoles>>(roleApiUrl);

            // get product user roles & set isAssigned flag
            if (!string.IsNullOrEmpty(_productUserId))
            {
                // existing user - get active authorities & check if exists in all roles
                var authorityApiUrl = $"{_apiEndPoint}user/active-authorities/{_editorProductUserId.ToLower()}/{_productUserId.ToLower()}/";
                var activeAuthorities = GetResultFromApi<IList<AoActiveAuthorities>>(authorityApiUrl);
                allRoles = CheckAuthorities(allRoles, activeAuthorities, productName, companyId);
            }

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetRoles-Received {allRoles.Count} roles for existing user with _editorProductUserId{_editorProductUserId} _productUserId {_productUserId}  companyId - {companyId} productName {productName}");

            return allRoles;
        }

        private IList<AoProperty> GetProperties(long companyId, string productName)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetProperties - at begining of method for user with _editorProductUserId{_editorProductUserId} _productUserId {_productUserId}  companyId - {companyId} productName {productName}");

            string productPropertyApiUrl = $"{_apiEndPoint}company/propertiesByDivision/{companyId}/{ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName))}"; //https://aodev.realpage.com/ysconfig/ws/company/propertiesByDivision/6698/BI
            IList<AoProperty> aoPropertyList = GetPropertiesForNewUser(productPropertyApiUrl);

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetProperties-Received {aoPropertyList.Count} properties for new user with _editorProductUserId{_editorProductUserId} _productUserId {_productUserId}  companyId - {companyId} productName {productName}");

            if (!string.IsNullOrEmpty(_productUserId))
            {
                productPropertyApiUrl = $"{_apiEndPoint}user/active-portfolio/{_editorProductUserId.ToLower()}/{_productUserId.ToLower()}/"; //https://aodev.realpage.com/ysconfig/ws/user/active-portfolio/tmilburn/acroyle
                aoPropertyList = GetPropertiesForExistingProductUser(aoPropertyList, productPropertyApiUrl, productName);

                WriteToDiagnosticLog(
                    $"ManageProductAssetOptimization.GetProperties-Received {aoPropertyList.Count} properties for existing user _editorProductUserId{_editorProductUserId} _productUserId {_productUserId}  companyId - {companyId} productName {productName}.");
            }

            return aoPropertyList;
        }

        private IList<int> GetActiveProperties(string samlEditorProductUserName, string samlSubjectProductUserName, string productName, int companyId)
        {
            var productPropertyApiUrl = $"{_apiEndPoint}user/active-portfolio/{samlEditorProductUserName.ToLower()}/{samlSubjectProductUserName.ToLower()}/";
            var aoUserProps = GetResultFromApi<IList<AoProperties>>(productPropertyApiUrl);

            if (aoUserProps == null)
                return null;

            var props = aoUserProps.SelectMany(x => x.Properties).ToList();

            var assignedPros = props.Where(x => x.Products.Any(d => d.Product == productName && d.IsEnabled) && x.CompanyId == companyId).ToList();

            return (from i in assignedPros select i.PropertyId).ToList();
        }

        private IList<int> GetSubjectUserAssignedCompaniesForProduct(IList<AoActiveAuthorities> aoActiveAuthorities, string aoProduct)
        {
            var c = aoActiveAuthorities.SelectMany(x => x.Products);
            return c.Where(p => p.Product == aoProduct).Select(x => x.CompanyId).ToList();
        }

        private IList<string> GetGbSupportedAoSubjectProductsAssigned(IList<AoActiveAuthorities> aoActiveAuthorities)
        {
            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetGbSupportedAoUserProductsToAssign - Begin.");

            var products = new List<string>();

            if (aoActiveAuthorities != null && aoActiveAuthorities.Count > 0)
            {
                var aoUserProducts = aoActiveAuthorities.SelectMany(c => c.Products).Select(s => s.Product).Distinct().ToList();

                if (aoUserProducts.Count > 0)
                {
                    foreach (var product in aoUserProducts)
                    {
                        if (ProductEnumHelper.CheckAoProductSupportedByGreenBook(product))
                        {
                            products.Add(product);
                        }
                    }
                }
            }

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.GetGbSupportedAoUserProductsToAssign at end of method - product count {products.Count}");

            return products;
        }

        private void UpdateProductRolePropertyDetails(IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails, IList<AoUserCompanyPropertyRoleDetail> copiedAoUserCompanyPropertyRoleDetails)
        {
            if (aoGbUserCompanyPropertyRoleDetails == null)
                return;

            // Ge unassigned products
            var unAssignedProducts = aoGbUserCompanyPropertyRoleDetails.Where(x => x.IsAssigned == false);
            var modifiedProducts = aoGbUserCompanyPropertyRoleDetails.Where(x => x.IsAssigned);

            // remove products
            foreach (var unAssignedProduct in unAssignedProducts)
            {
                var matches = copiedAoUserCompanyPropertyRoleDetails.Where(p => p.ProductName == unAssignedProduct.ProductName).ToList();
                if (matches.Any())
                {
                    foreach (var match in matches)
                    {
                        copiedAoUserCompanyPropertyRoleDetails.Remove(match);
                    }
                }
            }

            // replace products
            foreach (var modifiedProduct in modifiedProducts)
            {
                var matches = copiedAoUserCompanyPropertyRoleDetails.Where(p => p.ProductName == modifiedProduct.ProductName && p.CompanyId == modifiedProduct.CompanyId).ToList();
                if (matches.Any())
                {
                    foreach (var match in matches)
                    {
                        copiedAoUserCompanyPropertyRoleDetails.Remove(match);
                        copiedAoUserCompanyPropertyRoleDetails.Add(modifiedProduct);
                    }
                }
                else
                {
                    copiedAoUserCompanyPropertyRoleDetails.Add(modifiedProduct);
                }
            }
        }

        private IList<AoUserCompanyPropertyRoleDetail> CopyEditorUserToCreateSuperUser(long sourceUserPersonaId)
        {
            WriteToDiagnosticLog($"ManageProductAssetOptimization.CopyEditorUserToCreateSuperUser - Begin - sourceUserPersonaId id - {sourceUserPersonaId}.");

            var aoUserCompanyPropertyRoleDetails = new List<AoUserCompanyPropertyRoleDetail>();

            // Get products assigned to user
            IList<string> aoProductsAvailableToAssign = GetGbSupportedAoEditorUserProductsToAssign(sourceUserPersonaId).ToList();

            // remove Axiometrics product
            aoProductsAvailableToAssign.Remove("AX");
            var allGroupsResponse = GetEditorUserAssignedPropertyGroups(sourceUserPersonaId);

            foreach (var aoProduct in aoProductsAvailableToAssign)
            {
                // Get Assigned companies for product
                var companyResponse = GetEditorUserAssignedCompaniesForProduct(sourceUserPersonaId, aoProduct);

                var propertyGroupList = new List<int>();

                // assigned groups
                var productGroups = allGroupsResponse.Where(x => x.Assignments.Contains(aoProduct));
                var grups = (from i in productGroups select i.GroupId).ToList();
                propertyGroupList.AddRange(grups);

                foreach (var company in companyResponse)
                {
                    // assign ALL roles 
                    var aoRoles = GetRoles(company.CompanyId, aoProduct);
                    var roleList = aoRoles.Select(x => x.Name).ToList();

                    // assign ALL properties 
                    var propertiesResponse = GetProperties(company.CompanyId, aoProduct);
                    var propertyList = (from i in propertiesResponse select i.PropertyId).ToList();

                    // get division
                    var divisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(aoProduct));

                    aoUserCompanyPropertyRoleDetails.Add(new AoUserCompanyPropertyRoleDetail
                    {
                        CompanyId = company.CompanyId,
                        DivisionName = divisionName,
                        ProductName = aoProduct,
                        PropertyGroups = propertyGroupList,
                        SelectedPortfolioValues = propertyList,
                        SelectedRoleValues = roleList,
                        IsAssigned = true
                    });
                }
            }

            WriteToDiagnosticLog(
                $"ManageProductAssetOptimization.CopyEditorUserToCreateSuperUser - End - sourceUserPersonaId id - {sourceUserPersonaId}.");

            return aoUserCompanyPropertyRoleDetails;
        }

        #endregion
    }

    #region AO Specific Classes

    public class AORoles
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        [JsonIgnore, JsonProperty("systemrole")]
        public bool IsCustom { get; set; }

        public string RoleType => IsCustom ? "Default" : "Custom";

        public bool IsAssigned { get; set; }
    }

    public class AoCompanyRoles
    {
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("companyName")] public string CompanyName { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        public bool IsAssigned { get; set; }
        public IList<AORoles> Roles { get; set; }
    }

    public class AoCompanyProperties
    {
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("companyName")] public string CompanyName { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        public bool IsAssigned { get; set; }
        public IList<AoProperty> Properties { get; set; }

        public string AssignedProperties { get; set; }
    }

    public class AOUser
    {
        [JsonProperty("login")] public string Login { get; set; }

        [JsonProperty("email")] public string Email { get; set; }

        [JsonProperty("firstName")] public string FirstName { get; set; }

        [JsonProperty("lastName")] public string LastName { get; set; }

        [JsonProperty("superuser")] public bool? IsSuperUser { get; set; }

        [JsonProperty("internalUser")] public bool? IsInternalUser { get; set; }

        //[JsonProperty("viewAllProperties")]
        //public bool? CanViewAllProperties { get; set; }

        //[JsonProperty("deleted")]
        //public bool? IsDeleted { get; set; }

        [JsonProperty("enabled")] public bool? IsEnabled { get; set; }

        [JsonProperty("divisions")] public IList<Divisions> Divisions { get; set; }

        [JsonProperty("groupsModel")] public IList<GroupModel> GroupsModel { get; set; }

        [JsonProperty("model")] public IList<Model> Model { get; set; }

        [JsonProperty("userId")] public string UserId { get; set; }

        [JsonProperty("oldUserId")] public string OldUserId { get; set; }
    }

    public class Groups
    {
        [JsonProperty("groupName")] public string GroupName { get; set; }

        [JsonProperty("groupId")] public int GroupId { get; set; }

        [JsonProperty("assignments")] public string[] Assignments { get; set; }
    }

    public class Divisions
    {
        [JsonProperty("division")] public string Division { get; set; }

        [JsonProperty("companies")] public IList<AoCompany> Companies { get; set; }

        [JsonProperty("groups")] public IList<Groups> Groups { get; set; }
    }

    public class AoCompany
    {
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("companyName")] public string CompanyName { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        public bool IsAssigned { get; set; }

    }

    public class AoProperties
    {
        [JsonIgnore] public string Division { get; set; }
        public IList<AoProperty> Properties { get; set; }
    }

    public class AoProperty
    {
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("propertyId")] public int PropertyId { get; set; }

        [JsonProperty("propertyName")] public string PropertyName { get; set; }

        [JsonIgnore] public string Relationship { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<AoProduct> Products { get; set; }

        [JsonProperty("statecode", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        public bool IsAssigned { get; set; }
    }

    public class AoProduct
    {
        [JsonProperty("product")] public string Product { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty(PropertyName = "isassigned")]
        public bool IsAssigned { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public int GbProductId { get; set; }

    }

    public class Model
    {
        [JsonProperty("selectedRoleValues")] public IList<string> SelectedRoleValues { get; set; }

        [JsonProperty("selectedPortfolioValues")]
        public IList<int> SelectedPortfolioValues { get; set; }

        [JsonProperty("divisionName")] public string DivisionName { get; set; }

        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("product")] public string Product { get; set; }
    }

    public class GroupModel
    {
        [JsonProperty("division")] public string Division { get; set; }

        [JsonProperty("groupId")] public int GroupId { get; set; }
        [JsonProperty("product")] public string ProductName { get; set; }
        [JsonProperty("enabled")] public bool IsEnabled { get; set; }

    }

    public class AoUserCompanyPropertyRoleDetail
    {
        [JsonProperty("selectedRoleValues")] public IList<string> SelectedRoleValues { get; set; }

        [JsonProperty("selectedPortfolioValues")]
        public IList<int> SelectedPortfolioValues { get; set; }

        [JsonProperty("divisionName")] public string DivisionName { get; set; }

        [JsonProperty("product")] public string ProductName { get; set; }
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        public IList<int> PropertyGroups { get; set; }

        public bool IsAssigned { get; set; }
    }

    public class AoUserCompanyPropertyRoleDetails
    {
        public IList<Divisions> Divisions { get; set; }
        public IList<GroupModel> GroupModel { get; set; }
        public IList<AoUserCompanyPropertyRoleDetail> AoUserCompanyPropertyRoleDetailList { get; set; }
        public bool IsAssigned { get; set; }
    }

    public class AoPropertyGroups
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public bool IsAssigned { get; set; }
    }

    #region Assignable Property Groups

    public class AoAssignableDivisionGroups
    {
        [JsonProperty("division")] public string Division { get; set; }

        [JsonProperty("groups")] public IList<AssignableGroup> Groups { get; set; }
    }

    public class AssignableGroup
    {
        [JsonProperty("propertygroupid")] public int PropertyGroupId { get; set; }

        [JsonProperty("groupname")] public string GroupName { get; set; }

        [JsonProperty("products")] public IList<DivisionGroupProduct> Products { get; set; }
    }

    public class DivisionGroupProduct
    {
        [JsonProperty("product")] public string Product { get; set; }

        [JsonProperty("valid")] public bool Valid { get; set; }

        [JsonProperty("assigned")] public bool Assigned { get; set; }
    }

    public class AoDivisionProduct
    {
        [JsonProperty("division")] public string Division { get; set; }

        [JsonProperty("divisionDescription")] public string DivisionDescription { get; set; }

        [JsonProperty("products")] public IList<AoProduct> Products { get; set; }
    }

    #endregion

    #region Visible Property Groups

    public class AoVisiblePropertyGroups
    {
        [JsonProperty("groups")] public IList<VisibleGroup> Groups { get; set; }
    }

    public class VisibleGroup
    {
        [JsonProperty("groupName")] public string GroupName { get; set; }

        [JsonProperty("groupId")] public int GroupId { get; set; }

        //[JsonProperty("groupOwner")]
        //public GroupOwner? GroupOwner { get; set; }

        [JsonProperty("properties")] public IList<VisibleGroupProperty> Properties { get; set; }
    }

    public class VisibleGroupProperty
    {
        [JsonProperty("propertyId")] public int PropertyId { get; set; }

        [JsonProperty("propertyName")] public string PropertyName { get; set; }
    }

    #endregion

    #region Active Authorities

    public class AoActiveAuthorities
    {
        [JsonProperty("division")] public string Division { get; set; }

        [JsonProperty("products")] public IList<AoProductAuthority> Products { get; set; }
    }

    public class AoProductAuthority
    {
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("authortyName")] public string AuthortyName { get; set; }

        [JsonProperty("product")] public string Product { get; set; }
    }

    #endregion

    #endregion
}
