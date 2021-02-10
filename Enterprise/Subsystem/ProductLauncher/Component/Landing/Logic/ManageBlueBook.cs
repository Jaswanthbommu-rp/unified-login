using JsonApiSerializer;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

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
        private bool useTranslatev2 = false;

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
            useDomains = GetBooleanProductSettings("BooksUseDomains");
            useUPFMId = GetBooleanProductSettings("BooksUseUPFMId");
            useTranslatev2 = GetBooleanProductSettings("BooksUseTranslatev2");

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
            useDomains = GetBooleanProductSettings("BooksUseDomains");
            useUPFMId = GetBooleanProductSettings("BooksUseUPFMId");
            useTranslatev2 = GetBooleanProductSettings("BooksUseTranslatev2");

            //bbUri = "https://booksapi.realpage.com";
            _httpClient = new HttpClient {BaseAddress = new Uri(bbUri)};
        }

        public ManageBlueBook(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, HttpMessageHandler messageHandler)
        {
            _productInternalSettingRepository = productInternalSettingRepository;
            _httpClient = new HttpClient(messageHandler) {BaseAddress = new Uri("http://localhost")};
            _defaultUserClaim = userClaim;

            productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
            useDomains = GetBooleanProductSettings("BooksUseDomains");
            useUPFMId = GetBooleanProductSettings("BooksUseUPFMId");
            useTranslatev2 = GetBooleanProductSettings("BooksUseTranslatev2");
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
                if (!useTranslatev2)
                {
                    companyMap = GetTranslateFromUPFMToProduct(companyRealPageId.ToString(), source, domain);
                }
                else
                {
                    companyMap = GetTranslateFromUPFMToProductv2(companyRealPageId.ToString(), source);
                }

                if (companyMap != null)
                {
                    return companyMap;
                }
            }

            if (useUPFMId && companyRealPageId != Guid.Empty)
            {
                var newCompanyMasterId = GetCompanyMasterIdForRPDMID(companyRealPageId.ToString(), domain);
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
                WriteToLog(LogEventLevel.Debug, "GetCompanyMap - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    companyMap = JsonConvert.DeserializeObject<List<CustomerCompanyMap>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() {{"companyMap", companyMap}};
                    WriteToLog(LogEventLevel.Debug, "GetCompanyMap - Got info.", logData);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getCompanyMapResource_{booksCompanyMasterId}_{source}_{includeExtra}_{domain}", companyMap, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogEventLevel.Debug, "GetCompanyMap - No info found.", logData);

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
            Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", uri}};
            WriteToLog(LogEventLevel.Debug, $"GetTranslateFromUPFMToProduct - Getting info. {productSource}/{domain}", logData);

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"GetTranslateFromUPFMToProduct_{companyRealPageId}_{productSource}_{domain}";
            List<CustomerCompanyMap> booksCustomerMaster = rpcache.GetFromCache<List<CustomerCompanyMap>>(cacheKey, 180, () =>
            {
                List<CustomerCompanyMap> companyListCache = new List<CustomerCompanyMap>();
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var translateCompanyInstance = JsonConvert.DeserializeObject<TranslateCompanyInstance>(response.Content.ReadAsStringAsync().Result);
                    logData = new Dictionary<string, object>() {{"response", translateCompanyInstance}, {"uri", uri}, {"productSource", productSource}, {"domain", domain}};
                    WriteToLog(LogEventLevel.Debug, $"GetTranslateFromUPFMToProduct - Got info. {productSource}/{domain}", logData);
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
            Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", uri}};
            WriteToLog(LogEventLevel.Debug, $"GetTranslateFromUPFMToProductv2 - Getting info. {productSource}", logData);

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"GetTranslateFromUPFMToProductv2_{companyRealPageId}_{productSource}";
            List<CustomerCompanyMap> booksCustomerMaster = rpcache.GetFromCache<List<CustomerCompanyMap>>(cacheKey, 180, () =>
            {
                List<CustomerCompanyMap> companyListCache = new List<CustomerCompanyMap>();
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var translateCompanyInstance = JsonConvert.DeserializeObject<TranslateCompanyInstance>(response.Content.ReadAsStringAsync().Result);
                    logData = new Dictionary<string, object>() {{"response", translateCompanyInstance}};
                    WriteToLog(LogEventLevel.Debug, "GetTranslateFromUPFMToProductv2 - Got info.", logData);
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
        public TranslatePropertyInstance GetTranslatePropertiesFromUPFMToProductv3(UPFMProperty upfmProperties, string productSource)
        {
            //https://booksapi-stg.realpage.com/translate/v3/propertyinstance/UPFM/IB
            //{"propertyInstanceSourceIds": ["5972c050-7072-4b3f-8b6a-e280b5e36eb0","5972c050-7072-4b3f-8b6a-e280b5e36eb0","ef1fad66-b1f6-4981-8bec-e2d12279aba2"]}
            TranslatePropertyInstance translatePropertyInstance = new TranslatePropertyInstance();
            string uri = $"translate/v3/propertyinstance/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}";
            Dictionary<string, object> logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri }, { "propertyInstanceSourceIds", upfmProperties } };
            WriteToLog(LogEventLevel.Debug, "GetTranslatePropertiesFromUPFMToProductv3 - Adding info.", logData);

            var jsonToSave = JsonConvert.SerializeObject(upfmProperties);//, new JsonApiSerializerSettings()).Replace("companyinstanceadd", "companyinstance");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(jsonToSave, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && response.IsSuccessStatusCode)
            {
                translatePropertyInstance = JsonConvert.DeserializeObject<TranslatePropertyInstance>(response.Content.ReadAsStringAsync().Result);
                logData = new Dictionary<string, object>() { { "response", translatePropertyInstance } };
                WriteToLog(LogEventLevel.Debug, "GetTranslatePropertiesFromUPFMToProductv3 - Got info.", logData);
                return translatePropertyInstance;
            }

            return translatePropertyInstance;
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
                    WriteToLog(LogEventLevel.Debug, "GetCompanyMasterIdForRPDMID - Got info.", logData);
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
                WriteToLog(LogEventLevel.Debug, "GetPropertyInstance - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    //propertyInstance = response.Content.ReadAsJsonApiManyAsync<PropertyInstanceResource>(_contractResolver, _cache).Result;
                    propertyInstance = JsonConvert.DeserializeObject<List<PropertyInstance>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() { { "propertyInstanceResource", propertyInstance } };
                    WriteToLog(LogEventLevel.Debug, "GetPropertyInstance - Got info.", logData);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getPropertyInstance_{companyInstanceId.ToString()}", propertyInstance, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() { { "response", response } };
                    WriteToLog(LogEventLevel.Debug, "GetPropertyInstance - No info found.", logData);
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
                WriteToLog(LogEventLevel.Debug, "GetCompanyPropertyInstance - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    companyPropertyInstanceResource = JsonConvert.DeserializeObject(jsonContent, typeof(CompanyPropertyRootObject)) as CompanyPropertyRootObject;
                    companyPropertyInstanceResource.data.attributes.getCompanyPropertyInstances = companyPropertyInstanceResource.data.attributes.getCompanyPropertyInstances.OrderBy(r => r.propertyName).ToList();
                    logData = new Dictionary<string, object>() {{"companyPropertyInstanceResource", companyPropertyInstanceResource}};
                    WriteToLog(LogEventLevel.Debug, "GetCompanyPropertyInstance - Got info.", logData);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getCompanyPropertyInstance_{companyInstanceId}", companyPropertyInstanceResource, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogEventLevel.Debug, "GetCompanyPropertyInstance - No info found.", logData);
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

        /// <summary>
        /// Used to get UPFM property instances for the given UPFM company id
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <returns></returns>
        public List<Guid> GetUPFMPropertyInstances(string companyRealPageId)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();

            var rpcache = new RPObjectCache();
            var cacheKey = $"getUPFMPropertyInstances_{companyRealPageId}";

            var companyPropertyInstanceResource = rpcache.GetFromCache<List<Guid>>(cacheKey, 60, () =>
            {
                //http://booksapi-qa.realpage.com/companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]=F5C090FA-78AB-452F-B504-98AAFEE09121&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true
                string uri = $"companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]={companyRealPageId}&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true&page[size]=9999";
                logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogEventLevel.Debug, "GetUPFMPropertyInstances - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    List<Guid> properties = new List<Guid>();
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
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
                    return properties;
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogEventLevel.Debug, "GetUPFMPropertyInstances - No info found.", logData);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return new List<Guid>();
                    }

                    response.EnsureSuccessStatusCode();
                    return null;
                }

            });
            
            return companyPropertyInstanceResource;
        }

        public List<Guid> GetPropertiesPerProductCenter (string companyRealPageId, ProductEnum product)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();

            var rpcache = new RPObjectCache();
            var cacheKey = $"getPropertiesPerProductCenter_{companyRealPageId}_{product}";

            var PropertyInstanceResource = rpcache.GetFromCache<List<Guid>>(cacheKey, 60, () =>
            {
                // http://booksapi-qa.realpage.com/propertiesperproductcenter/UPFM/F5C090FA-78AB-452F-B504-98AAFEE09121/57
                string uri = $"propertiesperproductcenter/UPFM/{companyRealPageId}/{(int)product}";
                logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri } };
                WriteToLog(LogEventLevel.Debug, "GetPropertiesPerProductCenter - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    List<Guid> properties = new List<Guid>();
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    var propertyInstanceList = JsonConvert.DeserializeObject(jsonContent, typeof(UPFMProductPropertyInstanceMap)) as UPFMProductPropertyInstanceMap;
                    if (propertyInstanceList != null )
                    {
                        foreach (var property in propertyInstanceList.data.attributes)
                        {
                            properties.Add(new Guid(property.propertyInstanceSourceId));
                        }
                    }
                    return properties;
                }
                else
                {
                    logData = new Dictionary<string, object>() { { "response", response } };
                    WriteToLog(LogEventLevel.Debug, "GetPropertiesPerProductCenter - No info found.", logData);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return new List<Guid>();
                    }

                    response.EnsureSuccessStatusCode();
                    return null;
                }

            });

            return PropertyInstanceResource;
        }
        /// <summary>
        /// Used to get UPFM property instances for the given UPFM company id
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <returns></returns>
        public List<Guid> GetProductPropertyInstances(int companyInstanceSourceId, string source)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();

            var rpcache = new RPObjectCache();
            var cacheKey = $"getProductPropertyInstances_{companyInstanceSourceId}_{source}";

            var companyPropertyInstanceResource = rpcache.GetFromCache<List<Guid>>(cacheKey, 60, () =>
            {
                //http://booksapi-qa.realpage.com/companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]=F5C090FA-78AB-452F-B504-98AAFEE09121&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true
                string uri = $"companypropertyinstancemap?include=propertyInstance&filter[source]={source}&filter[companyinstance.companyInstanceSourceId]={companyInstanceSourceId}&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true&page[size]=9999";
                logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri } };
                WriteToLog(LogEventLevel.Debug, "GetProductPropertyInstances - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    List<Guid> properties = new List<Guid>();
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
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
                    return properties;
                }
                else
                {
                    logData = new Dictionary<string, object>() { { "response", response } };
                    WriteToLog(LogEventLevel.Debug, "GetUPFMPropertyInstances - No info found.", logData);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return new List<Guid>();
                    }

                    response.EnsureSuccessStatusCode();
                    return null;
                }

            });

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
            WriteToLog(LogEventLevel.Debug, "AddBooksGreenBookCompanyInstance - Adding info.", logData);

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
            WriteToLog(LogEventLevel.Debug, $"DeleteBooksGreenBookCompanyInstance - deleting instance {companyInstance.CompanyInstanceId}.", logData);
            var response = _httpClient.DeleteAsync(uri).Result;
            logData = new Dictionary<string, object>() {{"StatusCode", response.StatusCode}};
            WriteToLog(LogEventLevel.Debug, $"DeleteBooksGreenBookCompanyInstance - deleted instance {companyInstance.CompanyInstanceId}.", logData);
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
            WriteToLog(LogEventLevel.Debug, "UpdateBooksGreenBookCompanyInstance - Updating info.", logData);

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
        /// Add the new UPFM property instance to books
        /// </summary>
        /// <param name="propertyInstance"></param>
        /// <returns></returns>
        public bool AddBooksGreenBookPropertyInstanceFromProvisioning(PropertyInstance propertyInstance)
        {
            string uri = $"propertyinstance/{propertyInstance.PropertyInstanceSourceId}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}";
            
            Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}, {"propertyInstance", propertyInstance}};
            var jsonToSave = JsonConvert.SerializeObject(propertyInstance, new JsonApiSerializerSettings()).Replace("\"propertyInstanceId\":0,", "");
            logData.Add("jsonToSave", jsonToSave);
            WriteToLog(LogEventLevel.Debug, "AddBooksGreenBookPropertyInstance - Adding info.", logData);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                Content = new StringContent(jsonToSave, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && response.IsSuccessStatusCode)
            {
                WriteToLog(LogEventLevel.Debug, "AddBooksGreenBookPropertyInstance - Added info successfully.");
                //var clientResponse = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                return true;
            }

            logData = new Dictionary<string, object>() {{"response", response}};
            WriteToLog(LogEventLevel.Error, "AddBooksGreenBookPropertyInstance - Failed to add info.", logData);
            
            return false;
        }

        /// <summary>
        /// Used to acknowledge provisioning events
        /// </summary>
        /// <param name="productCenterEnablement"></param>
        /// <returns></returns>
        public bool AcknowledgeProvisioningEvent(ProductCenterEnablement productCenterEnablement)
        {
            string uri = $"productcenterenablement/enable";
            
            Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}, {"productCenterEnablement", productCenterEnablement}};
            var jsonToSave = JsonConvert.SerializeObject(productCenterEnablement, new JsonApiSerializerSettings()).Replace("\"details\"", "\"productCenterEnablement\"");
            logData.Add("jsonToSave", jsonToSave);
            WriteToLog(LogEventLevel.Debug, "AcknowledgeProvisioningEvent - Adding info.", logData);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(jsonToSave, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && response.IsSuccessStatusCode)
            {
                WriteToLog(LogEventLevel.Debug, "AcknowledgeProvisioningEvent - Added info successfully.");
                //var clientResponse = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                return true;
            }
            
            logData = new Dictionary<string, object>() {{"response", response}};
            WriteToLog(LogEventLevel.Error, "AcknowledgeProvisioningEvent - Failed to add info.", logData);

            return true;
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
                int splitSize = (int) (booksCompanyMasterList.Count * .08);
                if (splitSize == 0)
                {
                    splitSize = 10;
                }

                var splitCompanyList = SplitList<UnifiedLoginCompany>(booksCompanyMasterList, splitSize);
                ConcurrentBag<Company> result = new ConcurrentBag<Company>();
                Parallel.ForEach(splitCompanyList, new ParallelOptions {MaxDegreeOfParallelism = 5}, companyList => { GetBooksCompanyDetails(_defaultUserClaim, companyList).ForEach(x => result.Add(x)); });

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
        /// <param name="userClaim"></param>
        /// <param name="companyList"></param>
        /// <returns></returns>
        private List<Company> GetBooksCompanyDetails(DefaultUserClaim userClaim, List<UnifiedLoginCompany> companyList)
        {
            string booksCompanyMasterIds = GetCompanyIds(companyList);

            var companyInstance = new List<Company>();
            string uri = $"customercompany?filter[customerCompanyId]=in:{booksCompanyMasterIds}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999";

            var logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
            WriteToLog(LogEventLevel.Debug, $"GetCompanyListByCompIds - Getting info - hashcode:{booksCompanyMasterIds.GetHashCode()}", logData, correlationId: userClaim.CorrelationId.ToString());
            var response = GetAsync(uri).Result;
            if (response.IsSuccessStatusCode)
            {
                companyInstance = JsonConvert.DeserializeObject<List<Company>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                logData = new Dictionary<string, object>() {{"CompanyInstance", companyInstance}};
                WriteToLog(LogEventLevel.Debug, $"GetCompanyListByCompIds - Got info - hashcode:{booksCompanyMasterIds.GetHashCode()}", logData, correlationId: userClaim.CorrelationId.ToString());
                return companyInstance;
            }

            logData = new Dictionary<string, object>() {{"response", response}};
            WriteToLog(LogEventLevel.Debug, $"GetCompanyListByCompIds - No info found - hashcode:{booksCompanyMasterIds.GetHashCode()}", logData, correlationId: userClaim.CorrelationId.ToString());

            return companyInstance;
        }

        public Company GetBooksCompanyDetailsByCompanyMasterId(long companyMasterId)
        {
            //https://booksapi-qa.realpage.com/customercompany?filter[customerCompanyId]=in:379&include=customerCompanyLocation
            //string uri = $"customercompany?filter[customerCompanyId]=in:{companyMasterId}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999";
            string uri = $"customercompany?filter[customerCompanyId]=in:{companyMasterId}&include=customerCompanyLocation";

            var logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri } };
            WriteToLog(LogEventLevel.Debug, $"GetBooksCompanyDetailsByCompanyMasterId - Getting info - companyMasterId:{companyMasterId}", logData, correlationId: _defaultUserClaim.CorrelationId.ToString());
            var response = GetAsync(uri).Result;
            if (response.IsSuccessStatusCode)
            {
                //var companyInstance = JsonConvert.DeserializeObject<Company>(response.Content.ReadAsStringAsync().Result.Replace("company", "customercompany"), new JsonApiSerializerSettings());
                var companyInstance = JsonConvert.DeserializeObject<List<Company>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                logData = new Dictionary<string, object>() { { "CompanyInstance", companyInstance } };
                WriteToLog(LogEventLevel.Debug, $"GetBooksCompanyDetailsByCompanyMasterId - Got info - companyMasterId:{companyMasterId}", logData, correlationId: _defaultUserClaim.CorrelationId.ToString());
                return companyInstance[0];
            }

            logData = new Dictionary<string, object>() { { "response", response } };
            WriteToLog(LogEventLevel.Debug, $"GetBooksCompanyDetailsByCompanyMasterId - No info found - companyMasterId:{companyMasterId}", logData, correlationId: _defaultUserClaim.CorrelationId.ToString());

            return new Company();
        }

        public List<CustomerCompanyDomain> GetListOfDomainsByCompany(long companyMasterId)
        {
            //domain/customercompany/379
            string uri = $"domain/customercompany/{companyMasterId}";

            var logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri } };
            WriteToLog(LogEventLevel.Debug, $"GetListOfDomainsByCompany - Getting info - companyMasterId:{companyMasterId}", logData, correlationId: _defaultUserClaim.CorrelationId.ToString());
            var response = GetAsync(uri).Result;
            if (response.IsSuccessStatusCode)
            {
                var companyInstance = JsonConvert.DeserializeObject<List<CustomerCompanyDomain>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                logData = new Dictionary<string, object>() { { "CompanyInstance", companyInstance } };
                WriteToLog(LogEventLevel.Debug, $"GetListOfDomainsByCompany - Got info - companyMasterId:{companyMasterId}", logData, correlationId: _defaultUserClaim.CorrelationId.ToString());
                return companyInstance;
            }

            logData = new Dictionary<string, object>() { { "response", response } };
            WriteToLog(LogEventLevel.Debug, $"GetListOfDomainsByCompany - No info found - companyMasterId:{companyMasterId}", logData, correlationId: _defaultUserClaim.CorrelationId.ToString());

            return new List<CustomerCompanyDomain>();
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
                var currentCompanyMasterId = GetCompanyMasterIdForRPDMID(companyRealPageId.ToString(), domain);
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
                WriteToLog(LogEventLevel.Debug, "GetCompanyCustomerInfo - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    //companyInstance = response.Content.ReadAsJsonApiAsync<CompanyResource>(_contractResolver, _cache).Result;
                    companyInstance = JsonConvert.DeserializeObject<CustomerCompany>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() {{"CompanyInstance", companyInstance}};
                    WriteToLog(LogEventLevel.Debug, "GetCompanyCustomerInfo - Got info.", logData);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogEventLevel.Debug, "GetCompanyCustomerInfo - No info found.", logData);
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
                WriteToLog(LogEventLevel.Debug, "GetVCompanyPropertyMap - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    vCompanyPropertyMap = JsonConvert.DeserializeObject<List<CustomerCompanyPropertyMap>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() {{"vCompanyPropertyMapResource", vCompanyPropertyMap}};
                    WriteToLog(LogEventLevel.Debug, "GetVCompanyPropertyMap - Got info.", logData);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogEventLevel.Debug, "GetVCompanyPropertyMap - No info found.", logData);
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
                WriteToLog(LogEventLevel.Debug, "ManageBlueBook.GetCustomerProperty - Getting info.", logData);
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
                    WriteToLog(LogEventLevel.Debug, "ManageBlueBook.GetCustomerProperty - Got info.", logData);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogEventLevel.Debug, "ManageBlueBook.GetCustomerProperty - No info found.", logData);
                    return null;
                }

                return productPropertyList;
            });
            return productPropertyList;
        }

        /// <summary>
        /// Used to get property instance details
        /// </summary>
        /// <param name="propertyInstanceId"></param>
        /// <returns></returns>
        public CustomerProperty GetCustomerPropertyDetails(string propertyInstanceId)
        {
            if (String.IsNullOrEmpty(propertyInstanceId))
            {
                throw new Exception("Invalid parameter propertyInstanceId.");
            }
            CustomerProperty customerProperty = new CustomerProperty();
            //List<ProductProperty> productPropertyList = new List<ProductProperty>();

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"getCustomerPropertyDetails_{propertyInstanceId}";

            customerProperty = rpcache.GetFromCache<CustomerProperty>(cacheKey, CacheTimeSeconds, () =>
            {
                string uri = $"customerproperty/{propertyInstanceId}";
                Dictionary<string, object> logData = new Dictionary<string, object>() {{"uri", _httpClient.BaseAddress + uri}};
                WriteToLog(LogEventLevel.Debug, "ManageBlueBook.GetCustomerPropertyDetails - Getting info.", logData);
                var response = GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    customerProperty = JsonConvert.DeserializeObject<CustomerProperty>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                     logData = new Dictionary<string, object>() {{"CustomerProperty", customerProperty}};
                    WriteToLog(LogEventLevel.Debug, "ManageBlueBook.GetCustomerPropertyDetails - Got info.", logData);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogEventLevel.Debug, "ManageBlueBook.GetCustomerPropertyDetails - No info found.", logData);
                    return null;
                }

                return customerProperty;
            });
            return customerProperty;
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
        /// <param name="correlationId"></param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, string correlationId = "" )
        {
            if (_defaultUserClaim != null)
            {
                correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";
            }
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);
            logger.Write(logType, exception, message);
        }

        private bool GetBooleanProductSettings(string settingName)
        {
            if (productInternalSettingList.Any(p => p.Name.Equals(settingName, StringComparison.OrdinalIgnoreCase)))
            {
                return Convert.ToBoolean(int.Parse(productInternalSettingList.First(a => a.Name.Equals(settingName, StringComparison.OrdinalIgnoreCase)).Value));
            }

            return false;
        }

    }
}




















