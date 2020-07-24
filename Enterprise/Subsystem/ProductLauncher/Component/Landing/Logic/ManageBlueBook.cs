using JsonApiSerializer;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage BlueBook APIs
    /// </summary>
    public class ManageBlueBook : IDisposable, IManageBlueBook
    {
        private DefaultUserClaim _defaultUserClaim;

        const string MediaTypeName = "application/vnd.api+json";
        const int CacheTimeSeconds = 300;
        const int AuthTokenRefreshMinutes = 50;

        const int MAXRETRYCOUNT = 5;

        readonly HttpClient _httpClient;
        readonly IList<ProductInternalSetting> productInternalSettingList;
        readonly IProductInternalSettingRepository _productInternalSettingRepository;

        readonly AuthTokenData _authTokenInfo = new AuthTokenData();

        private bool useDomains = false;
        private bool useUPFMId = false;

        ObjectCache _manageBlueBookCache = MemoryCache.Default;

        /// <summary>
        /// Default constructor
        /// </summary>
        [Obsolete]
        public ManageBlueBook()
        {
            string bbUri = "";

            #region GetSettings

            productInternalSettingList = _manageBlueBookCache["productInternalSetting_" + (int)ProductEnum.UnifiedPlatform] as List<ProductInternalSetting>;
            if (productInternalSettingList == null)
            {
                _productInternalSettingRepository = new ProductInternalSettingRepository();
                productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                _manageBlueBookCache.Set("productInternalSetting_" + (int)ProductEnum.UnifiedPlatform, productInternalSettingList, policy);
            }

            #endregion

            bbUri = productInternalSettingList.First(a => a.Name.Equals("BlueBookAPIEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)))
            {
				useDomains = Convert.ToBoolean(int.Parse(productInternalSettingList.First(a => a.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)).Value));
            }

            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase)))
            {
				useUPFMId = Convert.ToBoolean(int.Parse(productInternalSettingList.First(a => a.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase)).Value));
            }

            //_authTokenInfo.Data.Name = "OS";//productInternalSettingList.First(a => a.Name.ToUpper() == "BLUEBOOKAPIUSER").Value;
            //_authTokenInfo.Data.Password = "P>qx3g6MEkt(G:-";//productInternalSettingList.First(a => a.Name.ToUpper() == "BLUEBOOKAPIPASSWORD").Value;
            
            _httpClient = new HttpClient {BaseAddress = new Uri(bbUri)};
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ManageBlueBook(DefaultUserClaim defaultUserClaim)
        {
            _defaultUserClaim = defaultUserClaim;
            string bbUri = "";

            #region GetSettings

            productInternalSettingList = _manageBlueBookCache["productInternalSetting_" + (int)ProductEnum.UnifiedPlatform] as List<ProductInternalSetting>;
            if (productInternalSettingList == null)
            {
                _productInternalSettingRepository = new ProductInternalSettingRepository();
                productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                _manageBlueBookCache.Set("productInternalSetting_" + (int)ProductEnum.UnifiedPlatform, productInternalSettingList, policy);
            }

            #endregion

            bbUri = productInternalSettingList.First(a => a.Name.Equals("BlueBookAPIEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)))
            {
                useDomains = Convert.ToBoolean(int.Parse(productInternalSettingList.First(a => a.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)).Value));
            }

            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase)))
            {
                useUPFMId = Convert.ToBoolean(int.Parse(productInternalSettingList.First(a => a.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase)).Value));
            }
            
            //bbUri = "https://booksapi.realpage.com";
            //_authTokenInfo.Data.Name = "OS";//productInternalSettingList.First(a => a.Name.ToUpper() == "BLUEBOOKAPIUSER").Value;
            //_authTokenInfo.Data.Password = "P>qx3g6MEkt(G:-";//productInternalSettingList.First(a => a.Name.ToUpper() == "BLUEBOOKAPIPASSWORD").Value;
            _httpClient = new HttpClient {BaseAddress = new Uri(bbUri)};
        }

        public ManageBlueBook(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, HttpMessageHandler messageHandler)
        {
            _productInternalSettingRepository = productInternalSettingRepository;
            _httpClient = new HttpClient(messageHandler) {BaseAddress = new Uri("http://localhost")};
            _defaultUserClaim = userClaim;

            productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                
            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)))
            {
                useDomains = Convert.ToBoolean(int.Parse(productInternalSettingList.First(a => a.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)).Value));
            }

            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase)))
            {
                useUPFMId = Convert.ToBoolean(int.Parse(productInternalSettingList.First(a => a.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase)).Value));
            }
        }

        /// <summary>
        /// Filter Company Map
        /// </summary>
        /// <param name="companyRealPageId">The guid for the company</param>
        /// <param name="booksCompanyMasterId">Master Company Id - RPUP</param>
        /// <param name="domain">The domain to query for the company</param>
        /// <returns>List of CompanyMapResource</returns>
        public IList<CustomerCompanyMap> GetCompanyMap(Guid companyRealPageId, long booksCompanyMasterId, string domain)
        {
            return GetCompanyMap(companyRealPageId, booksCompanyMasterId, source: null, domain: domain);
        }

        /// <summary>
        /// Used to get the information about the given company from the BlackBook system
        /// </summary>
        /// <param name="companyRealPageId">The guid for the company</param>
        /// <param name="booksCompanyMasterId">Master Company Id</param>
        /// <param name="source">A filter on source if given</param>
        /// <param name="domain">The domain to query for the company</param>
        /// <param name="includeExtra">Extra Uri Includes (Optional)</param>
        /// <param name="includeGreenBookCares">Filter result using greenbook cares flag</param>
        /// <param name="useTranslate"></param>
        /// <returns>List of CompanyMapResource</returns>
        public IList<CustomerCompanyMap> GetCompanyMap(Guid companyRealPageId, long booksCompanyMasterId, string source, string domain, string includeExtra = "", bool includeGreenBookCares = true, bool useTranslate = true)
        {
            if (booksCompanyMasterId == -1 || companyRealPageId == DefaultUserClaim.EmployeeCompanyRealPageId)
            {
                // shortcut out for RealPage Employee company
                return null;
            }
            IList<CustomerCompanyMap> companyMap = new List<CustomerCompanyMap>();

            if (useTranslate && useUPFMId && companyRealPageId != Guid.Empty && string.IsNullOrEmpty(includeExtra) && !string.IsNullOrEmpty(source) && !source.Equals(ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)))
            {
                companyMap = GetTranslateFromUPFMToProduct(companyRealPageId.ToString().ToUpper(), source, domain);
                if (companyMap != null)
                {
                    return companyMap;
                }
            }

            if (useUPFMId && companyRealPageId != Guid.Empty)
            {
                // need to send guid in uppercase because books is case sensitive.
                var newCompanyMasterId = GetCompanyMasterIdForRPDMID(companyRealPageId.ToString().ToUpper(), domain);
                booksCompanyMasterId = (newCompanyMasterId != 0) ? newCompanyMasterId : booksCompanyMasterId;
            }
            
            if (source == null)
            {
                source = "";
            }

            string domainFilter = "";

            if (!string.IsNullOrEmpty(domain) && useDomains)
            {
                //domainFilter = $"filter[companyInstance.customerEnvironment]={domain}&";
                domainFilter = $"filter[companyInstance.domain]={domain}&";
            }

            string companyFilter = $"filter[customerCompanyId]={booksCompanyMasterId}&";

            companyMap = _manageBlueBookCache[$"getCompanyMapResource_{booksCompanyMasterId}_{source}_{includeExtra}_{domain}"] as List<CustomerCompanyMap>;

            if (companyMap == null)
            {
                string uri = $"customercompanymap?" + (includeGreenBookCares ? "filter[companyInstance.greenBookCares]=true&" : "") + domainFilter + companyFilter + $"include=companyInstance&include=companyInstance.attributes";
                if (!string.IsNullOrEmpty(source))
                {
                    uri += "&filter[source]=" + source;
                }

                if (!string.IsNullOrWhiteSpace(includeExtra))
                {
                    uri += "&include=" + includeExtra;
                }

                var logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogType.Diagnostic, "GetCompanyMap - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    companyMap = JsonConvert.DeserializeObject<List<CustomerCompanyMap>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() {{"companyMap", companyMap}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyMap - Got info.", logData);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getCompanyMapResource_{booksCompanyMasterId}_{source}_{includeExtra}_{domain}", companyMap, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyMap - No info found.", logData);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);
                    }

                    return null;
                }
            }

            return companyMap;
        }

        /// <summary>
        /// Get all instances related to the given UPFM instance source. Filtering on the given domain
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="productSource"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        private IList<CustomerCompanyMap> GetTranslateFromUPFMToProduct(string companyRealPageId, string productSource, string domain)
        {
            //translate/companyinstance/684382D3-F2F8-4F42-8D29-935F834C6888/UPFM/OS?filter[customerEnvironment]=Primary
            string uri = $"translate/companyinstance/{companyRealPageId}/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}?filter[customerEnvironment]={domain}";

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"GetTranslateFromUPFMToProduct_{companyRealPageId}_{productSource}_{domain}";
            List<CustomerCompanyMap> booksCustomerMaster = rpcache.GetFromCache<List<CustomerCompanyMap>>(cacheKey, 180, () =>
            {
                List<CustomerCompanyMap> companyListCache = new List<CustomerCompanyMap>();
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var translateCompanyInstance = JsonConvert.DeserializeObject<TranslateCompanyInstance>(response.Content.ReadAsStringAsync().Result);
                    Dictionary<string, object> logData = new Dictionary<string, object>() {{"response", translateCompanyInstance}, {"uri", uri}, {"productSource", productSource}, {"domain", domain}};
                    WriteToLog(LogType.Diagnostic, $"GetTranslateFromUPFMToProduct - Got info. {productSource}/{domain}", logData);
                    CustomerCompanyMap map = new CustomerCompanyMap(){ CompanyInstance = new List<CompanyInstance>()};
                    map.CompanyInstanceSourceId = translateCompanyInstance.Data.Attributes.TranslatedCompanyInstances[0].CompanyInstanceSourceId;
                    map.Source = productSource;
                    companyListCache.Add(map);
                    return companyListCache;
                }

                return null;
            });

            return booksCustomerMaster;
        }

        /// <summary>
        /// Get all instances related to the given UPFM instance source. Filters domain automatically
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="productSource"></param>
        /// <returns></returns>
        private IList<CustomerCompanyMap> GetTranslateFromUPFMToProductv2(string companyRealPageId, string productSource)
        {
            //translate/v2/companyinstance/684382D3-F2F8-4F42-8D29-935F834C6888/UPFM/OS?filter[customerEnvironment]=Primary
            string uri = $"translate/v2/companyinstance/{companyRealPageId}/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}";

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"GetTranslateFromUPFMToProductv2_{companyRealPageId}_{productSource}";
            List<CustomerCompanyMap> booksCustomerMaster = rpcache.GetFromCache<List<CustomerCompanyMap>>(cacheKey, 180, () =>
            {
                List<CustomerCompanyMap> companyListCache = new List<CustomerCompanyMap>();
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var translateCompanyInstance = JsonConvert.DeserializeObject<TranslateCompanyInstance>(response.Content.ReadAsStringAsync().Result);
                    Dictionary<string, object> logData = new Dictionary<string, object>() {{"response", translateCompanyInstance}};
                    WriteToLog(LogType.Diagnostic, "GetTranslateFromUPFMToProductv2 - Got info.", logData);
                    CustomerCompanyMap map = new CustomerCompanyMap(){ CompanyInstance = new List<CompanyInstance>()};
                    map.CompanyInstanceSourceId = translateCompanyInstance.Data.Attributes.TranslatedCompanyInstances[0].CompanyInstanceSourceId;
                    map.Source = productSource;
                    companyListCache.Add(map);
                    return companyListCache;
                }

                return null;
            });

            return booksCustomerMaster;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        private long GetCompanyMasterIdForRPDMID(string companyRealPageId, string domain)
        {
            string uri = $"customercompanymap?filter[companyInstanceSourceId]={companyRealPageId}&include=companyInstance";

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"GetCompanyMasterIdForRPDMID_{companyRealPageId}";
            CustomerCompanyMap booksCustomerMaster = rpcache.GetFromCache<CustomerCompanyMap>(cacheKey, 180, () =>
            {
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var companyMap = JsonConvert.DeserializeObject<List<CustomerCompanyMap>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    Dictionary<string, object> logData = new Dictionary<string, object>() {{"companyMap", companyMap}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyMasterIdForRPDMID - Got info.", logData);
                    return companyMap[0];
                }

                return null;
            });
            return booksCustomerMaster?.CustomerCompanyId ?? 0;
        }

        /// <summary>
        /// Get a list of property instances under the given company instance in the BlueBook system
        /// </summary>
        /// <param name="companyInstanceId"></param>
        /// <returns></returns>
        public IList<PropertyInstance> GetPropertyInstance(long companyInstanceId)
        {
            IList<PropertyInstance> propertyInstance = new List<PropertyInstance>();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            propertyInstance = _manageBlueBookCache[$"getPropertyInstance_{companyInstanceId.ToString()}"] as List<PropertyInstance>;
            if (propertyInstance == null)
            {
                string uri = $"dashboard/gb/getCompanyPropertyInstances?funcargs={companyInstanceId.ToString()}";
                //string uri = $"propertyinstance?filter[greenBookCares]=true&filter[companyPropertyInstanceMap.companyInstanceId]={companyInstanceId.ToString()}&sort=PropertyName&page[size]=3000";
                logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri } };
                WriteToLog(LogType.Diagnostic, "GetPropertyInstance - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    //propertyInstance = response.Content.ReadAsJsonApiManyAsync<PropertyInstanceResource>(_contractResolver, _cache).Result;
                    propertyInstance = JsonConvert.DeserializeObject<List<PropertyInstance>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() { { "propertyInstanceResource", propertyInstance } };
                    WriteToLog(LogType.Diagnostic, "GetPropertyInstance - Got info.", logData);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getPropertyInstance_{companyInstanceId.ToString()}", propertyInstance, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() { { "response", response } };
                    WriteToLog(LogType.Diagnostic, "GetPropertyInstance - No info found.", logData);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return propertyInstance;
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                        return null;
                    }
                }
            }

            return propertyInstance;
        }

        /// <summary>
        /// Get a list of property instances under the given company instance in the BlueBook system
        /// </summary>
        /// <param name="companyInstanceId"></param>
        /// <returns></returns>
        public CompanyPropertyRootObject GetCompanyPropertyInstance(long companyInstanceId)
        {
            CompanyPropertyRootObject companyPropertyInstanceResource = new CompanyPropertyRootObject();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            companyPropertyInstanceResource = _manageBlueBookCache[$"getCompanyPropertyInstance_{companyInstanceId}"] as CompanyPropertyRootObject;
            if (companyPropertyInstanceResource == null)
            {
                string uri = $"dashboard/gb/getCompanyPropertyInstances?funcargs={companyInstanceId}";
                logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogType.Diagnostic, "GetCompanyPropertyInstance - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    companyPropertyInstanceResource = JsonConvert.DeserializeObject(jsonContent, typeof(CompanyPropertyRootObject)) as CompanyPropertyRootObject;
                    companyPropertyInstanceResource.data.attributes.getCompanyPropertyInstances = companyPropertyInstanceResource.data.attributes.getCompanyPropertyInstances.OrderBy(r => r.propertyName).ToList();
                    logData = new Dictionary<string, object>() {{"companyPropertyInstanceResource", companyPropertyInstanceResource}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyPropertyInstance - Got info.", logData);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getCompanyPropertyInstance_{companyInstanceId}", companyPropertyInstanceResource, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyPropertyInstance - No info found.", logData);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return companyPropertyInstanceResource;
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                        return null;
                    }
                }
            }

            return companyPropertyInstanceResource;
        }

        public List<Guid> GetUPFMPropertyInstances(string companyRealPageId)
        {
            //CompanyPropertyRootObject companyPropertyInstanceResource = new CompanyPropertyRootObject();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            var rpcache = new RPObjectCache();
            var cacheKey = $"getUPFMPropertyInstances_{companyRealPageId}";

            var companyPropertyInstanceResource = rpcache.GetFromCache<List<Guid>>(cacheKey, 60, () =>
            {
                //http://booksapi-qa.realpage.com/companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]=F5C090FA-78AB-452F-B504-98AAFEE09121&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true

                string uri = $"companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]={companyRealPageId}&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true";
                logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogType.Diagnostic, "GetUPFMPropertyInstances - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    List<Guid> properties = new List<Guid>();
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    //CompanyPropertyRootObject
                    var propertyInstanceList = JsonConvert.DeserializeObject(jsonContent, typeof(UPFMPropertyInstanceRootObject)) as UPFMPropertyInstanceRootObject;
                    if (propertyInstanceList != null && propertyInstanceList.data.Any())
                    {
                        foreach (var property in propertyInstanceList.data)
                        {
                            foreach (var detail in property.attributes.propertyInstance)
                            {
                                properties.Add(new Guid(detail.PropertyInstanceSourceId));
                            }
                        }
                    }
                    //result.data.attributes.getCompanyPropertyInstances = result.data.attributes.getCompanyPropertyInstances.OrderBy(r => r.propertyName).ToList();
                    //logData = new Dictionary<string, object>() {{"companyPropertyInstanceResource", companyPropertyInstanceResource}};
                    //WriteToLog(LogType.Diagnostic, "GetCompanyPropertyInstance - Got info.", logData);
                    //CacheItemPolicy policy = new CacheItemPolicy();
                    //policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);

                    

                    //properties.Add(new Guid(""));
//                    properties.Add(new Guid("334CD10B-C8C6-4FE6-9652-C1BAB632950A"));
//properties.Add(new Guid("235FAEC7-9E34-47D1-A519-BE3935F748E5"));
//properties.Add(new Guid("0499A7CA-3132-4720-9F97-3147A971F2A9"));
//properties.Add(new Guid("5A76F34D-68CC-4DB5-A8E0-3741243F8970"));
//properties.Add(new Guid("26FA61FA-BAFF-4881-B784-A8E6ECF797A1"));
//properties.Add(new Guid("D4D58C5F-5562-42A4-86A4-BC013571A890"));
//properties.Add(new Guid("2F47EC63-2A07-444E-8DAE-2893102962FA"));
//properties.Add(new Guid("B4E3D7B5-3DFB-4994-BD80-6818F5B3223F"));
//properties.Add(new Guid("4E87EC63-947D-4DAC-8619-46383C37EAF7"));
//properties.Add(new Guid("CB53F305-1D53-4D96-9EF5-5AB0DB37EEA9"));
//properties.Add(new Guid("5C95E744-B9E8-4CB6-8B70-89F330B834BB"));
//properties.Add(new Guid("EE00AE7F-1776-4802-8731-3CB63C148432"));
//properties.Add(new Guid("A08B30D9-F554-4BAF-85F1-A75F9588519F"));
//properties.Add(new Guid("76AE5CC4-C26D-4486-BDD1-943F39B0DECA"));
//properties.Add(new Guid("EC6516A7-272F-4359-AF03-6FC21AA251C2"));
//properties.Add(new Guid("42E35DD5-B978-400E-8D9E-AA0B164EF479"));
//properties.Add(new Guid("FED8C2CB-B401-48E4-BF63-D5873C162E0E"));
//properties.Add(new Guid("99489BA4-FD02-4C5A-AE57-AB505A1DD62B"));
//properties.Add(new Guid("D505534F-4151-4070-AC12-BFCC610C259B"));
//properties.Add(new Guid("5232822F-B3C6-4A12-90CD-50E57182538C"));
//properties.Add(new Guid("A64C048B-406E-4F8B-8BDE-6D848F9110BB"));
//properties.Add(new Guid("26F19BF2-67E2-434E-A810-4C0F1020A9AF"));
//properties.Add(new Guid("C6C01354-A493-487C-A968-E8AC5C10B19B"));
////properties.Add(new Guid("B5746DF0-0AE7-4AF5-9E8A-28493DEEE789"));
////properties.Add(new Guid("A7360FCC-3E40-4586-BDA0-571533CDE71C"));
////properties.Add(new Guid("1C909EB9-6E6E-4E08-9A19-D753F55C63B9"));
////properties.Add(new Guid("707E261A-70B9-470C-A42E-F6C75C9C6E43"));
////properties.Add(new Guid("59015225-FB85-44AC-B4CD-10EC8C27D57D"));
////properties.Add(new Guid("91249A12-7E01-464D-A26F-F3863919E46C"));
////properties.Add(new Guid("C621ADEF-8943-42D9-B946-E69C9FF72EE5"));
////properties.Add(new Guid("A548338B-5DB9-4C82-8626-AAB2EE3A4910"));
////properties.Add(new Guid("2B8429C8-EEA2-4126-8FC5-1663C07D8068"));
//properties.Add(new Guid("796F328C-5145-43F2-AB00-E24F19258534"));
//properties.Add(new Guid("2DD296B9-ED74-4E2C-840F-D4BA8170ACDC"));
//properties.Add(new Guid("47042E1C-F649-442F-99EC-74512D0B27BF"));
//properties.Add(new Guid("B7708274-60EF-4D20-86AB-11EAD8BD7F27"));
//properties.Add(new Guid("B9710617-7FBA-489F-8F62-E095214E1A1B"));
//properties.Add(new Guid("FFAC3AB2-6C18-4236-93E4-959F787394A5"));
//properties.Add(new Guid("1DA4E090-C758-4A0D-9353-84D7FFFE7753"));
//properties.Add(new Guid("53BA62BE-6D17-45C8-ACE8-AE67D8A4C6E1"));
//properties.Add(new Guid("47BC0B7F-0401-4A80-91AA-7817AE649BEF"));
//properties.Add(new Guid("6B2C428A-3D48-48DA-B1F7-D6C135BD6240"));
//properties.Add(new Guid("F3B62D86-0A90-4A15-993A-EC688BB13FD5"));
//properties.Add(new Guid("1F4E8CD2-79CC-4968-8BFA-884CE64B1402"));
//properties.Add(new Guid("FF04BB2A-D609-4809-8BC3-1CB743C820A6"));
//properties.Add(new Guid("66BC2FFE-AFA1-4579-94A5-AE8538BD391C"));
//properties.Add(new Guid("26B78E07-D8FE-44AE-AFD9-A0A04505800F"));
//properties.Add(new Guid("AC87EAA5-B088-4B20-9B62-C69067DB1F2A"));
//properties.Add(new Guid("EF1FAD66-B1F6-4981-8BEC-E2D12279ABA2"));
//properties.Add(new Guid("60A9142C-587C-4CDA-9086-38DB8313203B"));
//properties.Add(new Guid("A57CD087-25AC-4479-AEE4-73E4B6A76E1C"));
//properties.Add(new Guid("CEB51735-445B-448B-AF7A-72F297C1CA16"));
//properties.Add(new Guid("974693DB-23BD-4CC3-A984-FBF9E5BF189C"));
//properties.Add(new Guid("809AA3FB-2626-444F-A19C-1AAB5EBA3B5A"));
//properties.Add(new Guid("0A7CA200-521F-4124-A6F4-9751385CD8A1"));
//properties.Add(new Guid("BCA476A4-400B-484F-A8AB-E765EE7B91F1"));
//properties.Add(new Guid("0A56925C-FBB1-40E5-9E0B-901136C60E07"));
//properties.Add(new Guid("B864349B-B774-4A96-AD69-41BB234AFCDA"));
//properties.Add(new Guid("3237E6C1-1C77-498F-9E13-E735E6EFF2BA"));
//properties.Add(new Guid("18A0179E-563D-4F79-9527-CFA192B914F8"));
//properties.Add(new Guid("7989FE57-76CD-4473-BF9F-D242D92B682C"));
//properties.Add(new Guid("ECFB912D-8E5D-4C86-9BA2-9C935ADDC50A"));
//properties.Add(new Guid("18927D15-6525-43D9-88B8-C19A4AF48E50"));
//properties.Add(new Guid("5972C050-7072-4B3F-8B6A-E280B5E36EB0"));
//properties.Add(new Guid("93287318-C66C-4EF8-B40D-8FE5B41F6BA5"));
//properties.Add(new Guid("1AB785C2-F165-4C80-9EF0-4AA03514FCD6"));
//properties.Add(new Guid("F4BD2376-B81A-4F0F-A771-BE4BB0C7627D"));
//properties.Add(new Guid("F5AB41F0-1F89-4840-B3C5-A138557E5931"));
//properties.Add(new Guid("1CABE921-CEEE-4395-9158-A02DAD741E3B"));
//properties.Add(new Guid("96C1350E-A55C-40EC-A592-C9A3C73A5B5F"));
//properties.Add(new Guid("E961A216-61A5-46A4-8055-49328A728BBA"));
//properties.Add(new Guid("51C2E013-7B2F-445D-A015-AEA2D09B173B"));
//properties.Add(new Guid("C2269687-1EC5-4846-8DE5-E17F154FD1C4"));
//properties.Add(new Guid("EF8656DE-0D07-4A41-ADDE-9E3C5F5BC5AF"));
//properties.Add(new Guid("D50D0692-A5F8-4D9A-814F-8E1654174FB9"));
//properties.Add(new Guid("9D1237E6-37D4-4F0C-81E1-90AFF3883C5E"));
//properties.Add(new Guid("1F49131A-E5D2-4233-9296-9129B31C2520"));
//properties.Add(new Guid("B72C56D2-6116-48FE-BC93-8653EF4FE050"));
//properties.Add(new Guid("E943597E-957F-4A7F-AADF-1386F85775CE"));
//properties.Add(new Guid("0B175431-2D24-41F5-8773-64EA4747B449"));
//properties.Add(new Guid("B031BFAF-D534-46A2-8699-CE836F6F8E05"));
//properties.Add(new Guid("FB1E9373-F8D0-43AB-9489-97F8C1F4BE31"));
//properties.Add(new Guid("01F94ECA-0F6F-4170-B1B7-D9921A744EE8"));
//properties.Add(new Guid("B08CC4B7-E823-4E0C-9476-1DC6670CD13D"));
//properties.Add(new Guid("8EB14B85-5976-4374-B82A-4D8E169DF977"));
//properties.Add(new Guid("3FA854A9-BF6C-4989-B826-F0A92FF81F5C"));
//properties.Add(new Guid("8BCC91FB-4A7D-4BA1-B595-C073396CE3EE"));
//properties.Add(new Guid("D9BCA7C3-F737-402D-8BEA-3741F7AE47AD"));
//properties.Add(new Guid("7A1D41BC-F725-4666-A2DB-55F5BABE5015"));
//properties.Add(new Guid("98C4A90E-A858-4A9E-93D5-DC7051E99129"));
//properties.Add(new Guid("B41FF75E-7F23-4DF1-BE58-221A31F5046F"));
//properties.Add(new Guid("BE76573D-25FC-421B-9188-973D3C959248"));
//properties.Add(new Guid("C1176AD7-FBF1-4FDC-8661-AB3F2B43329C"));
//properties.Add(new Guid("B234F826-FC3D-4A7C-A273-AED6A375AFB1"));
//properties.Add(new Guid("90B43C29-8398-4BBD-84E6-E61E409EB826"));
//properties.Add(new Guid("6A9C2569-8D3B-46B1-9477-3A1D4DCD5013"));
//properties.Add(new Guid("031A2F34-4B73-4946-992E-BECC411EF04C"));
//properties.Add(new Guid("176B6D95-2EB9-414D-8B6A-482F3503DEE2"));
//properties.Add(new Guid("A3E941EF-B973-4F88-9B14-362E30EC8D2D"));
//properties.Add(new Guid("C44591B5-0989-41D9-AA82-ED8FDFA7B21D"));
//properties.Add(new Guid("3C4E922C-FB26-4D13-8DD6-B83C1BCE4DCD"));
//properties.Add(new Guid("3AD9AD96-90A1-434D-8B77-30D588504298"));
//properties.Add(new Guid("B8507A02-CF57-41A8-8022-E5D0A40B7635"));
//properties.Add(new Guid("3D4912C0-BE43-41A3-A624-7C14C94786A2"));
//properties.Add(new Guid("20C8563C-7151-4159-805E-4427C64FCD7C"));
//properties.Add(new Guid("4AE4BEDF-C67F-4BAD-AB33-EC890CEA860A"));
//properties.Add(new Guid("4393E7CD-8AC6-4E81-8FBC-E90294C621A6"));
//properties.Add(new Guid("3CA8747D-AFED-4992-889D-67C5B7FE1903"));
//properties.Add(new Guid("2753FEDF-F517-4491-B580-1BA0B3AAF25A"));
//properties.Add(new Guid("0D488E88-1767-40F0-9931-6B7D4000A377"));
//properties.Add(new Guid("7D8CF926-2CF6-485B-9500-0A5A4648FF98"));
//properties.Add(new Guid("BF8C9B2F-C5F6-4750-A55B-1133C464C663"));
//properties.Add(new Guid("493D3076-2F54-431E-9374-7791C70DA028"));
//properties.Add(new Guid("7A3979F2-2A54-4398-BCDD-D26ABFC6EC2E"));
//properties.Add(new Guid("2B3862D2-EF9A-4960-8C96-870B3F044A8D"));
//properties.Add(new Guid("5FCB11C2-F6BA-4266-AF5B-E682565C0EFB"));
//properties.Add(new Guid("1FCE347C-7F73-4375-B85C-9FF867763771"));
                    return properties;
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "GetUPFMPropertyInstances - No info found.", logData);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return new List<Guid>();
                    }

                    response.EnsureSuccessStatusCode();
                    return null;
                }

            });

            /*
            var companyPropertyInstanceResource = _manageBlueBookCache[$"getUPFMPropertyInstances_{companyRealPageId}"] as List<Guid>;
            if (companyPropertyInstanceResource == null)
            {
                string uri = $"dashboard/gb/getCompanyPropertyInstances?funcargs={companyRealPageId}";
                logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogType.Diagnostic, "GetUPFMPropertyInstances - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    //companyPropertyInstanceResource = JsonConvert.DeserializeObject(jsonContent, typeof(CompanyPropertyRootObject)) as CompanyPropertyRootObject;
                    //companyPropertyInstanceResource.data.attributes.getCompanyPropertyInstances = companyPropertyInstanceResource.data.attributes.getCompanyPropertyInstances.OrderBy(r => r.propertyName).ToList();
                    //logData = new Dictionary<string, object>() {{"companyPropertyInstanceResource", companyPropertyInstanceResource}};
                    //WriteToLog(LogType.Diagnostic, "GetCompanyPropertyInstance - Got info.", logData);
                    //CacheItemPolicy policy = new CacheItemPolicy();
                    //policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getCompanyPropertyInstance_{companyInstanceId}", companyPropertyInstanceResource, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyPropertyInstance - No info found.", logData);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return companyPropertyInstanceResource;
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                        return null;
                    }
                }
            }
            */

            return companyPropertyInstanceResource;
        }

        /// <summary>
        /// Used to add a new company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        public bool AddBooksGreenBookCompanyInstance(CompanyInstance companyInstance)
        {
            string uri = $"companyinstance";

            Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}, {"companyInstance", companyInstance}};
            WriteToLog(LogType.Diagnostic, "AddBooksGreenBookCompanyInstance - Adding info.", logData);

            var jsonToSave = JsonConvert.SerializeObject(companyInstance, new JsonApiSerializerSettings()).Replace("companyinstanceadd", "companyinstance");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(jsonToSave, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && response.IsSuccessStatusCode)
            {
                //var clientResponse = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Used to delete an existing company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        public bool DeleteBooksGreenBookCompanyInstance(CompanyInstance companyInstance)
        {
            string uri = $"companyinstance/{companyInstance.CompanyInstanceId}?modifiedBy={WebUtility.UrlEncode(companyInstance.ModifiedBy)}";

            Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}, {"companyInstance", companyInstance}};
            WriteToLog(LogType.Diagnostic, $"DeleteBooksGreenBookCompanyInstance - deleting instance {companyInstance.CompanyInstanceId}.", logData);
            var response = _httpClient.DeleteAsync(uri).Result;
            logData = new Dictionary<string, object>() {{"StatusCode", response.StatusCode}};
            WriteToLog(LogType.Diagnostic, $"DeleteBooksGreenBookCompanyInstance - deleted instance {companyInstance.CompanyInstanceId}.", logData);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Used to update an existing company instance
        /// </summary>
        /// <param name="companyInstance"></param>
        /// <returns></returns>
        public string UpdateBooksGreenBookCompanyInstance(CompanyInstance companyInstance)
        {
            string uri = $"companyinstance/{companyInstance.CompanyInstanceSourceId}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}";

            Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}, {"companyInstance", companyInstance}};
            WriteToLog(LogType.Diagnostic, "UpdateBooksGreenBookCompanyInstance - Updating info.", logData);

            var jsonToSave = JsonConvert.SerializeObject(companyInstance, new JsonApiSerializerSettings()).Replace("companyinstanceadd", "companyinstance");
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                Content = new StringContent(jsonToSave, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && !response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return "instance not found";
                }

                return "an unknown error occurred. " + response.StatusCode;
            }

            return "";
        }

        /// <summary>
        /// Used to get a list of company id's for the given company list
        /// </summary>
        /// <param name="companies"></param>
        /// <returns></returns>
        private string GetCompanyIds(List<UnifiedLoginCompany> companies)
        {
            string compIds = "";
            foreach (var item in companies)
            {
                if (item.BooksCustomerMasterId > 0)
                {
                    if (compIds == "")
                    {
                        compIds = item.BooksCustomerMasterId.ToString();
                    }

                    compIds += "," + item.BooksCustomerMasterId;
                }
            }

            return compIds;
        }

        /// <summary>
        /// Used to split a list into sub smaller lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="locations"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        /// <summary>
        /// Used to get the information about the list of companies by companyIds from the BlueBook system
        /// </summary>
        /// <param name="booksCompanyMasterList"></param>        
        /// <returns></returns>
        public IList<Company> GetCompanyListByCompIds(List<UnifiedLoginCompany> booksCompanyMasterList)
        {
            IList<Company> companyInstance = new List<Company>();
            Dictionary<string, object> logData;

            // get the hash of the full company list
            int booksCompanyMasterHash = GetCompanyIds(booksCompanyMasterList).GetHashCode();

            companyInstance = _manageBlueBookCache[$"getCompanysByCompIds_{booksCompanyMasterHash}"] as List<Company>;
            if (companyInstance == null)
            {
                int splitSize = (int) (booksCompanyMasterList.Count * .2);
                if (splitSize == 0)
                {
                    splitSize = 10;
                }

                var splitCompanyList = SplitList<UnifiedLoginCompany>(booksCompanyMasterList, splitSize);
                ConcurrentBag<Company> result = new ConcurrentBag<Company>();
                Parallel.ForEach(splitCompanyList, new ParallelOptions {MaxDegreeOfParallelism = 5}, companyList => { GetBooksCompanyDetails(companyList).ForEach(x => result.Add(x)); });

                companyInstance = result.ToList();
                if (companyInstance.Count > 0)
                {
                    CacheItemPolicy policy = new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(86400)};
                    // 24 hrs cached 86400 secs
                    _manageBlueBookCache.Set($"getCompanysByCompIds_{booksCompanyMasterHash}", companyInstance, policy);
                }
            }

            return companyInstance;
        }

        /// <summary>
        /// Used to get company details for the given company list
        /// </summary>
        /// <param name="companyList"></param>
        /// <returns></returns>
        private List<Company> GetBooksCompanyDetails(List<UnifiedLoginCompany> companyList)
        {
            string booksCompanyMasterIds = GetCompanyIds(companyList);

            var companyInstance = new List<Company>();
            string uri = $"customercompany?filter[customerCompanyId]=in:{booksCompanyMasterIds}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999";

            var logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
            WriteToLog(LogType.Diagnostic, $"GetCompanyListByCompIds - Getting info - hashcode:{booksCompanyMasterIds.GetHashCode()}", logData);
            var response = GetAsync(uri).Result;
            if (response.IsSuccessStatusCode)
            {
                companyInstance = JsonConvert.DeserializeObject<List<Company>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                logData = new Dictionary<string, object>() {{"CompanyInstance", companyInstance}};
                WriteToLog(LogType.Diagnostic, $"GetCompanyListByCompIds - Got info - hashcode:{booksCompanyMasterIds.GetHashCode()}", logData);
                return companyInstance;
            }

            logData = new Dictionary<string, object>() {{"response", response}};
            WriteToLog(LogType.Diagnostic, "GetCompanyListByCompIds - No info found - hashcode:{booksCompanyMasterIds.GetHashCode()}", logData);

            return companyInstance;
        }

        /// <summary>
        /// Used to get the information about the company, using the books customer master id or the UPFM id
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <param name="domain"></param>
        /// <param name="booksCompanyMasterId"></param>
        /// <returns></returns>
        public CustomerCompany GetCompanyCustomerInfo(Guid companyRealPageId, string domain, long booksCompanyMasterId)
        {
            CustomerCompany companyInstance = new CustomerCompany();
            if (useUPFMId && companyRealPageId != Guid.Empty)
            {
                // need to send guid in uppercase because books is case sensitive.
                var currentCompanyMasterId = GetCompanyMasterIdForRPDMID(companyRealPageId.ToString().ToUpper(), domain);
                booksCompanyMasterId = (currentCompanyMasterId != 0) ? currentCompanyMasterId : booksCompanyMasterId;
            }

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"getCompanyCustomerInfo_{booksCompanyMasterId}";

            companyInstance = rpcache.GetFromCache<CustomerCompany>(cacheKey, CacheTimeSeconds, () =>
            {
                // load from api
                //string uri = $"company/{companyId}?include=customerCompany";
                string uri = $"customercompany/{booksCompanyMasterId}";

                Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogType.Diagnostic, "GetCompanyCustomerInfo - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    //companyInstance = response.Content.ReadAsJsonApiAsync<CompanyResource>(_contractResolver, _cache).Result;
                    companyInstance = JsonConvert.DeserializeObject<CustomerCompany>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() {{"CompanyInstance", companyInstance}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyCustomerInfo - Got info.", logData);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyCustomerInfo - No info found.", logData);
                    return null;
                }

                return companyInstance;
            });

            return companyInstance;
        }

        /// <summary>
        /// Get a list of property master records under the given company instance in the BlueBook system
        /// </summary>
        /// <param name="booksCompanyMasterId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IList<CustomerCompanyPropertyMap> GetVCompanyPropertyMap(long booksCompanyMasterId, string filter)
        {
            IList<CustomerCompanyPropertyMap> vCompanyPropertyMap = new List<CustomerCompanyPropertyMap>();

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"getVCompanyPropertyMap_{booksCompanyMasterId}";

            vCompanyPropertyMap = rpcache.GetFromCache<IList<CustomerCompanyPropertyMap>>(cacheKey, CacheTimeSeconds, () =>
            {
                // load from api
                string uri = $"customercompanyproperty?filter[customerCompanyId]={booksCompanyMasterId.ToString()}&filter[migrationStatus]=in:%27staged%27,%27migrated%27&sort=PropertyName&page[number]=1&page[size]=9999";
                Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogType.Diagnostic, "GetVCompanyPropertyMap - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    vCompanyPropertyMap = JsonConvert.DeserializeObject<List<CustomerCompanyPropertyMap>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() {{"vCompanyPropertyMapResource", vCompanyPropertyMap}};
                    WriteToLog(LogType.Diagnostic, "GetVCompanyPropertyMap - Got info.", logData);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "GetVCompanyPropertyMap - No info found.", logData);
                    return null;
                }

                return vCompanyPropertyMap;
            });

            return vCompanyPropertyMap;
        }

        public IList<ProductProperty> GetCustomerProperty(long booksCompanyMasterId = 0, string include = null, string filter = null)
        {
            if (booksCompanyMasterId == 0)
            {
                booksCompanyMasterId = _defaultUserClaim.CustomerMasterId;
            }

            if (booksCompanyMasterId == 0)
            {
                throw new Exception("Invalid parameter booksCompanyMasterId.");
            }

            string includeFields = string.Empty;

            bool bIncludeFields = ((!string.IsNullOrWhiteSpace(include)) && (include.Split(new char[] {','}).Length > 0));

            if (bIncludeFields)
            {
                includeFields = "fields[customerproperty]=" + include.Replace(" ", string.Empty) + "&";
            }

            filter = string.IsNullOrWhiteSpace(filter) ? "&filter[isActive]=true&page[size]=9999" : filter;

            List<CustomerProperty> customerPropertyList = new List<CustomerProperty>();
            List<ProductProperty> productPropertyList = new List<ProductProperty>();

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"getCustomerProperty_{booksCompanyMasterId}" + (bIncludeFields ? "_" + include.Replace(",", string.Empty) : string.Empty);

            productPropertyList = rpcache.GetFromCache<List<ProductProperty>>(cacheKey, CacheTimeSeconds, () =>
            {
                string uri = $"customerproperty?{includeFields}filter[customerCompanyId]={booksCompanyMasterId.ToString()}{filter}";
                Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogType.Diagnostic, "ManageBlueBook.GetCustomerProperty - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    customerPropertyList = JsonConvert.DeserializeObject<List<CustomerProperty>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    productPropertyList = customerPropertyList.Select(p => new ProductProperty
                    {
                        ID = p.attributes != null ? p.attributes.customerPropertyId : null,
                        Name = p.attributes.propertyName,
                        Street1 = p.attributes.address != null ? p.attributes.address.address : null,
                        City = p.attributes.address != null ? p.attributes.address.city : null,
                        State = p.attributes.address != null ? p.attributes.address.state : null,
                        Zip = p.attributes.address != null ? p.attributes.address.postalCode : null
                    }).OrderBy(p => p.Name).ToList();

                    logData = new Dictionary<string, object>() {{"ManageBlueBook.GetCustomerProperty", customerPropertyList}};
                    WriteToLog(LogType.Diagnostic, "ManageBlueBook.GetCustomerProperty - Got info.", logData);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "ManageBlueBook.GetCustomerProperty - No info found.", logData);
                    return null;
                }

                return productPropertyList;
            });
            return productPropertyList;
        }

        #region Privates

        /// <summary>
        /// Used to set the required token for the books api call and then call the service
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetAsync(string uri)
        {
            bool doneProcessing = false;
            int failedCount = 0;
            HttpResponseMessage response = new HttpResponseMessage();

            if (!AddAuthHeader())
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            while (!doneProcessing)
            {
                response = _httpClient.GetAsync(uri).Result;
                doneProcessing = response.IsSuccessStatusCode;
                if (!doneProcessing)
                {
                    if (!(response.StatusCode == HttpStatusCode.Unauthorized))
                    {
                        doneProcessing = true;
                    }
                    else
                    {
                        // reset the token so it gets a new one if we got an unauthorized error
                        _manageBlueBookCache.Remove("bluebookToken");
                        AddAuthHeader();
                        failedCount += 1;
                    }

                    if (failedCount >= MAXRETRYCOUNT)
                    {
                        doneProcessing = true;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Used to add the token header for Books
        /// </summary>
        /// <returns></returns>
        private bool AddAuthHeader()
        {
            return true; // don't do the auth header right now

            string _token = _manageBlueBookCache["bluebookToken"] as string;
            if (_token == null)
            {
                if (!GetAuthToken())
                {
                    //return false;
                }

                _token = _manageBlueBookCache["bluebookToken"] as string;
            }

            if (_token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }

            return true;
        }

        /// <summary>
        /// Used to get the auth token for the books
        /// </summary>
        /// <returns></returns>
        private bool GetAuthToken()
        {
            try
            {
                string url = "/login";
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = new StringContent(JsonConvert.SerializeObject(_authTokenInfo), System.Text.Encoding.Default, "application/vnd.api+json");
                // need to blank out the content charset because the books api doesn't like if one is sent with the content type
                req.Content.Headers.ContentType.CharSet = "";
                if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                }

                var response = _httpClient.SendAsync(req);

                if (response.Result.IsSuccessStatusCode)
                {
                    var tokenResult = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                    if (tokenResult.token != null)
                    {
                        string _token = tokenResult.token;
                        CacheItemPolicy policy = new CacheItemPolicy();
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(AuthTokenRefreshMinutes);
                        _manageBlueBookCache.Set("bluebookToken", _token, policy);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }

        /// <summary>
        /// Used to get an auth token for the books
        /// </summary>
        private class AuthTokenData
        {
            [JsonProperty("data")] public AuthToken Data { get; set; }

            public AuthTokenData()
            {
                Data = new AuthToken();
            }
        }

        /// <summary>
        /// Used to get an auth token for the books
        /// </summary>
        private class AuthToken
        {
            /// <summary>
            /// The email to use for the token
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// The password to use for the token
            /// </summary>
            [JsonProperty("password")]
            public string Password { get; set; }
        }

        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        /// <param name="exception"></param>
        private void WriteToLog(LogType logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            string correlationId = "";
            if (_defaultUserClaim != null)
            {
                correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";

            }

            Log.Write(logType, new LogDetails
            {
                Message = message,
                AdditionalInfo = logData,
                ProductModule = this.GetType().ToString(),
                Exception = exception,
                CorrelationId = correlationId
            });
        }
    }
}