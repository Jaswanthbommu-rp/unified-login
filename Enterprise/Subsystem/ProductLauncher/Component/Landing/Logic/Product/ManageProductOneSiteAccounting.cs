using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using blueBook = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// 
    /// </summary>
    public class ManageProductOneSiteAccounting : ManageProductBase, IManageProductOneSiteAccounting
    {
        private string _username;
        private string _password;
        private string _intactLogin;
        private string _intactPassword;

        private string _companyName;
        private DefaultUserClaim _userClaims;
        private bool _isUnRestrictedAccessToProp = false;
        private const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
        private const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
        private const string ROLE_ASSIGN = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
        private const string ROLE_UNASSIGN = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";

        // Services
        private IOneSiteAccountingProductService _service = new OneSiteAccountingProductService();

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="userClaims"> User Claims</param>
        public ManageProductOneSiteAccounting(DefaultUserClaim userClaims) : base((int)ProductEnum.FinancialSuite, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _productId = (int)ProductEnum.FinancialSuite;

            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new Logic.ManageBlueBook(userClaims);

            _productUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _username = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value));
            _password = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value));
            _intactLogin = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "INTACTUSER").Value));
            _intactPassword = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "INTACTPASSWORD").Value));

            _service.Url = _productUrl;
            _service.PreAuthenticate = true;
            _service.Credentials = new System.Net.NetworkCredential(_username, _password);
            _userClaims = userClaims;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaim"></param>
        /// <param name="service"></param>
        /// <param name="samlRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePartyRelationship"></param>
        /// <param name="httpMessageHandler"></param>
        /// <param name="repository"></param>
        public ManageProductOneSiteAccounting(Guid editorRealPageId, DefaultUserClaim userClaim, IOneSiteAccountingProductService service, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, HttpMessageHandler httpMessageHandler, IRepository repository)
            : base((int)ProductEnum.FinancialSuite, userClaim, repository, httpMessageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _service = service;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
            _userClaims = userClaim;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaim"></param>
        /// <param name="service"></param>
        /// <param name="samlRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="manageElectronicAddress"></param>
        /// <param name="managePerson"></param>
        /// <param name="manageUserLogin"></param>
        /// <param name="managePartyRelationship"></param>
        /// <param name="httpMessageHandler"></param>
        /// <param name="repository"></param>
        public ManageProductOneSiteAccounting(Guid editorRealPageId, DefaultUserClaim userClaim, IOneSiteAccountingProductService service, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManageElectronicAddress manageElectronicAddress, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship, HttpMessageHandler httpMessageHandler, IRepository repository)
            : base((int)ProductEnum.FinancialSuite, userClaim, repository, httpMessageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _service = service;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _manageElectronicAddress = manageElectronicAddress;
            _managePerson = managePerson;
            _manageUserLogin = manageUserLogin;
            _managePartyRelationship = managePartyRelationship;
            _userClaims = userClaim;
        }

        #region Property

        /// <summary>
        /// Get the properties for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUserProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

            Property[] prop = new Property[1] { new Property() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetUserProperties", $"_productUserId = {_productUserId}" });
            prop[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];
            LocationID[] location;
            IList<ProductProperty> list = new List<ProductProperty>();

            try
            {
                location = _service.GetAllProperties(prop, wsParams, out results2);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "location", location } }, messageProperties: new object[] { "GetUserProperties", "Result from api" });
                list = location.ToGBProperties();

                if (list == null || list.Count() == 0)
                {
                    var propertyList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
                    var gbProp = propertyList.ToGBProperties();
                    if (gbProp != null)
                    {
                        list = gbProp;
                    }
                    else if (results2.Length > 0)
                    {
                        string message = results2[0].TotalRows1;
                        if (message.ToUpper().Contains("NOT A VALID USERID"))
                        {
                            throw new Exception("Invalid user");
                        }
                    }
                }

                Dictionary<string, bool> allProperties = new Dictionary<string, bool>();
                if (list.Any(a => a.IsAssigned == true))
                {
                    allProperties.Add("allProperties", false);
                }
                else
                {
                    allProperties.Add("allProperties", true);
                }
                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = list.Count,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = allProperties
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserProperties", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Get the property Groups for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUserPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

            Property[] prop = new Property[1] { new Property() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetUserPropertyGroups", $"_productUserId = {_productUserId}" });
            prop[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];
            LocationGroupID[] location;
            IList<ProductPropertyGroup> list;

            try
            {

                if (userPersonaId > 0)
                {
                    RPObjectCache rpCache = new RPObjectCache();
                    string cacheKey = $"GetUserPropertyGroups_{userPersonaId}_{_userClaims.OrganizationPartyId}";
                    list = rpCache.GetFromCache(cacheKey, 60, () =>
                    {
                        location = _service.GetAllPropertyGroups(prop, wsParams, out results2);
                        WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "location", location } }, messageProperties: new object[] { "GetUserPropertyGroups", "Result from api" });
                        return location.ToGBPropertyGroup();

                    });

                }
                else
                {
                    location = _service.GetAllPropertyGroups(prop, wsParams, out results2);
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "location", location } }, messageProperties: new object[] { "GetUserPropertyGroups", "Result from api" });
                    list = location.ToGBPropertyGroup();

                }


                if (list == null)
                {
                    if (results2.Length > 0)
                    {
                        string message = results2[0].TotalRows1;
                        if (message.ToUpper().Contains("NOT A VALID USERID"))
                        {
                            throw new Exception("Invalid user");
                        }
                    }
                    list = new List<ProductPropertyGroup>();
                }


                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = list.Count,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = null
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserPropertyGroups", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Get the property Groups for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public List<ProductPropertyGroup> GetAllPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
            Property[] prop = new Property[1] { new Property() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetAllPropertyGroups", $"_productUserId = {_productUserId}" });
            prop[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];
            LocationGroupID[] location;
            List<ProductPropertyGroup> list;

            try
            {

                location = _service.GetAllPropertyGroups(prop, wsParams, out results2);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "location", location } }, messageProperties: new object[] { "GetAllPropertyGroups", "Result from api" });
                list = location.ToGBPropertyGroup();

                if (list == null)
                {
                    list = new List<ProductPropertyGroup>();
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllPropertyGroups", "Returned null data - no error" });
                }
            }
            catch (Exception ex)
            {
                list = null;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetAllPropertyGroups", $"Error: {ex.Message}" });
            }
            return list;
        }

        /// <summary>
        /// Get the property Groups for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetPropertyGroupEntities(long editorPersonaId, long userPersonaId, string locationGrpId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

            Property[] prop = new Property[1] { new Property() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            if (!String.IsNullOrEmpty(locationGrpId))
            {
                loginInfo.Add(new NameValuePair { Name = "locGroupIds", Value = locationGrpId });
            }
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetPropertyGroupEntities", $"_productUserId = {_productUserId}" });
            prop[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];
            LocationGroupID[] location;
            IList<ProductPropertyGroup> list;

            try
            {
                location = _service.GetAllPropertyGroupMembers(prop, wsParams, out results2);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "location", location } }, messageProperties: new object[] { "GetPropertyGroupEntities", "Result from api" });
                list = location.ToGBPropertyGroup();

                if (list == null)
                {
                    if (results2.Length > 0)
                    {
                        string message = results2[0].TotalRows1;
                        if (message.ToUpper().Contains("NOT A VALID USERID"))
                        {
                            throw new Exception("Invalid user");
                        }
                    }
                    list = new List<ProductPropertyGroup>();
                }

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = list.Count,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = null
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetPropertyGroupEntities", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Get the properties for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUserPropertiesNew(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserPropertiesNew", $"_productUserId = {_productUserId} - START" });

                List<ACProperty> companyPropertiesList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
                companyPropertiesList = companyPropertiesList.FindAll(m => m.PropertyId != string.Empty && m.PropertyName != string.Empty);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserPropertiesNew", $"_productUserId = {_productUserId} - END" });

                //List<ACCompany> cmpList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);

                if (companyPropertiesList.Count(p => !string.IsNullOrEmpty(p.MConsoleId.Trim())) != 0)
                {
                    //We have MConsole company here
                    companyPropertiesList.ForEach(x => x.Id = string.Concat(x.Id + "|" + x.CompanyId));
                }

                response = new ListResponse()
                {
                    Records = companyPropertiesList.Cast<object>().ToList(),
                    TotalRows = companyPropertiesList.Count,
                    RowsPerPage = companyPropertiesList.Count,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = null
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserPropertiesNew", $"Error: {ex.Message}" });
                //UI calls GetProperty but sometimes it's diplaying the data in Entities tab, that's why this message should be Entity instead of Property
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = CommonMessageConstants.EntityErrorMessage
                };
            }
            return response;
        }

        /// <summary>
        /// Get the user details for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public NameValuePair[] GetUser(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
            SharedObjects.Product.OneSiteAccounting.User[] user = new SharedObjects.Product.OneSiteAccounting.User[1] { new SharedObjects.Product.OneSiteAccounting.User() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName},
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetUser", $"_productUserId = {_productUserId}" });
            user[0].NameValuePair = loginInfo.ToArray();

            NameValuePair[] userResp = null;
            IList<ProductProperty> list;

            try
            {
                userResp = _service.GetUser(user);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user details", userResp } }, messageProperties: new object[] { "GetUser", "Result from api" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUser", $"Error: {ex.Message}" });
            }
            return userResp;
        }

        /// <summary>
        /// Get the properties for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUserCompanies(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }

            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

            List<ACCompany> cmpList;
            AccountingUser aUser = new AccountingUser();

            try
            {
                cmpList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);
                NameValuePair[] userResp = null;

                List<int> prdIds = GetProductIdsByOrg();
                if (prdIds != null)
                {
                    if (prdIds.Contains((int)ProductEnum.SiteSpendManagement))
                    {
                        aUser.IsSiteSpendManagementAssignedToCompany = true;
                    }
                }

                //Get User details
                if (userPersonaId != 0)
                {
                    // Get User Data
                    userResp = GetUser(editorPersonaId, userPersonaId, datafilter);
                    if (userResp != null)
                    {
                        foreach (var item in userResp)
                        {
                            if (item.Name.ToUpperInvariant() == "UNRESTRICTED")
                            {
                                aUser.HasAccessToAllCurrentFutureProperties = item.Value == "true" ? true : false;
                            }

                            if (item.Name.ToUpperInvariant() == "RPPORTALUSER")
                            {
                                aUser.HasAccessToSiteSpendManagementOnly = item.Value == "true" ? true : false;
                            }

                            if (item.Name.ToUpperInvariant() == "ADMIN")
                            {
                                aUser.IsAccountingAdmin = item.Value == "true" ? true : false;
                            }
                        }
                    }

                    aUser.HasAccessToAllCurrentFutureProperties = ComputeFlagBasedOnCompanyAndPropertySelected(editorPersonaId, userPersonaId, datafilter);
                }

                var propertyList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
                aUser.IsMConsolePMC = (propertyList.Count(p => ((ACProperty)p).MConsoleId.Trim() != string.Empty) > 0) ? true : false;

                if (userResp == null)
                {
                    userResp = new NameValuePair[1];
                }

                response = new ListResponse()
                {
                    Records = cmpList.Cast<object>().ToList(),
                    TotalRows = cmpList.Count,
                    RowsPerPage = cmpList.Count,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = aUser
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserCompanies", $"Error: {ex.Message}" });
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
                    //UI calls GetPropertyGroup but it's displaying the data in Companies Tab, so that's why the message should be "Companies"
                    response.ErrorReason = CommonMessageConstants.CompanyTabErrorMessage;
                }
            }

            return response;
        }

        private bool ComputeFlagBasedOnCompanyAndPropertySelected(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            bool hasAccessToAllCurrentAndFutureProperties = false;
            List<ACCompany> companyList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);
            ListResponse propertyList = GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter);
            ListResponse propertyGroupList = GetUserPropertyGroups(editorPersonaId, userPersonaId, datafilter);

            int totalCompanies = 0;
            int totalProperties = 0;
            int totalCompaniesSelected = 0;
            int totalPropertiesUnSelected = 0;
            int totalPropertiesSelected = 0;


            totalCompanies = companyList.Count;
            totalCompaniesSelected = companyList.Count(c => c.isAssigned == true);

            totalProperties = propertyList.Records.Count;
            totalPropertiesUnSelected = propertyList.Records.Count(p => ((ACProperty)p).IsAssigned == false);
            totalPropertiesSelected = propertyList.Records.Count(p => ((ACProperty)p).IsAssigned == true);

            if ((totalCompanies == totalCompaniesSelected) && (totalProperties == totalPropertiesUnSelected))
                hasAccessToAllCurrentAndFutureProperties = true;

            int totalPropertyGroups = propertyGroupList.Records.Count;
            var selectedPropertiesGroups = propertyGroupList.Records.Where(p => ((ProductPropertyGroup)p).IsAssigned == true);

            if ((totalPropertyGroups > 0) && (selectedPropertiesGroups.Count() > 0) && (totalPropertiesSelected == 0))
            {
                List<string> selectedLocEntities = new List<string>();

                foreach (ProductPropertyGroup item in selectedPropertiesGroups)
                {
                    selectedLocEntities.AddRange(item.AssignedProperties);
                }

                if (propertyList.Records.Count > 0)
                {
                    hasAccessToAllCurrentAndFutureProperties = propertyList.Records.Any(x => selectedLocEntities.Contains(((ACProperty)x).PropertyName));
                }
            }

            return hasAccessToAllCurrentAndFutureProperties;
        }

        private List<ACCompany> GetUserCompaniesDetails(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            var comp = new SharedObjects.Product.OneSiteAccounting.Company[1] { new SharedObjects.Product.OneSiteAccounting.Company() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName},
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetUserCompaniesDetails", $"_productUserId = {_productUserId}" });
            comp[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];

            CompanyID[] company;
            List<ACCompany> cmpList;
            AccountingUser aUser = new AccountingUser();

            try
            {
                company = _service.getCompaniesAPI(comp);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "company", company } }, messageProperties: new object[] { "GetUserCompaniesDetails", "Result from api" });
                cmpList = company.ToGBCompanies();

                if (cmpList == null)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserCompaniesDetails", "Returned null data from getCompaniesAPI api - no error" });
                    cmpList = new List<ACCompany>();
                }


            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserCompaniesDetails", $"Error: {ex.Message}" });
                cmpList = new List<ACCompany>();
            }

            return cmpList;
        }

        /// <summary>
        /// Get all the company-properties
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        private List<ACProperty> GetAllCompanyProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            SharedObjects.Product.OneSiteAccounting.Company[] comp = new SharedObjects.Product.OneSiteAccounting.Company[1] { new SharedObjects.Product.OneSiteAccounting.Company() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName},
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetAllCompanyProperties", $"_productUserId = {_productUserId}" });
            comp[0].NameValuePair = loginInfo.ToArray();

            EntityID[] entitys;
            List<ACProperty> list;

            try
            {
                entitys = _service.getPropertiesAPI(comp);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "entity", entitys } }, messageProperties: new object[] { "GetAllCompanyProperties", "Result from api" });
                list = entitys.ToGBEnteties();

                if (list == null)
                {
                    list = new List<ACProperty>();
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllCompanyProperties", "Returned null data from getPropertiesAPI api - no error" });
                }

            }
            catch (Exception ex)
            {
                list = null;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetAllCompanyProperties", $"Error: {ex.Message}" });
            }

            return list;
        }

        #endregion

        #region Roles

        /// <summary>
        /// Used to get the list of roles for the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUserRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

            Role[] role = new Role[1] { new Role() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
                  {
                     new NameValuePair { Name = "CompanyID", Value = _companyName },
                     new NameValuePair { Name = "Login", Value = _intactLogin },
                     new NameValuePair { Name = "Password", Value = _intactPassword }
                 };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetUserRoles", $"_productUserId = {_productUserId}" });
            role[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];
            RoleName[] roleList;
            IList<ProductRole> list;

            try
            {
                roleList = _service.GetAllRoles(role, wsParams, out results2);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleList", roleList }, { "results2", results2 } }, messageProperties: new object[] { "GetUserRoles", "Result from api" });
                list = roleList.ToGBRoles();

                if (list == null)
                {
                    if (results2.Length > 0)
                    {
                        string message = results2[0].TotalRows1;
                        if (message.ToUpper().Contains("NOT A VALID USERID"))
                        {
                            throw new Exception("Invalid user");
                        }
                    }
                    list = new List<ProductRole>();
                }

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserRoles", $"Error: {ex.Message}" });
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

        #endregion

        /// <summary>
        /// Get current companies and assign to user for Allow access to all current and future companies
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="propertiesToAssign"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <param name="batchProcessType"></param>
        /// <param name="additionalParameters"></param>
        /// <returns></returns>
        public string AssignAllCurrentCompaniesToUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, bool isAccountingAdmin, BatchProcessType batchProcessType, out List<AdditionalParameters> additionalParameters)
        {
            RequestParameter datafilter = new RequestParameter();
            List<ACCompany> currentCompanyList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "currentCompanyList", currentCompanyList } }, messageProperties: new object[] { "AssignAllCurrentCompaniesToUser", "Current companies to be assigned to user" });
            propertiesToAssign.Clear();

            foreach (ACCompany company in currentCompanyList)
            {
                propertiesToAssign.Add(company.Id);
            }

            return UpdatePropertiesToUser(editorPersonaId, userPersonaId, propertiesToAssign, isAccountingAdmin, out additionalParameters, batchProcessType);
        }

        /// <summary>
        /// Update the properties assigned to the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="propertiesToAssign"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <param name="batchProcessType"></param>
        /// <param name="additionalParametersProperties"></param>
        /// <returns></returns>
        public string UpdatePropertiesToUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, bool isAccountingAdmin, out List<AdditionalParameters> additionalParametersProperties, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            string assignSuccessful = "";
            additionalParametersProperties = new List<AdditionalParameters>();
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "propertiesToAssign", propertiesToAssign } }, messageProperties: new object[] { "UpdatePropertiesToUser", "Begin" });
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response.ErrorReason; }

            if (String.IsNullOrEmpty(_productUserId))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", "Missing product user. _productUserId = empty" });
                return "Missing product user";
            }

            RequestParameter datafilter = new RequestParameter();
            string propertyIDAddList = "All";
            string propertyIDRemoveList = "";
            List<string> propertiesToRemove = new List<string>();
            List<ACProperty> currentPropertyList = new List<ACProperty>();
            List<ProductPropertyGroup> currentLocationGrpList = new List<ProductPropertyGroup>();
            List<ACProperty> currentEntitiesList = new List<ACProperty>();
            bool isMConsolePMC = false;
            //var a = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter); // companies
            //var b = GetUserPropertyGroups(editorPersonaId, userPersonaId, datafilter); // locationgroups
            //var c = GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter); // entities

            bool superUser = IsSuperUser(userPersonaId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", $"isSuperUser = {superUser}" });

            if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", "Start UserTypeRegularToAdmin or UserTypeExternalToAdmin" });
                    propertyIDRemoveList = "";

                    currentPropertyList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter); //Companies Tab
                    currentLocationGrpList = GetAllPropertyGroups(editorPersonaId, userPersonaId, datafilter); //Location Groups Tab
                    var entitiesListResponse = GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter); //Entities Tab
                    if (entitiesListResponse != null && entitiesListResponse.Records != null)
                    {
                        currentEntitiesList = entitiesListResponse.Records.Cast<ACProperty>().ToList();
                    }
                    isMConsolePMC = (currentPropertyList.Count(p => ((ACProperty)p).MConsoleId.Trim() != string.Empty) > 0) ? true : false;

                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "currentPropertyList", currentPropertyList } }, messageProperties: new object[] { "UpdatePropertiesToUser", "CurrentPropertyList" });
                    // Get the current property list what is already assigned and remove them.
                    foreach (ACProperty prop in currentPropertyList)
                    {
                        if (prop.IsAssigned)
                        {
                            if (prop.MConsoleId == string.Empty)
                            {
                                propertiesToRemove.Add(prop.PropertyId);
                            }
                            else
                            {
                                propertiesToRemove.Add(prop.MConsoleId);
                            }
                        }
                    }

                    if (currentLocationGrpList != null)
                    {
                        foreach (ProductPropertyGroup propLG in currentLocationGrpList)
                        {
                            if ((bool)propLG.IsAssigned)
                            {
                                propertiesToRemove.Add(propLG.ID);
                            }
                        }
                    }

                    if (propertiesToRemove.Count > 0)
                    {
                        propertyIDRemoveList = string.Join(",", propertiesToRemove);
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", $"propertiesToRemove = {propertiesToRemove}. End UserTypeRegularToAdmin or UserTypeExternalToAdmin" });
                    propertyIDAddList = "All";
                }

                if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", "Start UserTypeAdminToRegular or UserTypeAdminToExternal" });

                    propertyIDAddList = "";

                    if (propertiesToAssign.Count > 0)
                    {
                        propertyIDAddList = string.Join(",", propertiesToAssign);
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "propertyIDAddList", propertyIDAddList } }, messageProperties: new object[] { "UpdatePropertiesToUser", "End UserTypeAdminToRegular or UserTypeAdminToExternal" });
                }
            }
            else
            {
                if (!superUser && propertiesToAssign[0].ToUpper() != "ALL")
                {
                    propertyIDAddList = "";
                    currentPropertyList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter); //Companies Tab
                    currentLocationGrpList = GetAllPropertyGroups(editorPersonaId, userPersonaId, datafilter); //Location Groups Tab
                    var entitiesListResponse = GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter); //Entities Tab
                    if (entitiesListResponse != null && entitiesListResponse.Records != null)
                    {
                        currentEntitiesList = entitiesListResponse.Records.Cast<ACProperty>().ToList();
                    }

                    //var propertyList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
                    isMConsolePMC = (currentPropertyList.Count(p => ((ACProperty)p).MConsoleId.Trim() != string.Empty) > 0) ? true : false;

                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "currentPropertyList", currentPropertyList } }, messageProperties: new object[] { "UpdatePropertiesToUser", "currentPropertyList" });
                    // compare the current property list to what was passed to determine what is new and what was removed.
                    foreach (ACProperty prop in currentPropertyList)
                    {
                        if (prop.PropertyId != string.Empty)
                        {
                            if (!(propertiesToAssign.Contains(prop.PropertyId)))
                            {
                                if (prop.IsAssigned)
                                {
                                    if (prop.MConsoleId == string.Empty)
                                    {
                                        // property doesn't exist, so add it to the list
                                        propertiesToRemove.Add(prop.PropertyId);
                                    }
                                    else
                                    {
                                        propertiesToRemove.Add(prop.MConsoleId);
                                    }
                                }
                            }
                            if (propertiesToAssign.Contains(prop.PropertyId) && prop.IsAssigned)
                            {

                                if (prop.MConsoleId == string.Empty)
                                {
                                    propertiesToAssign.Remove(prop.PropertyId);
                                }
                                else
                                {
                                    propertiesToAssign.Remove(prop.MConsoleId);
                                }
                            }
                        }
                        else
                        {
                            if (!(propertiesToAssign.Contains(prop.CompanyId)))
                            {
                                if (prop.IsAssigned)
                                {
                                    if (prop.MConsoleId == string.Empty)
                                    {
                                        // property doesn't exist, so add it to the list
                                        propertiesToRemove.Add(prop.PropertyId);
                                    }
                                    else
                                    {
                                        propertiesToRemove.Add(prop.MConsoleId);
                                    }
                                }
                            }

                        }
                    }

                    if (currentLocationGrpList != null)
                    {
                        foreach (ProductPropertyGroup propLG in currentLocationGrpList)
                        {
                            if ((bool)propLG.IsAssigned)
                            {
                                if (!(propertiesToAssign.Contains(propLG.ID)))
                                {
                                    propertiesToRemove.Add(propLG.ID);
                                }
                                else
                                {
                                    propertiesToAssign.Remove(propLG.ID);
                                }
                            }
                        }
                    }

                    if (propertiesToAssign.Count > 0)
                    {
                        propertyIDAddList = string.Join(",", propertiesToAssign);
                    }
                    if (propertiesToRemove.Count > 0)
                    {
                        propertyIDRemoveList = string.Join(",", propertiesToRemove);
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "propertyIDAddList", propertyIDAddList }, { "propertyIDRemoveList", propertyIDRemoveList } }, messageProperties: new object[] { "UpdatePropertiesToUser", "Property details" });
                }
            }

            NameValuePair[] user = new NameValuePair[4]
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword },
                new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
            };
            NameValuePair[] newUser = user;
            Array.Resize(ref newUser, newUser.Length + 1);
            newUser[4] = new NameValuePair { Name = "replace", Value = "" };
            user = newUser;

            if (superUser)
            {
                if ((propertiesToAssign.Count > 0) && (propertiesToAssign[0].ToUpper() != "ALL"))
                {
                    propertyIDAddList = string.Join(",", propertiesToAssign);
                }

                if (batchProcessType != BatchProcessType.UserTypeRegularToAdmin)
                {
                    // dont need to assign anything because super users get everything automatically in Accounting
                    propertyIDRemoveList = "";
                }
                if (batchProcessType != BatchProcessType.UserTypeExternalToAdmin)
                {
                    propertyIDRemoveList = "";
                }
            }

            string result = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(propertyIDRemoveList))
                {
                    user[4].Name = "PropertyIdsToRemove";
                    user[4].Value = propertyIDRemoveList;
                    // dont save the password to the log!
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(user) } }, messageProperties: new object[] { "UpdatePropertiesToUser", $"RemovePropertiesFromUser. userPersonaId={userPersonaId}" });
                    result = _service.RemovePropertiesFromUser(user);
                    if (result != null && (!result.ToUpper().Contains("PROVIDED USER PROPERTIES REMOVED SUCCESSFULLY") && !result.ToUpper().Contains("PROVIDED USER PROPERTIES DELETED SUCCESSFULLY")))
                    {
                        return assignSuccessful += "Failed to remove. " + result;
                    }

                    assignSuccessful = string.Empty;

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", $"RemovePropertiesFromUser. userPersonaId={userPersonaId}. Result={assignSuccessful}" });
                }
                //var propertyList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
                //bool isMConsolePMC = (propertyList.Count(p => ((ACProperty)p).MConsoleId.Trim() != string.Empty) > 0) ? true : false;

                if (!string.IsNullOrWhiteSpace(propertyIDAddList) && (isMConsolePMC || !_isUnRestrictedAccessToProp))
                {
                    user[4].Name = "PropertyIdsToAdd";
                    user[4].Value = propertyIDAddList;
                    var logData = new Dictionary<string, object>
                    {
                        { "user[0]", user[0] },
                        { "user[1]", user[1] },
                        { "user[3]", user[3] },
                        { "user[4]", user[4] }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", JsonConvert.SerializeObject(logData) } }, messageProperties: new object[] { "UpdatePropertiesToUser", $"userPersonaId={userPersonaId}" });
                    result = _service.AssignPropertiesToUser(user);
                    if (result != null && !result.ToUpper().Contains("PROVIDED USER PROPERTIES ADDED SUCCESSFULLY"))
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", $"AssignPropertiesToUser. userPersonaId={userPersonaId}. Result=Failed Reason: {assignSuccessful}" });
                        return assignSuccessful += "Failed to assign. " + result;
                    }

                    assignSuccessful = string.Empty;

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", $"AssignPropertiesToUser. userPersonaId={userPersonaId}. Result=Success" });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdatePropertiesToUser", $"Error: {ex.Message}" });
                return "An error occurred. " + ex.Message;
            }

            var activityDetails = GetPropertiesAdditionalParameters(propertiesToAssign, propertiesToRemove, currentPropertyList, currentLocationGrpList, currentEntitiesList, isMConsolePMC);
            additionalParametersProperties.AddRange(activityDetails);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesToUser", "Finished" });
            return assignSuccessful;
        }

        /// <summary>
        /// Update the roles assigned to the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="rolesToAssign"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <param name="batchProcessType"></param>
        /// <param name="additionalParametersRoles"></param>
        /// <returns></returns>
        public string UpdateRolesToUser(long editorPersonaId, long userPersonaId, List<string> rolesToAssign, bool isAccountingAdmin, out List<AdditionalParameters> additionalParametersRoles, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            additionalParametersRoles = new List<AdditionalParameters>();
            string assignSuccessful = "";
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", "Begin" });
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response.ErrorReason; }

            if (String.IsNullOrEmpty(_productUserId))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", "Missing product user. _productUserId = empty" });
                return "Missing product user";
            }
            RequestParameter datafilter = new RequestParameter();

            string roleIDAddList = "";
            string roleIDRemoveList = "";
            List<string> rolesToRemove = new List<string>();
            bool superUser = IsSuperUser(userPersonaId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", $"isSuperUser = {superUser}" });
            ListResponse currentRoleList = GetUserRoles(editorPersonaId, userPersonaId, datafilter);
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "currentRoleList", currentRoleList } }, messageProperties: new object[] { "UpdateRolesToUser", "currentRoleList" });

            if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                // For RegularToAdmin User REMOVE existing roles and update to ALL
                if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", "Start UserTypeRegularToAdmin or UserTypeExternalToAdmin" });

                    // Add all ADMIN roles 
                    List<ProductRole> currentList = currentRoleList.Records.Cast<ProductRole>().ToList();
                    foreach (ProductRole role in currentList)
                    {
                        if ((role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && role.IsAssigned == false) || role.IsAssigned)
                        {
                            rolesToAssign.Add(role.ID);
                        }
                    }

                    if (rolesToAssign.Count > 0)
                    {
                        roleIDAddList = string.Join(",", rolesToAssign);
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleIDAddList", roleIDAddList } }, messageProperties: new object[] { "UpdateRolesToUser", "End UserTypeRegularToAdmin or UserTypeExternalToAdmin" });
                }

                if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", "Start UserTypeAdminToRegular or UserTypeAdminToExternal" });
                    // Remove Admin Roles
                    foreach (ProductRole role in currentRoleList.Records)
                    {
                        if (role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && role.IsAssigned == true)
                        {
                            rolesToRemove.Add(role.ID);
                        }
                    }

                    if (rolesToRemove.Count > 0)
                    {
                        roleIDRemoveList = string.Join(",", rolesToRemove);
                    }

                    // Assign the newly passed Roles
                    if (rolesToAssign.Count > 0)
                    {
                        roleIDAddList = string.Join(",", rolesToAssign);
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleIDRemoveList", roleIDRemoveList }, { "roleIDAddList", roleIDAddList } }, messageProperties: new object[] { "UpdateRolesToUser", "End UserTypeRegularToAdmin or UserTypeExternalToAdmin" });
                }
            }
            else
            {
                if (superUser && string.IsNullOrEmpty(_productUserId))
                {
                    List<ProductRole> currentList = currentRoleList.Records.Cast<ProductRole>().ToList();
                    foreach (ProductRole role in currentList)
                    {
                        if (role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && role.IsAssigned == false)
                        {
                            rolesToAssign.Add(role.ID);
                        }
                    }
                }
                else
                {
                    bool isSuperExistsInProduct = superUser && !string.IsNullOrEmpty(_productUserId);
                    // compare the current role list to what was passed to determine what is new and what was removed.
                    foreach (ProductRole role in currentRoleList.Records)
                    {
                        if (!(rolesToAssign.Contains(role.ID)))
                        {
                            if (role.IsAssigned)
                            {
                                // property doesn't exist, so add it to the list
                                rolesToRemove.Add(role.ID);
                            }
                        }
                        if (rolesToAssign.Contains(role.ID) && role.IsAssigned)
                        {
                            rolesToAssign.Remove(role.ID);
                        }

                        if (role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) && isSuperExistsInProduct)
                        {
                            rolesToRemove.Remove(role.ID);
                            if (!role.IsAssigned)
                            {
                                rolesToAssign.Add(role.ID);
                            }
                        }
                    }
                }
                if (rolesToAssign.Count > 0)
                {
                    roleIDAddList = string.Join(",", rolesToAssign);
                }
                if (rolesToRemove.Count > 0)
                {
                    roleIDRemoveList = string.Join(",", rolesToRemove);
                }
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleIDRemoveList", roleIDRemoveList }, { "roleIDAddList", roleIDAddList } }, messageProperties: new object[] { "UpdateRolesToUser", "Add/Remove roles list" });
            }

            NameValuePair[] user = new NameValuePair[4]
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword },
                new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
            };
            NameValuePair[] newUser = user;
            Array.Resize(ref newUser, newUser.Length + 1);
            newUser[4] = new NameValuePair { Name = "replace", Value = "" };
            user = newUser;

            string result = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(roleIDRemoveList))
                {
                    user[4].Name = "RoleIdsToRemove";
                    user[4].Value = roleIDRemoveList;
                    // dont save the password to the log!
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(user) } }, messageProperties: new object[] { "UpdateRolesToUser", $"RemoveRolesFromUser. userPersonaId={userPersonaId}" });
                    result = _service.RemoveRolesFromUser(user);
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "result", result } }, messageProperties: new object[] { "UpdateRolesToUser", "RemoveRolesFromUser result" });
                    if (!result.ToUpper().Contains("REMOVED PROVIDED ROLES SUCCESSFULLY")) //PROVIDED USER ROLES REMOVED SUCCESSFULLY
                    {
                        return assignSuccessful += "Failed to remove. " + result;
                    }

                    assignSuccessful = string.Empty;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", "userPersonaId={userPersonaId}. Result success" });
                }
                if (!string.IsNullOrWhiteSpace(roleIDAddList))
                {
                    user[4].Name = "RoleIdsToAdd";
                    user[4].Value = roleIDAddList;
                    var logData = new Dictionary<string, object>
                    {
                        { "user[0]", user[0] },
                        { "user[1]", user[1] },
                        { "user[3]", user[3] },
                        { "user[4]", user[4] }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateRolesToUser", $"userPersonaId={userPersonaId}" });
                    result = _service.AssignRolesToUser(user);
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "result", result } }, messageProperties: new object[] { "UpdateRolesToUser", "AssignRolesToUser result" });
                    if (!result.ToUpper().Contains("PROVIDED USER ROLES ADDED SUCCESSFULLY"))
                    {
                        return assignSuccessful += "Failed to assign. " + result;
                    }

                    assignSuccessful = string.Empty;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", $"userPersonaId={userPersonaId}. Result success" });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRolesToUser", $"Error: {ex.Message}" });
                return "An error occurred. " + ex.Message;
            }

            if (rolesToAssign.Count > 0)
            {
                var assignedRoles = currentRoleList.Records.Cast<ProductRole>()
                .Where(f => rolesToAssign.Contains(f.ID))
                .Select(f => new AdditionalParameters { Key = "Financial Suite Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", f.Name) })
                .ToList();

                additionalParametersRoles.AddRange(assignedRoles);
            }
            if (rolesToRemove.Count > 0)
            {
                var removedRoles = currentRoleList.Records.Cast<ProductRole>()
                .Where(f => rolesToRemove.Contains(f.ID))
                .Select(f => new AdditionalParameters { Key = "Financial Suite Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", f.Name) })
                .ToList();

                additionalParametersRoles.AddRange(removedRoles);
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesToUser", "Finished" });
            return assignSuccessful;
        }

        /// <summary>
		/// Change user type 
		/// </summary>
        public string ChangeAccountingServiceUserType(long createUserPersonaId, long assignUserPersonaId, List<string> rpList, List<string> PropertyList, List<string> CompanyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp, BatchProcessType batchProcessType)
        {
            return ManageAccountingUser(createUserPersonaId, assignUserPersonaId, rpList, PropertyList, CompanyList, isAccountingAdmin, isSiteSpendManagementUser, isUnRestrictedAccessToProp, out List<AdditionalParameters> additionalParameters, batchProcessType);
        }

        /// <summary>
        /// Updated to create/update a user in Accounting
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="RoleList"></param>
        /// <param name="PropertyList"></param>
        /// <param name="CompanyList"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <param name="isSiteSpendManagementUser"></param>
        /// <param name="isUnRestrictedAccessToProp"></param>
        /// <param name="batchProcessType"></param>
		/// <param name="additionalParameters"></param>
        /// <returns></returns>
        public string ManageAccountingUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, List<string> CompanyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", "Beginning" });
            additionalParameters = new List<AdditionalParameters>();
            try
            {
                bool isAdmin = false;
                bool adminRoles = false;
                string supervisorId = string.Empty;
                List<string> rolesToCarryForward = new List<string>();
                List<string> adminRolesCarryForward = new List<string>();
                ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError) { return listResponse.ErrorReason; }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"Accounting Admin = {isAccountingAdmin}, SiteSpendManagementUser/Portal User = {isSiteSpendManagementUser}, Access to Current and Future Properties = {isUnRestrictedAccessToProp}" });
                supervisorId = GetSupervisorUserDetails(userPersonaId);

                string accountingLoginName = "";

                Persona userPersona = _managePersona.GetPersona(userPersonaId);
                Guid realPageId = userPersona.RealPageId;

                IC.Person person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                bool isSuperUser = IsSuperUser(userPersona.PersonaId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"isSuperUser = {isSuperUser}" });

                // get the email address
                string userEmailAddress = "";
                IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
                if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY"))
                {
                    userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "PRIMARY" select a.AddressString).FirstOrDefault();
                }
                else
                {
                    // this must look like a real email address or Intact will fail to create the user
                    // For user with RegularUser No Email ==> when an email is entered
                    if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "EMAIL"))
                    {
                        userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                    }
                    else
                    {
                        userEmailAddress = userLogin.LoginName;
                    }
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"Before email fix userEmailAddress = {userEmailAddress}" });
                // verify email address looks valid, will fail if not
                userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"After email fix userEmailAddress = {userEmailAddress}" });

                if (string.IsNullOrEmpty(_productUserId))
                {
                    // get a login name that isn't in use for the new user
                    bool foundUserName = false;
                    int incrementor = 0;
                    string lastNameNoWhiteSpace = person.LastName.TrimWhiteSpace();
                    string newproductUsername = (person.FirstName.TrimWhiteSpace().Substring(0, 1) + lastNameNoWhiteSpace.Substring(0, (lastNameNoWhiteSpace.Length >= 19 ? 19 : lastNameNoWhiteSpace.Length))).ToLower();
                    accountingLoginName = newproductUsername;
                    // give up after 10 tries
                    while (!foundUserName)
                    {
                        if (CheckIfUserLoginIsUsed(_editorPersona.PersonaId, accountingLoginName))
                        {
                            incrementor++;
                            accountingLoginName = newproductUsername + incrementor.ToString();
                        }
                        else
                        {
                            foundUserName = true;
                        }

                    }
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"Generated accountingLoginName = {accountingLoginName}" });
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"Used _productUsername = {_productUsername}" });
                    accountingLoginName = _productUsername;
                }
                string randomPassword = Guid.NewGuid().ToString().Replace("-", "");

                accountingLoginName = RemoveSpecialCharacter(accountingLoginName);

                List<NameValuePair> parameters = new List<NameValuePair>{
                    new NameValuePair { Name = "CompanyID", Value = _companyName },
                    new NameValuePair { Name = "Login", Value = _intactLogin },
                    new NameValuePair { Name = "Password", Value = _intactPassword },
                    new NameValuePair { Name = "LoginId", Value = accountingLoginName },
                };

                string userResultString = "";
                string firstName = person.FirstName.Substring(0, person.FirstName.Length >= 40 ? 40 : person.FirstName.Length);
                string lastName = person.LastName.Substring(0, person.LastName.Length >= 40 ? 40 : person.LastName.Length);


                parameters.Add(new NameValuePair { Name = "ConInfoFirstName", Value = firstName });
                parameters.Add(new NameValuePair { Name = "ConInfoLastName", Value = lastName });
                parameters.Add(new NameValuePair { Name = "ConInfoEmail1", Value = userEmailAddress });
                parameters.Add(new NameValuePair { Name = "ConInfoContactName", Value = "" });
                parameters.Add(new NameValuePair { Name = "Description", Value = firstName + " " + lastName });
                parameters.Add(new NameValuePair { Name = "LoginDisabled", Value = "false" });
                parameters.Add(new NameValuePair { Name = "UnRestricted", Value = isUnRestrictedAccessToProp || isSuperUser ? "true" : "false" });  //Allow access to all current and future properties - Toggle from UI
                parameters.Add(new NameValuePair { Name = "SSOEnabled", Value = "true" });
                parameters.Add(new NameValuePair { Name = "SSOCompanyEnabled", Value = "Enabled" });
                parameters.Add(new NameValuePair { Name = "Visible", Value = "true" });
                parameters.Add(new NameValuePair { Name = "Status", Value = "true" });

                parameters.Add(new NameValuePair { Name = "PortalUser", Value = (isSiteSpendManagementUser == true ? "true" : "false") }); // Site Spend Management User - Portal User - Toggle from UI
                parameters.Add(new NameValuePair { Name = "Admin", Value = (isSuperUser || isAccountingAdmin == true ? "true" : "false") }); // For RealPage Admin || Accounting admin toggle from UI
                parameters.Add(new NameValuePair { Name = "SupervisorUserId", Value = supervisorId });

                if (string.IsNullOrEmpty(_productUserId))
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);
                    parameters.Add(new NameValuePair { Name = "UserType", Value = "business user" });
                    parameters.Add(new NameValuePair { Name = "PWDNeverExpires", Value = "true" });
                    parameters.Add(new NameValuePair { Name = "PWDQlyNotEnforced", Value = "true" });

                    NameValuePair[] user = parameters.ToArray();
                    NameValuePair[] userResult;
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", JsonConvert.SerializeObject(RemovePrivateData(user)) } }, messageProperties: new object[] { "ManageAccountingUser", $"Creating user. userPersonaId = {userPersonaId}" });
                    userResult = _service.CreateUser(user);

                    if (userResult[0].Value.ToUpper().Contains("CAN'T CREATE THE USER") || userResult[0].Value.ToUpper().Contains("SECURITY QUESTIONS AND ANSWER COULD NOT BE UPDATED"))
                    {
                        WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userResult", userResult } }, messageProperties: new object[] { "ManageAccountingUser", $"Error creating user. userPersonaId = {userPersonaId}" });
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                        return userResult[0].Value;
                    }

                    //_samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, _pmcID);
                    for (int i = 0; i < userResult.Length; i++)
                    {
                        // pull out the needed info
                        string key = userResult[i].Name.ToUpper();
                        switch (key) // SystemIdentifier
                        {
                            case "SYSTEMIDENTIFIER":
                                string pmcuserlogin = userResult[i].Value;
                                _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, pmcuserlogin);
                                _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, pmcuserlogin.Split('|')[1]);
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"Created user. saving product login = {pmcuserlogin}" });

                                var loginInfo = new NameValuePair[4]
                                {
                                new NameValuePair { Name = "CompanyID", Value = _companyName },
                                new NameValuePair { Name = "Login", Value = _intactLogin },
                                new NameValuePair { Name = "Password", Value = _intactPassword },
                                new NameValuePair { Name = "SystemIdentifier", Value = pmcuserlogin }
                                };
                                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo) } }, messageProperties: new object[] { "ManageAccountingUser", "EnableGreenBookUser Begin" });

                                var message = _service.EnableGreenBookUser(loginInfo);
                                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "Message", message } }, messageProperties: new object[] { "ManageAccountingUser", "EnableGreenBookUser End" });

                                break;
                        }
                    }
                    // update the users greenbook status

                }
                else
                {
                    if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                    {
                        if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                        {
                            isAdmin = true;
                        }
                        //parameters.Add(new NameValuePair { Name = "Admin", Value = (isAdmin == true || isAccountingAdmin == true ? "true" : "false") });

                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"BatchProcessType = {batchProcessType}" });
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAccountingUser", $"UserType change. isAdmin = {isAdmin}" });
                    }

                    parameters.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
                    NameValuePair[] user = parameters.ToArray();
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", JsonConvert.SerializeObject(RemovePrivateData(user)) } }, messageProperties: new object[] { "ManageAccountingUser", "UpdateUser" });

                    RequestParameter datafilter = new RequestParameter();
                    ListResponse currentRoleList = GetUserRoles(editorPersonaId, userPersonaId, datafilter);
                    if (isAdmin)
                    {
                        foreach (ProductRole role in currentRoleList.Records)
                        {
                            if (role.IsAssigned == true)
                            {
                                rolesToCarryForward.Add(role.ID);
                            }
                        }
                    }
                    if (isSuperUser)
                    {
                        if (RoleList.Count() == 0)
                        {
                            adminRoles = true;
                            foreach (ProductRole role in currentRoleList.Records)
                            {
                                if (!role.Name.Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (role.IsAssigned)
                                    {
                                        adminRolesCarryForward.Add(role.ID);
                                    }
                                }
                            }
                        }
                    }
                    userResultString = _service.UpdateUser(user);

                    ChangeStatusAccountingUser(editorPersonaId, userPersonaId, true);
                }

                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                // check result string results

                if (RoleList == null)
                {
                    RoleList = new List<string>();
                }
                if (PropertyList == null)
                {
                    PropertyList = new List<string>();
                }
                if (isAdmin)
                {
                    RoleList = rolesToCarryForward; // batchProcessType == BatchProcessType.UserTypeRegularToAdmin;
                }
                if (isSuperUser && adminRoles)
                {
                    RoleList = adminRolesCarryForward;
                }

                // For SuperUser users -  Accounting sets the Admin related roles - no need to clear prev roles			
                string updateResultRoles = UpdateRolesToUser(editorPersonaId, userPersonaId, RoleList, isAccountingAdmin, out List<AdditionalParameters> additionalParametersRoles, batchProcessType);
                additionalParameters.AddRange(additionalParametersRoles);
                if (!string.IsNullOrEmpty(updateResultRoles))
                {
                    return updateResultRoles;
                }

                // For Accounting Admin users, assign the selected companies. GB-7188
                if (isAccountingAdmin && CompanyList.Count > 0 && PropertyList[0].ToUpper() == "ALL")
                {
                    PropertyList.Clear();
                    PropertyList = CompanyList;
                }

                // For SuperUser/IsAccounting Admin users -  Accounting sets ALL properties as unrestricted- no need to clear properties
                if ((!isSuperUser && !isUnRestrictedAccessToProp) && PropertyList.Count > 0)
                {
                    string updateResultProp = UpdatePropertiesToUser(editorPersonaId, userPersonaId, PropertyList, isAccountingAdmin, out List<AdditionalParameters> additionalParametersProperties, batchProcessType);
                    additionalParameters.AddRange(additionalParametersProperties);
                    if (!string.IsNullOrEmpty(updateResultProp))
                    {
                        return updateResultProp;
                    }
                }

                if ((isSuperUser || isUnRestrictedAccessToProp))
                {
                    _isUnRestrictedAccessToProp = true;
                    string updateResultProp = AssignAllCurrentCompaniesToUser(editorPersonaId, userPersonaId, PropertyList, isAccountingAdmin, batchProcessType, out List<AdditionalParameters> additionalParametersCompanies);
                    additionalParameters.AddRange(additionalParametersCompanies);
                    if (!string.IsNullOrEmpty(updateResultProp))
                    {
                        return updateResultProp;
                    }
                }

                if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                {
                    WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, batchProcessType);
                }

                return "";

            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageAccountingUser", $"Error for user with editorPersona id - {editorPersonaId}. Error: {ex.Message}" });
                return $"Error - {ex.Message}";
            }
        }

        private string RemoveSpecialCharacter(string accountingLoginName)
        {
            switch (accountingLoginName)
            {
                case "portluser":
                case "realpage":
                case "CPAUser":
                case "ExtUser":
                case "SvcUser":
                case "Services":
                case "CNS_":
                    accountingLoginName = $"{accountingLoginName}-1";
                    break;
            }

            var reg = new Regex(@"[^\w\s\-\.]");
            accountingLoginName = reg.Replace(accountingLoginName, string.Empty);

            if (accountingLoginName.Length > 80)
                accountingLoginName = accountingLoginName.Substring(1, 80);

            return accountingLoginName;
        }

        /// <summary>
        /// Update Accounting User Profile
        /// </summary> 
        public string UpdateAccountingUserProfile(long editorPersonaId, long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAccountingUserProfile", "Beginning" });
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            string supervisorId = string.Empty;
            string accountingLoginName = "";

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid realPageId = userPersona.RealPageId;

            IC.Person person = _managePerson.GetPerson(realPageId);

            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            bool isSuperUser = IsSuperUser(userPersona.PersonaId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAccountingUserProfile", $"isSuperUser = {isSuperUser}" });

            supervisorId = GetSupervisorUserDetails(userPersonaId);
            // get the email address
            string userEmailAddress = "";
            IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
            if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY"))
            {
                userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "PRIMARY" select a.AddressString).FirstOrDefault();
            }
            else
            {
                // For user with RegularUser No Email ==> when an email is entered
                if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "EMAIL"))
                {
                    userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                }
                else
                {
                    // this must look like a real email address or Intact will fail to create the user
                    userEmailAddress = userLogin.LoginName;
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAccountingUserProfile", $"Before email fix userEmailAddress = {userEmailAddress}" });
            // verify email address looks valid, will fail if not
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAccountingUserProfile", $"After email fix userEmailAddress = {userEmailAddress}" });
            if (!string.IsNullOrEmpty(_productUserId))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAccountingUserProfile", $"Used _productUsername = {_productUsername}" });
                accountingLoginName = _productUsername;
            }

            List<NameValuePair> parameters = new List<NameValuePair>{
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword },
                new NameValuePair { Name = "LoginId", Value = accountingLoginName },
            };
            string result = "";
            string firstName = person.FirstName.Substring(0, person.FirstName.Length >= 40 ? 40 : person.FirstName.Length);
            string lastName = person.LastName.Substring(0, person.LastName.Length >= 40 ? 40 : person.LastName.Length);

            parameters.Add(new NameValuePair { Name = "FirstName", Value = firstName });
            parameters.Add(new NameValuePair { Name = "LastName", Value = lastName });
            parameters.Add(new NameValuePair { Name = "Email", Value = userEmailAddress });
            parameters.Add(new NameValuePair { Name = "Description", Value = firstName + " " + lastName });
            parameters.Add(new NameValuePair { Name = "SupervisorUserId", Value = supervisorId });

            if (!string.IsNullOrEmpty(_productUserId))
            {
                parameters.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
                NameValuePair[] user = parameters.ToArray();
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", JsonConvert.SerializeObject(RemovePrivateData(user)) } }, messageProperties: new object[] { "UpdateAccountingUserProfile", $"Updating user. userPersonaId = {userPersonaId}" });
                result = _service.UpdateUserDetails(user);

                if (result.Trim().ToUpper().Contains("SUCCESSFULLY"))
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAccountingUserProfile", $"Updated profile successfully userPersonaId:{userPersonaId}" });

                    // Activity Logging
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "Updated User profile in Financial Suite.");
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAccountingUserProfile", $"Updated User profile in Financial Suite failed userPersonaId:{userPersonaId}" });
                    return "Update Profile failed. " + result;
                }
            }

            // check result string results                      

            return "";
        }

        private object RemovePrivateData(NameValuePair[] user)
        {
            user = user.Where(x => x.Name.ToUpper() != "PASSWORD").ToArray();
            return user;
        }

        /// <summary>
        /// Used to enable/disable an Accounting user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public string ChangeStatusAccountingUser(long editorPersonaId, long userPersonaId, bool isActive)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError || _productUsername == null) { return listResponse.ErrorReason; }
            Dictionary<string, object> logData = new Dictionary<string, object>();

            try
            {
                List<NameValuePair> parameters = new List<NameValuePair>{
                    new NameValuePair { Name = "CompanyID", Value = _companyName },
                    new NameValuePair { Name = "Login", Value = _intactLogin },
                    new NameValuePair { Name = "Password", Value = _intactPassword },
                    new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
                };
                logData = new Dictionary<string, object> { { "parameters", RemovePrivateData(parameters.ToArray()) } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ChangeStatusAccountingUser", $"Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive}" });
                string result = "";
                if (isActive)
                {
                    result = _service.EnableUser(parameters.ToArray());
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                }
                else
                {
                    result = _service.DisableUser(parameters.ToArray());
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Inactive);
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeStatusAccountingUser", $"Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive}" });
                return "Updated failed";
            }
        }

        /// <summary>
        /// Used to enable/disable an Accounting user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="isLinked"></param>
        /// <returns></returns>
        public bool ChangeAccountingUserClaimStatus(long editorPersonaId, long userPersonaId, bool isLinked)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError || _productUsername == null) { return false; }

            bool result = false;

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "ProductUserId", _productUserId } }, messageProperties: new object[] { "ChangeAccountingUserClaimStatus", $"userPersonaId = {userPersonaId}, isLinked = {isLinked}" });
                _service.ChangeClaimStatus(_productUserId, isLinked, _intactLogin, _intactPassword, _productUsername);
                result = true;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeAccountingUserClaimStatus", $"Updating user status. userPersonaId = {userPersonaId}, isActive = {isLinked.ToString()}. Error: {ex.Message}" });
            }
            return result;
        }

        /// <summary>
        /// Used to delete an Accounting user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        public string DeleteAccountingUser(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            // the Accounting user deleting the user
            try
            {
                List<NameValuePair> parameters = new List<NameValuePair>{
                    new NameValuePair { Name = "CompanyID", Value = _companyName },
                    new NameValuePair { Name = "Login", Value = _intactLogin },
                    new NameValuePair { Name = "Password", Value = _intactPassword },
                    new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "parameters", JsonConvert.SerializeObject(RemovePrivateData(parameters.ToArray())) } }, messageProperties: new object[] { "DeleteAccountingUser", $"Deleting user. userPersonaId = {userPersonaId}" });
                _service.DeleteUser(parameters.ToArray());
                // now remove the attributes from this persona so a new user can be created later
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "DeleteAccountingUser", $"Delete user. userPersonaId = {userPersonaId}. Error: {ex.Message}" });
                return "There was a problem deleting the user";
            }
            return "";
        }

        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            string result = ChangeStatusAccountingUser(editorPersonaId, userPersonaId, false);
            if (result.Trim().ToUpper().Contains("INACTIVATED"))
            {
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Success userPersonaId:{userPersonaId}" });

            }
            else
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Failed userPersonaId:{userPersonaId}. Error: {result}" });
                return "Unassign failed. " + result;
            }

            return "";
        }

        #region Roles & Rights

        /// <summary>
        /// Used to get the list of roles and the count of rights associated to that role 
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetRolesCount(long editorPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            Permissions[] permissions = new Permissions[1] { new Permissions() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetRolesCount", $"_productUserId = {_productUserId}" });
            permissions[0].NameValuePair = loginInfo.ToArray();

            try
            {
                var permissionList = _service.GetApplicationPermissions(permissions);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleList", permissionList } }, messageProperties: new object[] { "GetRolesCount", "Result from api" });
                IList<ProductRole> list = permissionList.ToRoles();

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRolesCount", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }

            return response;
        }

        /// <summary>
        /// Used to get the list of ALL roles 
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetAllRoles(long editorPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

            Role[] role = new Role[1] { new Role() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetAllRoles", $"_productUserId = {_productUserId}" });
            role[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "wsParams", JsonConvert.SerializeObject(wsParams) } }, messageProperties: new object[] { "GetAllRoles", "Calling api" });
                var roleList = _service.GetAllRoles(role, wsParams, out results2);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleList", roleList }, { "results2", results2 } }, messageProperties: new object[] { "GetAllRoles", "Result from api" });

                response = new ListResponse()
                {
                    Records = roleList.Cast<object>().ToList(),
                    TotalRows = roleList.Length,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetAllRoles", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Used to get the list of rights 
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <returns></returns>        
        public ListResponse GetRights(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            Permissions[] permissions = new Permissions[1] { new Permissions() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                     new NameValuePair { Name = "CompanyID", Value = _companyName },
                     new NameValuePair { Name = "Login", Value = _intactLogin },
                     new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetRights", $"_productUserId = {_productUserId}" });
            permissions[0].NameValuePair = loginInfo.ToArray();

            try
            {
                var permissionList = _service.GetApplicationPermissions(permissions);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleList", permissionList } }, messageProperties: new object[] { "GetRights", "Result from api" });
                var list = permissionList.ToRights();

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRights", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Used to get the list of applications / modules
        /// </summary>
        /// <param name="editorPersonaId"></param>       
        /// <returns></returns>
        public ListResponse GetApplications(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }


            Applications[] applications = new Applications[1] { new Applications() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
                  {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                //loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetApplications", $"_productUserId = {_productUserId}" });
            applications[0].NameValuePair = loginInfo.ToArray();

            try
            {
                RPObjectCache rpcache = new RPObjectCache();
                var cacheKey = "AccountingApplications_" + _companyName;
                var appList = rpcache.GetFromCache<ApplicationID[]>(cacheKey, 600, () =>
                {
                    return _service.GetApplications(applications);
                });


                //appList = _service.GetApplications(applications);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "appList", appList } }, messageProperties: new object[] { "GetApplications", "Result from api" });

                var list = appList.ToCenters();

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Length,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetApplications", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Used to get a list of roles associated to the given right 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="rightId"></param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public ListResponse GetRolesForRight(long editorPersonaId, RequestParameter datafilter, int rightId, bool assignedOnly, ProductRightAcct right)
        {
            //RoleList roleListResult = new RoleList();
            ListResponse response = new ListResponse();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }


            Permissions[] permissions = new Permissions[1] { new Permissions() };
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(right.ModuleID))
            {
                loginInfo.Add(new NameValuePair { Name = "ModuleID", Value = right.ModuleID });
            }
            if (!String.IsNullOrEmpty(right.ID.ToString()))
            {
                loginInfo.Add(new NameValuePair { Name = "rightID", Value = right.RightID.ToString() });
            }
            if (!String.IsNullOrEmpty(right.Right))
            {
                loginInfo.Add(new NameValuePair { Name = "right", Value = right.Right });
            }
            if (!String.IsNullOrEmpty(right.Action))
            {
                loginInfo.Add(new NameValuePair { Name = "action", Value = right.Action });
            }
            if (!String.IsNullOrEmpty(right.Alias))
            {
                loginInfo.Add(new NameValuePair { Name = "actionLabel", Value = right.ActionLabel });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetRolesForRight", $"_productUserId = {_productUserId}" });
            permissions[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];

            try
            {

                var permissionList = _service.GetPermissionRoles(permissions);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleList", permissionList }, { "results2", results2 } }, messageProperties: new object[] { "GetRolesForRight", "Result from api" });
                IList<ProductRole> list = permissionList.ToRolesList();

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRolesForRight", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }

            return response;
        }

        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
        /// <param name="rightId">The right being assigned</param>
        /// <param name="rolesToAdd">A list of role ids to add to the role</param>
        /// <param name="rolesToRemove">A list of role ids to remove from the role</param>
        /// <param name="right"></param>
        public ListResponse UpdateRolesForRight(long editorPersonaId, int rightId, List<ProductRoleAcct> rolesToAdd, List<ProductRoleAcct> rolesToRemove, ProductRightAcct right)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            int arrLength = rolesToAdd.Count + rolesToRemove.Count;
            RolePermission[] rolePermissions = new RolePermission[arrLength];
            RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[] user = new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[1] { new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            List<string> addedRoles = new List<string>();
            List<string> removedRoles = new List<string>();
            int i = 0;
            foreach (var item in rolesToAdd)
            {
                RolePermission rp = new RolePermission();

                rp.moduleid = right.ModuleID;
                rp.right = right.Right;
                rp.action = right.Action;
                //rp.roleid = item.ID.ToString();
                rp.roleName = item.Name;
                rp.value = "true";
                rolePermissions[i] = rp;
                i++;
                addedRoles.Add(item.Name);

            }

            foreach (var item in rolesToRemove)
            {
                RolePermission rp = new RolePermission();

                rp.moduleid = right.ModuleID;
                rp.right = right.Right;
                rp.action = right.Action;
                //rp.roleid = item.ID.ToString();
                rp.roleName = item.Name;
                rp.value = "false";
                rolePermissions[i] = rp;
                i++;
                removedRoles.Add(item.Name);
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "UpdateRolesForRight", $"_productUserId = {_productUserId}" });
            user[0].NameValuePair = loginInfo.ToArray();

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "rolePermissions", JsonConvert.SerializeObject(rolePermissions) } }, messageProperties: new object[] { "UpdateRolesForRight", "Before api" });
                var output = _service.AssignRolePermissions(user, rolePermissions);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "output", JsonConvert.SerializeObject(output) } }, messageProperties: new object[] { "UpdateRolesForRight", "Result from api" });

                string error = string.Empty;
                bool isError = false;

                if (output[0].Value.IndexOf("fail") != -1)
                {
                    error = output[1].Value;
                    isError = true;
                }
                response = new ListResponse()
                {
                    Records = output.Cast<object>().ToList(),
                    TotalRows = output.Length,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
                if (!isError)
                {
                    if (rolesToAdd.Any() || rolesToRemove.Any())
                    {
                        UpdateRolesByRightLogMessage(editorPersonaId, right.Description, addedRoles, removedRoles);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRolesForRight", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }
        public void UpdateRolesByRightLogMessage(long editorPersonaId, string rightName, List<string> rolesToAdd, List<string> rolesToRemove)
        {
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);
            if (rolesToAdd != null)
            {
                foreach (var role in rolesToAdd)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = rightName, Value = ROLE_ASSIGN.Replace("RoleName", role) });
                }
            }
            if (rolesToRemove != null)
            {
                foreach (var role in rolesToRemove)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = rightName, Value = ROLE_UNASSIGN.Replace("RoleName", role) });
                }
            }
            var message = "";
            message = impersonatorUserInfo != null
              ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed roles to {rightName} in Financial Suite."
            : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed roles to {rightName} in Financial Suite.";

            unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, 8);
        }
        /// <summary>
        /// Used to get a list of rights associated to the given role id
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId"></param>        
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetRightsForRole(long editorPersonaId, RequestParameter datafilter, string roleName, int roleId = 0)
        {
            //RoleList roleListResult = new RoleList();
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            Permissions[] permissions = new Permissions[1] { new Permissions() };
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            if (!String.IsNullOrEmpty(roleId.ToString()))
            {
                loginInfo.Add(new NameValuePair { Name = "RoleName", Value = roleName });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetRightsForRole", $"_productUserId = {_productUserId}" });
            permissions[0].NameValuePair = loginInfo.ToArray();

            try
            {
                var roleList = _service.GetRolePermissions(permissions);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "roleList", roleList } }, messageProperties: new object[] { "GetRightsForRole", "Result from api" });
                var list = roleList.ToRights();

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRightsForRole", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }

            return response;
        }

        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
        /// <param name="roleId">The role being assigned</param>
        /// <param name="rightsToAdd">A list of right ids to add to the role</param>
        /// <param name="rightsToRemove">A list of right ids to remove from the role</param>
        public ListResponse UpdateRightsForRole(long editorPersonaId, int roleId, string roleName, List<ProductRightAcct> rightsToAdd, List<ProductRightAcct> rightsToRemove)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            int arrLength = rightsToAdd.Count + rightsToRemove.Count;
            RolePermission[] rolePermissions = new RolePermission[arrLength];
            RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[] user = new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[1] { new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            List<string> addedRights = new List<string>();
            List<string> removedRights = new List<string>();
            int i = 0;
            foreach (var item in rightsToAdd)
            {
                RolePermission rp = new RolePermission();

                rp.moduleid = item.ModuleID;
                rp.right = item.Right;
                rp.action = item.Action;
                //rp.roleid = roleId.ToString();
                rp.roleName = roleName;
                rp.value = "true";
                rolePermissions[i] = rp;
                i++;
                addedRights.Add(item.Description);
            }

            foreach (var item in rightsToRemove)
            {
                RolePermission rp = new RolePermission();

                rp.moduleid = item.ModuleID;
                rp.right = item.Right;
                rp.action = item.Action;
                //rp.roleid = roleId.ToString();
                rp.roleName = roleName;
                rp.value = "false";
                rolePermissions[i] = rp;
                i++;
                removedRights.Add(item.Description);
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "UpdateRightsForRole", $"_productUserId = {_productUserId}" });
            user[0].NameValuePair = loginInfo.ToArray();

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "rolePermissions", JsonConvert.SerializeObject(rolePermissions) } }, messageProperties: new object[] { "UpdateRightsForRole", "Before api" });
                NameValuePair[] output = _service.AssignRolePermissions(user, rolePermissions);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "output", JsonConvert.SerializeObject(output) } }, messageProperties: new object[] { "UpdateRightsForRole", "Result from api" });
                string error = string.Empty;
                bool isError = false;

                if (output[0].Value.IndexOf("fail") != -1)
                {
                    error = "Error - Unable to assign rights"; //output[1].Value;
                    isError = true;
                }

                response = new ListResponse()
                {
                    Records = output.Cast<object>().ToList(),
                    TotalRows = output.Length,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = error,
                    IsError = isError
                };

                if (!isError)
                {
                    if (rightsToAdd.Any() || rightsToRemove.Any())
                    {
                        UpdateRightsToRoleLogMessage(editorPersonaId, roleName, addedRights, removedRights);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRightsForRole", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        public void UpdateRightsToRoleLogMessage(long editorPersonaId, string roleName, List<string> rightsToAdd, List<string> rightsToRemove)
        {
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            if (rightsToAdd != null)
            {
                foreach (var right in rightsToAdd)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_ASSIGN.Replace("RightName", right) });
                }
            }
            if (rightsToRemove != null)
            {
                foreach (var right in rightsToRemove)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_UNASSIGN.Replace("RightName", right) });
                }
            }
            var message = "";
            message = impersonatorUserInfo != null
              ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed rights to {roleName} in Financial Suite."
            : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed rights to {roleName} in Financial Suite.";

            unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, 8);
        }

        /// <summary>
        /// Used to add/update a role in OneSite Accounting
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite Accounting user making the change.</param>       
        /// <param name="roleName"></param>        
        /// <returns></returns>
        public ListResponse CreateRole(long editorPersonaId, string roleName)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            NameValuePair[] input = new NameValuePair[1] { new NameValuePair() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            if (!String.IsNullOrEmpty(roleName))
            {
                loginInfo.Add(new NameValuePair { Name = "Name", Value = roleName });
                loginInfo.Add(new NameValuePair { Name = "Description", Value = "" });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "CreateRole", $"_productUserId = {_productUserId}" });
            input = loginInfo.ToArray();

            NameValuePair[] output;
            IList<ProductRightAcct> list;

            try
            {
                output = _service.CreateRole(input);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "output", JsonConvert.SerializeObject(output) } }, messageProperties: new object[] { "CreateRole", "Result from api" });
                string error = string.Empty;
                bool isError = false;


                if (output[0].Name.IndexOf("Error") != -1)
                {
                    error = output[0].Value;
                    isError = true;
                }

                response = new ListResponse()
                {
                    Records = output.Cast<object>().ToList(),
                    TotalRows = output.Length,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = error,
                    IsError = isError
                };
                if (!isError)
                {
                    ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                    unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, roleName, "ADD", "Financial Suite", null, 8);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateRole", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Used to Delete a role in Onesite accounting
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="roleId"></param>
        /// <returns></returns>
        public ListResponse DeleteRole(long editorPersonaId, long roleId, string roleName)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            NameValuePair[] input = new NameValuePair[1] { new NameValuePair() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            if (!String.IsNullOrEmpty(roleId.ToString()))
            {
                //loginInfo.Add(new NameValuePair { Name = "RoleID", Value = roleId.ToString() });
                loginInfo.Add(new NameValuePair { Name = "Name", Value = roleName });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "DeleteRole", $"_productUserId = {_productUserId}" });
            input = loginInfo.ToArray();

            NameValuePair[] output;
            IList<ProductRightAcct> list;

            try
            {
                output = _service.DeleteRole(input);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "output", JsonConvert.SerializeObject(output) } }, messageProperties: new object[] { "DeleteRole", "Result from api" });
                string error = string.Empty;
                bool isError = false;

                if (output[0].Name.IndexOf("Error") != -1)
                {
                    //error = output[0].Value;
                    error = "Role cannot be deleted because it is currently assigned to users";
                    isError = true;
                }

                response = new ListResponse()
                {
                    Records = output.Cast<object>().ToList(),
                    TotalRows = output.Length,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = error,
                    IsError = isError
                };
                if (!isError)
                {
                    ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                    unifiedLogin.DeleteRoleLogMessage(editorPersonaId, roleId, roleName, "Financial Suite", 8);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "DeleteRole", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Used to Clone a role in Onesite accounting
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="inheritedRoleName"></param>
        /// <param name="roleName"></param>        
        /// <returns></returns>
        public ListResponse CloneRole(long editorPersonaId, string roleName, string inheritedRoleName)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            NameValuePair[] input = new NameValuePair[1] { new NameValuePair() };

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            loginInfo.Add(new NameValuePair { Name = "NewName", Value = roleName });
            loginInfo.Add(new NameValuePair { Name = "Description", Value = "" });
            loginInfo.Add(new NameValuePair { Name = "Name", Value = inheritedRoleName });

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "loginInfo", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "CloneRole", $"_productUserId = {_productUserId}" });
            input = loginInfo.ToArray();

            NameValuePair[] output;
            IList<ProductRightAcct> list;

            try
            {
                output = _service.CreateRole(input);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "output", JsonConvert.SerializeObject(output) } }, messageProperties: new object[] { "CloneRole", "Result from api" });
                string error = string.Empty;
                bool isError = false;


                if (output[0].Name.IndexOf("Error") != -1)
                {
                    error = output[0].Value;
                    isError = true;
                }
                response = new ListResponse()
                {
                    Records = output.Cast<object>().ToList(),
                    TotalRows = output.Length,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = error,
                    IsError = isError
                };
                if (!isError)
                {
                    ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                    unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, roleName, "ADD", "Financial Suite", null, 8);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CloneRole", $"Error: {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        #endregion

        #region Private

        private List<int> GetProductIdsByOrg()
        {
            ProductRepository pr = new ProductRepository();
            return (List<int>)pr.GetProductIdsByCompany(_userClaims.OrganizationRealPageGuid);
        }

        /// <summary>
        /// Used to see if a new user login being added already exists or not
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        private bool CheckIfUserLoginIsUsed(long editorPersonaId, string userLogin)
        {
            ListResponse response = new ListResponse();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, 0);

            bool userExists = false;

            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword },
                new NameValuePair { Name = "UserID", Value = userLogin },
            };

            NameValuePair[] user = loginInfo.ToArray();
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "CheckIfUserLoginIsUsed", "Begin" });
            try
            {
                var result = _service.CheckIfUserIDIsUsed(user);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "result", JsonConvert.SerializeObject(result) } }, messageProperties: new object[] { "CheckIfUserLoginIsUsed", "Result from api" });
                if (result.ToUpper() == "YES")
                {
                    userExists = true;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CheckIfUserLoginIsUsed", $"Error: {ex.Message}" });
                throw ex;
            }
            return userExists;
        }

        /// <summary>
        /// Used to get information about the calling user and user being modified
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        private new ListResponse GetCompanyEditorAndUserDetails(long editorPersonaId, long userPersonaId)
        {
            ListResponse response = new ListResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyEditorAndUserDetails", "Begin" });
            response = verifyPersona(editorPersonaId);
            if (response.IsError)
            {
                return response;
            }

            // get the editors persona from the result
            _editorPersona = response.Records[0] as Persona;

            _companyName = GetAccountingCompanyFromPersona(_editorPersona);
            if (string.IsNullOrEmpty(_companyName))
            {
                response.IsError = true;
                response.ErrorReason = "Missing company name";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyEditorAndUserDetails", $"Missing company name. _editorPersona={_editorPersona}" });
                return response;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyEditorAndUserDetails", $"CompanyName = {_companyName}" });
            if (userPersonaId != 0)
            {
                // verify the persona being changed belongs to the same company as the user making the changes
                Persona user = _managePersona.GetPersona(userPersonaId);
                if (user == null || user.Organization.PartyId != _editorPersona.Organization.PartyId)
                {
                    response.IsError = true;
                    response.ErrorReason = "Invalid user persona";
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyEditorAndUserDetails", $"Error invalid user persona. userPersonaId={userPersonaId}" });
                    return response;
                }
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productAttributes", productAttributes } }, messageProperties: new object[] { "GetCompanyEditorAndUserDetails", $"userPersonaId={userPersonaId}" });
                // the Accounting user making the change to the role, get the Company from the user
                if (productAttributes.Any(a => a.Name.ToUpper() == "PRODUCTUSERNAME"))
                {
                    _productUsername = (from a in productAttributes where a.Name.ToUpper() == "PRODUCTUSERNAME" select a.Value).FirstOrDefault().Replace(":", "|");
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyEditorAndUserDetails", $"userPersonaId={userPersonaId} _productUsername={_productUsername}" });
                }
                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    _productUserId = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault().Replace(":", "|");
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyEditorAndUserDetails", $"userPersonaId={userPersonaId} _productUserId={_productUserId}" });
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyEditorAndUserDetails", "Finished" });
            return response;
        }

        /// <summary>
        /// Get the Accounting CompanyName for the admin user
        /// </summary>
        /// <param name="persona"></param>
        /// <returns></returns>
        private string GetAccountingCompanyFromPersona(Persona persona)
        {
            string companyName = "";
            IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(persona.PersonaId, _productId);
            // the Accounting user making the change to the role, get the Company from the user
            string uniqueIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
            if (uniqueIdentifier == null)
            {
                // get the CompanyName from BlueBook because the user doesn't have the Company for Accounting yet
                //IList<CompanyMap> companyMap = _blueBook.GetCompanyMap(persona.Organization.BooksMasterId, BlueBookProductConstants.Accounting);
                IList<blueBook.CustomerCompanyMap> companyMap = _blueBook.GetCompanyMap(persona.Organization.RealPageId, persona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.FinancialSuite, domain: persona.Organization.OrganizationDomain.Name);

                if (companyMap != null && companyMap.Count > 0 && companyMap.First(a => a.Source.ToUpper() == BlueBookProductConstants.FinancialSuite) != null)
                {
                    companyName = companyMap.First(a => a.Source.ToUpper() == BlueBookProductConstants.FinancialSuite).CompanyInstanceSourceId;
                }
            }
            else
            {
                companyName = uniqueIdentifier.Split('|')[0];
            }
            return companyName;
        }

        private List<AdditionalParameters> GetPropertiesAdditionalParameters(List<string> propToAssign, List<string> propToRemove, List<ACProperty> currentPropertyList, List<ProductPropertyGroup> currentLocationGrpList, List<ACProperty> currentEntitiesList, bool isMConsolePMC)
        {
            List<AdditionalParameters> logs = new List<AdditionalParameters>();
            try
            {
                if (propToAssign.Count > 0)
                {
                    var assignedCurrentProps = new List<AdditionalParameters>();
                    if (isMConsolePMC)
                    {
                        var companiesData = propToAssign.Where(s => !s.Contains("|")).Distinct().ToList();
                        
                        foreach (var d in companiesData)
                        {
                            assignedCurrentProps.Add(new AdditionalParameters { Key = "Financial Suite Companies", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", d) });
                        }
                    }
                    else
                    {
                        assignedCurrentProps.AddRange(currentLocationGrpList
                        .Where(f => propToAssign.Contains(f.ID))
                        .Select(f => new AdditionalParameters { Key = "Financial Suite Location Groups", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.Name) })
                        .ToList());
                    }

                    //currentPropertyList
                    //    .Where(f => companiesData.Contains(f.CompanyId))
                    //    .Select(f => new AdditionalParameters { Key = "Financial Suite Companies", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.CompanyName) })
                    //    .Distinct()
                    //    .ToList();
                    if (isMConsolePMC)
                    {
                        assignedCurrentProps.AddRange(currentEntitiesList
                        .Where(f => propToAssign.Contains(f.MConsoleId))
                        .Select(f => new AdditionalParameters { Key = "Financial Suite Entities", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.MConsoleId) })
                        .ToList());
                    }
                    else
                    {
                        assignedCurrentProps.AddRange(currentEntitiesList
                        .Where(f => propToAssign.Contains(f.PropertyId))
                        .Select(f => new AdditionalParameters { Key = "Financial Suite Entities", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.PropertyId) })
                        .ToList());
                    }
                    if (assignedCurrentProps.Count > 0)
                    {
                        logs.AddRange(assignedCurrentProps);
                    }
                }
                if (propToRemove.Count > 0)
                {
                    //var removedCurrentProps = currentPropertyList
                    //    .Where(f => propToRemove.Contains(f.CompanyId))
                    //    .Select(f => new AdditionalParameters { Key = "Financial Suite Companies", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", f.PropertyName) })
                    //    .ToList();
                    var removedCurrentProps = new List<AdditionalParameters>();

                    if(isMConsolePMC)
                    {
                        var companiesData = propToRemove.Where(s => !s.Contains("|")).Distinct().ToList();
                        foreach (var d in companiesData)
                        {
                            removedCurrentProps.Add(new AdditionalParameters { Key = "Financial Suite Companies", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", d) });
                        }
                    }
                    else
                    {
                        removedCurrentProps.AddRange(currentLocationGrpList
                        .Where(f => propToRemove.Contains(f.ID))
                        .Select(f => new AdditionalParameters { Key = "Financial Suite Location Groups", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", f.Name) })
                        .ToList());
                    }

                    if (isMConsolePMC)
                    {
                        removedCurrentProps.AddRange(currentEntitiesList
                            .Where(f => propToRemove.Contains(f.MConsoleId))
                            .Select(f => new AdditionalParameters { Key = "Financial Suite Entities", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", f.MConsoleId) })
                            .ToList());
                    }
                    else
                    {
                        removedCurrentProps.AddRange(currentEntitiesList
                            .Where(f => propToRemove.Contains(f.PropertyId))
                            .Select(f => new AdditionalParameters { Key = "Financial Suite Entities", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", f.PropertyId) })
                            .ToList());
                    }
                    if (removedCurrentProps.Count > 0)
                    {
                        logs.AddRange(removedCurrentProps);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetPropertiesAdditionalParameters", $"Error when evaluating additional parameters: {ex.Message}" });
            }
            return logs;
        }

        #endregion

        #region Migration
        /// <summary>
        /// Get List of Accounting Users for Migration 
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
            var claimResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError) { response.ErrorReason = claimResponse.ErrorReason; return response; }

            var userInfo = new Component.SharedObjects.Product.OneSiteAccounting.User[1];
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword },
                new NameValuePair { Name = "OneTimeExport", Value = "true" }
            };
            if (!string.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            userInfo[0] = new Component.SharedObjects.Product.OneSiteAccounting.User()
            {
                NameValuePair = loginInfo.ToArray()
            };

            var filter = true;
            var startRow = 0;
            var resultPerPage = 1000;
            if (datafilter != null)
            {
                if (datafilter.FilterBy.ContainsKey("filter"))
                {
                    filter = datafilter.FilterBy["filter"].ToUpper() == "NONMIGRATED";
                }
                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerPage = datafilter.Pages.ResultsPerPage;
                }
            }
            FilterSortParameters wsParams = new FilterSortParameters
            {
                StartPosition = startRow.ToString(),
                PageLength = resultPerPage.ToString(),
                FilterConditionList = new FilterConditionList[]
                {
                    new FilterConditionList(){
                        LogicalOperator = "OR",
                        FilterCondition = new FilterCondition[]
                        {
                            new FilterCondition()
                            {
                                PropertyName = "excludeassign",
                                ComparisionOperator = "equalto",
                                SearchValue = filter ? "1" : "0"
                            }
                        }
                    }
                }
            };
            WriteToDiagnosticLog("{ActionName} - {state}", new Dictionary<string, object> { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "GetMigrationUsers", "GetAllUsers" });
            TotalRows[] results2 = new TotalRows[1];
            var users = _service.GetAllUsers(userInfo, wsParams, out results2);
            var totalRow = results2.FirstOrDefault() ?? new TotalRows() { TotalRows1 = "0" };

            if (users == null)
            {
                if (totalRow != null && totalRow.TotalRows1.ToUpper().Contains("NOT A VALID USERID"))
                {
                    response.ErrorReason = "Invalid user.";
                }
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error: {response.ErrorReason} , editorPersona id - {editorPersonaId}." });
                return response;
            }
            var migrationUsers = new List<MigrationUser>();
            foreach (var user in users)
            {
                var migrationUser = new MigrationUser
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.UserID,
                    UserId = user.UserID,
                    Email = user.EmailAddress,
                    CompanyInstanceSourceId = _companyName,
                    Title = user.TITLE,
                    MiddleName = user.MIDDLENAME,
                    LastActivity = user.LASTACCESSDATE,
                    Phone = user.PHONENUMBER,
                    Status = user.USERSTATUS == "F" ? "Disabled" : "Active"
                };
                migrationUsers.Add(migrationUser);
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}. migrationUsers.Count {migrationUsers.Count}" });

            response.RowsPerPage = Convert.ToInt32(wsParams.PageLength);
            response.ErrorReason = string.Empty;
            response.IsError = false;
            response.TotalPages = 1;
            response.Records = migrationUsers.Cast<object>().ToList();
            response.TotalRows = string.IsNullOrEmpty(totalRow.TotalRows1) ? 0 : Convert.ToInt32(totalRow.TotalRows1);
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

            var claimResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError) { migrateResponse.Message = claimResponse.ErrorReason; return migrateResponse; }

            foreach (var migateUser in migrateUsers)
            {
                var loginInfo = new NameValuePair[4]
                {
                    new NameValuePair { Name = "CompanyID", Value = _companyName },
                    new NameValuePair { Name = "Login", Value = _intactLogin },
                    new NameValuePair { Name = "Password", Value = _intactPassword },
                    new NameValuePair { Name = "SystemIdentifier", Value = $"{_companyName}|{migateUser.UserId}" }
                };
                var message = "";
                if (migateUser.UsingUnifiedLogin)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", new Dictionary<string, object> { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "UpdateUsersMigrationStatus", "EnableGreenBookUser" });
                    message += _service.EnableGreenBookUser(loginInfo);
                    WriteToDiagnosticLog("{ActionName} - {state}", new Dictionary<string, object> { { "Message", message } }, messageProperties: new object[] { "UpdateUsersMigrationStatus", "EnableGreenBookUser result" });
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", new Dictionary<string, object> { { "user", RemovePrivateData(loginInfo.ToArray()) } }, messageProperties: new object[] { "UpdateUsersMigrationStatus", "DisableGreenBookUser" });
                    message += _service.DisableGreenBookUser(loginInfo);
                    WriteToDiagnosticLog("{ActionName} - {state}", new Dictionary<string, object> { { "Message", message } }, messageProperties: new object[] { "UpdateUsersMigrationStatus", "DisableGreenBookUser result" });
                }
                migrateResponse.Message += message;
            }
            migrateResponse.Status = true;
            return migrateResponse;
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disables the Accounting Product user
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, string userName, bool isActive = false)
        {
            ListResponse listResponse = new ListResponse();
            var result = "";
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError) { return false; }

            string companyInstanceSourceId = GetAccountingCompanyFromPersona(_editorPersona);
            if (string.IsNullOrEmpty(companyInstanceSourceId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                return false;
            }

            List<NameValuePair> parameters = new List<NameValuePair>{
                    new NameValuePair { Name = "CompanyID", Value = companyInstanceSourceId },
                    new NameValuePair { Name = "Login", Value = _intactLogin },
                    new NameValuePair { Name = "Password", Value = _intactPassword },
                    new NameValuePair { Name = "SystemIdentifier", Value = userName}
            };

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "parameters", RemovePrivateData(parameters.ToArray()) } }, messageProperties: new object[] { "ChangeUserStatus", $"Updating user status for user = {companyInstanceSourceId}|{userName}, isActive = {isActive}" });

                if (isActive)
                    result = _service.EnableUser(parameters.ToArray());
                else
                    result = _service.DisableUser(parameters.ToArray());

                if (result.Trim().ToUpper().Contains("INACTIVATED"))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Enable/Disable success userName:{userName}" });
                    return true;
                }

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Enable/Disable failed userName:{userName}" });
                return false;

            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {companyInstanceSourceId}|{userName} by editorPersonaId = {editorPersonaId}. Error: {ex.Message}" });
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Used to build the XML required to call the OneSite web services
    /// </summary>
    public static class ManageProductOneSiteAccountingHelpers
    {
        /// <summary>
        /// Used to convert a OneSite Accounting property into a GreenBook property to be used by the UI
        /// </summary>
        /// <param name="properties">The list of properties to convert</param>
        /// <returns></returns>
        public static IList<ProductProperty> ToGBProperties(this LocationID[] properties)
        {
            if (properties == null) return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (LocationID loc in properties)
            {
                results.Add(new ProductProperty
                {
                    Name = loc.Name,
                    ID = loc.LocationID1,
                    Street1 = loc.Address1,
                    Street2 = loc.Address2,
                    City = loc.City,
                    State = loc.State,
                    Zip = loc.Zip,
                    IsAssigned = (loc.Assigned.ToLower() == "true" ? true : false)
                });
            }
            return (from property in results orderby property.ID, property.Name select property).ToList();
        }

        /// <summary>
        /// Used to convert a OneSite Accounting property into a GreenBook property to be used by the UI
        /// </summary>
        /// <param name="properties">The list of properties to convert</param>
        /// <returns></returns>
        public static IList<ProductProperty> ToGBProperties(this List<ACProperty> properties)
        {
            if (properties == null) return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (ACProperty prop in properties)
            {
                results.Add(new ProductProperty
                {
                    Name = prop.PropertyName,
                    ID = prop.Id,
                    IsAssigned = prop.IsAssigned
                });
            }
            return results.OrderBy(x => x.ID).ThenBy(x => x.Name).ToList();
        }

        /// <summary>
        /// Used to convert a OneSite Accounting property group into a GreenBook property group to be used by the UI
        /// </summary>
        /// <param name="propertyGroups">The list of propertyGroups to convert</param>
        /// <returns></returns>
        public static List<ProductPropertyGroup> ToGBPropertyGroup(this LocationGroupID[] propertyGroups)
        {
            if (propertyGroups == null) return null;
            IList<ProductPropertyGroup> results = new List<ProductPropertyGroup>();
            foreach (LocationGroupID loc in propertyGroups)
            {
                results.Add(new ProductPropertyGroup
                {
                    Name = loc.Name,
                    ID = loc.ID,
                    IsAssigned = (loc.Assigned.ToLower() == "true" ? true : false),
                    AssignedProperties = loc.Memberids.Split(',').ToList()
                });
            }
            return (from propertyGroup in results orderby propertyGroup.ID, propertyGroup.Name, propertyGroup.AssignedProperties select propertyGroup).ToList();
        }

        /// <summary>
        /// Used to convert a OneSite Accounting role into a GreenBook role to be used by the UI
        /// </summary>
        /// <param name="roles">The list of roles to convert</param>
        /// <returns></returns>
        public static IList<ProductRole> ToGBRoles(this RoleName[] roles)
        {
            if (roles == null) return null;
            IList<ProductRole> results = new List<ProductRole>();
            foreach (RoleName role in roles)
            {

                results.Add(new ProductRole
                {
                    ID = role.Recordno,
                    Name = role.Name,
                    Description = role.Description,
                    IsAssigned = (role.Assigned.ToLower() == "true" ? true : false)
                });
            }
            return (from role in results orderby role.Name select role).ToList();
        }

        /// <summary>
        /// Used to convert a OneSite Accounting right into a GreenBook right to be used by the UI
        /// </summary>
        /// <param name="permissions">The list of rights to convert</param>
        /// <returns></returns>
        public static IList<ProductRightAcct> ToRights(this PermissionID[] permissions)
        {
            if (permissions == null) return null;
            IList<ProductRightAcct> results = new List<ProductRightAcct>();
            int i = 1;
            char[] c = new char[1];
            c[0] = '|';
            foreach (PermissionID permission in permissions)
            {
                try
                {
                    if (permission.action.Trim().ToUpper() != "NONE")
                    {
                        results.Add(new ProductRightAcct
                        {
                            ID = i,//int.Parse(permission.rightID), // due to duplicate rightIds from Accounting
                            RightID = permission.rightID.Length == 0 ? 0 : int.Parse(permission.rightID),
                            Alias = permission.right + " - " + permission.actionLabel,
                            CenterName = permission.application,
                            Description = permission.right + " - " + permission.actionLabel,
                            RolesAssigned = permission.roles.Trim().Length == 0 ? 0 : permission.roles.Split(c).Length,
                            Assigned = permission.value.ToUpper().Trim() == "TRUE" ? true : false,
                            ModuleID = permission.moduleID.Length == 0 ? GetApplicationModuleID(permission.application) : permission.moduleID,
                            Action = permission.action,
                            Right = permission.right,
                            ActionLabel = permission.actionLabel

                        }

                        );
                        i++;
                    }
                }
                catch { }
            }
            return results;
        }

        /// <summary>
		/// Used to convert a OneSite Accounting property into a GreenBook property to be used by the UI
		/// </summary>
		/// <param name="companies">The list of properties to convert</param>
		/// <returns></returns>
		public static List<ACCompany> ToGBCompanies(this CompanyID[] companies)
        {
            if (companies == null) return null;
            List<ACCompany> results = new List<ACCompany>();
            foreach (CompanyID cmp in companies)
            {
                results.Add(new ACCompany
                {
                    Id = cmp.CompanyID1,
                    Name = cmp.CompanyName,
                    isAssigned = cmp.Assigned == string.Empty ? false : bool.Parse(cmp.Assigned)
                });
            }
            return results;
        }

        /// <summary>
        /// Used to convert a OneSite Accounting property into a GreenBook property to be used by the UI
        /// </summary>
        /// <param name="enteties">The list of properties to convert</param>
        /// <returns></returns>
        public static List<ACProperty> ToGBEnteties(this EntityID[] enteties)
        {
            if (enteties == null) return null;
            List<ACProperty> results = new List<ACProperty>();
            foreach (EntityID loc in enteties)
            {
                results.Add(new ACProperty
                {
                    Id = loc.EntityID1,
                    PropertyId = loc.EntityID1,
                    PropertyName = loc.EntityName,
                    CompanyId = loc.CompanyID,
                    CompanyName = loc.CompanyName,
                    IsAssigned = loc.Assigned == string.Empty ? false : bool.Parse(loc.Assigned),
                    MConsoleId = loc.MConsoleEntityID,
                    IsCompanyAssigned = loc.Assigned == string.Empty ? false : (bool.Parse(loc.Assigned) == true && loc.EntityID1 == string.Empty) ? true : false,
                    BookID = loc.BookID

                });
            }
            return results;
        }

        /// <summary>
        /// Get the application moduleID
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private static string GetApplicationModuleID(string application)
        {
            string moduleId = "";
            switch (application.Trim().ToUpper())
            {
                case "ACCOUNTS PAYABLE":
                    moduleId = "3.AP";
                    break;
                case "ACCOUNTS RECEIVABLE":
                    moduleId = "4.AR";
                    break;
                case "CASH MANAGEMENT":
                    moduleId = "11.CM";
                    break;
                case "ASSURANCE":
                    moduleId = "19.AS";
                    break;
                case "INTACCT-OPENAIR INTEGRATION":
                    moduleId = "22.AIR";
                    break;
                case "MY PRACTICE":
                    moduleId = "13.PR";
                    break;
                case "PAYROLL":
                    moduleId = "20.CBS";
                    break;
                case "MY ACCOUNTING":
                    moduleId = "14.ACCT";
                    break;
                case "CONSOLE":
                    moduleId = "15.CPA";
                    break;
                case "401(K)":
                    moduleId = "29.DEC";
                    break;
                case "IMPORT EXPORT":
                    moduleId = "32.IE";
                    break;
                case "TIME & BILLING":
                    moduleId = "23.TB";
                    break;
                case "COMPANY":
                    moduleId = "1.CO";
                    break;
                case "CONSOLIDATION":
                    moduleId = "10.CS";
                    break;
                case "GENERAL LEDGER":
                    moduleId = "2.GL";
                    break;
                case "INVENTORY CONTROL":
                    moduleId = "7.INV";
                    break;
                case "INVENTORY":
                    moduleId = "7.INV";
                    break;
                case "PLATFORM SERVICES":
                    moduleId = "39.CERP";
                    break;
                case "PROJECTS":
                    moduleId = "48.PROJACCT";
                    break;
                case "PURCHASING":
                    moduleId = "9.PO";
                    break;
                case "REVENUE MANAGEMENT":
                    moduleId = "54.REVREC";
                    break;
                case "TIME & EXPENSES":
                    moduleId = "6.EE";
                    break;
                case "BUSINESS DEVELOPMENT":
                    moduleId = "24.BD";
                    break;
                case "CLIENT EXPENSES":
                    moduleId = "21.CE";
                    break;
                case "TAX EXPORT":
                    moduleId = "17.TX";
                    break;
                case "ORDER ENTRY":
                    moduleId = "8.SO";
                    break;
                case "MANAGEMENT CONSOLE":
                    moduleId = "18.MPRAC";
                    break;
                case "FINANCIAL ANALYSIS":
                    moduleId = "30.LT";
                    break;
                case "MY CLIENTS":
                    moduleId = "16.CL";
                    break;
                case "PAYROLL - ADP":
                    moduleId = "31.ADP";
                    break;
                case "WEB SERVICES":
                    moduleId = "33.XDA";
                    break;
                case "WRITEUP APPLICATION":
                    moduleId = "34.WU";
                    break;
                case "WRITE-UP":
                    moduleId = "36.WUPACK";
                    break;
                case "MULTI ENTITY CONSOLE":
                    moduleId = "37.ME";
                    break;
                case "INTACCT-SALESFORCE INTEGRATION":
                    moduleId = "38.SFDC";
                    break;
                case "PAYMENT SERVICES":
                    moduleId = "40.CCP";
                    break;
                case "WELLS FARGO PAYMENT MANAGER":
                    moduleId = "42.WFPM";
                    break;
                case "INTACCT-QUICKARROW INTEGRATION":
                    moduleId = "44.QARROW";
                    break;
                case "GLOBAL CONSOLIDATIONS":
                    moduleId = "45.ATLAS";
                    break;
                case "INTACCT-CLARIZEN INTEGRATION":
                    moduleId = "49.CLARIZEN";
                    break;
                case "AVALARA TAX":
                    moduleId = "43.AVA";
                    break;
                case "ONESITE ACCOUNTS RECEIVABLE":
                    moduleId = "51.OAR";
                    break;
                case "QUICKBOOKS MIGRATION":
                    moduleId = "47.QB";
                    break;
                case "INTACCT COLLABORATE":
                    moduleId = "53.CHAT";
                    break;
                case "EXTERNAL SERVICES PROVIDER":
                    moduleId = "56.ESP";
                    break;
                case "CONTRACT MODULE":
                    moduleId = "55.CONTRACT";
                    break;
                case "VENDOR PAYMENT SERVICES":
                    moduleId = "52.OPYMTS";
                    break;
                case "INTACCT-ZUORA INTEGRATION":
                    moduleId = "50.ZUORA";
                    break;
                case "DATA DELIVERY SERVICE":
                    moduleId = "51.DDS";
                    break;
                case "ADVANCED CRM INTEGRATION":
                    moduleId = "61.SFDC2";
                    break;
                case "PROPERTY MANAGEMENT":
                    moduleId = "99.PM";
                    break;
                case "REPORTING":
                    moduleId = "59.CRW";
                    break;
                case "DIGITAL BOARD BOOK":
                    moduleId = "57.DBB";
                    break;
                case "SPEND MANAGEMENT":
                    moduleId = "58.SC";
                    break;
                case "ADMINISTRATION":
                    moduleId = "0.ADMIN";
                    break;
                default:
                    break;
            }
            return moduleId;
        }

        /// <summary>
        /// Used to convert a OneSite Accounting role into a GreenBook role to be used by the UI
        /// </summary>
        /// <param name="permissions">The list of roles to convert</param>
        /// <returns></returns>
        public static List<ProductRole> ToRoles(this PermissionID[] permissions)
        {
            if (permissions == null) return null;
            List<ProductRole> results = new List<ProductRole>();

            foreach (PermissionID permission in permissions)
            {
                try
                {
                    roleDetails(ref results, permission);
                }
                catch (Exception ex)
                {
                    var response = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = ex.Message
                    };
                }

            }
            return results;
        }

        /// <summary>
        /// Used to convert a OneSite Accounting role into a role list to be used by the UI
        /// </summary>
        /// <param name="permissions">The list of roles to convert</param>
        /// <returns></returns>
        public static List<ProductRole> ToRolesList(this PermissionuID[] permissions)
        {
            if (permissions == null) return null;
            List<ProductRole> results = new List<ProductRole>();
            int i = 1;
            foreach (PermissionuID permission in permissions)
            {
                try
                {
                    ProductRole r = new ProductRole();
                    r.IsAssigned = bool.Parse(permission.assigned);
                    r.Name = permission.roleName;
                    r.Roletype = "Custom";
                    //r.ID = permission.roleID;
                    r.ID = i.ToString();
                    results.Add(r);
                }
                catch (Exception ex)
                {
                    var response = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = ex.Message
                    };
                }
                i++;

            }
            return results;
        }

        public static void roleDetails(ref List<ProductRole> results, PermissionID permission)
        {

            char[] c = new char[1];
            c[0] = '|';

            if (!String.IsNullOrEmpty(permission.roles))
            {
                //string[] roles = permission.roles.Split(c);
                var roles = permission.roles.Split(c).Distinct(); // Accounting sending duplicate roles sometimes

                foreach (var item in roles)
                {
                    string[] x = new string[1];
                    x[0] = "@@";
                    string[] nameId = item.Split(x, StringSplitOptions.None);

                    ProductRole pr = results.FirstOrDefault(p => int.Parse(p.ID) == int.Parse(nameId[0]));

                    if (pr == null)
                    {
                        ProductRole r = new ProductRole();

                        r.Name = nameId[1];
                        r.RightsAssigned = permission.action.Trim().ToUpper() == "NONE" ? "0" : "1";
                        r.Roletype = "Custom";
                        r.ID = nameId[0];
                        results.Add(r);
                    }
                    else
                    {
                        pr.RightsAssigned = (int.Parse(pr.RightsAssigned) + 1).ToString();
                    }
                }
            }

        }

        /// <summary>
        /// Used to convert a OneSite Accounting applications  to be used by the UI
        /// </summary>
        /// <param name="applications">The list of applications to convert</param>
        /// <returns></returns>
        public static string[] ToCenters(this ApplicationID[] applications)
        {
            string[] results = new string[applications.Length];
            int i = 0;
            foreach (ApplicationID app in applications)
            {
                results.SetValue(app.Name, i);
                i++;
            }

            return results;
        }

        /// <summary>
        /// Used to convert a OneSite Accounting user into a GreenBook user to be used by the UI
        /// </summary>
        /// <param name="users">The list of users to convert</param>
        /// <returns></returns>
        public static IList<Component.SharedObjects.Product.ProductUser> ToGBUsers(this Component.SharedObjects.Product.OneSiteAccounting.User[] users)
        {
            if (users == null) return null;
            IList<Component.SharedObjects.Product.ProductUser> results = new List<Component.SharedObjects.Product.ProductUser>();
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datafilter"></param>
        /// <param name="defaultFieldToSort"></param>
        /// <param name="start"></param>
        /// <param name="pageLength"></param>
        /// <param name="excludeAssigned"></param>
        /// <returns></returns>
        public static FilterSortParameters GenerateSearchAndPaging(RequestParameter datafilter, string defaultFieldToSort, int start, int pageLength, bool excludeAssigned = false)
        {
            //handle pagination
            FilterSortParameters wsParams = new FilterSortParameters { StartPosition = start.ToString(), PageLength = pageLength.ToString() };

            // nothing to filter
            if (datafilter == null) { datafilter = new RequestParameter(); }

            //handle sorting
            if (datafilter.SortBy.Count == 0)
            {
                datafilter = new RequestParameter() { SortBy = new Dictionary<string, string>() };
                datafilter.SortBy.Add("name", "asc");
            }

            SortConditionList sortList = new SortConditionList();
            if (datafilter.SortBy != null && datafilter.SortBy.Count > 0)
            {
                SortCondition[] scList = (from a in datafilter.SortBy.ToList() select new SortCondition { ColumnName = a.Key, SortDirection = a.Value }).ToArray();
                sortList.SortCondition = scList;
                wsParams.SortConditionList = new SortConditionList[] { sortList };
            }

            List<FilterCondition> filterCondition = new List<FilterCondition>();
            filterCondition.Add(new FilterCondition() { PropertyName = "excludeassign", ComparisionOperator = "equalto", SearchValue = (excludeAssigned == true ? "1" : "0") });
            FilterConditionList filterList = new FilterConditionList();
            if (datafilter.FilterBy != null && datafilter.FilterBy.Count > 0)
            {
                filterList.LogicalOperator = "OR";
                filterCondition.AddRange((from a in datafilter.FilterBy.ToList() select new FilterCondition { PropertyName = a.Key, SearchValue = a.Value, ComparisionOperator = "OR" }));
            }
            filterList.FilterCondition = filterCondition.ToArray();
            wsParams.FilterConditionList = new FilterConditionList[] { filterList };
            return wsParams;
        }
    }
}
