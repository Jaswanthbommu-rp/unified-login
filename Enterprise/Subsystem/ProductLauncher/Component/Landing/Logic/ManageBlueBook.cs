using JsonApiSerializer;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
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

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    // get the list of onesite properties for the given BlackBook OneSite id
    // http://booksapi-stg.realpage.com/companypropertyinstancemap?filter[companyInstanceId]=650532&include=propertyInstance&fields[propertyInstance]=propertyInstanceSourceId,propertyInstanceId,source,propertyName,isActive

    // get all the products for a given company
    // http://booksapi-stg.realpage.com/companymap?filter[companyId]=2326
    // http://booksapi-stg.realpage.com/companymap?filter[companyId]=2326&include=companyInstance

    /// <summary>
    /// Manage BlueBook APIs
    /// </summary>
    public class ManageBlueBook : IDisposable, IManageBlueBook
    {
        private DefaultUserClaim _defaultUserClaim;

        const string MediaTypeName = "application/vnd.api+json";
        const int CacheTimeSeconds = 300;
        const int AuthTokenRefreshMinutes = 50;
        const int LandingProductID = 3;

        const int MAXRETRYCOUNT = 5;

        readonly HttpClient _httpClient;
        readonly IList<ProductInternalSetting> productInternalSettingList;
        readonly IProductInternalSettingRepository _productInternalSettingRepository;

        readonly AuthTokenData _authTokenInfo = new AuthTokenData();

        private bool useDomains = false;
        private bool useRPFMId = false;

        ObjectCache _manageBlueBookCache = MemoryCache.Default;

        /// <summary>
        /// Default constructor
        /// </summary>
        [Obsolete]
        public ManageBlueBook()
        {
            string bbUri = "";

            #region GetSettings

            productInternalSettingList = _manageBlueBookCache["productInternalSetting_" + LandingProductID] as List<ProductInternalSetting>;
            if (productInternalSettingList == null)
            {
                _productInternalSettingRepository = new ProductInternalSettingRepository();
                productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(LandingProductID);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                _manageBlueBookCache.Set("productInternalSetting_" + LandingProductID, productInternalSettingList, policy);
            }

            #endregion

            bbUri = productInternalSettingList.First(a => a.Name.Equals("BlueBookAPIEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)))
            {
                useDomains = Convert.ToBoolean(productInternalSettingList.First(a => a.Name.Equals("BlueBookAPIEndPoint", StringComparison.OrdinalIgnoreCase)).Value);
            }

            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseRPFMId", StringComparison.OrdinalIgnoreCase)))
            {
                useRPFMId = Convert.ToBoolean(productInternalSettingList.First(a => a.Name.Equals("BooksUseRPFMId", StringComparison.OrdinalIgnoreCase)).Value);
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

            productInternalSettingList = _manageBlueBookCache["productInternalSetting_" + LandingProductID] as List<ProductInternalSetting>;
            if (productInternalSettingList == null)
            {
                _productInternalSettingRepository = new ProductInternalSettingRepository();
                productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(LandingProductID);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                _manageBlueBookCache.Set("productInternalSetting_" + LandingProductID, productInternalSettingList, policy);
            }

            #endregion

            bbUri = productInternalSettingList.First(a => a.Name.Equals("BlueBookAPIEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseDomains", StringComparison.OrdinalIgnoreCase)))
            {
                useDomains = Convert.ToBoolean(productInternalSettingList.First(a => a.Name.Equals("BlueBookAPIEndPoint", StringComparison.OrdinalIgnoreCase)).Value);
            }

            if (productInternalSettingList.Any(p => p.Name.Equals("BooksUseRPFMId", StringComparison.OrdinalIgnoreCase)))
            {
                useRPFMId = Convert.ToBoolean(productInternalSettingList.First(a => a.Name.Equals("BooksUseRPFMId", StringComparison.OrdinalIgnoreCase)).Value);
            }

            useDomains = true;
            useRPFMId = true;
            //bbUri = "https://booksapi.realpage.com";
            //_authTokenInfo.Data.Name = "OS";//productInternalSettingList.First(a => a.Name.ToUpper() == "BLUEBOOKAPIUSER").Value;
            //_authTokenInfo.Data.Password = "P>qx3g6MEkt(G:-";//productInternalSettingList.First(a => a.Name.ToUpper() == "BLUEBOOKAPIPASSWORD").Value;

            _httpClient = new HttpClient {BaseAddress = new Uri(bbUri)};
        }

        public ManageBlueBook(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository)
        {
            _productInternalSettingRepository = productInternalSettingRepository;
            _defaultUserClaim = userClaim;
        }

        public ManageBlueBook(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, HttpMessageHandler messageHandler)
        {
            _productInternalSettingRepository = productInternalSettingRepository;
            _httpClient = new HttpClient(messageHandler) {BaseAddress = new Uri("http://localhost")};
            _defaultUserClaim = userClaim;
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
        /// <returns>List of CompanyMapResource</returns>
        public IList<CustomerCompanyMap> GetCompanyMap(Guid companyRealPageId, long booksCompanyMasterId, string source, string domain, string includeExtra = "", bool includeGreenBookCares = true)
        {
            if (booksCompanyMasterId == -1)
            {
                // shortcut out for RealPage Employee company
                return null;
            }
            IList<CustomerCompanyMap> companyMap = new List<CustomerCompanyMap>();

            if (useRPFMId && companyRealPageId != Guid.Empty && !string.IsNullOrEmpty(source))
            {
                companyMap = GetTranslateFromUPFMToProduct(companyRealPageId.ToString().ToUpper(), source, domain);
                if (companyMap != null)
                {
                    return companyMap;
                }
            }

            if (useRPFMId && companyRealPageId != Guid.Empty)
            {
                // need to send guid in uppercase because books is case sensitive.
                var newCompanyMasterId = GetCompanyMasterIdForRPDMID(companyRealPageId.ToString().ToUpper(), domain);
                booksCompanyMasterId = (newCompanyMasterId != 0) ? newCompanyMasterId : booksCompanyMasterId;
            }
            
            string domainFilter = "";

            if (source == null)
            {
                source = "";
            }

            if (!string.IsNullOrEmpty(domain) && useDomains)
            {
                //includeGreenBookCares = false;
                domainFilter = $"filter[companyInstance.domain]={domain}&";
            }

            string companyFilter = $"filter[customerCompanyId]={booksCompanyMasterId}&";

            companyMap = _manageBlueBookCache[$"getCompanyMapResource_{booksCompanyMasterId}_{source}_{includeExtra}_{domainFilter}"] as List<CustomerCompanyMap>;

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
                    //companyMap = response.Content.ReadAsJsonApiManyAsync<CompanyMap>(_contractResolver, _cache).Result;
                    companyMap = JsonConvert.DeserializeObject<List<CustomerCompanyMap>>(response.Content.ReadAsStringAsync().Result, new JsonApiSerializerSettings());
                    logData = new Dictionary<string, object>() {{"companyMap", companyMap}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyMap - Got info.", logData);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                    _manageBlueBookCache.Set($"getCompanyMapResource_{booksCompanyMasterId}_{source}_{includeExtra}_{domainFilter}", companyMap, policy);
                }
                else
                {
                    logData = new Dictionary<string, object>() {{"response", response}};
                    WriteToLog(LogType.Diagnostic, "GetCompanyMap - No info found.", logData);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // return an empty CompanyMapResource because it wasn't found
                        return companyMap;
                    }

                    return null;
                }
            }

            return companyMap;
        }

        private IList<CustomerCompanyMap> GetTranslateFromUPFMToProduct(string companyRealPageId, string productSource, string domain)
        {
            //translate/companyinstance/684382D3-F2F8-4F42-8D29-935F834C6888/UPFM/OS?filter[customerEnvironment]=Primary
            string uri = $"translate/companyinstance/{companyRealPageId}/UPFM/{productSource}?filter[customerEnvironment]={domain}";

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
                    map.CompanyInstanceSourceId = translateCompanyInstance.Data.Attributes.CompanyInstanceSourceId;
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
            //string uri = $"customercompanymap?filter[source]=UPFM&filter[companyInstanceSourceId]={companyRealPageId}&" + (includeGreenBookCares ? "filter[companyInstance.greenBookCares]=true&" : "" ) + domainFilter + $"include=companyInstance";
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
            if (booksCustomerMaster == null)
            {
                return 0;
            }

            return booksCustomerMaster.CustomerCompanyId;
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
                var clientResponse = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
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
        /// <param name="booksCompanyMasterIds"></param>        
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
            string uri = $"customercompany?filter[customerCompanyId]=in:{booksCompanyMasterIds.ToString()}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999";

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
        /// Used to get the information about the company for RPUP
        /// </summary>
        /// <param name="booksCompanyMasterId"></param>        
        /// <returns></returns>
        public CustomerCompany GetCompanyCustomerInfo(long booksCompanyMasterId)
        {
            CustomerCompany companyInstance = new CustomerCompany();

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