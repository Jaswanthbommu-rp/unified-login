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
                    Dictionary<string, object> logData = new Dictionary<string, object>() {{"response", translateCompanyInstance}};
                    WriteToLog(LogType.Diagnostic, "GetTranslateFromUPFMToProduct - Got info.", logData);
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
                //string uri = $"dashboard/gb/getCompanyPropertyInstances?funcargs={companyRealPageId}";
                string uri = $"dashboard/gb/getCompanyPropertyInstances?funcargs={379}";
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

                    List<Guid> properties = new List<Guid>();

                    //properties.Add(new Guid(""));
                    properties.Add(new Guid("7511F398-C54A-4D5F-A91B-6140B372EB32"));
                    properties.Add(new Guid("1B3EF6AA-5FCA-49D4-9B75-7879AA56D45D"));
                    properties.Add(new Guid("91D282E2-5A75-4851-A366-065AC7E3C18A"));
                    properties.Add(new Guid("570F6E27-609A-4D69-814E-F90064D12701"));
                    properties.Add(new Guid("35E958CD-F70A-4C59-932E-62D0D718F7FE"));
                    properties.Add(new Guid("8AFC8595-4D6C-4346-8E1E-36FA53667FF1"));
                    properties.Add(new Guid("7E980609-5A13-4673-9598-827951B9401E"));
                    properties.Add(new Guid("8A4C97E5-8FAB-42AE-8AC2-36FC90AA76B3"));
                    properties.Add(new Guid("465F0B4C-F608-4B04-8BEE-AD98F28B3EEB"));
                    properties.Add(new Guid("FD5C7DC6-3248-4B90-A0E2-148830E7D250"));
                    properties.Add(new Guid("0B617967-2EF7-436D-A262-FC2867F8D7C2"));
                    properties.Add(new Guid("5F840747-8641-4B42-9EB7-B374E4FB9536"));
                    properties.Add(new Guid("98484E78-4BB7-431E-B22F-545E2BA4ED2E"));
                    properties.Add(new Guid("A5A0DCE1-ED63-4EDD-BB96-92F7AB0DA6BD"));
                    properties.Add(new Guid("9E17F140-23D6-439A-A6D1-6129DF618EC3"));
                    properties.Add(new Guid("9C5E8BFF-1E7E-420C-AE47-68FFB9906CE5"));
                    properties.Add(new Guid("6261018B-71DA-4D55-85AB-3C3657081B6F"));
                    properties.Add(new Guid("C97F61CC-9CD7-495E-9EB8-71DE4833E1DE"));
                    properties.Add(new Guid("0AA1E3FE-95C1-4D88-94B0-DC346F095DA8"));
                    properties.Add(new Guid("6D483E01-85B2-4144-AEC6-033EDF416CD9"));
                    properties.Add(new Guid("947EED29-A62F-40A6-A92E-052A488AAB16"));
                    properties.Add(new Guid("BCC11E12-A01D-41B9-B8B2-D69CEDB2E93A"));
                    properties.Add(new Guid("BA62A537-0FCB-4C75-9E14-8A0B91414667"));
                    properties.Add(new Guid("778609C6-E43C-4F33-BE59-7B602E39A7AE"));
                    properties.Add(new Guid("D91C0FCF-4B0E-4EE2-B848-08E9741FBA90"));
                    properties.Add(new Guid("FD82C92D-BE19-46DA-A81D-DE94714622F3"));
                    properties.Add(new Guid("9924691D-7FAE-48E0-8C6B-85CD8A261236"));
                    properties.Add(new Guid("808B6D50-A5ED-4E68-A252-D80128599085"));
                    properties.Add(new Guid("54C61F57-41B5-4A8F-8093-8C721F0C5F0D"));
                    properties.Add(new Guid("8A206136-EE0C-44A0-BF2A-BA3B0A747EB6"));
                    properties.Add(new Guid("C9BB4253-97EA-45AC-82C8-2155526B017C"));
                    properties.Add(new Guid("C158D5DC-862E-4C87-8044-F89D41D8E1C9"));
                    properties.Add(new Guid("405DCCCC-E873-4097-BEE8-AC0A444552E9"));
                    properties.Add(new Guid("46092887-C71B-4F0C-B2DF-97775542EA93"));
                    properties.Add(new Guid("B7F29F91-1CB9-4F53-9BB7-B3636C808FA8"));
                    properties.Add(new Guid("C724A8A3-3506-4FCC-8110-8EE8DCE82049"));
                    properties.Add(new Guid("4EE47DA3-6093-4DCE-AF5A-B9D460B18BC6"));
                    properties.Add(new Guid("BF45DE50-262A-4182-82A1-13BC666ECA9A"));
                    properties.Add(new Guid("F3DC4F1A-4A97-400B-B9A4-8170E143BAA5"));
                    properties.Add(new Guid("6CEA9500-AFE7-4522-84F4-A8EF155ADA68"));
                    properties.Add(new Guid("3092C111-E2DB-48E1-B1B0-4846980CB979"));
                    properties.Add(new Guid("61145935-8D81-4E74-990A-6496AD4929A9"));
                    properties.Add(new Guid("75E5B0EC-C7AB-4921-B30C-64221A02B38D"));
                    properties.Add(new Guid("0C0FBC52-899B-475D-A889-EDEA3FE50984"));
                    properties.Add(new Guid("8B159CE6-70A4-46FE-9CBA-6938EC637431"));
                    properties.Add(new Guid("5B0FBDF5-BEA2-40AC-AFED-4984C3B29FCA"));
                    properties.Add(new Guid("CC3AA3BF-91F2-489C-A9BB-4193E5A5A0B1"));
                    properties.Add(new Guid("B3EE82FC-37F9-49D8-A4CB-80489D8B354B"));
                    properties.Add(new Guid("25266F16-F39F-4852-95D4-B59DDF87D168"));
                    properties.Add(new Guid("B089F192-8E73-416A-9FF7-C31DB66DC5B9"));
                    properties.Add(new Guid("641C2297-C9FB-4239-8E8C-2B748A1EDDAD"));
                    properties.Add(new Guid("05F7D4A3-C427-46D6-9FFF-7EAF9ECEB997"));
                    properties.Add(new Guid("BE179F81-DB93-4BA6-B52C-A8FDC27EB90A"));
                    properties.Add(new Guid("74FCCEC9-4759-42AF-A823-B243EFF0ABB5"));
                    properties.Add(new Guid("69BA7290-C200-40C1-AA69-B350C62DF4D3"));
                    properties.Add(new Guid("393B67A3-A209-41E4-9F86-1A8CA685D62F"));
                    properties.Add(new Guid("220BDED0-37F5-419B-BBB8-E5125D35F7C7"));
                    properties.Add(new Guid("3284E4BC-0349-4A7E-9D53-B5D14A53FD47"));
                    properties.Add(new Guid("48BC3DB8-4008-419B-8D77-D153A8BF7641"));
                    properties.Add(new Guid("2420FBF0-59F1-4F59-8604-C093DE3FAA95"));
                    properties.Add(new Guid("41C16282-B682-40A0-9063-442FB8AABC5C"));
                    properties.Add(new Guid("1CBA23CF-BAC8-4154-8750-2F6116EE1D3C"));
                    properties.Add(new Guid("E3C0CDF5-9068-46C4-9187-FC231DA277B1"));
                    properties.Add(new Guid("CA831FF5-8CFF-4D0E-B6EA-F3122FD888DF"));
                    properties.Add(new Guid("75098B4A-FA01-426A-A7F1-BF551234C858"));
                    properties.Add(new Guid("F09394F1-32D5-4768-A611-5CA0629E9C4E"));
                    properties.Add(new Guid("DC2C97E9-CA73-4B22-93DB-63037E62F572"));
                    properties.Add(new Guid("1651C15B-6048-4FAC-BE9E-47DD057A0516"));
                    properties.Add(new Guid("9EFD8B64-BFF7-400E-8701-498BDE919BC9"));
                    properties.Add(new Guid("8BDF79CB-94A7-44E6-8188-19B6D8DBAB9B"));
                    properties.Add(new Guid("D8A512F2-04EF-4266-809A-F9D018325D31"));
                    properties.Add(new Guid("2AA44A73-2C41-4397-B8E2-D846C9C872CF"));
                    properties.Add(new Guid("90794D03-2580-46C0-BFE8-96F89FD0243D"));
                    properties.Add(new Guid("E6FF8EF0-6DB7-4433-A825-19313CF84BD5"));
                    properties.Add(new Guid("C003D277-4F40-4A32-BC47-DEF3F032BC34"));
                    properties.Add(new Guid("D625F6BD-3A57-42E0-BBB9-36D7429EDBC5"));
                    properties.Add(new Guid("0D7AE4CD-F89A-4FB1-B04F-DB7D9FB57851"));
                    properties.Add(new Guid("AC6C4115-D83F-44BC-82B0-D53B81D0E418"));
                    properties.Add(new Guid("B6387C6F-5F0B-425A-B8F4-C9D073E8875F"));
                    properties.Add(new Guid("38A99B44-1003-4540-8593-A2B65FA061AF"));
                    properties.Add(new Guid("20C53C01-DD6D-4999-A148-0C64318D1972"));
                    properties.Add(new Guid("6CBD762A-D704-43B8-96AC-B1CBD6705AB9"));
                    properties.Add(new Guid("F8D05783-64B7-47A9-95AE-93320C02380E"));
                    properties.Add(new Guid("56DDDFD2-3931-4D09-8B4B-E316084BA664"));
                    properties.Add(new Guid("0B87A005-F327-4975-AA21-1E696FBD8A96"));
                    properties.Add(new Guid("7887D8E0-E68C-4172-A160-A836B8C0BAC8"));
                    properties.Add(new Guid("53346525-42B4-4F05-9797-C8D2455D73D8"));
                    properties.Add(new Guid("45D4C128-87F7-4A8B-A320-2C378196C59A"));
                    properties.Add(new Guid("0DA6E33A-A841-4486-888A-F69FC3480888"));
                    properties.Add(new Guid("58C17534-72E3-4128-8A8D-69913241336F"));
                    properties.Add(new Guid("47BC1DC6-B299-4075-87D6-DA80124EB790"));
                    properties.Add(new Guid("F0828F88-EBFB-43AB-B7DC-0D90DD726EEB"));
                    properties.Add(new Guid("F2E785FD-774D-479E-B00E-F62614E3122B"));
                    properties.Add(new Guid("A8FE9622-4E10-4177-A192-5E3879A1CD89"));
                    properties.Add(new Guid("6581B487-9F3E-48DC-9B33-761B10B0B281"));
                    properties.Add(new Guid("28026F6C-6D18-418B-BC4E-71487CC9B97B"));
                    properties.Add(new Guid("B19702F3-86CB-4A56-AAFD-50C6149C741F"));
                    properties.Add(new Guid("C2214E58-C5BC-4CB7-A520-A7DF6C0A6018"));
                    properties.Add(new Guid("893253BF-CF30-4B90-B466-891990320ACC"));
                    properties.Add(new Guid("3448ED90-8EDC-432D-95AB-BF437A8A01DE"));
                    properties.Add(new Guid("135A763E-235B-42D1-88A6-DA0756410CFC"));
                    properties.Add(new Guid("E0D135B4-66D2-4989-870F-2D8E6A53351B"));
                    properties.Add(new Guid("4E7D8E5F-5189-4B11-BA72-E7454DC6E8FB"));
                    properties.Add(new Guid("2B31182C-78A7-4A8F-AC7E-C4B2FAF94681"));
                    properties.Add(new Guid("7505A95B-6C87-4C78-9C23-0B4A7498F58E"));
                    properties.Add(new Guid("D1FC71DF-BF2E-4E65-BFD3-3CEA60EB9D35"));
                    properties.Add(new Guid("607D59B6-E3EB-420C-9BB4-16B53DA0B9B7"));
                    properties.Add(new Guid("78637BCE-B7A0-498E-BF60-ED633349D644"));
                    properties.Add(new Guid("2C1C26DB-5146-4E56-B0E1-21068114AC10"));
                    properties.Add(new Guid("C8C5B463-A9DE-43D2-AB81-11F87F1460ED"));
                    properties.Add(new Guid("37CF1887-D84C-4A4C-AFD3-894D2140C744"));
                    properties.Add(new Guid("CDE160CF-C35B-49E6-B732-80535C23BF97"));
                    properties.Add(new Guid("0D23CDB8-5A60-42A2-B018-188A94A73C78"));
                    properties.Add(new Guid("1492F3AC-8F60-43FF-9D93-1BFDE5AA9D7D"));
                    properties.Add(new Guid("5DADEB20-1259-4B42-AA71-90A270016946"));
                    properties.Add(new Guid("E57EA938-6D1D-40E5-A6B0-86CE155D2E13"));
                    properties.Add(new Guid("D4FB3551-C378-4999-9322-6A9465428BB5"));
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