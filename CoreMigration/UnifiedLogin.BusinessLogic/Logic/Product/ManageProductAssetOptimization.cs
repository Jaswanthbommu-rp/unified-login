using Newtonsoft.Json;
//using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    ///
    /// </summary>
    public class ManageProductAssetOptimization : ManageProductBase, IManageProductAssetOptimization
    {
        #region Private members
        private readonly string _apiUser;
        private readonly string _apiPassword;
        private readonly string _apiEndPoint;
        private readonly string _aoSuperUser;
        private readonly DefaultUserClaim _userClaims;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly IProductRepository _productRepository;
        const int CacheTimeSeconds = 300;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userClaims">DefaultUserClaim of user</param>
        public ManageProductAssetOptimization(DefaultUserClaim userClaims) : base((int)ProductEnum.AssetOptimizer, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _productId = (int)ProductEnum.AssetOptimizer;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _editorRealPageId = userClaims.UserRealPageGuid;
            _userClaims = userClaims;
            _blueBook = new ManageBlueBook(userClaims);

            _apiEndPoint = _productInternalSettingList.First(a => a.Name.Equals("APIEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            _apiUser = _productInternalSettingList.First(a => a.Name.Equals("APIUserName", StringComparison.OrdinalIgnoreCase)).Value;
            _apiPassword = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.Equals("APIPassword", StringComparison.OrdinalIgnoreCase)).Value));
            _aoSuperUser = _productInternalSettingList.First(a => a.Name.Equals("ProductSuperUserLoginName", StringComparison.OrdinalIgnoreCase)).Value;
            _organizationRepository = new OrganizationRepository(userClaims);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///Get companies
        /// </summary>
        public ListResponse GetCompanies(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"Beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                // to get _editorProductUserId

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName} - {result.ErrorReason}" });
                    return result;
                }

                string productUserId = _productUserId;
                var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/";
                var editorUserProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);
                var productDivisionName =
                    ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));

                var ac = editorUserProfile.Divisions.Where(x => x.Division == productDivisionName).ToList();
                var allCompanies = ac.SelectMany(f => f.Companies).ToList();

                if (userPersonaId == 0 && string.IsNullOrEmpty(_productUserId) && !string.IsNullOrWhiteSpace(userLoginName))
                {
                    productUserId = userLoginName;
                }

                if (!string.IsNullOrEmpty(productUserId))
                {
                    productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/{productUserId.ToLower()}/";
                    var productUserProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

                    if (productUserProfile != null)
                    {
                        var productUserComp =
                        productUserProfile.Divisions.Where(x => x.Division == productDivisionName).ToList();
                        var productUserCompanies = productUserComp.SelectMany(f => f.Companies).ToList();

                        allCompanies = FilterAssignedCompanies(allCompanies, productUserCompanies);
                    }
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}." });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the Companies.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies",$"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} " +
                    $"for product {productName} " }, exception: ex);
            }

            return response;
        }

        /// <summary>
        ///Get companies and roles
        /// </summary>
        public ListResponse GetCompaniesWithRoles(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompaniesWithRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}" });

            var response = new ListResponse();
            try
            {
                var allCompanies = GetCompanies(editorPersonaId, userPersonaId, productName, datafilter, userLoginName).Records.Cast<AoCompany>();

                IList<AoCompanyRoles> companyRoles = new List<AoCompanyRoles>();
                // for each company get roles
                foreach (var company in allCompanies)
                {
                    List<AORoles> roles = GetRoles(company.CompanyId, productName, userLoginName, userPersonaId).ToList();

                    if (roles?.Count > 0)
                    {
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
                }

                response = new ListResponse()
                {
                    Records = companyRoles.Cast<object>().ToList(),
                    TotalRows = companyRoles.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompaniesWithRoles", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the GetCompaniesWithRoles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompaniesWithRoles", $"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        ///Get product roles
        /// </summary>
        public ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}" });

            var response = new ListResponse();
            ListResponse result = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string aoCompanyId = company.CompanyInstanceSourceId;
                if (string.IsNullOrEmpty(aoCompanyId))
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", "Error looking for company id in bluebook." });
                    return result;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Found blue book company source id {aoCompanyId}" });

                List<AORoles> roles = GetRoles(Convert.ToInt32(aoCompanyId), productName, userLoginName, userPersonaId).ToList();
                List<ProductRole> companyRoles = new List<ProductRole>();
                if (roles?.Count > 0)
                {
                    roles = roles.OrderBy(x => x.DisplayName).ToList();
                    int i = 1;

                    foreach (var role in roles)
                    {
                        companyRoles.Add(new ProductRole
                        {
                            ID = i.ToString(),
                            Name = role.Name,
                            Description = role.DisplayName,
                            IsAssigned = role.IsAssigned
                        });
                        i++;
                    }
                }

                response = new ListResponse()
                {
                    Records = companyRoles.Cast<object>().ToList(),
                    TotalRows = companyRoles.Count,
                    RowsPerPage = companyRoles.Count,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}" }, exception: ex);

                response = new ListResponse();
                response.IsError = true;

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                }
            }

            return response;
        }

        /// <summary>
        /// Get Companies With Properties
        /// </summary>
        public ListResponse GetCompaniesWithProperties(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompaniesWithProperties", $"Beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." });

            var response = new ListResponse();
            try
            {
                var allCompanies =
                    GetCompanies(editorPersonaId, userPersonaId, productName, datafilter, userLoginName).Records.Cast<AoCompany>();

                IList<AoCompanyProperties> companyProperties = new List<AoCompanyProperties>();
                // for each compnay get properties
                foreach (var company in allCompanies)
                {
                    AoPropertyList objAoPropertyList = GetProperties(company.CompanyId, productName, userLoginName, userPersonaId);
                    objAoPropertyList.Properties = objAoPropertyList.Properties.OrderBy(x => x.PropertyName).ToList();


                    if (objAoPropertyList.Properties != null)
                    {
                        string assignedCount = $"{objAoPropertyList.Properties.Count(p => p.IsAssigned)} of {objAoPropertyList.Properties.Count}";

                        companyProperties.Add(new AoCompanyProperties
                        {
                            CompanyId = company.CompanyId,
                            CompanyName = company.CompanyName,
                            IsAssigned = company.IsAssigned,
                            Status = company.Status,
                            AssignedProperties = assignedCount,
                            Properties = objAoPropertyList.Properties
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompaniesWithProperties", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the GetCompaniesWithProperties.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompaniesWithProperties", $"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get Operators
        /// </summary>
        public ListResponse GetOperators(long editorPersonaId, long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOperators", $"Beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}." });
            Dictionary<string, bool> allProperties = new Dictionary<string, bool>();
            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                IList<tag> objAoOperatorList = new List<tag>();
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetOperators", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} - {result.ErrorReason}" });
                    return result;
                }
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string aoCompanyId = company.CompanyInstanceSourceId;
                string productPropertyApiUrl = $"{_apiEndPoint}company/{aoCompanyId}/delegated/operators"; //https://aoqa.realpage.com/ysconfig/ws/company/2772/delegated/operators
                var operatorsResponse = GetResultFromApi<IList<tag>>(productPropertyApiUrl);
                if (operatorsResponse == null)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No Operators received from product for company : {aoCompanyId} user with editorPersona id - {editorPersonaId}." });
                    response.ErrorReason = "No Operators received for company.";
                    return response;
                }
                objAoOperatorList = operatorsResponse.ToList();

                response = new ListResponse()
                {
                    Records = objAoOperatorList.Cast<object>().ToList(),
                    TotalRows = objAoOperatorList.Count,
                    RowsPerPage = objAoOperatorList.Count,
                    ErrorReason = string.Empty,
                    TotalPages = 1,
                    Additional = allProperties
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOperators", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}." });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the GetOperators.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetOperators", $"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}." }, exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Get Properties With Operators
        /// </summary>
        public ListResponse GetPropertiesWithOperators(long editorPersonaId, long userPersonaId, string operatorCode, string operatorValue)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesWithOperators", $"Beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}." });
            Dictionary<string, bool> allProperties = new Dictionary<string, bool>();
            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesWithOperators", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} - {result.ErrorReason}" });
                    return result;
                }
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string aoCompanyId = company.CompanyInstanceSourceId;

                string productPropertyApiUrl = $"{_apiEndPoint}company/{aoCompanyId}/delegated/properties?operatorCode={Uri.EscapeDataString(operatorCode)}&operatorValue={Uri.EscapeDataString(operatorValue)}"; //https://aoqa.realpage.com/ysconfig/ws/company/7434/delegated/properties?operatorCode=Kai_Tag&operatorValue=Kai2
                var tag = new tag
                {
                    operatorCode = operatorCode,
                    operatorValue = operatorValue
                };
                IList<ProductProperty> companyProperties = new List<ProductProperty>();
                var apiresult = GetResultFromApi<IList<AoProperty>>(productPropertyApiUrl);
                IList<AoProperty> apiResponse = apiresult != null ? apiresult.ToList() : new List<AoProperty>();

                if (apiResponse.Count != null && apiResponse.Count > 0)
                {
                    foreach (var property in apiResponse)
                    {
                        companyProperties.Add(new ProductProperty
                        {
                            ID = property.PropertyId.ToString(),
                            Name = property.PropertyName
                        });
                    }
                }
                response = new ListResponse()
                {
                    Records = companyProperties.Cast<object>().ToList(),
                    TotalRows = companyProperties.Count,
                    RowsPerPage = companyProperties.Count,
                    ErrorReason = string.Empty,
                    TotalPages = 1,
                    Additional = allProperties
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesWithOperators", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}." });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the GetPropertiesWithOperators.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesWithOperators", $"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId}." }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get product Properties
        /// </summary>
        public ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Beginning of method for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." });

            var response = new ListResponse();
            ListResponse result = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            Dictionary<string, bool> allProperties = new Dictionary<string, bool>();
            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string aoCompanyId = company.CompanyInstanceSourceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Found blue book company source id {aoCompanyId}" });

                IList<ProductProperty> companyProperties = new List<ProductProperty>();
                // for  properties
                AoPropertyList objAoPropertyList = GetProperties(Convert.ToInt32(aoCompanyId), productName, userLoginName, userPersonaId);
                objAoPropertyList.Properties = objAoPropertyList.Properties.OrderBy(x => x.PropertyName).ToList();

                if (objAoPropertyList.Properties != null)
                {
                    string assignedCount = $"{objAoPropertyList.Properties.Count(p => p.IsAssigned)} of {objAoPropertyList.Properties.Count}";
                    allProperties.Add("allProperties", objAoPropertyList.allProperties);
                    foreach (var property in objAoPropertyList.Properties)
                    {
                        companyProperties.Add(new ProductProperty
                        {
                            ID = property.PropertyId.ToString(),
                            Name = property.PropertyName,
                            IsAssigned = property.IsAssigned,
                            State = property.State
                        });
                    }
                }

                response = new ListResponse()
                {
                    Records = companyProperties.Cast<object>().ToList(),
                    TotalRows = companyProperties.Count,
                    RowsPerPage = companyProperties.Count,
                    ErrorReason = string.Empty,
                    TotalPages = 1,
                    Additional = allProperties
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." });
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {userPersonaId} for product {productName}." }, exception: ex);
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
            try
            {
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
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                    return response;
                }

                var blueAOCompanyInfo = GetProductCompanyInstanceId(_udmSourceCode);
                ProductRepository productRepository = new ProductRepository();
                string product = Convert.ToString((int)ProductEnum.AssetOptimizer);
                IList<SharedObjects.Product.OrganizationProductUser> productUserList = productRepository.GetProductUsersByCompany(_editorPersona.OrganizationPartyId, product);
                List<AssetOptimizationMigrationUser> usersData = new List<AssetOptimizationMigrationUser>();
                var orgMigrationUsersData = migrationResponse.Where(m => m.CompanySourceInstanceId != null && m.CompanySourceInstanceId.Equals(blueAOCompanyInfo.CompanyInstanceSourceId)).ToList();
                if (productUserList?.Count > 0)
                {
                    orgMigrationUsersData.RemoveAll(o => productUserList.Any(p => p.ProductUserName == o.UserName));
                }
                usersData = orgMigrationUsersData;

                var migrationUsers = new List<MigrationUser>();
                foreach (var user in usersData)
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}." });
                response.RowsPerPage = migrationResponse.Count;
                response.ErrorReason = string.Empty;
                response.IsError = false;
                response.TotalPages = 1;
                response.Records = migrationUsers.Cast<object>().ToList();
                response.TotalRows = migrationResponse.Count;
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);

            }
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
            IList<UserOrganization> userPersonaOrganizationList = new List<UserOrganization>();
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

            foreach (MigrateUser migrateUser in migrateUsers)
            {
                userPersonaOrganizationList = _userLoginRepository.ListOrganizationByLoginName(migrateUser.UnifiedLoginUserName);
                var externaluser = userPersonaOrganizationList.Where(m => m.OrganizationPartyId == _userClaims.OrganizationPartyId && m.PartyRoleTypeId == UserTypeConstants.ExternalUser).ToList();
                if (externaluser.Any())
                {
                    migrateUsers.Remove(migrateUser);
                }
            }

            if (!migrateUsers.Any())
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "Not updating status for AO External user." });
                migrateResponse.Status = true;
                migrateResponse.Message = "success";
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
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "PutAsJsonAsync" }, logData: logData);
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"PostAsJsonAsync" }, logData: logData);
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
            return ManageAssetOptimizationUser(createUserPersonaId, assignUserPersonaId, rolePropAoUserCompanyPropertyRoleDetailList, out var additionalParameters, batchProcessType);
        }

        /// <summary>
        /// Create/update a user in AO-BI
        /// </summary>
        public string ManageAssetOptimizationUser(long editorPersonaId, long productUserPersonaId, IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"Begin create/update user for user with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });
            additionalParameters = new List<AdditionalParameters>();
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, productUserPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"Error for user with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                string returnResult = "";
                List<string> userAOProducts = new List<string>();
                bool isRealpageAccessUser = false;
                var persona = _managePersona.GetPersona(productUserPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var productUserGbLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
                UserLogin userLogin = _manageUserLogin.GetUserLogin(realPageId, persona.OrganizationPartyId);

                //Bug 677720: PME-204114/ATLANTIC PACIFIC PROPERTY MANAGEMENT LLC/New AO email address getting created when a Unified Login user is updated.
                var productUserName = GetSamlProductUserName(productUserPersonaId, "");
                productUserGbLogin.LoginName = !string.IsNullOrEmpty(productUserName) ? productUserName : productUserGbLogin.LoginName;

                IList<Persona> personaList = _managePersona.ListActivePersona(persona.RealPageId, false);
                IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);
                var personaOrganization = organizationList.FirstOrDefault(i => i.PartyId == persona.OrganizationPartyId);
                bool hasMultiCompany = personaList.Count(p => p.OrganizationPartyId != persona.OrganizationPartyId && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;
                bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

                if (productUserGbLogin == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"User Login Name not exist in greenbook for editorPersonaId - {editorPersonaId}." });

                    return "User Login Name not exists in greenbook.";
                }

                var blueAOCompanyInfo = GetProductCompanyInstanceId(_udmSourceCode);
                if (blueAOCompanyInfo.CompanyInstanceSourceId == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"Error - Get CompanyMap - greenBookCares not enabled {blueAOCompanyInfo}" });

                    return "Company Setup Error: Please Contact Support.";
                }

                string userEmailAddress = GetUserEmailAddress(realPageId, productUserGbLogin.LoginName, productUserPersonaId);
                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"Error - No Valid Email Address Found - For User {productUserGbLogin.LoginName}" });
                    return "Valid Email Address Error: Please Contact Support.";
                }

                var aoUser = new AOUser
                {
                    IsInternalUser = false, // Initial release is w/o internal user
                    IsEnabled = true,
                    IsSuperUser = false,
                    Email = userEmailAddress.ToLower(),

                    Login = productUserGbLogin.LoginName.ToLower(),
                    OldUserId = string.Empty,
                    UserId = productUserGbLogin.LoginName.ToLower(),

                    FirstName = person.FirstName,
                    LastName = person.LastName,
                };
                if (!IsSuperUser(productUserPersonaId) && !userLogin.IsActive.Value)
                {
                    aoUser.IsEnabled = false;
                }

                var companyAdmin = _organizationRepository.GetOrganizationAdminUserRealPageId(persona.Organization.RealPageId);

                if (companyAdmin == realPageId)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"Begining realpage access user creation process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });

                    var aOSpecialEditorUser = _productInternalSettingList.First(a => a.Name.Equals("AOSpecialEditorUser", StringComparison.OrdinalIgnoreCase)).Value;

                    // Get all ao products available in the company
                    var productsApiUrl = $"{_apiEndPoint}company/{blueAOCompanyInfo.CompanyInstanceSourceId}/products";
                    var allAOProducts = GetResultFromApi<IList<GroupModel>>(productsApiUrl);

                    IList<GroupModel> groupsModel = new List<GroupModel>();
                    IList<Model> modelList = new List<Model>();
                    foreach (var item in allAOProducts)
                    {
                        if (ProductEnumHelper.CheckAoProductSupportedByGreenBook(item.ProductName))
                        {
                            var groupModel = new GroupModel
                            {
                                Division = item.Division,
                                ProductName = item.ProductName,
                                IsEnabled = true
                            };

                            var model = new Model
                            {
                                CompanyId = Convert.ToInt32(blueAOCompanyInfo.CompanyInstanceSourceId),
                                DivisionName = item.Division,
                                Product = item.ProductName,
                                SelectedPortfolioValues = new List<int>(),
                                SelectedRoleValues = new List<string>(),
                                allProperties = true
                            };
                            groupsModel.Add(groupModel);
                            modelList.Add(model);
                        }
                    }

                    aoUser.GroupsModel = groupsModel;
                    aoUser.Model = modelList;

                    //Create user method with AO Special Editor user
                    if (string.IsNullOrEmpty(productUserName))
                    {
                        returnResult = PostApi($"{_apiEndPoint}user/profile/{aOSpecialEditorUser.ToLower()}/", aoUser);
                    }
                    else
                    {
                        returnResult = PutApi($"{_apiEndPoint}user/profile/{aOSpecialEditorUser.ToLower()}/", aoUser);
                    }
                    if (string.IsNullOrEmpty(returnResult))
                    {
                        // Create GB Product association - for realpage access user
                        var productList = (from x in aoUser.Model select x.Product).Distinct().ToList();

                        CreateProductUserInGreenBook(editorPersonaId, productUserPersonaId, productList, productUserGbLogin.LoginName.ToLower());
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"Completed realpage access user creation process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });

                    return returnResult;
                }
                else
                {
                    if (IsSuperUser(productUserPersonaId) && aoGbUserCompanyPropertyRoleDetails.Any(m => !m.IsAssigned))
                    {
                        aoUser.IsEnabled = false;
                    }
                    if (aoGbUserCompanyPropertyRoleDetails != null)
                    {
                        foreach (var data in aoGbUserCompanyPropertyRoleDetails)
                        {
                            if (data.CompanyId == 0)
                            {
                                data.CompanyId = Convert.ToInt32(blueAOCompanyInfo.CompanyInstanceSourceId);
                            }
                        }
                    }

                    userAOProducts = GetAOProductsForNewMultiCompanyUser(editorPersonaId, productUserGbLogin.LoginName);
                    var inputProducts = aoGbUserCompanyPropertyRoleDetails.Select(s => s.ProductName).ToList();

                    List<string> deletedProducts = userAOProducts.Except(inputProducts).ToList();
                    List<string> addedExistingProducts = userAOProducts.Concat(inputProducts).Intersect(inputProducts).ToList();

                    Dictionary<string, List<ProductRole>> requiredProductRoles = new Dictionary<string, List<ProductRole>>();
                    Dictionary<string, List<ProductProperty>> requiredProductProperties = new Dictionary<string, List<ProductProperty>>();
                    Dictionary<string, List<AoPropertyGroup>> requiredProductPropertyGroups = new Dictionary<string, List<AoPropertyGroup>>();
                    List<string> mergedList = addedExistingProducts.Concat(deletedProducts).Distinct().ToList();

                    if (mergedList.Any())
                    {
                        //GetRoles
                        foreach (string s in mergedList)
                        {
                            var roles = GetProductRoles(editorPersonaId, productUserPersonaId, s, new RequestParameter(), "");
                            requiredProductRoles.Add(s, roles.Records.Cast<ProductRole>().ToList());
                        }
                    }
                    var noPropertiesProductList = _productInternalSettingRepository.GetProductInternalSettings(3);
                    var productsWithNoProperties = new List<int>();
                    if (noPropertiesProductList.Exists(s => s.Name.Equals("UserAccessDetails_ProductsWithNoProperties")))
                    {
                        var val = noPropertiesProductList.Find(s => s.Name.Equals("UserAccessDetails_ProductsWithNoProperties", StringComparison.OrdinalIgnoreCase)).Value;
                        productsWithNoProperties = val.Split(',').Select(int.Parse).ToList();
                    }

                    var selectedAOProductsWithPropTab = _productRepository.GetAllProducts().Where(x => x.UDMSourceCode == "AO" && mergedList.Contains(x.BooksProductCode) && !productsWithNoProperties.Contains(x.ProductId));
                    var aoPropsProducts = selectedAOProductsWithPropTab.Select(s => s.BooksProductCode).ToList();
                    foreach (var s in aoPropsProducts)
                    {
                        //GetProperties
                        var properties = GetProductProperties(editorPersonaId, productUserPersonaId, s, new RequestParameter(), "");
                        if (properties.Records != null)
                        {
                            requiredProductProperties.Add(s, properties.Records.Cast<ProductProperty>().ToList());
                        }

                        //GetPropertyGroups
                        var propertyGroups = GetProductPropertyGroups(editorPersonaId, productUserPersonaId, s, "");
                        if (propertyGroups.Records != null)
                        {
                            requiredProductPropertyGroups.Add(s, propertyGroups.Records?.Cast<AoPropertyGroup>().ToList());
                        }
                    }

                    if (string.IsNullOrEmpty(_productUsername) && organizationList?.Count > 1 && userAOProducts?.Count > 0)
                    {
                        //Check to see if user has multi company, then get user products and assign before any updates
                        CreateProductUserInGreenBook(editorPersonaId, productUserPersonaId, userAOProducts, productUserGbLogin.LoginName.ToLower());
                        _productUsername = productUserGbLogin.LoginName.ToLower();
                    }

                    bool aoProductAssigned = aoGbUserCompanyPropertyRoleDetails.Any(p => p.IsAssigned);

                    // Check if GB super user
                    if (IsSuperUser(productUserPersonaId))
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"User is super user with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });

                        aoGbUserCompanyPropertyRoleDetails = CopyEditorUserToCreateSuperUser(editorPersonaId);

                        // For Investment Analytics (MA) assign US market to super user
                        var allGroups = GetAllPropertyGroups();
                        var usGroupId = allGroups.Groups.FirstOrDefault(x => x.GroupName == "US")?.GroupId;

                        if (usGroupId != null && usGroupId != 0)
                        {
                            var ss = aoGbUserCompanyPropertyRoleDetails.Where(x => x.ProductName == "MA")
                                    .SelectMany(c => c.PropertyGroups)
                                    .ToList();
                            if (!ss.Contains(usGroupId.Value))
                            {
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

                    if (!IsSuperUser(productUserPersonaId))
                    {
                        foreach (var item in aoGbUserCompanyPropertyRoleDetails.Where(x => x.SelectedPortfolioValues != null && x.SelectedPortfolioValues.Count() > 0))
                        {
                            if (item.SelectedPortfolioValues[0] == -1)
                            {
                                // assign ALL properties 
                                var propertiesResponse = GetProperties(item.CompanyId, item.ProductName);
                                var propertyList = (from i in propertiesResponse.Properties select i.PropertyId).ToList();

                                item.allProperties = true;
                                item.SelectedPortfolioValues = propertyList;
                            }
                        }
                    }

                    //Create/Update single/multi company AO Products
                    if (aoGbUserCompanyPropertyRoleDetails.Count > 0)
                    {
                        if (userAOProducts?.Count == 0)
                        {
                            aoUser.GroupsModel = GetBundledGroups(aoGbUserCompanyPropertyRoleDetails);
                            aoUser.Divisions = new List<Divisions>();
                            aoUser.Model = GetModel(aoGbUserCompanyPropertyRoleDetails);

                            if(!aoUser.Model.Any())
                            {
                                aoUser.IsEnabled = false;
                            }

                            //Create user method with AO Special Editor user
                            if (string.IsNullOrEmpty(productUserName))
                            {
                                returnResult = PostApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);
                            }
                            else
                            {
                                returnResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);
                            }

                            if (string.IsNullOrEmpty(returnResult))
                            {
                                // Create GB Product association - for new user insert record
                                var productList = (from x in aoUser.Model select x.Product).Distinct().ToList();

                                CreateProductUserInGreenBook(editorPersonaId, productUserPersonaId, productList, productUserGbLogin.LoginName.ToLower());
                            }

                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"Completed user creation process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });
                            //Activity log details
                            if (addedExistingProducts.Any())
                            {
                                additionalParameters.AddRange(ExtractActivityDetailLogs(addedExistingProducts, requiredProductRoles, requiredProductProperties, requiredProductPropertyGroups, aoUser, aoPropsProducts, editorPersonaId, productUserPersonaId));
                            }
                            if (deletedProducts.Any())
                            {
                                additionalParameters.AddRange(ExtractActivityDetailLogs(deletedProducts, requiredProductRoles, requiredProductProperties, requiredProductPropertyGroups, aoUser, aoPropsProducts, editorPersonaId, productUserPersonaId));
                            }
                            return returnResult;
                        }
                        // Update User logic
                        // Get Copy of User from AO
                        var copiedAoUserCompanyPropertyRoleDetails = CopyRegularUser(editorPersonaId, productUserPersonaId);
                        // store existing assigned products
                        var existingAoProducts = copiedAoUserCompanyPropertyRoleDetails;

                        UpdateProductRolePropertyDetails(aoGbUserCompanyPropertyRoleDetails, copiedAoUserCompanyPropertyRoleDetails, persona);

                        aoUser.GroupsModel = GetBundledGroups(copiedAoUserCompanyPropertyRoleDetails);
                        aoUser.Divisions = new List<Divisions>();
                        aoUser.Model = GetModel(copiedAoUserCompanyPropertyRoleDetails);

                        aoUser.UserId = _productUserId.ToLower();
                        aoUser.OldUserId = _productUserId.ToLower();

                        returnResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);

                        if (IsSuperUser(productUserPersonaId))
                        {
                            foreach (AoUserCompanyPropertyRoleDetail user in aoGbUserCompanyPropertyRoleDetails)
                            {
                                user.IsAssigned = aoProductAssigned;
                            }
                        }

                        if (string.IsNullOrEmpty(returnResult))
                        {
                            UpdateProductUserInGreenBook(editorPersonaId, productUserPersonaId, productUserGbLogin.LoginName.ToLower(), existingAoProducts, aoGbUserCompanyPropertyRoleDetails);

                            //Activity log details
                            if (addedExistingProducts.Any())
                            {
                                additionalParameters.AddRange(ExtractActivityDetailLogs(addedExistingProducts, requiredProductRoles, requiredProductProperties, requiredProductPropertyGroups, aoUser, aoPropsProducts, editorPersonaId, productUserPersonaId));
                            }
                            if (deletedProducts.Any())
                            {
                                additionalParameters.AddRange(ExtractActivityDetailLogs(deletedProducts, requiredProductRoles, requiredProductProperties, requiredProductPropertyGroups, aoUser, aoPropsProducts, editorPersonaId, productUserPersonaId));
                            }
                        }
                        else
                        {
                            // check if error is because of removing all products
                            try
                            {
                                if (IsSuperUser(productUserPersonaId))
                                {
                                    foreach (AoUserCompanyPropertyRoleDetail user in aoGbUserCompanyPropertyRoleDetails)
                                    {
                                        user.IsAssigned = true;
                                    }
                                }
                                var jsObj = JsonConvert.DeserializeObject<dynamic>(returnResult);
                                if (jsObj.errorResults[0].message.Value.Equals("A user must be attached to at least one company and one role", StringComparison.OrdinalIgnoreCase))
                                {
                                    // keep old products to avoid API error
                                    copiedAoUserCompanyPropertyRoleDetails = CopyRegularUser(editorPersonaId, productUserPersonaId);
                                    // store existing assigned products
                                    existingAoProducts = copiedAoUserCompanyPropertyRoleDetails;
                                    UnAssignProductRolePropertyDetails(aoGbUserCompanyPropertyRoleDetails, copiedAoUserCompanyPropertyRoleDetails, persona);
                                    // get existing AP details
                                    aoUser.GroupsModel = GetBundledGroups(copiedAoUserCompanyPropertyRoleDetails);
                                    aoUser.Divisions = new List<Divisions>();
                                    aoUser.Model = GetModel(copiedAoUserCompanyPropertyRoleDetails);
                                    // disable user
                                    aoUser.IsEnabled = false;
                                    var disableUserResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);
                                    if (IsSuperUser(productUserPersonaId))
                                    {
                                        foreach (AoUserCompanyPropertyRoleDetail user in aoGbUserCompanyPropertyRoleDetails)
                                        {
                                            user.IsAssigned = aoProductAssigned;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(disableUserResult))
                                    {
                                        // Disable products from GB
                                        UpdateProductUserInGreenBook(editorPersonaId, productUserPersonaId, productUserGbLogin.LoginName.ToLower(), existingAoProducts, aoGbUserCompanyPropertyRoleDetails);
                                        return string.Empty;
                                    }

                                    return
                                        $"Error while setting disable flag on user {aoUser.Login} API -{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()} , disableUserResult - {disableUserResult}";
                                }

                                return returnResult;
                            }
                            catch (Exception ex)
                            {
                                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAssetOptimizationUser", $"ERROR for user {productUserGbLogin.LoginName.ToLower()} while parsing AO PUT API response to check condition if all products removed. Result from API {_apiEndPoint}user/profile/{_editorProductUserId.ToLower()} is {returnResult}" });
                                return returnResult;
                            }
                        }
                    }
                    return returnResult;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageAssetOptimizationUser", $"Exception during user creation process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });
                return ex.Message;
            }
        }

        private List<AdditionalParameters> ExtractActivityDetailLogs(List<string> productsList, Dictionary<string, List<ProductRole>> requiredProductRoles, Dictionary<string, List<ProductProperty>> requiredProductProperties, Dictionary<string, List<AoPropertyGroup>> requiredProductPropertyGroups, AOUser aoUser, List<string> aoPropsProducts, long editorPersonaId, long productUserPersonaId)
        {
            var additionalParams = new List<AdditionalParameters>();
            try
            {
                //roles
                foreach (var s in productsList)
                {
                    var productName = ProductEnumHelper.GetAoProductDescription(ProductEnumHelper.GetAoProductEnum(s));

                    var oldRoles = requiredProductRoles.First(r => r.Key == s).Value.FindAll(f => f.IsAssigned);
                    var currentRoles = aoUser.Model.FirstOrDefault(e => e.Product == s);

                    var removedRoleNames = oldRoles.Select(or => or.Name).ToList();
                    var currentRoleNames = currentRoles?.SelectedRoleValues ?? new List<string>();

                    var removedRoles = removedRoleNames.Except(currentRoleNames).ToList();
                    var addedRoles = currentRoleNames.Except(removedRoleNames).ToList();

                    //Old Roles
                    if (removedRoles.Any())
                    {
                        foreach (var r in removedRoles)
                        {
                            additionalParams.Add(new AdditionalParameters { Key = productName + " Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", r) });
                        }
                    }

                    //New Roles
                    if (addedRoles.Any())
                    {
                        foreach (var r in addedRoles)
                        {
                            additionalParams.Add(new AdditionalParameters { Key = productName + " Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", r) });
                        }
                    }
                }

                foreach (var p in aoPropsProducts)
                {
                    var productName = ProductEnumHelper.GetAoProductDescription(ProductEnumHelper.GetAoProductEnum(p));
                    var oldProps = new List<ProductProperty>();

                    if (requiredProductProperties != null && requiredProductProperties.FirstOrDefault(r => r.Key == p).Value != null)
                    {
                        oldProps = requiredProductProperties.FirstOrDefault(r => r.Key == p).Value.FindAll(f => f.IsAssigned == true);
                    }
                    var removedPropsIds = oldProps.Select(op => int.Parse(op.ID)).ToList();
                    var currentPropsIds = aoUser.Model.FirstOrDefault(m => m.Product == p)?.SelectedPortfolioValues ?? new List<int>();
                    var allProp = requiredProductProperties.FirstOrDefault(r => r.Key == p).Value;

                    var removedProps = removedPropsIds.Except(currentPropsIds).ToList();
                    var addedProps = currentPropsIds.Except(removedPropsIds).ToList();

                    //Old Props
                    if (removedProps.Any() && allProp != null)
                    {
                        foreach (var pr in removedProps)
                        {
                            additionalParams.Add(new AdditionalParameters { Key = productName + " Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", allProp.Find(f => f.ID == pr.ToString()).Name) });
                        }
                    }

                    //New Props
                    if (addedProps.Any() && allProp != null)
                    {
                        foreach (var pr in addedProps)
                        {
                            additionalParams.Add(new AdditionalParameters { Key = productName + " Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", allProp.Find(f => f.ID == pr.ToString()).Name) });
                        }
                    }

                    var allPropGrps = requiredProductPropertyGroups.FirstOrDefault(r => r.Key == p).Value;
                    var oldPropGrps = requiredProductPropertyGroups.FirstOrDefault(r => r.Key == p).Value?.FindAll(f => f.IsAssigned)?.Select(pg => int.Parse(pg.ID));
                    if (oldPropGrps == null)
                    {
                        oldPropGrps = new List<int>();
                    }
                    List<int> newPropGrps = new List<int>();
                    if (aoUser.GroupsModel.Count > 0)
                    {
                        newPropGrps = aoUser.GroupsModel?.Select(pg => pg.GroupId).ToList();
                    }

                    var removedPropGrps = oldPropGrps?.Except(newPropGrps).ToList();
                    var addedPropGrps = newPropGrps?.Except(oldPropGrps).ToList();

                    //Old Prop Groups
                    if (removedPropGrps.Any() && allPropGrps != null)
                    {
                        foreach (var grp in removedPropGrps)
                        {
                            additionalParams.Add(new AdditionalParameters { Key = productName + " Property Groups", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", allPropGrps.Find(ap => ap.ID == grp.ToString()).Name) });
                        }
                    }

                    //New Prop Groups
                    if (addedPropGrps.Any() && allPropGrps != null)
                    {
                        foreach (var pg in addedPropGrps)
                        {
                            //var grp = requiredProductPropertyGroups.First(r => r.Key == p).Value.Find(f => f.ID == pg.GroupId.ToString());
                            additionalParams.Add(new AdditionalParameters { Key = productName + " Property Groups", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", allPropGrps.Find(ap => ap.ID == pg.ToString()).Name) });
                        }
                    }
                }

                return additionalParams;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ExtractActivityDetailLogs", $"Error building Activity logs for AO. editorPersonaId: {editorPersonaId}, productUserPersonaId: {productUserPersonaId}" });
                return additionalParams;
            }
        }

        private string CreateUpdateAOBIProduct(string userEmailAddress,
                                               long editorPersonaId,
                                               long productUserPersonaId,
                                               IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails,
                                               Persona persona,
                                               Person person,
                                               UserLoginOnly productUserGbLogin)
        {
            string biProductUserName = "";
            string result = "";
            IList<AoUserCompanyPropertyRoleDetail> existingAoProducts = null;
            IList<AoUserCompanyPropertyRoleDetail> unAssignedProducts = null;

            biProductUserName = GetSamlProductUserName(productUserPersonaId, "BI");

            var biAOUser = new AOUser
            {
                IsInternalUser = false,
                IsEnabled = true,
                IsSuperUser = false,
                Email = userEmailAddress.ToLower(),
                Login = productUserGbLogin.LoginName.ToLower(),
                OldUserId = string.Empty,
                UserId = productUserGbLogin.LoginName.ToLower(),
                FirstName = person.FirstName,
                LastName = person.LastName,
            };

            if (!string.IsNullOrEmpty(biProductUserName))
            {
                unAssignedProducts = aoGbUserCompanyPropertyRoleDetails.Where(x => x.IsAssigned == false).ToList();
                if (unAssignedProducts.Count() > 0)
                {
                    biAOUser.IsEnabled = false;
                    aoGbUserCompanyPropertyRoleDetails = CopyRegularUser(editorPersonaId, productUserPersonaId, biProductUserName);
                    // store existing assigned products
                    existingAoProducts = aoGbUserCompanyPropertyRoleDetails;
                }
                //foreach (var unAssignedProduct in unAssignedProducts)
                //{
                //	var matches = aoGbUserCompanyPropertyRoleDetails.Where(p => p.ProductName == unAssignedProduct.ProductName).ToList();
                //	if (matches.Any())
                //	{
                //		////Remove Roles for the product which un assigned
                //		//foreach (var match in matches)
                //		//{
                //		//	//match.SelectedRoleValues = new List<string>();
                //		//	aoGbUserCompanyPropertyRoleDetails.Remove(match);
                //		//}
                //	}
                //}

            }

            biAOUser.GroupsModel = GetBundledGroups(aoGbUserCompanyPropertyRoleDetails);
            biAOUser.Divisions = new List<Divisions>();
            biAOUser.Model = GetModel(aoGbUserCompanyPropertyRoleDetails);

            //IsAOBIProductExistsInOtherOrganization(editorPersonaId, productUserGbLogin.LoginName)
            if (string.IsNullOrEmpty(biProductUserName))
            {
                string biLoginName = "";
                // get a login name that isn't in use for the new user
                bool foundUserName = false;
                int incrementor = 1;
                string newproductUsername = $"{person.FirstName.TrimWhiteSpace().Substring(0, 1)}" + $"{person.LastName.TrimWhiteSpace()}".ToLower();
                biLoginName = $"{newproductUsername}{incrementor.ToString()}@noreply.com";

                while (!foundUserName)
                {
                    if (CheckUniqueAOUserName(biLoginName))
                    {
                        incrementor++;
                        biLoginName = $"{newproductUsername}{incrementor.ToString()}@noreply.com";
                    }
                    else
                    {
                        foundUserName = true;
                    }

                }

                biAOUser.Login = biLoginName.ToLower();
                biAOUser.UserId = biLoginName.ToLower();

                var createBIResult = PostApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", biAOUser);

                if (string.IsNullOrEmpty(createBIResult))
                {
                    // Create BI GB Product association - for new user insert record						
                    _samlRepository.CreateSamlUserAttribute(productUserPersonaId, (int)ProductEnum.AoBusinessIntelligence, SamlAttributeEnum.productUsername, biAOUser.Login.ToLower());
                    _samlRepository.CreateSamlUserAttribute(productUserPersonaId, (int)ProductEnum.AoBusinessIntelligence, SamlAttributeEnum.UserId, biAOUser.Login.ToLower());
                    UpdateProductSettingProductStatus(productUserPersonaId, _productSettingType_ProductStatus, (int)ProductEnum.AoBusinessIntelligence, (int)ProductBatchStatusType.Success);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateAOBIProduct", $"Completed BI user creation process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });
                    return createBIResult;
                }
                var jsObj = JsonConvert.DeserializeObject<dynamic>(createBIResult);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateAOBIProduct", $"Exception during BI user creation process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." }, logData: jsObj, exception: null);
            }
            else
            {
                //	// Update User logic
                biAOUser.UserId = biProductUserName.ToLower();
                biAOUser.OldUserId = biProductUserName.ToLower();

                var updateResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", biAOUser);

                if (string.IsNullOrEmpty(updateResult))
                {
                    if (biAOUser.IsEnabled == true)
                    {
                        UpdateProductSettingProductStatus(productUserPersonaId, _productSettingType_ProductStatus, (int)ProductEnum.AoBusinessIntelligence, (int)ProductBatchStatusType.Success);
                    }
                    else
                    {
                        UpdateProductUserInGreenBook(editorPersonaId, productUserPersonaId, productUserGbLogin.LoginName.ToLower(), existingAoProducts, unAssignedProducts);
                        updateResult = "unAssignedProducts";
                        //UpdateProductSettingProductStatus(productUserPersonaId, _productSettingType_ProductStatus, (int)ProductEnum.AoBusinessIntelligence, (int)ProductBatchStatusType.Deleted);
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateAOBIProduct", $"Completed BI user update process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." });
                    return updateResult;
                }
                var jsObj = JsonConvert.DeserializeObject<dynamic>(updateResult);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateAOBIProduct", $"Exception during BI user update process with editorPersona id - {editorPersonaId} and userPersonaId {productUserPersonaId}." }, logData: jsObj, exception: null);
            }
            return result;
        }
        /// <summary>
        /// Update User Profile
        /// </summary>
        public string UpdateUserProfile(long editorPersonaId, long userPersonaId)
        {
            string result = string.Empty;
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Begin Update User Profile for user with editorPersona id - {editorPersonaId}." });
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                bool loginNameChanged = false;
                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
                string userEmailAddress = GetUserEmailAddress(realPageId, userLogin.LoginName, userPersonaId);
                IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);
                persona.Organization = organizationList.FirstOrDefault(i => i.PartyId == persona.OrganizationPartyId);

                IList<Persona> personaList = _managePersona.ListActivePersona(persona.RealPageId, false);
                string productUserName = "";
                bool hasMultiCompany = personaList.Count(p => p.OrganizationPartyId != persona.OrganizationPartyId && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;
                bool isExternalUser = persona.Organization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && persona.Organization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", "Error.No Valid Notification Email Provided." });
                    // write an error
                    return "ManageProductAssetOptimization - Error.No Valid Notification Email Provided";
                }

                var aoUser = new AOUser
                {
                    IsInternalUser = false, // Initial release is w/o internal user
                    IsEnabled = true,
                    IsSuperUser = false,
                    Email = userEmailAddress.ToLower(),

                    Login = _productUsername.ToLower(),
                    OldUserId = _productUserId.ToLower(),
                    UserId = _productUserId.ToLower(),

                    FirstName = person.FirstName,
                    LastName = person.LastName,
                };
                if ((hasMultiCompany && !persona.Organization.PrimaryOrganization) || isExternalUser)
                {
                    long primaryCompanyPersonaId = personaList.Where(x => x.OrganizationPartyId != persona.OrganizationPartyId).Select(y => y.PersonaId).FirstOrDefault();
                    productUserName = GetSamlProductUserName(primaryCompanyPersonaId);
                }
                else
                {
                    productUserName = _productUsername;
                }
                //If the User's LoginName changed in the PrimaryOrganization then update it in the Product
                if (!_productUsername.Equals(userLogin.LoginName, StringComparison.OrdinalIgnoreCase))
                {
                    aoUser.Login = userEmailAddress.ToLower();
                    aoUser.UserId = _productUsername.ToLower();
                    loginNameChanged = true;
                }
                var updateResult = "";
                var copiedAoUserCompanyPropertyRoleDetails = CopyRegularUser(editorPersonaId, userPersonaId, productUserName);

                if (!isExternalUser)
                {
                    aoUser.GroupsModel = GetBundledGroups(copiedAoUserCompanyPropertyRoleDetails);
                    aoUser.Divisions = new List<Divisions>();
                    aoUser.Model = GetModel(copiedAoUserCompanyPropertyRoleDetails);

                    updateResult = PutApi($"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/", aoUser);
                }

                // get existing AP details

                if (string.IsNullOrEmpty(updateResult) && loginNameChanged)
                {
                    UpdateProductUserInGreenBook(editorPersonaId, userPersonaId, _productUsername.ToLower(), copiedAoUserCompanyPropertyRoleDetails, copiedAoUserCompanyPropertyRoleDetails, loginNameChanged);
                }

                return updateResult;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                return $"Error - {ex.Message}";
            }
        }

        /// <summary>
        /// CopyRegularUser
        /// </summary>
        public IList<AoUserCompanyPropertyRoleDetail> CopyRegularUser(long editorUserPersonaId, long subjectUserPersonaId, string productUserName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CopyRegularUser", $"Begin - editorPersona id - {editorUserPersonaId}. sourceUserPersonaId {subjectUserPersonaId}" });

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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CopyRegularUser", $"Error -unable to find product User name with editorUserPersonaId   - {editorUserPersonaId}, subjectUserPersonaId {subjectUserPersonaId}." });

                throw new Exception($"Error - unable to find product User name with editorUserPersonaId   - {editorUserPersonaId}, subjectUserPersonaId {subjectUserPersonaId}");
            }

            string productUserProfileApiUrl = $"{_apiEndPoint}user/active-authorities/{samlEditorProductUserName}/{samlSubjectProductUserName}/";
            var aoActiveAuthorities = GetResultFromApi<IList<AoActiveAuthorities>>(productUserProfileApiUrl);

            IList<string> aoProductsAvailableToAssign = GetGbSupportedAoSubjectProductsAssigned(aoActiveAuthorities);

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

                    var propertyApiUrl = $"{_apiEndPoint}user/products/{samlSubjectProductUserName.ToLower()}/{companyId}";
                    bool isAllProperties = GetAllPropertiesStatusForExistingProductUser(propertyApiUrl, aoProduct);
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
                        IsAssigned = true,
                        allProperties = isAllProperties
                    });
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CopyRegularUser", $"End - editorPersona id - {editorUserPersonaId}. sourceUserPersonaId {subjectUserPersonaId}" });

            return aoUserCompanyPropertyRoleDetails;
        }

        /// <summary>
        /// Get Properties assigned to Group
        /// </summary> 
        public ListResponse GetPropertiesInGroup(long editorPersonaId, long userPersonaId, int propertyGroupId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesInGroup", $"Begin with editorPersona id - {editorPersonaId}." });

            var response = new ListResponse();

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesInGroup", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesInGroup", $"Error for user with editor AO user Id - {_editorProductUserId} " }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get Properties assigned to Group
        /// </summary> 
        public ListResponse GetGroupProperties(long editorPersonaId, long userPersonaId, int propertyGroupId, int productId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGroupProperties", $"Begin with editorPersona id - {editorPersonaId}." });

            var response = new ListResponse();

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetGroupProperties", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
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

                var props = GetPropertyByGroupId(propertyGroupId, productId);
                props = props.OrderBy(x => x.Name).ToList();

                if (props != null && props.Count > 0)
                {
                    response = new ListResponse()
                    {
                        Records = props.Cast<object>().ToList(),
                        TotalRows = props.Count,
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetGroupProperties", $"Error for user with editor AO user Id - {_editorProductUserId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get Property Groups
        /// </summary>
        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, string productName, IList<string> selectedCompanies, string userLoginName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                IList<AoPropertyGroups> propertyGroups = new List<AoPropertyGroups>();

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
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
                            propertyGroups.Add(new AoPropertyGroups { GroupId = grp.GroupId, GroupName = grp.GroupName });
                        }

                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Received {groups.Count} groups for existing user." });
                    }
                }

                string productUserId = _productUserId;
                if (userPersonaId == 0 && string.IsNullOrEmpty(_productUserId) && !string.IsNullOrWhiteSpace(userLoginName))
                {
                    productUserId = userLoginName;
                }
                if (!string.IsNullOrEmpty(productUserId)) // Called during updating Existing User
                {
                    // existing user
                    var assgnPropertGroups = GetAssignablePropertyGroups(productName, selectedCompanies);

                    var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/{productUserId.ToLower()}/";
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
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"No groups received from product for user with editorPersona id - {editorPersonaId}." });

                    response.IsError = true;
                    response.ErrorReason = "No groups received from product.";
                    return response;
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Received {propertyGroups.Count} groups for existing user with editorPersona id - {editorPersonaId} & userPersonaId {userPersonaId}" });

                propertyGroups = propertyGroups.OrderBy(x => x.GroupName).ToList();

                response = new ListResponse()
                {
                    Records = propertyGroups.Cast<object>().ToList(),
                    TotalRows = propertyGroups.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}." });
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                }

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get Product Property Groups
        /// </summary>
        public ListResponse GetProductPropertyGroups(long editorPersonaId, long userPersonaId, string productName, string userLoginName = "")
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                IList<AoPropertyGroups> propertyGroups = new List<AoPropertyGroups>();
                IList<AoPropertyGroup> aoPropertyGroups = new List<AoPropertyGroup>();
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string aoCompanyId = company.CompanyInstanceSourceId;
                if (string.IsNullOrEmpty(aoCompanyId))
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", "Error looking for company id in bluebook." });
                    return result;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Found blue book company source id {aoCompanyId}" });

                IList<string> selectedCompanies = new List<string>();
                selectedCompanies.Add(aoCompanyId);

                if (productName == "MA" || productName == "AX")
                {
                    // return all groups
                    var groups = GetAllPropertyGroups().Groups;

                    foreach (var grp in groups)
                    {
                        propertyGroups.Add(new AoPropertyGroups { GroupId = grp.GroupId, GroupName = grp.GroupName });
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Received {groups.Count} groups for existing user." });
                }

                string productUserId = _productUserId;
                if (userPersonaId == 0 && string.IsNullOrEmpty(_productUserId) && !string.IsNullOrWhiteSpace(userLoginName))
                {
                    productUserId = userLoginName;
                }

                // Cache applied: single retrieval of assignable groups
                var assgnPropertGroups = GetAssignablePropertyGroups(productName, selectedCompanies);
                if (!string.IsNullOrEmpty(productUserId)) // Called during updating Existing User
                {
                    var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{_editorProductUserId.ToLower()}/{productUserId.ToLower()}/";
                    var userProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);
                    propertyGroups = GetPropertyGroupsForExistingUser(assgnPropertGroups, userProfile, productName);
                }
                else
                {
                    propertyGroups = GetPropertyGroupsForNewUser(assgnPropertGroups);
                }

                if (propertyGroups == null || propertyGroups.Count == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"No groups received from product for user with editorPersona id - {editorPersonaId}." });

                    response.IsError = true;
                    response.ErrorReason = "No groups received from product.";
                    return response;
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Received {propertyGroups.Count} groups for existing user with editorPersona id - {editorPersonaId} & userPersonaId {userPersonaId}" });

                propertyGroups = propertyGroups.OrderBy(x => x.GroupName).ToList();

                foreach (var grp in propertyGroups)
                {
                    aoPropertyGroups.Add(new AoPropertyGroup
                    {
                        ID = grp.GroupId.ToString(),
                        Name = grp.GroupName,
                        IsAssigned = grp.IsAssigned
                    });
                }

                response = new ListResponse()
                {
                    Records = aoPropertyGroups.Cast<object>().ToList(),
                    TotalRows = aoPropertyGroups.Count,
                    RowsPerPage = aoPropertyGroups.Count,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}." });
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                }

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Gets products available for assigning
        /// </summary> 
        public IList<string> GetGbSupportedAoEditorUserProductsToAssign(long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGbSupportedAoEditorUserProductsToAssign", $"ManageProductAssetOptimization.GetGbSupportedAoUserProductsToAssign - Begin with editorPersona id - {userPersonaId}." });

            var products = new List<string>();
            var aoUserProducts = new List<string>();

            var samlProductUserName = GetSamlProductUserName(userPersonaId).ToLower();
            string productUserProfileApiUrl = "";
            if (!string.IsNullOrEmpty(samlProductUserName))
            {
                productUserProfileApiUrl = $"{_apiEndPoint}user/divisions/{samlProductUserName}/";
                var aoDivisionProduct = GetResultFromApi<IList<AoDivisionProduct>>(productUserProfileApiUrl);

                if (aoDivisionProduct != null && aoDivisionProduct.Count > 0)
                {
                    aoUserProducts = aoDivisionProduct.SelectMany(c => c.Products).Select(s => s.Product).ToList();
                }
            }

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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGbSupportedAoEditorUserProductsToAssign", $"End of method - product count {products.Count}" });

            return products;
        }

        /// <summary>
        /// Gets ao products with User admin role 
        /// </summary> 
        public IList<string> GetGbSupportedAoProductsWithUserAdminRole(long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGbSupportedAoProductsWithUserAdminRole", $"Begin with editorPersona id - {userPersonaId}." });

            var products = new List<string>();
            var aoUserProducts = new List<string>();
            ListResponse result = new ListResponse();
            var samlProductUserName = GetSamlProductUserName(userPersonaId).ToLower();
            string productUserProfileApiUrl = "";
            if (!string.IsNullOrEmpty(samlProductUserName))
            {
                productUserProfileApiUrl = $"{_apiEndPoint}user/divisions/unity/{samlProductUserName}/";
                if (_editorPersona == null)
                {
                    result = GetCompanyEditorAndUserDetails(userPersonaId, 0);
                }
                var blueAOCompanyInfo = GetProductCompanyInstanceId(_udmSourceCode);
                var aoDivisionProduct = GetResultFromApi<IList<AoDivisionProduct>>(productUserProfileApiUrl);

                if (aoDivisionProduct != null && aoDivisionProduct.Count > 0)
                {
                    aoUserProducts = aoDivisionProduct.SelectMany(c => c.Products).Where(p => p.CompanyId.Equals(blueAOCompanyInfo.CompanyInstanceSourceId)).Select(s => s.Product).ToList();
                }
            }

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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGbSupportedAoProductsWithUserAdminRole", $"End of method for user with editorPersona id - {userPersonaId} samlProductUserName - {samlProductUserName}. productUserProfileApiUrl {productUserProfileApiUrl}, product count {products.Count}" });

            return products;
        }
        /// <summary>
        /// Gets products available for assigning
        /// </summary> 
        public List<string> GetAOProductsForNewMultiCompanyUser(long editorUserPersonaId, string loginName)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAOProductsForNewMultiCompanyUser", $"Begin with name - {loginName}." });

            var products = new List<string>();
            ListResponse result = new ListResponse();
            string productUserProductApiUrl = "";
            try
            {
                if (_editorPersona == null)
                {
                    result = GetCompanyEditorAndUserDetails(editorUserPersonaId, 0);
                }
                var blueAOCompanyInfo = GetProductCompanyInstanceId(_udmSourceCode);

                productUserProductApiUrl = $"{_apiEndPoint}user/ao-token?userId={loginName}";
                var objProductData = GetResultFromApi<AoUserConfigAuthorities>(productUserProductApiUrl);

                if (objProductData != null)
                {
                    var aoUserProducts = objProductData.ysconfigAuthorities.Where(c => c.company.Equals(blueAOCompanyInfo.CompanyInstanceSourceId)).ToList();

                    if (aoUserProducts.Count > 0)
                    {
                        products = aoUserProducts.Select(a => a.product).Distinct().ToList();
                    }
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAOProductsForNewMultiCompanyUser", $"End of method for user with ProductUserName - {loginName}. productUserProfileApiUrl {productUserProductApiUrl}, products {products.ToString()}" });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetAOProductsForNewMultiCompanyUser", $"Error for user {loginName} while getting AO Data from API {productUserProductApiUrl}" }, exception: ex);
            }

            return products;
        }

        /// <summary>
        /// Check BI  product available for user in other companiies
        /// </summary> 
        private bool IsAOBIProductExistsInOtherOrganization(long editorUserPersonaId, string loginName)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsAOBIProductExistsInOtherOrganization", $"Begin with name - {loginName}." });

            var products = new List<string>();
            ListResponse result = new ListResponse();
            string productUserProductApiUrl = "";
            try
            {
                if (_editorPersona == null)
                {
                    result = GetCompanyEditorAndUserDetails(editorUserPersonaId, 0);
                }
                var blueAOCompanyInfo = GetProductCompanyInstanceId(_udmSourceCode);

                productUserProductApiUrl = $"{_apiEndPoint}user/ao-token?userId={loginName}";
                var objProductData = GetResultFromApi<AoUserConfigAuthorities>(productUserProductApiUrl);

                if (objProductData == null)
                {
                    return false;
                }

                var aoUserProducts = objProductData.ysconfigAuthorities.Where(c => c.company != blueAOCompanyInfo.CompanyInstanceSourceId).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsAOBIProductExistsInOtherOrganization", $"End of method for user with ProductUserName - {loginName}. productUserProfileApiUrl {productUserProductApiUrl}, products {aoUserProducts.ToString()}" });

                if (aoUserProducts.Count > 0)
                {
                    return aoUserProducts.Where(a => a.product == "BI").Distinct().Count() > 0 ? true : false;
                }

            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "IsAOBIProductExistsInOtherOrganization", $"Error for user {loginName} while getting AO Data from API {productUserProductApiUrl}" }, exception: ex);
            }

            return false;
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
        private string GetSamlProductUserName(long userPersonaId, string productName = "")
        {
            string userName = string.Empty;
            IList<SamlAttributes> productAttributes = new List<SamlAttributes>();
            if (userPersonaId != 0)
            {
                if (string.IsNullOrEmpty(productName))
                {
                    productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, (int)ProductEnum.AssetOptimizer);
                }
                else
                {
                    productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, (int)ProductEnumHelper.GetAoProductEnum(productName));
                }

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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetEditorUserAssignedCompaniesForProduct", $"Beginning of method for user with editorPersona id - {personaId}" });

            var samlProductUserName = GetSamlProductUserName(personaId).ToLower();

            if (string.IsNullOrEmpty(samlProductUserName))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetEditorUserAssignedCompaniesForProduct", $"Error -unable to find product User name with persona id - {personaId}." });
                return null;
            }

            var productDivisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));

            var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{samlProductUserName}/";
            var productUserProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

            var productUserComp = productUserProfile.Divisions.Where(x => x.Division == productDivisionName).ToList();
            var allCompanies = productUserComp.SelectMany(f => f.Companies).ToList();

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetEditorUserAssignedCompaniesForProduct", $"End of method for user with editorPersona id - {personaId} editorSamlProductUserName - {samlProductUserName} productName {productName}" });

            return allCompanies;
        }

        private IList<Groups> GetEditorUserAssignedPropertyGroups(long editorPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetEditorUserAssignedPropertyGroups", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var editorSamlProductUserName = GetSamlProductUserName(editorPersonaId).ToLower();

            if (string.IsNullOrEmpty(editorSamlProductUserName))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetEditorUserAssignedPropertyGroups", $"Error -unable to find product User name with persona id - {editorPersonaId}." });
                return null;
            }

            var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{editorSamlProductUserName}/{editorSamlProductUserName}/";
            var userProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

            return userProfile.Divisions.Where(r => r.Groups != null).SelectMany(x => x.Groups).ToList();
        }

        #endregion

        private IList<Groups> GetSubjectUserAssignedPropertyGroups(string editorProductUserId, string productUserId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetSubjectUserAssignedPropertyGroups", $"Beginning of method for user with editorProductUserId - {editorProductUserId} productUserId {productUserId}" });

            var productUserProfileApiUrl = $"{_apiEndPoint}user/profile/{editorProductUserId.ToLower()}/{productUserId.ToLower()}/";
            var userProfile = GetResultFromApi<AOUser>(productUserProfileApiUrl);

            if (userProfile == null)
            {
                return new List<Groups>();
            }

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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesInGroups", "Beginning of method." });

            var response = new List<AoProperty>();

            AoVisiblePropertyGroups visiblePropertyGroups = GetAllPropertyGroups();

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertiesInGroups", $"Received {visiblePropertyGroups.Groups.Count} groups for existing user." });

            foreach (var x in visiblePropertyGroups.Groups.Where(z => z.Properties != null && z.GroupId == propertyGroupId).SelectMany(s => s.Properties))
            {
                response.Add(new AoProperty { PropertyId = x.PropertyId, PropertyName = x.PropertyName });
            }

            return response;
        }

        private IList<ProductProperty> GetGroupProperties(int propertyGroupId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGroupProperties", "Beginning of method." });

            var response = new List<ProductProperty>();

            AoVisiblePropertyGroups visiblePropertyGroups = GetAllPropertyGroups();

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGroupProperties", $"Received {visiblePropertyGroups.Groups.Count} groups for existing user." });

            foreach (var x in visiblePropertyGroups.Groups.Where(z => z.Properties != null && z.GroupId == propertyGroupId).SelectMany(s => s.Properties))
            {
                response.Add(new ProductProperty { ID = x.PropertyId.ToString(), Name = x.PropertyName, State = "" });
            }

            return response;
        }

        private List<ProductProperty> GetPropertyByGroupId(int groupId, int productId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyByGroupId", "Beginning of method." });
            List<VisibleGroupProperty> propertyGroups = new List<VisibleGroupProperty>();
            var response = new List<ProductProperty>();
            var productList = _productRepository.GetAllProducts();
            var productCode = ProductEnumHelper.GetProductCodeByProductId(productId, productList); ;

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyByGroupId", "ManageProductAssetOptimization.GetAllPropertyGroups- Null cache value. Getting new Groups." });
            //https://aoqa.realpage.com/ysconfig/ws/user/snarani/groups/assignable/properties?groupId=27673
            var groupApiUrl = $"{_apiEndPoint}user/{_editorProductUserId.ToLower()}/groups/assignable/properties?groupId={groupId}";
            propertyGroups = GetResultFromApi<List<VisibleGroupProperty>>(groupApiUrl);
            propertyGroups = propertyGroups.Where(pg => pg.Products.Contains(productCode)).ToList();
            foreach (var x in propertyGroups)
            {
                response.Add(new ProductProperty { ID = x.PropertyId.ToString(), Name = x.PropertyName, State = "" });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyByGroupId", $"Received {propertyGroups.Count} groups for existing user." });
            return response;
        }

        private AoVisiblePropertyGroups GetAllPropertyGroups()
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllPropertyGroups", "Beginning of method." });

            ObjectCache groupCache = MemoryCache.Default;
            AoVisiblePropertyGroups propertyGroups = new AoVisiblePropertyGroups();
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"propertyGroups_AO_{_editorProductUserId.ToLower()}";

            propertyGroups = rpcache.GetFromCache<AoVisiblePropertyGroups>(cacheKey, CacheTimeSeconds, () =>
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllPropertyGroups", "Null cache value. Getting new Groups." });

                var groupApiUrl = $"{_apiEndPoint}user/groups/visible/{_aoSuperUser.ToLower()}/{_editorProductUserId.ToLower()}/";
                propertyGroups = GetResultFromApi<AoVisiblePropertyGroups>(groupApiUrl);

                return propertyGroups;
            });

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllPropertyGroups", $"Received {propertyGroups.Groups.Count} groups for existing user." });

            return propertyGroups;
        }

        private IList<AoAssignableDivisionGroups> GetAssignablePropertyGroups(string productName, IList<string> selectedCompanies)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAssignablePropertyGroups", "Beginning of method." });

            var companiesKeyPart = (selectedCompanies != null && selectedCompanies.Count > 0)
                ? string.Join("_", selectedCompanies.OrderBy(x => x))
                : "NONE";

            var cacheKey = $"AO_AssignableGroups_{_editorProductUserId.ToLower()}_{productName.ToUpper()}_{companiesKeyPart}";
            var rpcache = new RPObjectCache();

            return rpcache.GetFromCache<IList<AoAssignableDivisionGroups>>(cacheKey, CacheTimeSeconds, () =>
            {
                var groupApiUrl = $"{_apiEndPoint}user/{_editorProductUserId.ToLower()}/groups/assignable?editingUser={_editorProductUserId.ToLower()}";
                var result = GetResultFromApi<AoVisiblePropertyGroups>(groupApiUrl);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAssignablePropertyGroups", $"Received {result?.Groups?.Count} groups from API." });

                AoAssignableDivisionGroups response = new AoAssignableDivisionGroups { Groups = new List<AssignableGroup>() };
                var finalResponse = new List<AoAssignableDivisionGroups>();

                if (result?.Groups != null)
                {
                    foreach (var grp in result.Groups)
                    {
                        response.Groups.Add(new AssignableGroup
                        {
                            PropertyGroupId = grp.GroupId,
                            GroupName = grp.GroupName,
                            Products = new List<DivisionGroupProduct>
                            {
                                new DivisionGroupProduct { Product = productName, Valid = true, Assigned = false }
                            }
                        });
                    }
                    finalResponse.Add(response);
                }

                return finalResponse;
            });
        }

        private IList<AoProperties> GetPropertiesForNewUser(string productPropertyApiUrl, long companyId, string productName)
        {
            var aoProps = GetResultFromApi<IList<AoProperties>>(productPropertyApiUrl);
            return aoProps;
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

        private bool GetAllPropertiesStatusForExistingProductUser(string productPropertyApiUrl, string productName)
        {
            var aoUserProps = GetResultFromApi<IList<AoPropertyList>>(productPropertyApiUrl);
            bool isAllProperties = false;
            if (aoUserProps != null)
            {
                isAllProperties = aoUserProps.Any(x => x.allProperties && x.ProductName.Equals(productName));
            }

            return isAllProperties;
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
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", $"Error - Response is not 200. baseUrlAndQuery {baseUrlAndQuery}, StatusCode - {response.StatusCode}" });
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
                    
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", $"Error - Response is not 200. PostApi, baseUrlAndQuery {baseUrlAndQuery}, StatusCode - {response.StatusCode}, jsonContent {jsonContent}, errorResult {result}" });
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
                        
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", $"Error - Response is not 200. PutApi, baseUrlAndQuery {baseUrlAndQuery}, StatusCode - {response.StatusCode}, jsonContent {jsonContent}, result {result}" });
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

            // dump diagnostic info
            DumpApiCallInfoToDiagnosticLog($"ManageProductAssetOptimization.CheckUniqueAOUserName - API Url - {productUserProfileApiUrl}",
                validationResult);

            if (validationResult != null)
                return validationResult.exists;

            throw new Exception($"CheckUniqueAOUserName returned invalid results - URL {productUserProfileApiUrl}");
        }

        private void CreateProductUserInGreenBook(long editorPersonaId, long userPersonaId, IList<string> aoProductList, string productLoginName)
        {
            // Default AO record
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Inserting in GB -productUsername -{productLoginName} for AO user" });
            _samlRepository.CreateSamlUserAttribute(userPersonaId, (int)ProductEnum.AssetOptimizer, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, (int)ProductEnum.AssetOptimizer, SamlAttributeEnum.UserId, productLoginName);
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductEnum.AssetOptimizer, (int)ProductBatchStatusType.Success);

            // AoDivisionType
            foreach (var product in aoProductList)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Inserting in GB -productUsername -{productLoginName}, product - {product}." });
                _samlRepository.CreateSamlUserAttribute(userPersonaId, (int)ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.productUsername, productLoginName);
                _samlRepository.CreateSamlUserAttribute(userPersonaId, (int)ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.UserId, productLoginName);

                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductEnumHelper.GetAoProductEnum(product), (int)ProductBatchStatusType.Success);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Create user Success. Set product status to Success. productUsername -{productLoginName}, product - {product}" });
            }
        }

        private void UpdateProductUserInGreenBook(long editorPersonaId,
            long userPersonaId,
            string productLoginName,
            IList<AoUserCompanyPropertyRoleDetail> existingAssignedProducts,
            IList<AoUserCompanyPropertyRoleDetail> aoUserCompanyPropertyRoleDetails,
            bool loginNameChanged = false)
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

            var bmProduct = existingAssignedProducts.Where(p => p.ProductName.Equals("BM")).ToList();
            if (productUnAssigned.Contains("BM") && !bmProduct.Any())
            {
                productUnAssigned.Remove("BM");
            }

            //set delete status if all products are unassigned.
            if (existingAssignedProducts.Count == productUnAssigned.Count)
            {
                // remove all association from GB
                UpdateProductSettingProductStatus(userPersonaId,
                    _productSettingType_ProductStatus, (int)ProductEnum.AssetOptimizer, (int)ProductBatchStatusType.Deleted);

                foreach (var item in aoUserCompanyPropertyRoleDetails)
                {
                    if (!item.IsAssigned)
                    {
                        if (!IsSuperUser(userPersonaId))
                        {
                            DeleteSamlUserProductInfoAndStatus(userPersonaId, (int)ProductEnumHelper.GetAoProductEnum(item.ProductName));
                        }
                        UpdateProductSettingProductStatus(userPersonaId,
                            _productSettingType_ProductStatus, (int)ProductEnumHelper.GetAoProductEnum(item.ProductName), (int)ProductBatchStatusType.Deleted);
                    }

                    if (item.IsAssigned)
                    {
                        UpdateProductSettingProductStatus(userPersonaId,
                            _productSettingType_ProductStatus, (int)ProductEnumHelper.GetAoProductEnum(item.ProductName), (int)ProductBatchStatusType.Success);
                    }
                }
            }
            else
            {
                if (productAssigned.Any())
                {
                    // First check default AO record exists 
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"Checking AO record in GB -productUsername -{productLoginName} for AO user" });
                    var samlUserDetails = _samlRepository.GetProductSamlDetails(userPersonaId, (int)ProductEnum.AssetOptimizer);
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductEnum.AssetOptimizer, (int)ProductBatchStatusType.Success);

                    if (!samlUserDetails.Any())
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"No AO record found in GB for AO user -{productLoginName}. Creating new one." });
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, (int)ProductEnum.AssetOptimizer,
                            SamlAttributeEnum.productUsername, productLoginName);
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, (int)ProductEnum.AssetOptimizer,
                            SamlAttributeEnum.UserId, productLoginName);

                    }
                    else if (loginNameChanged)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"Checking AO record in GB - productUsername -{productLoginName} for AO user and exist product assigned loginName changed" });
                        UpdateSamlUserAttribute(userPersonaId, (int)ProductEnum.AssetOptimizer, SamlAttributeEnum.productUsername, productLoginName);
                        UpdateSamlUserAttribute(userPersonaId, (int)ProductEnum.AssetOptimizer, SamlAttributeEnum.UserId, productLoginName);
                    }

                    //if product is assigned
                    foreach (var product in productAssigned)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"Checking if product {product} exists in GB for AO user - {productLoginName}" });
                        samlUserDetails = _samlRepository.GetProductSamlDetails(userPersonaId,
                            (int)ProductEnumHelper.GetAoProductEnum(product));

                        if (!samlUserDetails.Any())
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"No {product} record found in GB for AO user -{productLoginName}. Creating new one." });

                            _samlRepository.CreateSamlUserAttribute(userPersonaId,
                                (int)ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.productUsername,
                                productLoginName);
                            _samlRepository.CreateSamlUserAttribute(userPersonaId,
                                (int)ProductEnumHelper.GetAoProductEnum(product), SamlAttributeEnum.UserId, productLoginName);

                        }
                        else if (loginNameChanged)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"Checking AO record in GB - productUsername -{productLoginName} for AO user and product assigned loginName changed" });
                            Dictionary<SamlAttributeEnum, string> settingList = new Dictionary<SamlAttributeEnum, string>();
                            settingList.Add(SamlAttributeEnum.productUsername, productLoginName);
                            settingList.Add(SamlAttributeEnum.UserId, productLoginName);

                            UpdateSamlUserAttributes(userPersonaId, settingList, (int)ProductEnumHelper.GetAoProductEnum(product));
                        }

                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductEnumHelper.GetAoProductEnum(product), (int)ProductBatchStatusType.Success);

                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"{product} record updated in GB for AO user -{productLoginName}. (UpdateProductSettingProductStatus)" });
                    }
                }

                if (productUnAssigned.Any())
                {
                    //if product is un-assigned then remove product from GB
                    foreach (var product in productUnAssigned)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"Checking if product {product} exists in GB for AO user - {productLoginName}" });

                        var samlUserDetails = _samlRepository.GetProductSamlDetails(userPersonaId, (int)ProductEnumHelper.GetAoProductEnum(product));

                        if (samlUserDetails.Any())
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"{product} record found in GB for AO user -{productLoginName}. Removing." });

                            if (!IsSuperUser(userPersonaId))
                            {
                                DeleteSamlUserProductInfoAndStatus(userPersonaId, (int)ProductEnumHelper.GetAoProductEnum(product));
                            }
                            UpdateProductSettingProductStatus(userPersonaId,
                                _productSettingType_ProductStatus, (int)ProductEnumHelper.GetAoProductEnum(product), (int)ProductBatchStatusType.Deleted);

                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserInGreenBook", $"{product} record removed from GB for AO user -{productLoginName}." });
                        }
                    }
                    var userAOProducts = GetAOProductsForNewMultiCompanyUser(editorPersonaId, productLoginName);
                    if (!userAOProducts.Any())
                    {
                        DeleteSamlUserProductInfoAndStatus(userPersonaId, (int)ProductEnum.AssetOptimizer);
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
                        SelectedRoleValues = aoUserCompanyPropertyRoleDetail.SelectedRoleValues ?? new List<string>(),
                        allProperties = aoUserCompanyPropertyRoleDetail.allProperties
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
                groupModelList.GroupBy(o => new { o.Division, o.GroupId, o.IsEnabled, o.ProductName })
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
                    response.Add(new AoPropertyGroups { GroupId = gp.PropertyGroupId, GroupName = gp.GroupName });
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
                        response.Add(new AoPropertyGroups { GroupId = gp.PropertyGroupId, GroupName = gp.GroupName });
                    }
                }
            }

            var groups = userProfile?.Divisions.Where(r => r.Groups != null).SelectMany(x => x.Groups);
            var productGropus = groups?.Where(x => x.Assignments.Any(f => f.Contains(productName))).ToList();

            if (productGropus != null)
            {
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
            if (activeAuthorities != null && activeAuthorities.Count > 0)
            {
                var assignedAuthNames = new HashSet<string>(
                    activeAuthorities
                        .Where(x => x.Products != null)
                        .SelectMany(s => s.Products)
                        .Where(z => z.Product == productName && z.CompanyId == companyId)
                        .Select(a => a.AuthortyName?.ToLowerInvariant()),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var role in allRoles)
                {
                    if (assignedAuthNames.Contains(role.Name?.ToLowerInvariant()))
                        role.IsAssigned = true;
                }
            }

            return allRoles;
        }

        private void AssociateAoUserWithGb(long editorPersonaId, long productUserPersonaId, string loginName, AOUser aoUser, CustomerCompanyMap customerCompanyMap)
        {
            //IList<string> products = GetAoProductsForUserNew(loginName);
            IList<string> products = new List<string>();
            var existingUserInfo = CopyRegularUser(editorPersonaId, productUserPersonaId, loginName);
            var groups = aoUser.Divisions.Where(r => r.Groups != null).SelectMany(x => x.Groups);
            //var productGroups = groups.Where(x => x.Assignments.Any(p => !string.IsNullOrEmpty(p))).ToList();
            int aoCompanyId = Convert.ToInt32(customerCompanyMap.CompanyInstanceSourceId);

            foreach (Groups group in groups)
            {
                foreach (string assignment in group.Assignments)
                {
                    if (ProductEnumHelper.CheckAoProductSupportedByGreenBook(assignment) && !products.Contains(assignment))
                    {
                        products.Add(assignment);
                    }
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "AssociateAoUserWithGb", $"EditorPersona id - {editorPersonaId} productUserPersonaId {productUserPersonaId}, Products - {string.Join<string>(",", products)}" });

            CreateProductUserInGreenBook(editorPersonaId, productUserPersonaId, products, loginName);
        }

        private IList<string> GetAoProductsForUserNew(string loginName)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAoProductsForUserNew", $"LoginName {loginName}" });

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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAoProductsForUserNew", $"Received products {products.Count} for user with loginName {loginName} API-URL {productUserProfileApiUrl}" });

            return products;
        }

        private IList<string> GetAoProductsForUser(string loginName)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAoProductsForUser", $"LoginName {loginName}" });

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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAoProductsForUser", $"Received products {products.Count} for user with loginName {loginName} API-URL {productUserProfileApiUrl}" });

            return products;
        }

        private IList<AORoles> GetRoles(int companyId, string productName, string userLoginName = "", long userPersonaId = 0)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning of method for user with _editorProductUserId{_editorProductUserId} _productUserId {_productUserId} companyId - {companyId} productName {productName}" });

            string roleApiUrl;
            string productUserId = _productUserId;
            IList<AORoles> allRoles = new List<AORoles>();
            IList<AORoles> rolesResult = new List<AORoles>();

            if (!string.IsNullOrWhiteSpace(userLoginName) && string.IsNullOrWhiteSpace(_productUserId))
            {
                productUserId = userLoginName;
            }


            if (productName == "BI" && userPersonaId > 0)
            {
                productUserId = GetSamlProductUserName(userPersonaId, "BI");
            }

            // Decide cache key based on whether we are dealing with existing or new user
            string cacheKey;
            if (!string.IsNullOrEmpty(productUserId))
            {
                // Existing user roles
                RPObjectCache rpcache = new RPObjectCache();
                cacheKey = $"AO_Exsisting_Roles_{_editorProductUserId.ToLower()}_{companyId}_{productName.ToUpper()}";
                allRoles = rpcache.GetFromCache<IList<AORoles>>(cacheKey, 10800, () =>
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Null cache value. Getting new Roles." });
                    roleApiUrl = $"{_apiEndPoint}user/roles/available/{_editorProductUserId.ToLower()}/{_editorProductUserId.ToLower()}/{companyId}/{productName}";
                    allRoles = GetResultFromApi<IList<AORoles>>(roleApiUrl);
                    return allRoles;
                });

            }
            else
            {
                // New user roles
                RPObjectCache rpcache = new RPObjectCache();
                cacheKey = $"AO_NEW_ROLES_{_editorProductUserId.ToLower()}_{companyId}_{productName.ToUpper()}";
                allRoles = rpcache.GetFromCache<IList<AORoles>>(cacheKey, 10800, () =>
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Null cache value (new user). Getting new Roles." });
                    roleApiUrl = $"{_apiEndPoint}user/roles/available/{_editorProductUserId.ToLower()}/{companyId}/{productName}";
                    allRoles = GetResultFromApi<IList<AORoles>>(roleApiUrl) ?? new List<AORoles>();
                    return allRoles;
                });
                return allRoles;
            }

            // For existing user determine assignments without mutating cached list
            if (!string.IsNullOrEmpty(productUserId))
            {
                var rolesSnapshot = allRoles
                    .Select(r => new AORoles
                    {
                        Name = r.Name,
                        DisplayName = r.DisplayName,
                        IsCustom = r.IsCustom,
                        IsAssigned = r.IsAssigned
                    })
                    .ToList();

                var authorityApiUrl = $"{_apiEndPoint}user/active-authorities/{_editorProductUserId.ToLower()}/{productUserId.ToLower()}/";
                var activeAuthorities = GetResultFromApi<IList<AoActiveAuthorities>>(authorityApiUrl);
                rolesResult = CheckAuthorities(rolesSnapshot, activeAuthorities, productName, companyId);
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Received {allRoles.Count} roles for user context _editorProductUserId={_editorProductUserId} _productUserId={_productUserId} companyId={companyId} productName={productName}" });

            return rolesResult;
        }

        private AoPropertyList GetProperties(long companyId, string productName, string userLoginName = "", long userPersonaId = 0)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Begin. editor={_editorProductUserId} subject={_productUserId} companyId={companyId} product={productName}" });

            var divisionName = ProductEnumHelper.GetAoDivisionName(ProductEnumHelper.GetAoProductEnum(productName));
            var baseApiUrl = $"{_apiEndPoint}company/propertiesByDivision/{companyId}/{divisionName}?editor={_editorProductUserId}";

            var cacheKey = $"AO_Properties_{_editorProductUserId.ToLower()}_{companyId}_{productName.ToUpper()}";

            // Use RPObjectCache instead of Redis
            var rpcache = new RPObjectCache();
            // Cache ONLY the raw property list (unassigned) so user-specific assignments do not pollute cache
            var cached = rpcache.GetFromCache<AoPropertyList>(cacheKey, 7200, () =>
            {
                var apiResult = GetPropertiesForNewUser(baseApiUrl, companyId, productName);
                var rawProps = apiResult?.FirstOrDefault()?.Properties ?? new List<AoProperty>();

                var mapped = rawProps.Select(p => new AoProperty
                {
                    CompanyId = p.CompanyId,
                    PropertyId = p.PropertyId,
                    PropertyName = p.PropertyName,
                    Relationship = p.Relationship,
                    Products = p.Products?.Select(prod => new AoProduct
                    {
                        Product = prod.Product,
                        IsEnabled = prod.IsEnabled,
                        IsAssigned = prod.IsAssigned,
                        GbProductId = prod.GbProductId,
                        CompanyId = prod.CompanyId
                    }).ToList(),
                    State = p.State,
                    PropertyProducts = p.PropertyProducts != null ? new List<string>(p.PropertyProducts) : new List<string>(),
                    IsAssigned = false // never cache user-specific assignment
                }).Where(a => a.PropertyProducts != null && a.PropertyProducts.Contains(productName)).ToList();

                return new AoPropertyList
                {
                    Properties = mapped,
                    Division = divisionName,
                    ProductName = productName,
                    allProperties = false
                };
            });

            // Work on a clone so we do not mutate the cached object with user assignments
            AoPropertyList objAoPropertyList = new AoPropertyList
            {
                Properties = cached.Properties?.Select(p => new AoProperty
                {
                    CompanyId = p.CompanyId,
                    PropertyId = p.PropertyId,
                    PropertyName = p.PropertyName,
                    Relationship = p.Relationship,
                    Products = p.Products?.Select(prod => new AoProduct
                    {
                        Product = prod.Product,
                        IsEnabled = prod.IsEnabled,
                        IsAssigned = prod.IsAssigned,
                        GbProductId = prod.GbProductId,
                        CompanyId = prod.CompanyId
                    }).ToList(),
                    State = p.State,
                    PropertyProducts = p.PropertyProducts != null ? new List<string>(p.PropertyProducts) : new List<string>(),
                    IsAssigned = false // reset; will be marked below if needed
                }).ToList() ?? new List<AoProperty>(),
                Division = cached.Division,
                ProductName = cached.ProductName,
                allProperties = false
            };

            // Determine subject (existing user scenario)
            var productUserId = _productUserId;
            if (string.IsNullOrWhiteSpace(productUserId) && !string.IsNullOrWhiteSpace(userLoginName))
                productUserId = userLoginName;

            if (productName == "BI" && userPersonaId > 0)
                productUserId = GetSamlProductUserName(userPersonaId, "BI");

            if (!string.IsNullOrWhiteSpace(productUserId))
            {
                // allProperties flag for this user
                var allPropsUrl = $"{_apiEndPoint}user/products/{productUserId.ToLower()}/{companyId}";
                objAoPropertyList.allProperties = GetAllPropertiesStatusForExistingProductUser(allPropsUrl, productName);

                // mark assigned properties for this user
                var activePortfolioUrl = $"{_apiEndPoint}user/active-portfolio/{_editorProductUserId.ToLower()}/{productUserId.ToLower()}/";
                objAoPropertyList.Properties = GetPropertiesForExistingProductUser(objAoPropertyList.Properties, activePortfolioUrl, productName);
            }

            objAoPropertyList.Properties = objAoPropertyList.Properties.OrderBy(p => p.PropertyName).ToList();

            WriteToDiagnosticLog("{ActionName} - {state}",
                messageProperties: new object[] { "GetProperties", $"End. count={objAoPropertyList.Properties.Count} allProperties={objAoPropertyList.allProperties} productUserId={productUserId ?? "<new>"} product={productName}" });

            return objAoPropertyList;
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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGbSupportedAoSubjectProductsAssigned", "Begin." });

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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGbSupportedAoSubjectProductsAssigned", $"End of method - product count {products.Count}" });

            return products;
        }

        private void UpdateProductRolePropertyDetails(IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails, IList<AoUserCompanyPropertyRoleDetail> copiedAoUserCompanyPropertyRoleDetails, Persona persona)
        {
            if (aoGbUserCompanyPropertyRoleDetails == null)
                return;

            // Ge unassigned products
            var unAssignedProducts = aoGbUserCompanyPropertyRoleDetails.Where(x => x.IsAssigned == false);
            var modifiedProducts = aoGbUserCompanyPropertyRoleDetails.Where(x => x.IsAssigned);

            IList<Persona> personaList = _managePersona.ListActivePersona(persona.RealPageId, false);
            bool hasMultiCompany = personaList.Any(p => p.OrganizationPartyId != persona.OrganizationPartyId && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId);

            // remove products
            foreach (var unAssignedProduct in unAssignedProducts)
            {
                var matches = copiedAoUserCompanyPropertyRoleDetails.Where(p => p.ProductName == unAssignedProduct.ProductName).ToList();
                if (matches.Any())
                {
                    //Remove Roles for the product which un assigned
                    foreach (var match in matches)
                    {
                        //Check to see if user has multicompany persona, then remove roles otherwise remove product completely
                        if (hasMultiCompany && !persona.Organization.PrimaryOrganization)
                        {
                            match.SelectedRoleValues = new List<string>();
                            match.SelectedPortfolioValues = new List<int>();
                        }
                        else
                        {
                            copiedAoUserCompanyPropertyRoleDetails.Remove(match);
                        }
                    }
                }
            }

            // replace products
            foreach (var modifiedProduct in modifiedProducts)
            {
                var matches = copiedAoUserCompanyPropertyRoleDetails.Where(p => p.ProductName == modifiedProduct.ProductName && p.CompanyId == modifiedProduct.CompanyId).ToList();
                //add logic to remove external user bi product
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

        private void UnAssignProductRolePropertyDetails(IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails, IList<AoUserCompanyPropertyRoleDetail> copiedAoUserCompanyPropertyRoleDetails, Persona persona)
        {
            if (aoGbUserCompanyPropertyRoleDetails == null)
                return;

            // Ge unassigned products
            var unAssignedProducts = aoGbUserCompanyPropertyRoleDetails.Where(x => !x.IsAssigned);

            IList<Persona> personaList = _managePersona.ListActivePersona(persona.RealPageId, false);
            bool hasMultiCompany = personaList.Any(p => p.OrganizationPartyId != persona.OrganizationPartyId && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId);

            // remove roles			
            if (!hasMultiCompany && persona.Organization.PrimaryOrganization)
            {
                foreach (var unAssignedProduct in unAssignedProducts)
                {
                    var matches = copiedAoUserCompanyPropertyRoleDetails.Where(p => p.ProductName == unAssignedProduct.ProductName).ToList();
                    if (matches.Any())
                    {
                        //Remove Roles for the product which un assigned
                        foreach (var match in matches)
                        {
                            match.SelectedRoleValues = new List<string>();
                        }
                    }
                }
            }

        }

        private IList<AoUserCompanyPropertyRoleDetail> CopyEditorUserToCreateSuperUser(long sourceUserPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CopyEditorUserToCreateSuperUser", $"Begin - sourceUserPersonaId id - {sourceUserPersonaId}." });

            var aoUserCompanyPropertyRoleDetails = new List<AoUserCompanyPropertyRoleDetail>();

            // Get products assigned to user
            IList<string> aoProductsAvailableToAssign = GetGbSupportedAoEditorUserProductsToAssign(sourceUserPersonaId).ToList();

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
                    var propertyList = (from i in propertiesResponse.Properties select i.PropertyId).ToList();

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
                        IsAssigned = true,
                        allProperties = true
                    });
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CopyEditorUserToCreateSuperUser", $"End - sourceUserPersonaId id - {sourceUserPersonaId}." });

            return aoUserCompanyPropertyRoleDetails;
        }

        private string GetUserEmailAddress(Guid realPageId, string logInName, long productUserPersonaId)
        {
            // get the email address
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserEmailAddress", "Begin get user email address" });
            string userEmailAddress = "";
            ManageElectronicAddress _manageElectronicAddress = new ManageElectronicAddress();
            IList<SharedObjects.IdentityConfig.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(realPageId, "");
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserEmailAddress", "Got list of electronic address" });
            if (_addresses != null)
            {
                if (_addresses.Any(a => a.AddressType.ToUpper() == "EMAIL"))
                {
                    userEmailAddress = (from a in _addresses
                                        where a.AddressType.ToUpper() == "EMAIL"
                                        select a.AddressString).FirstOrDefault();
                }
            }

            if (IsRegularUserNoEmail(productUserPersonaId))
            {
                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserEmailAddress", "Error.No Valid Notification Email Provided." });
                    // write an error
                    return "ManageProductAssetOptimization - Error.No Valid Notification Email Provided";
                }

                //userEmailAddress = !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(logInName) ?
                //						string.Concat(logInName, "@NoReply.com") : logInName;
            }
            else if (string.IsNullOrEmpty(userEmailAddress))
            {
                userEmailAddress = logInName;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserEmailAddress", "Using login name for email address." });
            }

            // verify email address looks valid, will fail if not
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserEmailAddress", $"Validating email address. Email: {userEmailAddress}" });
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserEmailAddress", $"Validated email address. Email: {userEmailAddress}" });

            return userEmailAddress;
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

    public class AoOperatorProperties
    {
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("companyName")] public string CompanyName { get; set; }

        [JsonProperty("UDMCompanyId")] public string UDMCompanyId { get; set; }

        [JsonProperty("tags")] public IList<tags> tags { get; set; }

    }

    public class tags
    {
        [JsonProperty("propertyAttributeCode")] public string PropertyAttributeCode { get; set; }
        [JsonProperty("propertyAttributeValue")] public string PropertyAttributeValue { get; set; }
        [JsonProperty("properties")] public IList<AoProperty> Properties { get; set; }
        [JsonProperty("tags")] public IList<tags> tag { get; set; }

    }

    public class tag
    {
        [JsonProperty("operatorCode")] public string operatorCode { get; set; }
        [JsonProperty("operatorValue")] public string operatorValue { get; set; }

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
    public class AoPropertyList
    {
        public bool allProperties { get; set; } = false;
        public IList<AoProperty> Properties { get; set; }
        [JsonProperty("division")] public string Division { get; set; }
        [JsonProperty("product")] public string ProductName { get; set; }
    }


    public class AoProperty
    {
        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("propertyId")] public int PropertyId { get; set; }

        [JsonProperty("propertyName")] public string PropertyName { get; set; }

        [JsonProperty("relationship")] public string Relationship { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<AoProduct> Products { get; set; }

        [JsonProperty("statecode", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("propertyproducts", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyProducts { get; set; }

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

        [JsonProperty(PropertyName = "companyid")]
        public string CompanyId { get; set; }

    }

    public class Model
    {
        [JsonProperty("selectedRoleValues")] public IList<string> SelectedRoleValues { get; set; }

        [JsonProperty("selectedPortfolioValues")]
        public IList<int> SelectedPortfolioValues { get; set; }

        [JsonProperty("divisionName")] public string DivisionName { get; set; }

        [JsonProperty("companyId")] public int CompanyId { get; set; }

        [JsonProperty("product")] public string Product { get; set; }
        [JsonProperty("allProperties")] public bool allProperties { get; set; } = false;
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

        public int ProductId { get; set; }

        public bool UsePrimaryProperties { get; set; }

        /// <summary>
        /// List of Properties to assign to a product with propertyinstanceIds
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ProductPrimaryProperties> ProductPrimaryProperties { get; set; }
        [JsonProperty("allProperties")]
        public bool allProperties { get; set; } = false;
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

    public class AoPropertyGroup
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsAssigned { get; set; }
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

        [JsonProperty("products")] public IList<string> Products { get; set; }
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
    #region User Products For All companies
    public class AoUserConfigAuthorities
    {
        //[JsonProperty("username")] public string Username { get; set; }
        //[JsonProperty("ysconfigAuthorities")] public IList<AoUserCompanyProduct> UserconfigAuthorities { get; set; }
        public bool @internal { get; set; }
        public bool authenticated { get; set; }
        public int failedLoginAttempts { get; set; }
        public string username { get; set; }
        public bool accountNonExpired { get; set; }
        public bool accountNonLocked { get; set; }
        public bool credentialsNonExpired { get; set; }
        public bool enabled { get; set; }
        public bool superUser { get; set; }
        public bool impersonated { get; set; }
        public object ipAddress { get; set; }
        public object serverAuthUrl { get; set; }
        public string requestTime { get; set; }
        public List<string> simpleGrantedAuthorities { get; set; }
        public List<YsconfigAuthority> ysconfigAuthorities { get; set; }
        public List<object> ysconfigRedactedAuthorities { get; set; }
        public string userFullName { get; set; }
        public string imposterUserName { get; set; }
    }

    public class YsconfigAuthority
    {
        public string product { get; set; }
        public string permission { get; set; }
        public string company { get; set; }
    }
    public class AoUserCompanyProduct
    {
        //, NullValueHandling = NullValueHandling.Ignore
        [JsonProperty("company")] public string CompanyId { get; set; }

        [JsonProperty("permission")] public string Permission { get; set; }

        [JsonProperty("product")] public string Product { get; set; }
    }
    #endregion
    #endregion
}
