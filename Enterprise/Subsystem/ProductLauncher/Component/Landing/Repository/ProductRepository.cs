using Dapper;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using EnterpriseProductUser = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise.ProductUsers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    /// <summary>
    /// Product Repository
    /// </summary>
    public class ProductRepository : BaseRepository, IProductRepository
    {
        private DefaultUserClaim _userClaim;
        IProductInternalSettingRepository _productInternalSettingRepository;
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
        private readonly IRepository _repository;
        private readonly IPersonaRepository _personaRepository;


        #region Ctor
        /// <summary>
        /// base Constructor
        /// </summary>
        public ProductRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _personaRepository = new PersonaRepository(_userClaim);
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        public ProductRepository(IRepository repository, DefaultUserClaim userClaim) : base(repository)
        {
            _userClaim = userClaim;
            _repository = repository;
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _personaRepository = new PersonaRepository(repository, userClaim);
        }

        /// <summary>
        /// Unit Test case.
        /// </summary>
        /// <param name="repository"></param>
        public ProductRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Used when the user is known
        /// </summary>
        /// <param name="userClaim"></param>
        public ProductRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
        {
            if (userClaim == null)
                userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };

            _userClaim = userClaim;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _personaRepository = new PersonaRepository(_userClaim);
        }

        #endregion

        /// <summary>
        /// Used to get a list of products for the given persona id
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="statusType"></param>
        /// <returns></returns>
        public IList<PersonaProduct> GetAllProductsByPersona(long personaId, ProductBatchStatusType statusType)
        {
            var procName = StoredProcNameConstants.SP_GetProductsByPersonaId;

            dynamic param = new
            {
                PersonaId = personaId,
                StatusTypeId = (int)statusType
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetMany<PersonaProduct>(procName, param);
                return result;
            }
        }

        /// Used to get a list of products for the given persona id
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="statusType"></param>
        /// <returns></returns>
        public IList<PersonaProductUserDetails> ListProductsByPersonaId(long personaId, int statusType)
        {
            var procName = StoredProcNameConstants.SP_ListProductsByPersonaId;

            dynamic param = new
            {
                PersonaId = personaId,
                ProductStatusValue = statusType
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetMany<PersonaProductUserDetails>(procName, param);
                return result;
            }
        }

        /// <summary>
        /// Get the SAML attribute names, types, and values by PersonaId and ProductId
        /// </summary>
        /// <param name="personaId">User personaId</param>
        /// <param name="productId">ProductId</param>
        /// <returns>list SamlAttributes object</returns>
        public IList<SamlAttributes> GetProductSamlDetails(long personaId, int productId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails, new { personaId, productId }).ToList();
            }
        }

        #region Public Methods

        /// <summary>
        /// Returns a list of products user has access to, filterable by favorites and resource only
        /// </summary>
        /// <param name="persona">persona</param>       
        /// <param name="productSelectType">productSelectType</param>
        /// <param name="security">security</param>
        /// <returns></returns>
        public IList<PersonaProductUserDetails> GetAssignedProductsByPersona(Persona persona, ProductSelectType? productSelectType = null, RouteSecurity security = null)
        {
            List<ProductEnum> isFavouriteProducts = new List<ProductEnum>();
            bool IsFavorite = false;
            ProductType productType = new ProductType();
            ProductSettingList productSetting = new ProductSettingList();
            ProductInternalSetting productInternalSetting = new ProductInternalSetting();
            var productInternalSettingList = new List<ProductInternalSetting>();
            IList<PersonaProductUserDetails> userProducts = new List<PersonaProductUserDetails>();

            IList<ProductUI> listProductUI = this.GetProducts(organizationRealPageId: persona.Organization.RealPageId, allProducts: true, replaceProductCodeWithUDMIfExists: true);

            using (var repository = GetRepository())
            {
                userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = persona.PersonaId, ProductStatusValue = ((Int32)UserUiStatusType.AccountCreationSuccessful).ToString() }).ToList();
            }

            IList<ProductSettingList> productSettings = GetProductSettingsByPersona(persona.PersonaId).ToList();
            IList<ProductType> productTypeList = GetProductTypes();

            // is EasyLMS a favorite product?
            CheckUserFavouriteProducts(productSettings, ProductEnum.EasyLMS, isFavouriteProducts);

            // is PropertyPhotos a favorite product?
            CheckUserFavouriteProducts(productSettings, ProductEnum.PropertyPhotos, isFavouriteProducts);

            // is VendorMarketplace a favorite product?
            CheckUserFavouriteProducts(productSettings, ProductEnum.VendorMarketplace, isFavouriteProducts);

            //List of Products By Persona
            userProducts.ToList().ForEach(p =>
            {
                if (p.ProductTypeId != 0)
                {
                    //Solution and Family where the product belong
                    productType = productTypeList.First(i => i.ProductTypeId == p.ProductTypeId);
                    if (productType != null)
                    {
                        p.SolutionId = productType.ProductTypeId;
                        p.Solution = productType.Name;
                        p.FamilyId = productType.ParentProductTypeId;
                        p.Family = productType.ParentProductTypeName;
                    }
                }

                //Is this product a Favorite for this Persona?
                IsFavorite = false;
                var setting = productSettings.Where(i => i.ProductId == p.ProductId);
                setting.ToList().ForEach(s =>
                {
                    if (s.Name.Equals("IsFavorite", StringComparison.OrdinalIgnoreCase))
                    {
                        IsFavorite = s.Value.Trim() == "1";
                    }

                    if (s.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && Convert.ToInt32(s.Value) > 0)
                    {
                        p.ProductStatus = Convert.ToInt32(s.Value);
                    }
                });

                //Product Settings

                #region Cache

                RPObjectCache rpcache = new RPObjectCache();
                var cacheKey = "productInternalSetting_" + p.ProductId.ToString();
                productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () =>
                {
                    // load from api
                    using (var settingRepo = GetRepository())
                    {
                        return settingRepo.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, new { ProductId = p.ProductId }).ToList();
                    }
                });

                #endregion

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ClientId", StringComparison.OrdinalIgnoreCase));
                p.ClientId = productInternalSetting?.Value.Trim();

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ClassName", StringComparison.OrdinalIgnoreCase));
                p.ClassName = productInternalSetting?.Value.Trim();

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("IsNewTab", StringComparison.OrdinalIgnoreCase));
                p.IsNewTab = (productInternalSetting != null) && productInternalSetting.Value.Trim() == "1";

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ProductUrl", StringComparison.OrdinalIgnoreCase));
                p.ProductUrl = productInternalSetting?.Value.Trim();

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("SettingsUrl", StringComparison.OrdinalIgnoreCase));
                p.SettingsUrl = productInternalSetting?.Value.Trim();

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("TitleId", StringComparison.OrdinalIgnoreCase));
                p.TitleId = productInternalSetting?.Value.Trim();

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("TitleUniqueId", StringComparison.OrdinalIgnoreCase));
                Guid titleGuid;
                Guid.TryParse(productInternalSetting?.Value, out titleGuid);
                p.TitleUniqueId = titleGuid;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("Subsolution", StringComparison.OrdinalIgnoreCase));
                p.Subsolution = productInternalSetting?.Value.Trim();

                //Is the Product enabled to be a Favorite?
                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("IsFavorite", StringComparison.OrdinalIgnoreCase));
                p.IsAllowFavorite = productInternalSetting != null && productInternalSetting.Value.Trim() == "1";
                //If enabled then, is the product a favorite in persona?
                p.IsFavorite = p.IsAllowFavorite && IsFavorite;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("LearnMore", StringComparison.OrdinalIgnoreCase));
                p.LearnMore = productInternalSetting?.Value.Trim();

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("HasSAccess", StringComparison.OrdinalIgnoreCase));
                p.HasAccess = (productInternalSetting != null) && (productInternalSetting.Value.Trim() == "1");

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("MetatagUniqueId", StringComparison.OrdinalIgnoreCase));
                p.MetatagUniqueId = productInternalSetting?.Value.Trim();

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ShowInAppSwitcher", StringComparison.OrdinalIgnoreCase));
                p.ShowInAppSwitcher = (productInternalSetting != null) && productInternalSetting.Value.Trim() == "1";

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ShowInUserListFilter", StringComparison.OrdinalIgnoreCase));
                p.ShowInUserListFilter = (productInternalSetting != null) && productInternalSetting.Value.Trim() == "1";

                if (productInternalSettingList.Any(item => item.Name.Equals("IsResource", StringComparison.OrdinalIgnoreCase)))
                {
                    productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("IsResource", StringComparison.OrdinalIgnoreCase));
                    p.IsResource = (productInternalSetting != null) && (productInternalSetting.Value.Trim() == "1");
                }

                p.TotalAccounts = 1;
            });


            if (productSelectType.HasValue && productSelectType.Value == ProductSelectType.FavoritesOnly)
            {
                userProducts = userProducts.Where(p => p.IsFavorite == true).ToList();
            }
            else if (productSelectType.HasValue && productSelectType.Value == ProductSelectType.ResourcesOnly)
            {
                IList<PersonaProductUserDetails> otherUserProducts = new List<PersonaProductUserDetails>();
                otherUserProducts = userProducts.Where(p => p.IsResource == false).ToList(); // products which are NOT resource

                userProducts = userProducts.Where(p => p.IsResource == true).ToList();
                // get the rights for the dashboard to see if this user has the resource rights

                // check the rights if they exist to only show what the user has access to

                //Get all resource type and compare with the ones persona has access to
                //If not found add it with HasAccess = false, IsResource = true                
                IList<ProductUI> productResource = this.GetProductsResourceType(persona.Organization.RealPageId);

                // remove any products that aren't in the companies product list
                foreach (PersonaProductUserDetails pd in userProducts.ToList())
                {
                    if (!productResource.Any(p => p.ProductId == pd.ProductId) && pd.ProductId != (int)ProductEnum.ProductLearningPortal)
                    {
                        userProducts.Remove(userProducts.First(p => p.ProductId == pd.ProductId));
                    }
                }

                productResource.ToList<ProductUI>().ForEach(r =>
                {
                    var resource = userProducts.SingleOrDefault(p => p.ProductId == r.ProductId);

                    if (resource == null)
                    {
                        // add the following products to everyone initially
                        if (
                            r.ProductId == (int)ProductEnum.ProductLearningPortal
                            || r.ProductId == (int)ProductEnum.MigrationTool
                            || r.ProductId == (int)ProductEnum.ProductUpdates
                            || r.ProductId == (int)ProductEnum.ProductUpdatesDashboard
                            || r.ProductId == (int)ProductEnum.SupportTool
                            || r.ProductId == (int)ProductEnum.OneSiteConversions
                            || r.ProductId == (int)ProductEnum.SettingsManagement
                            || r.ProductId == (int)ProductEnum.CIMPL
                            || r.ProductId == (int)ProductEnum.VendorMarketplace
                            || r.ProductId == (int)ProductEnum.HelpCenter
                            || r.ProductId == (int)ProductEnum.PMEDasboard
                            || r.ProductId == (int)ProductEnum.P2EngagementQueue
                            || r.ProductId == (int)ProductEnum.UnifiedSettings
                            || r.ProductId == (int)ProductEnum.ESupply
                            || r.ProductId == (int)ProductEnum.ManagedServices
                            || r.ProductId == (int)ProductEnum.TrustDashboard
                        )
                        {
                            userProducts.Add(new PersonaProductUserDetails
                            {
                                ProductId = r.ProductId,
                                ProductName = r.ProductName,
                                TitleId = r.TitleId,
                                IsResource = true,
                                ProductUrl = r.ProductUrl,
                                HasAccess = true,
                                IsNewTab = r.IsNewTab,
                                PersonaId = persona.PersonaId
                            });
                        }
                    }
                    else
                    {
                        resource.HasAccess = true;
                    }
                });

                if (_userClaim.Rights.All(rght => rght != null && !rght.Equals("ProductLearningPortal", StringComparison.OrdinalIgnoreCase)))
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.ProductLearningPortal))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.ProductLearningPortal));
                    }
                }
                if (_userClaim.Rights.All(rght => rght != null && !rght.Equals("AccessPMEDashboard", StringComparison.OrdinalIgnoreCase)))
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.PMEDasboard))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.PMEDasboard));
                    }
                }
                if (_userClaim.Rights.All(rght => rght != null && !rght.Equals("AccessP2EngagementQueue", StringComparison.OrdinalIgnoreCase)))
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.P2EngagementQueue))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.P2EngagementQueue));
                    }
                }
                if (_userClaim.Rights.All(rght => rght != null && !rght.Equals("MigrationTool", StringComparison.OrdinalIgnoreCase)) || _userClaim.RealPageEmployee)
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.MigrationTool))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.MigrationTool));
                    }
                }

                if (!_userClaim.Rights.Any(rght => rght.Equals("AccessToUnifiedPlatform", StringComparison.OrdinalIgnoreCase)
                                                   || rght.Equals("AccessToUnifiedSettings", StringComparison.OrdinalIgnoreCase)
                                                   || rght.Equals("ViewOnlySupportToolAccess", StringComparison.OrdinalIgnoreCase)))
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.SupportTool))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.SupportTool));
                    }
                }

                if ((_userClaim.Rights.All(rght => !rght.Equals("AccessOneSiteConversions", StringComparison.OrdinalIgnoreCase))) || ((otherUserProducts.Any(a => a.ProductId == (int)ProductEnum.OneSite).Equals(false)) || ((otherUserProducts.Any(a => a.ProductId == (int)ProductEnum.OneSite).Equals(true)) && otherUserProducts.FirstOrDefault(a => a.ProductId == (int)ProductEnum.OneSite).ProductStatus != (int)ProductBatchStatusType.Success)))
                {
                    // User should have OneSite Product and OneSite Conversions Right assigned
                    if ((userProducts.Any(a => a.ProductId == (int)ProductEnum.OneSiteConversions)))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.OneSiteConversions));
                    }
                }

                if (_userClaim.Rights.All(rght => rght != null &&
                                          !rght.Equals("ViewCIMPLQuestions", StringComparison.OrdinalIgnoreCase) &&
                                          !rght.Equals("EmployeeViewCIMPLQuestions", StringComparison.OrdinalIgnoreCase)))
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.CIMPL))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.CIMPL));
                    }
                }

                if (_userClaim.Rights.All(rght => rght != null && !rght.Equals("ViewUnifiedSettings", StringComparison.OrdinalIgnoreCase) &&
                                                                    !rght.Equals("Settings", StringComparison.OrdinalIgnoreCase) &&
                                                                     !rght.Equals("ManageSettingsTemplates", StringComparison.OrdinalIgnoreCase) &&
                                                                      !rght.Equals("Managecompanylevelsettings", StringComparison.OrdinalIgnoreCase)))
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.UnifiedSettings))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.UnifiedSettings));
                    }
                }

                if ((listProductUI != null) && (listProductUI.Count > 0))
                {
                    //Remove Product Learning Portal access if the user has access to EasyLMS
                    ProductUI easyLMSProduct = listProductUI.ToList().Find(p => p.ProductId == (int)ProductEnum.EasyLMS);
                    if ((easyLMSProduct != null) && (userProducts.Any(a => a.ProductId == (int)ProductEnum.ProductLearningPortal)))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.ProductLearningPortal));
                    }
                }

                //Remove Resource Settings Management if the user does not have the Ability to access / view Settings Management Console right
                if (_userClaim.Rights.All(rght => !rght.Equals("AccessSettingMGMTConsole", StringComparison.OrdinalIgnoreCase)))
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.SettingsManagement))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.SettingsManagement));
                    }
                }

                //Remove Resource Vendor Marketplace if the user does not have the Ability to access Vendor Marketplace right
                if (_userClaim.Rights.All(rght => rght.Equals("EmployeeAccessVendorMarketPlace", StringComparison.OrdinalIgnoreCase)))

                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.VendorMarketplace))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.VendorMarketplace));
                    }
                }

                // Support Tool User should not have access to Client Portal
                if (_userClaim.ImpersonatedBy != Guid.Empty)
                {
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.ClientPortal))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.ClientPortal));
                    }
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.AdminSupportPortal))
                    {
                        userProducts.Remove(userProducts.First(a => a.ProductId == (int)ProductEnum.AdminSupportPortal));
                    }
                }

                userProducts = userProducts.OrderBy(u => u.ProductName).ToList();
            }
            else
            {
                userProducts = userProducts.Where(p => p.IsResource != true).ToList();

                //Include product EasyLMS to display as a tile if it's assigned to User's Organization 
                if ((listProductUI != null) && (listProductUI.Count > 0))
                {
                    // try adding EasyLMS
                    AddAdditionalProduct(persona, listProductUI, userProducts, ProductEnum.EasyLMS, isFavouriteProducts.Any(p => p == ProductEnum.EasyLMS));

                    // need Marketing Center user for Property Photos, so only show if user also has a MC user
                    if (userProducts.Any(a => a.ProductId == (int)ProductEnum.MarketingCenter).Equals(true) && userProducts.FirstOrDefault(a => a.ProductId == (int)ProductEnum.MarketingCenter).ProductStatus == (int)ProductBatchStatusType.Success)
                    {
                        if (_userClaim.Rights.Any(rght => rght != null && rght.Equals("AccessPropertyPhotos", StringComparison.OrdinalIgnoreCase)))
                        {
                            AddAdditionalProduct(persona, listProductUI, userProducts, ProductEnum.PropertyPhotos, isFavouriteProducts.Any(p => p == ProductEnum.PropertyPhotos));
                        }
                    }
                    if (_userClaim.Rights.Any(rght => rght != null && rght.Equals("AccessUnifiedReporting", StringComparison.OrdinalIgnoreCase)) || _userClaim.Rights.Any(rght => rght != null && rght.Equals("EmployeeAccessUnifiedReportingAdminConsole", StringComparison.OrdinalIgnoreCase)))
                    {
                        AddAdditionalProduct(persona, listProductUI, userProducts, ProductEnum.Reporting, isFavouriteProducts.Any(p => p == ProductEnum.Reporting));
                    }
                }
            }

            userProducts.ToList<ProductUserDetails>().ForEach(p =>
            {
                p.HasAccess = (!productSelectType.HasValue || (productSelectType.HasValue && productSelectType.Value != ProductSelectType.ResourcesOnly)) ? true : p.HasAccess;
                p.ActivitiesList = new List<Activities>() { new Activities() { MetatagUniqueId = new List<string>() { p.MetatagUniqueId } } };
                p.ProductCode = ((ProductEnum)p.ProductId).ToEnumDescription();
                if (p.HasAccess && p.ProductUrl != null)
                {
                    if (p.ProductId != (int)ProductEnum.SupportTool)
                    {
                        p.ProductUrl = $"product-redirect.html?prod={p.ProductId}&persona={persona.PersonaId}";
                    }
                }
            });

            return userProducts;
        }

        /// <summary>
		/// Used to check if the user has set the favourite flag on a particular product
		/// </summary>
		/// <param name="productSettings">The list of products available for the current company</param>
		/// <param name="product">The product to check for</param>
		/// <param name="isFavouriteProducts">A list of products that have been set as favourites</param>
		private static void CheckUserFavouriteProducts(IList<ProductSettingList> productSettings, ProductEnum product, List<ProductEnum> isFavouriteProducts)
        {
            //Is the product a Favorite for this Persona?
            ProductSettingList productSettingList = productSettings.FirstOrDefault(i => (i.ProductId == (int)product && i.Name.Equals("IsFavorite", StringComparison.OrdinalIgnoreCase) && i.Value.Trim() == "1"));
            if (productSettingList != null)
            {
                isFavouriteProducts.Add(product);
            }
        }

        /// <summary>
        /// Used to add a product that does not require product level user ids or product specific data
        /// </summary>
        /// <param name="persona"></param>
        /// <param name="listProductUI"></param>
        /// <param name="userProducts"></param>
        /// <param name="productEnum"></param>
        /// <param name="isFavorite"></param>
        private static void AddAdditionalProduct(Persona persona, IList<ProductUI> listProductUI, IList<PersonaProductUserDetails> userProducts, ProductEnum productEnum, bool isFavorite)
        {
            ProductUI product = listProductUI.ToList().Find(p => p.ProductId == (int)productEnum);
            if (product != null && !userProducts.Any(p => p.ProductId == product.ProductId))
            {
                userProducts.Add(new PersonaProductUserDetails
                {
                    PersonaId = persona.PersonaId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    OrganizationName = persona.Organization.Name,
                    TitleUniqueId = product.TitleUniqueId,
                    MetatagUniqueId = product.TitleId,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    TitleId = product.TitleId,
                    IsResource = product.IsResource,
                    ProductUrl = product.ProductUrl,
                    HasAccess = product.HasAccess,
                    IsNewTab = product.IsNewTab,
                    FamilyId = product.FamilyId,
                    Family = product.Family,
                    IsFavorite = isFavorite,
                    ShowInAppSwitcher = product.ShowInAppSwitcher,
                    ShowInUserListFilter = product.ShowInUserListFilter,
                    Subsolution = product.Subsolution,
                });
            }
        }

        /// <summary>
        /// Returns list of products that are resource type by filtering organization products
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <returns></returns>
        public IList<ProductUI> GetProductsResourceType(Guid organizationRealPageId)
        {
            using (var repository = GetRepository())
            {
                IList<ProductUI> productResources = new List<ProductUI>();
                IList<ProductUI> products = new List<ProductUI>();
                products = this.GetProducts(organizationRealPageId, resourceOnly: true);
                productResources = products.Where(p => p.IsResource == true).ToList();

                if (!productResources.Any(p => p.ProductId == (int)ProductEnum.ProductLearningPortal))
                {
                    productResources.Add(
                        new ProductUI
                        {
                            ProductId = (int)ProductEnum.ProductLearningPortal,
                            ProductName = "Product Learning Portal",
                            TitleId = "Product Learning Portal",
                            ProductUrl = "product/productlearningportal",
                            HasAccess = false,
                            IsResource = true,
                            IsNewTab = true
                        }
                    );
                }

                if (!productResources.Any(p => p.ProductId == (int)ProductEnum.HelpCenter))
                {
                    productResources.Add(
                        new ProductUI
                        {
                            ProductId = (int)ProductEnum.HelpCenter,
                            ProductName = "Simon Help Center",
                            TitleId = "Simon Help Center",
                            ProductUrl = "product/helpcenter",
                            HasAccess = false,
                            IsResource = true,
                            IsNewTab = true
                        }
                    );
                }

                // need to follow up later when conversions is ready to be tested
                /*if (!productResources.Any(p => p.ProductName == "OneSite Conversions"))
				{
				    productResources.Add(new ProductUI { ProductId = 1, ProductName = "OneSite Conversions", TitleId = "OneSite Conversions", ProductUrl = "product/onesiteconversions", HasAccess = false, IsResource = true, IsNewTab = true });
				}
				*/
                return productResources;
            }
        }

        /// <summary>
        /// Returns a list of all product settings that an organization has
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <param name="productId">The id of the product to be retrieved.</param>
        /// <returns></returns>
        public IList<ProductSettingList> GetProductSettings(Guid organizationRealPageId, int productId)
        {
            using (var repository = GetRepository())
            {
                try
                {
                    return repository.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization, new { OrganizationRealPageId = organizationRealPageId, ProductId = productId }).ToList();
                }
                catch (Exception ex)
                {
                    string test = ex.Message;
                    return new List<ProductSettingList>();
                }
            }
        }

        /// <summary>
        /// Returns a list of all product settings that an organization has
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <returns></returns>
        public IList<ProductSettingList> GetProductSettings(Guid organizationRealPageId)
        {
            using (var repository = GetRepository())
            {
                try
                {
                    return repository.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization, new { OrganizationRealPageId = organizationRealPageId }).ToList();
                }
                catch (Exception ex)
                {
                    string test = ex.Message;
                    return new List<ProductSettingList>();
                }
            }
        }

        /// <summary>
        /// Returns a list of all product settings for persona
        /// </summary>
        /// <param name="personaId">personaId</param>
        /// <returns></returns>
        public IList<ProductSettingList> GetProductSettingsByPersona(long personaId)
        {
            if (personaId == 0)
            {
                return new List<ProductSettingList>();
            }

            using (var repository = GetRepository())
            {
                try
                {
                    return repository.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, new { PersonaId = personaId }).ToList();
                }
                catch
                {
                    return new List<ProductSettingList>();
                }
            }
        }

        /// <summary>
        /// Used to get a list of products by company party id
        /// </summary>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        public IList<ProductUI> GetProductsByCompany(long organizationPartyId)
        {
            {
                //IList<ProductUI> products = new List<ProductUI>();
                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"getProductsByCompanyPartyId_{organizationPartyId}";

                IList<ProductUI> products = rpCache.GetFromCache<IList<ProductUI>>(cacheKey, 180, () =>
                {
                    using (var repository = GetRepository())
                    {
                        products = repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, new { PartyId = organizationPartyId }).ToList();
                    }

                    return products;
                });

                return products;
            }
        }

        /// <summary>
        /// Used to get a list of products by company party id
        /// </summary>
        /// <param name="organizationPartyId"></param>
        /// <param name="productId">products "," sepereated</param>
        /// <returns></returns>
        public IList<OrganizationProductUser> GetProductUsersByCompany(long organizationPartyId, string productId)
        {
            {
                //IList<ProductUI> products = new List<ProductUI>();
                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"getProductUsersByCompanyPartyId_{organizationPartyId}";

                IList<OrganizationProductUser> productUsers = rpCache.GetFromCache<IList<OrganizationProductUser>>(cacheKey, 180, () =>
                {
                    using (var repository = GetRepository())
                    {
                        productUsers = repository.GetMany<OrganizationProductUser>(StoredProcNameConstants.SP_ListProductUsersForOrganization, new { OrgPartyId = organizationPartyId, ProductId = productId }).ToList();
                    }

                    return productUsers;
                });

                return productUsers;
            }
        }

        /// <summary>
        /// Used to get a list of products ids for a company by the company guid
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public IList<int> GetProductIdsByCompany(Guid organizationRealPageId)
        {
            if (_repository != null)
            {
                // unit test
                return ProductIdList(null, organizationRealPageId);
            }

            var rpCache = new RPObjectCache();
            var cacheKey = $"getProductIdListByCompanyGuid_{organizationRealPageId}";
            return rpCache.GetFromCache(cacheKey, 180, () => ProductIdList(null, organizationRealPageId));
        }

        /// <summary>
        /// Used to get a list of products ids for a company by the company party id
        /// </summary>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        public IList<int> GetProductIdsByCompany(long organizationPartyId)
        {
            if (_repository != null)
            {
                // unit test
                return ProductIdList(organizationPartyId, null);
            }

            var rpCache = new RPObjectCache();
            var cacheKey = $"getProductIdsByCompanyPartyId_{organizationPartyId}";

            return rpCache.GetFromCache(cacheKey, 180, () => ProductIdList(organizationPartyId, null));
        }

        private IList<int> ProductIdList(long? organizationPartyId, Guid? organizationRealPageId)
        {
            IList<int> productIdList = new List<int>();

            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PartyId = organizationPartyId,
                    OrganizationRealPageId = organizationRealPageId
                };
                IList<ProductUI> productList = repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, param);
                foreach (ProductUI pui in productList)
                {
                    productIdList.Add(pui.ProductId);
                }
            }

            return productIdList;
        }

        /// <summary>
        /// Returns a list of products that an organization has license using its organizationRealPageId
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <param name="personaId">personaId</param>
        /// <param name="resourceOnly">Only return resource type products</param>
        /// <param name="allProducts">Return all product types</param>
        /// <param name="replaceProductCodeWithUDMIfExists">True when the product code being returned should be the UDM code if one exists, otherwise leave productcode alone</param>
        /// <returns>List of Products</returns>
        public IList<ProductUI> GetProducts(Guid organizationRealPageId, long personaId = 0, bool resourceOnly = false, bool allProducts = false, bool replaceProductCodeWithUDMIfExists = true)
        {
            //IList<ProductUI> cachedProducts = new List<ProductUI>();
            RPObjectCache rpCache = new RPObjectCache();
            IList<ProductUI> products = new List<ProductUI>();
            var productInternalSetting = new ProductInternalSetting();
            //var productInternalSettingList = new List<ProductInternalSetting>();

            #region Cache

            //ObjectCache getProductsCache = MemoryCache.Default;

            //try
            //{
            // because the caching stores the data by reference, we have to get a new copy of it otherwise we start modifying the data in the cache, which makes it dirty
            //    ((List<ProductUI>)getProductsCache["getProductsCache"]).ForEach(o => products.Add(o.Clone() as ProductUI));
            //}
            //catch (Exception ex) { products = null; }
            //if (products == null)
            var cacheKey = $"getListProductsByOrganization_{organizationRealPageId}";
            //{
            //using (var repository = GetRepository())
            //{
            products = rpCache.GetFromCache<IList<ProductUI>>(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    return repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, new { OrganizationRealPageId = organizationRealPageId }).ToList();
                }
            });
            IList<ProductSettingList> personaProductSettings = GetProductSettingsByPersona(personaId);
            var productList = GetAllProducts();
            products.ToList().ForEach(p =>
            {
                p.ProductCode = ProductEnumHelper.GetProductCodeByProductId(p.ProductId, productList);
                p.UDMSourceCode = ProductEnumHelper.GetUDMSourceCodeByProductId(p.ProductId, productList);
                if (replaceProductCodeWithUDMIfExists && !string.IsNullOrEmpty(p.UDMSourceCode))
                {
                    p.ProductCode = p.UDMSourceCode;
                }
                var personaSetting = personaProductSettings.Where(i => i.ProductId == p.ProductId);

                personaSetting.ToList().ForEach(s =>
                {
                    if ((s.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase)) && (Convert.ToInt32(s.Value) > 0))
                    {
                        p.ProductStatus = Convert.ToInt32(s.Value);
                    }
                });

                //Product Settings
                cacheKey = $"productInternalSetting_{p.ProductId}";
                var productInternalSettingList = rpCache.GetFromCache(cacheKey, 180, () =>
                {
                    using (var repository = GetRepository())
                    {
                        return repository.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, new { ProductId = p.ProductId }).ToList();
                    }
                });

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ClientId", StringComparison.OrdinalIgnoreCase));
                p.ClientId = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ClassName", StringComparison.OrdinalIgnoreCase));
                p.ClassName = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("IsNewTab", StringComparison.OrdinalIgnoreCase));
                p.IsNewTab = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : false;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ProductUrl", StringComparison.OrdinalIgnoreCase));
                p.ProductUrl = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("SettingsUrl", StringComparison.OrdinalIgnoreCase));
                p.SettingsUrl = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("TitleId", StringComparison.OrdinalIgnoreCase));
                p.TitleId = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("TitleUniqueId", StringComparison.OrdinalIgnoreCase));
                Guid titleGuid;
                Guid.TryParse(productInternalSetting?.Value, out titleGuid);
                p.TitleUniqueId = titleGuid;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("Subsolution", StringComparison.OrdinalIgnoreCase));
                p.Subsolution = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                //Is the Product enabled to be a Favorite?
                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("IsFavorite", StringComparison.OrdinalIgnoreCase));
                p.IsAllowFavorite = (productInternalSetting != null) ? ((productInternalSetting.Value.Trim() == "1")) : false;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("LearnMore", StringComparison.OrdinalIgnoreCase));
                p.LearnMore = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("HasAccess", StringComparison.OrdinalIgnoreCase));
                p.HasAccess = (productInternalSetting != null) ? (productInternalSetting.Value.Trim() == "1") : false;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ShowInUserListFilter", StringComparison.OrdinalIgnoreCase));
                p.ShowInUserListFilter = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : false;

                productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ShowInAppSwitcher", StringComparison.OrdinalIgnoreCase));
                p.ShowInAppSwitcher = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : false;

                if (productInternalSettingList.Any(item => item.Name.Equals("IsResource", StringComparison.OrdinalIgnoreCase)))
                {
                    productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("IsResource", StringComparison.OrdinalIgnoreCase));
                    p.IsResource = (productInternalSetting != null) ? (productInternalSetting.Value.Trim() == "1") : false;
                }

                p.IsInUDM = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("UpdateProductinUDM", StringComparison.OrdinalIgnoreCase))?.Value?.Trim() == "1";
            });

            if (allProducts)
            {
                return products;
            }

            if (!resourceOnly)
            {
                return products.Where(p => p.IsResource != true || p.ProductId == 14 || p.ProductId == 89 || p.ProductId ==38).ToList();
            }
            //}

            //CacheItemPolicy policy = new CacheItemPolicy();
            //policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(300);
            //getProductsCache.Set("getProductsCache", cachedProducts, policy);
            //products = new List<ProductUI>();
            //((List<ProductUI>)cachedProducts).ForEach(o => products.Add(o.Clone() as ProductUI));
            //}

            #endregion

            return products;
        }

        /// <summary>
		/// Returns a list of productTypes
		/// </summary>
		/// <returns></returns>
		public IList<ProductType> GetProductTypes()
        {
            #region Cache

            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getProductTypesCache";
            IList<ProductType> productTypesList = rpCache.GetFromCache<IList<ProductType>>(cacheKey, 600, () =>
            {
                using (var repository = GetRepository())
                {
                    return repository.GetMany<ProductType>(StoredProcNameConstants.SP_ListProductTypes, null).ToList();
                }
            });

            #endregion

            return productTypesList;
        }

        /// <summary>
        /// Create ProductSetting (Expire the setting if exists)
        /// </summary>
        /// <param name="PersonaId">User PersonaId</param>
        /// <param name="ProductId">ProductId</param>
        /// <param name="ProductSettingTypeId">Product Setting TypeId</param>
        /// <param name="Value">Product Setting Type Value</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateProductSetting(long PersonaId, int ProductId, int ProductSettingTypeId, string Value)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            int ProductSettingId = 0;
            int ConfigurationId = 0;
            Dictionary<string, object> dataLog = new Dictionary<string, object>();

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", $"Calling SP_CreatePersonaConfiguration : personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
            using (var repository = GetRepository())
            {
                //Begin the transaction
                //repository.UnitOfWork.BeginTransaction();
                try
                {
                    //Setup the parameter values to CreatePersonaConfiguration
                    dynamic param = new
                    {
                        PersonaId = PersonaId,
                        ProductId = ProductId,
                    };
                    //Create CreatePersonaConfiguration
                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersonaConfiguration, param);
                    dataLog = new Dictionary<string, object> { { "repositoryResponse", repositoryResponse } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSetting", $"Called SP_CreatePersonaConfiguration : personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                    if (repositoryResponse.Id == 0)
                    {
                        repositoryResponse.ErrorMessage = "CreateProductSetting Error: CreatePersonaConfiguration failed.";
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", "SP_CreatePersonaConfiguration failed" });
                    }
                    else
                    {
                        ConfigurationId = Convert.ToInt32(repositoryResponse.Id);
                        //Setup the parameter values to CreateProductSetting
                        param = new
                        {
                            ProductId = ProductId,
                            ProductSettingTypeId = ProductSettingTypeId,
                            Value = Value,
                            FromDate = utcNow,
                            ProductSettingId = ProductSettingId
                        };
                        //CreateProductSetting
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductSetting, param);
                        dataLog = new Dictionary<string, object> { { "repositoryResponse", repositoryResponse } };
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSetting", $"Called SP_CreateProductSetting - personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });

                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = "CreateProductSetting Error: CreateProductSetting failed.";
                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", "SP_CreateProductSetting failed." });
                        }
                        else
                        {
                            ProductSettingId = Convert.ToInt32(repositoryResponse.Id);
                            //Setup the parameter values to CreateProductConfigurationbyPersonaId
                            param = new
                            {
                                PersonaId = PersonaId,
                                ConfigurationId = ConfigurationId,
                                ProductId = ProductId,
                                ProductSettingID = ProductSettingId
                            };
                            //CreateProductConfigurationbyPersonaId
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductConfigurationbyPersonaId, param);
                            dataLog = new Dictionary<string, object> { { "repositoryResponse", repositoryResponse } };
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSetting", $"Called SP_CreateProductConfigurationbyPersonaId - personaid:{PersonaId} ConfigurationId:{ConfigurationId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "CreateProductSetting Error: CreateProductConfigurationbyPersonaId failed.";
                                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", "SP_CreateProductConfigurationbyPersonaId failed." });
                            }
                            else
                            {
                                // update the persona configuration with the latest setting result
                                param = new
                                {
                                    PersonaId = PersonaId,
                                    ProductId = ProductId,
                                    ProductSettingID = ProductSettingId
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonaConfiguration, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "CreateProductSetting Error: UpdatePersonaConfiguration failed.";
                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", "SP_UpdatePersonaConfiguration failed." });
                                }
                                else
                                {
                                    var productSettings = _productInternalSettingRepository.GetProductSettingByType("WarnOnProductError");
                                    if (productSettings.Any(p => p.ProductId == ProductId && p.Value == "1"))
                                    {
                                        param = new
                                        {
                                            PersonaId = PersonaId
                                        };
                                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_ManagePersonaProductError, param);
                                        if (repositoryResponse.Id == 0)
                                        {
                                            repositoryResponse.ErrorMessage = "CreateProductSetting Error: ManagePersonaProductError failed.";
                                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", "SP_ManagePersonaProductError failed." });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    repositoryResponse = new RepositoryResponse();
                    repositoryResponse.ErrorMessage = $"Create/Update Product Setting Error: {exception.Message}";
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: exception, messageProperties: new object[] { "CreateProductSetting", $"Create/Update Product Setting Error - personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                }
                finally
                {
                    if (repositoryResponse?.ErrorMessage.Length == 0)
                    {
                        //Commit and end transaction.
                        //repository.UnitOfWork.Commit();
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", $"Success - personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                    }
                    else
                    {
                        //Rollback transaction and dispose it.
                        //repository.UnitOfWork.Rollback();
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", $"Error - personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                    }
                }

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Create ProductSetting (Expire the setting if exists)
        /// </summary>
        /// <param name="repository">repository passed from tranaction</param>
        /// <param name="PersonaId">User PersonaId</param>
        /// <param name="ProductId">ProductId</param>
        /// <param name="ProductSettingTypeId">Product Setting TypeId</param>
        /// <param name="Value">Product Setting Type Value</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateProductSetting(IRepository repository, long PersonaId, int ProductId, int ProductSettingTypeId, string Value)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            int ProductSettingId = 0;
            int ConfigurationId = 0;
            Dictionary<string, object> dataLog = new Dictionary<string, object>();

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", $"Starting - personaid:{PersonaId}, productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value} starting" });
            try
            {
                //Setup the parameter values to CreatePersonaConfiguration
                dynamic param = new
                {
                    PersonaId = PersonaId,
                    ProductId = ProductId,
                };
                //Create CreatePersonaConfiguration
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersonaConfiguration, param);
                dataLog = new Dictionary<string, object> { { "repositoryResponse", repositoryResponse } };
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSetting", $"Calling SP_CreatePersonaConfiguration - personaid:{PersonaId}, productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                if (repositoryResponse.Id == 0)
                {
                    repositoryResponse.ErrorMessage = "CreateProductSetting Error: CreatePersonaConfiguration failed.";
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", $"SP_CreatePersonaConfiguration failed - personaid:{PersonaId}, productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value} starting" });
                }
                else
                {
                    ConfigurationId = Convert.ToInt32(repositoryResponse.Id);
                    //Setup the parameter values to CreateProductSetting
                    param = new
                    {
                        ProductId = ProductId,
                        ProductSettingTypeId = ProductSettingTypeId,
                        Value = Value,
                        FromDate = utcNow,
                        ProductSettingId = ProductSettingId
                    };
                    //CreateProductSetting
                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductSetting, param);
                    dataLog = new Dictionary<string, object> { { "repositoryResponse", repositoryResponse } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSetting", $"Called SP_CreateProductSetting - personaid:{PersonaId}, productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });

                    if (repositoryResponse.Id == 0)
                    {
                        repositoryResponse.ErrorMessage = "CreateProductSetting Error: CreateProductSetting failed.";
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", "SP_CreateProductSetting failed." });
                    }
                    else
                    {
                        ProductSettingId = Convert.ToInt32(repositoryResponse.Id);
                        //Setup the parameter values to CreateProductConfigurationbyPersonaId
                        param = new
                        {
                            PersonaId = PersonaId,
                            ConfigurationId = ConfigurationId,
                            ProductId = ProductId,
                            ProductSettingID = ProductSettingId
                            //Value = Value
                        };
                        //CreateProductConfigurationbyPersonaId
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductConfigurationbyPersonaId, param);
                        dataLog = new Dictionary<string, object> { { "repositoryResponse", repositoryResponse } };
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSetting", $"Called SP_CreateProductConfigurationbyPersonaId - personaid:{PersonaId} ConfigurationId:{ConfigurationId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = "CreateProductSetting Error: CreateProductConfigurationbyPersonaId failed.";
                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", dataLog, messageProperties: new object[] { "CreateProductSetting", $"SP_CreateProductConfigurationbyPersonaId failed." });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                repositoryResponse = new RepositoryResponse();
                repositoryResponse.ErrorMessage = $"Create/Update Product Setting Error: {exception.Message}";
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: exception, messageProperties: new object[] { "CreateProductSetting", $"Create/Update Product Setting Error personaid:{PersonaId}, productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
            }
            finally
            {
                if (repositoryResponse?.ErrorMessage.Length == 0)
                {
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", $"Success - personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                }
                else
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductSetting", $"Error - personaid:{PersonaId} productid:{ProductId} ProductSettingTypeId:{ProductSettingTypeId} Value:{Value}" });
                }
            }

            return repositoryResponse;
        }

        /// <summary>
        /// Returns a list of productSettingType
        /// </summary>
        /// <returns></returns>
        public IList<ProductSettingType> ListProductSettingType()
        {
            var cacheKey = "listProductSettingType";
            var rpObjectCache = new RPObjectCache();
            var productSettingTypes = rpObjectCache.GetFromCache(cacheKey, 120, () =>
            {
                // load from database
                using (var repository = GetRepository())
                {
                    return repository.GetMany<ProductSettingType>(StoredProcNameConstants.SP_ListProductSettingType, null).ToList(); 
                }
            });
            return productSettingTypes;
        }

        /// <summary>
        /// Update a Product Batch
        /// </summary>
        /// <returns>Repository response object</returns>
        public bool UpdateProductBatch(int productBatchId, int statusTypeId, string inputJson = null, string errorDetails = null)
        {
            using (var repository = GetRepository())
            {
                //var result = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateProductBatch,
                //    new { productBatchId, statusTypeId, inputJson, errorDetails });

                var result = repository.Execute<int>(StoredProcNameConstants.SP_UpdateProductBatch,
                   new { productBatchId, statusTypeId, inputJson, errorDetails });

                return result == 1;
            }
        }

        /// <summary>
        /// Save Persona product Properties
        /// </summary>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="personaProductProperties"></param>
        /// <returns></returns>
        public bool SavePersonaProductProperties(long assignUserPersonaId, int productId, string personaProductProperties)
        {
            using (var repository = GetRepository())
            {
                object param = new
                {
                    PersonaId = assignUserPersonaId,
                    ProductId = productId,
                    json = personaProductProperties
                };

                var result = repository.Execute<bool>(StoredProcNameConstants.SP_SavePersonaProductProperties, param);
                return result;
            }
        }

        /// <summary>
        /// Used to update the persona product setting type for the given user and setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="settingType"></param>
        /// <param name="value"></param>
        public void UpdateProductSettingProductStatus<T>(long userPersonaId, int productId, string settingType, T value)
        {
            // add the new status flag to the product before we start
            IList<ProductSettingType> productSettingTypes = ListProductSettingType();
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            string statusValue = value.ToString();

            // get the id for ProductStatus type
            if (productSettingTypes.Any(a => a.Name.ToUpper() == settingType.ToUpper()))
            {
                int productStatusTypeId = (from a in productSettingTypes where a.Name.ToUpper() == settingType.ToUpper() select a.ProductSettingTypeId).FirstOrDefault();
                repositoryResponse = CreateProductSetting(userPersonaId, productId, productStatusTypeId, statusValue.ToString());
            }
        }

        /// <summary>
        /// Returns List of Product Batch Statuses
        /// </summary>
        public IList<ProductBatchStatus> ListProductBatchStatuses(Guid realPageId, long assignUserPersonaId)
        {
            using (var repository = GetRepository())
            {
                return
                    repository.GetMany<ProductBatchStatus>(StoredProcNameConstants.SP_ListProductBatchStatusesByRealPageId,
                        new { realPageId, assignUserPersonaId }).ToList();
            }
        }

        /// <summary>
        /// Returns the id of ProductSettingType
        /// <param name="productSettingName">productSettingName</param>
        /// </summary>
        public int GetProductSettingType(string productSettingName)
        {
            using (var repository = GetRepository())
            {
                int productSettingTypeId = 0;
                DynamicParameters param = new DynamicParameters();
                param.Add("@Name", productSettingName, dbType: DbType.String, direction: ParameterDirection.Input);
                param.Add("@ProductSettingTypeId", productSettingTypeId, dbType: DbType.Int32, direction: ParameterDirection.Output);

                try
                {
                    repository.Execute(StoredProcNameConstants.SP_GetProductSettingType, param);
                    productSettingTypeId = param.Get<int>("@ProductSettingTypeId");
                }
                catch
                {
                }

                return productSettingTypeId;
            }
        }

        /// <summary>
        /// Returns a list of productfamilies
        /// </summary>
        /// <param name="organizationRealPageId">The unique identifier for the organization</param>
        /// <param name="editorRealPageId">Editor user enterprise Id</param>
        /// <param name="personRealPageId">Edited User enterprise Id</param>
        /// <param name="accessFilter">Products Filter</param>
        /// <returns>List of Product Families</returns>
        //public IList<ProductFamily> GetProductFamilies(Guid? personRealPageId = null, string accessFilter = null)
        public IList<ProductFamily> GetProductFamilies(Guid organizationRealPageId, Guid editorRealPageId, Guid? personRealPageId = null, string accessFilter = null, string loginName = null)
        {
            //if personRealPageId is valid guid then we are editing the user assigned products
            bool setIsAssigned = ((personRealPageId != Guid.Empty) && (personRealPageId != null));
            IList<ProductSettingList> editorProductSettingList = new List<ProductSettingList>();
            IList<PersonaProductUserDetails> userProducts = new List<PersonaProductUserDetails>();
            IList<ProductSettingList> productSettingList = new List<ProductSettingList>();
            ProductSettingList productSetting = new ProductSettingList();
            ProductInternalSetting productInternalSetting = new ProductInternalSetting();
            IList<string> aoUserProducts = null;
            List<string> aoNonMigratedUserProducts = null;
            IList<ProductFamily> productFamilyList = null;
            IList<Solution> personaProductUserDetails = null;

            using (var repository = GetRepository())
            {
                productFamilyList = repository.GetMany<ProductFamily>(StoredProcNameConstants.SP_ListProductFamilies, null).ToList();
                personaProductUserDetails = repository.GetMany<Solution>(StoredProcNameConstants.SP_ListProductsByOrganization, new { OrganizationRealPageId = organizationRealPageId }).ToList();
            }

            try
            {
                // If AO family exists then load solutions based on editor user
                if (personaProductUserDetails.Any(c => c.ProductId == (int)ProductEnum.AssetOptimizer))
                {
                    // Get ProductTypes
                    var productTypes = GetProductTypes();

                    // get products for personaId
                    var aoLogic = new ManageProductAssetOptimization(_userClaim);

                    aoUserProducts = aoLogic.GetGbSupportedAoProductsWithUserAdminRole(_userClaim.PersonaId);

                    if (personRealPageId == null && loginName != null)
                    {
                        aoNonMigratedUserProducts = aoLogic.GetAOProductsForNewMultiCompanyUser(_userClaim.PersonaId, loginName);
                        if (aoNonMigratedUserProducts?.Count > 0)
                        {
                            aoNonMigratedUserProducts.RemoveAll(p => p.Contains("BM"));
                        }
                    }

                    //var aoProductFamily = productFamilyList.Where(p => p.ProductTypeId == 400).ToList().FirstOrDefault();
                    if (personaProductUserDetails.Count > 0)
                    {
                        foreach (var aoProduct in aoUserProducts)
                        {
                            if (aoProduct != "BM")
                            {
                                var aoProductEnum = ProductEnumHelper.GetAoProductEnum(aoProduct);
                                var prodDetails = GetBooksMasterProductDetail((int)aoProductEnum);
                                personaProductUserDetails.Add(new Solution
                                {
                                    FamilyId = 400,
                                    IsAssigned = false,
                                    ProductId = (int)aoProductEnum,
                                    ProductCode = prodDetails.BooksProductCode,
                                    ProductName = prodDetails.Name,
                                    SolutionId =
                                        productTypes.Where(x => x.Name.Trim() == prodDetails.Name.Trim())
                                            .Select(z => z.ProductTypeId)
                                            .FirstOrDefault()
                                });
                            }
                            if(accessFilter == "RoleTemplate" && aoProduct == "BM")
                            {
                                var aoProductEnum = ProductEnumHelper.GetAoProductEnum(aoProduct);
                                var prodDetails = GetBooksMasterProductDetail((int)aoProductEnum);
                                personaProductUserDetails.Add(new Solution
                                {
                                    FamilyId = 400,
                                    IsAssigned = false,
                                    ProductId = (int)aoProductEnum,
                                    ProductCode = prodDetails.BooksProductCode,
                                    ProductName = prodDetails.Name,
                                    SolutionId =
                                        productTypes.Where(x => x.Name.Trim() == prodDetails.Name.Trim())
                                            .Select(z => z.ProductTypeId)
                                            .FirstOrDefault()
                                });

                            }
                        }
                    }
                    personaProductUserDetails = personaProductUserDetails.OrderBy(n => n.ProductName).ToList();
                }
            }
            catch (Exception ex)
            {
                // Exception can occur if AO is down, in such case don't impact other products
                // Nav bar will not show any AO products

                string message = "Exception while getting AO products.";

                Log.Write(LogEventLevel.Error, ex, message);
            }

            //get the active personaId for the edited enterprise UserId
            long personaId = _userClaim.PersonaId;

            //get user login for edit persona
            using (var repository = GetRepository())
            {
                editorProductSettingList = repository.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, new { PersonaId = _userClaim.PersonaId, ProductStatus = ((Int32)UserUiStatusType.AccountCreationSuccessful).ToString() }).ToList();
            }

            if (setIsAssigned)
            {
                //get the active personaId for the user being edited enterprise UserId
                using (var repository = GetRepository())
                {
                    var personaList = repository.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, new { RealPageId = personRealPageId });
                    personaId = personaList.FirstOrDefault(p => p.OrganizationPartyId == _userClaim.OrganizationPartyId).PersonaId;
                }

                //get user login for persona
                IManageUserLogin userLoginLogic = new ManageUserLogin();
                var userLogin = userLoginLogic.GetUserLoginOnly(personRealPageId.Value);

                UserLoginRepository userLoginRepository = new UserLoginRepository();
                var organizationStatus = userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, _userClaim.OrganizationPartyId, false);

                //get list of product by personaId
                using (var repository = GetRepository())
                {
                    if (organizationStatus.Status == UserUiStatusType.Disabled)
                    {
                        userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = personaId, ProductStatusValue = ((Int32)UserUiStatusType.Deactivated).ToString() }).ToList();
                        productSettingList = repository.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, new { PersonaId = personaId, ProductStatus = ((Int32)UserUiStatusType.Deactivated).ToString() }).ToList();
                    }
                    else
                    {
                        userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = personaId, ProductStatusValue = ((Int32)UserUiStatusType.AccountCreationSuccessful).ToString() }).ToList();
                        productSettingList = repository.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, new { PersonaId = personaId}).ToList();
                    }
                }
            }
            SamlRepository samlRepository = new SamlRepository();
            var rpcache = new RPObjectCache();
            var cacheKey = $"ProductSettingsByOrganization_{organizationRealPageId}";
            IList<ProductSettingList> ProductSettingsByOrganizationList = rpcache.GetFromCache<IList<ProductSettingList>>(cacheKey, 120, () =>
            {
                return GetProductSettings(organizationRealPageId);
            });

            if (productFamilyList != null)
            {
                productFamilyList.ToList().ForEach(f =>
                {
                    f.Solutions = personaProductUserDetails.Where(p => p.FamilyId == f.ProductTypeId).ToList();

                    f.Solutions.ToList().ForEach(s =>
                    {
                        //Product Settings

                        #region Cache

                        ObjectCache productInternalSettingCache = MemoryCache.Default;

                        var productInternalSettingList = productInternalSettingCache["productInternalSetting_" + s.ProductId.ToString()] as List<ProductInternalSetting>;
                        if (productInternalSettingList == null)
                        {
                            using (var repository = GetRepository())
                            {
                                productInternalSettingList = repository.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, new { ProductId = s.ProductId }).ToList();
                            }

                            CacheItemPolicy policy = new CacheItemPolicy();
                            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(120);
                            productInternalSettingCache.Set("productInternalSetting_" + s.ProductId.ToString(), productInternalSettingList, policy);
                        }

                        #endregion

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("SubSolution", StringComparison.OrdinalIgnoreCase));
                        s.SubSolution = (productInternalSetting != null) ? productInternalSetting.Value.Trim() : null;

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ShowInUserDetails", StringComparison.OrdinalIgnoreCase));
                        s.ShowInUserDetails = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : false;

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ShowInRolesAndRights", StringComparison.OrdinalIgnoreCase));
                        s.ShowInRolesAndRights = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : false;

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("LockOnProductAccess", StringComparison.OrdinalIgnoreCase));
                        s.LockOnProductAccess = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : true;

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ProductAPIRequiresUser", StringComparison.OrdinalIgnoreCase));
                        s.ProductAPIRequiresUser = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : true;

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("NotificationEmailRequiredForUserWithNoEmail", StringComparison.OrdinalIgnoreCase));
                        s.NotificationEmailRequiredForUserWithNoEmail = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : true;

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ProductNotAvailableForRegularUserNoEmail", StringComparison.OrdinalIgnoreCase));
                        s.ProductNotAvailableForRegularUserNoEmail = (productInternalSetting != null) ? productInternalSetting.Value.Trim() == "1" : true;

                        if (f.ProductTypeId == 400)
                        {
                            var orgProductSetting = ProductSettingsByOrganizationList.FirstOrDefault(item => item.ProductId == (int)ProductEnum.AssetOptimizer && item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));
                            s.UsePrimaryProperties = (orgProductSetting != null) ? orgProductSetting.Value.Trim() == "1" : false;
                        }
                        else
                        {
                            var orgProductSetting = ProductSettingsByOrganizationList.FirstOrDefault(item => item.ProductId == s.ProductId && item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));
                            s.UsePrimaryProperties = (orgProductSetting != null) ? orgProductSetting.Value.Trim() == "1" : false;
                        }

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("ShowInRoleTemplate", StringComparison.OrdinalIgnoreCase));
                        s.ShowInRoleTemplate = (productInternalSetting != null) && (productInternalSetting.Value.Trim() == "1" ? true : false) ;

                        productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("EnableProductForAdminUserEdit", StringComparison.OrdinalIgnoreCase));
                        s.EnableProductForAdminUserEdit = (productInternalSetting != null) && (productInternalSetting.Value.Trim() == "1" ? true : false);

                        if (setIsAssigned == true)
                        {
                            s.IsAssigned = userProducts.Any(item => item.ProductId == s.ProductId);
                            productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("IsUserCreationOnTileClick", StringComparison.OrdinalIgnoreCase));
                            if (productInternalSetting != null)
                            {
                                if (productInternalSetting.Value.Trim() == "true")
                                {
                                    IList<SamlAttributes> samlAttributesDetails = samlRepository.GetProductSamlDetails(personaId, s.ProductId);
                                    if (samlAttributesDetails.Count() == 0)
                                    {
                                        s.IsAssigned = false;
                                    }
                                }
                            }
                            productSetting = productSettingList.FirstOrDefault(item => item.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && item.ProductId == s.ProductId);
                            if (productSetting != null)
                            {
                                s.ProductStatus = Convert.ToInt32(productSetting.Value);
                                if (s.ProductStatus == (int)ProductBatchStatusType.Deleted || s.ProductStatus == (int)ProductBatchStatusType.Inactive)
                                {
                                    s.IsAssigned = false;
                                }
                            }

                            productSetting = productSettingList.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == s.ProductId);
                            if (productSetting != null)
                            {
                                s.PersonaUsedPrimaryProperties = productSetting.Value.Trim() == "1" ? true : false;
                            }
                        }

                        if (aoNonMigratedUserProducts?.Count > 0 && !setIsAssigned && !string.IsNullOrWhiteSpace(s.ProductCode))
                        {
                            if (aoNonMigratedUserProducts.Any(item => item.Contains(s.ProductCode)))
                            {
                                s.IsAssigned = true;
                                productSetting = productSettingList.FirstOrDefault(item => item.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && item.ProductId == s.ProductId);
                                if (productSetting != null)
                                {
                                    s.ProductStatus = Convert.ToInt32(productSetting.Value);
                                    if (s.ProductStatus == (int)ProductBatchStatusType.Deleted || s.ProductStatus == (int)ProductBatchStatusType.Inactive || s.ProductStatus == (int)ProductBatchStatusType.Error)
                                    {
                                        s.IsAssigned = false;
                                    }
                                }
                            }
                        }
                        //Does the Product requires a User (e.g. Resident Portal)
                        if (s.ProductAPIRequiresUser == true)
                        {
                            productSetting = editorProductSettingList.FirstOrDefault(item => item.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && item.ProductId == s.ProductId);
                            //If the Product status setting not existing since we called the Product Settings with a AccountCreationSuccessful status
                            //OR the status is not a Success (e.g. Deleted - User does not have access)
                            if ((productSetting == null) || (Convert.ToInt32(productSetting.Value) != (int)ProductBatchStatusType.Success))
                            {
                                //Remove the product from the Solution (do not display under Product Access)
                                Solution solution = new Solution();
                                solution = f.Solutions.FirstOrDefault(p => p.ProductId == s.ProductId);
                                productFamilyList.ToList().FirstOrDefault(pf => pf.Solutions.Remove(solution));
                            }
                        }
                    });
                });
            }

            if (_userClaim.IsRPEmployee || _userClaim.ImpersonatedBy != null)
            {
                Guid userRealPageGUIDForAdGroup = _userClaim.UserRealPageGuid;
                var checkForADGroupProductAccess = _productInternalSettingRepository.GetProductSettingByType("CheckADGroupUserMgmt");
                var lockOnProductAccessRights = _productInternalSettingRepository.GetProductSettingByType("LockOnProductAccessRight");
                List<AdGroup> adGroupsForPersona = new List<AdGroup>();
                long impersonatePersonaId = 0;
                if (checkForADGroupProductAccess != null && checkForADGroupProductAccess.Count > 0)
                {
                    if (_userClaim.ImpersonatedBy != null && _userClaim.ImpersonatedBy != Guid.Empty)
                    {
                        userRealPageGUIDForAdGroup = _userClaim.ImpersonatedBy;
                    }
                    IList<Persona> persona = _personaRepository.ListPersona(userRealPageGUIDForAdGroup);
                    impersonatePersonaId = persona.FirstOrDefault(x => x.Organization.RealPageId == EmployeeCompanyRealPageId)?.PersonaId ?? 0;
                    //If user doesn't exist in employee company then skip ADGroup check.
                    if (impersonatePersonaId > 0)
                    {
                        adGroupsForPersona = GetAdGroupsForUser(impersonatePersonaId);
                    }
                }



                //allways set "Platform Services" (productId - 500) => Landing (productId - 3) => IsAssigned to True -- For GB Roles and Rights
                productFamilyList.ToList().ForEach(p =>
                {
                    Solution solution = new Solution();
                    if (p.Name.Equals("Administration", StringComparison.OrdinalIgnoreCase))
                    {
                        //always set "Platform Services" (productId - 500) => Landing (productId - 3) => IsAssigned to True -- For GB Roles and Rights
                        solution = p.Solutions.FirstOrDefault(s => s.ProductId == (int)ProductEnum.UnifiedPlatform);
                        if (solution != null)
                        {
                            solution.IsAssigned = true;
                        }
                    }
                    else if (p.Name.Equals("Property Management", StringComparison.OrdinalIgnoreCase) && (personaProductUserDetails.Any(c => c.ProductId == (int)ProductEnum.EasyLMS)))
                    {
                        // Set IsAssigned to true if Organization has EasyLMS
                        solution = p.Solutions.FirstOrDefault(s => s.ProductId == (int)ProductEnum.EasyLMS);
                        if (solution != null)
                        {
                            solution.IsAssigned = true;
                        }
                    }
                    //else if (p.Name.Equals("Property Management", StringComparison.OrdinalIgnoreCase) && (personaProductUserDetails.Any(c => c.ProductId == (int)ProductEnum.RealConnect)))
                    //{
                    //    // Set IsAssigned to true if Organization has RealConnect
                    //    solution = p.Solutions.FirstOrDefault(s => s.ProductId == (int)ProductEnum.RealConnect);
                    //    if (solution != null)
                    //    {
                    //        solution.IsAssigned = true;
                    //    }
                    //}

                    CheckProductRight(ref p, lockOnProductAccessRights, checkForADGroupProductAccess, adGroupsForPersona, impersonatePersonaId);

                });
            }

            // now remove any products which are not matching product access filter
            if (accessFilter != null)
            {
                productFamilyList.ToList().ForEach(p =>
                {
                    if (accessFilter.Equals("UserDetails", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var soln in p.Solutions.ToList())
                        {
                            if (!soln.ShowInUserDetails)
                            {
                                p.Solutions.Remove(soln);
                            }
                        }
                    }

                    if (accessFilter.Equals("RolesAndRights", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var soln in p.Solutions.ToList())
                        {
                            if (!soln.ShowInRolesAndRights)
                            {
                                p.Solutions.Remove(soln);
                            }
                        }
                    }

                    if (accessFilter.Equals("RoleTemplate", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var soln in p.Solutions.ToList())
                        {
                            if (!soln.ShowInRoleTemplate)
                            {
                                p.Solutions.Remove(soln);
                            }
                        }
                    }
                });
            }

            // now remove any empty families
            productFamilyList.ToList().ForEach(p =>
            {
                if (p.Solutions.Count == 0)
                {
                    productFamilyList.Remove(p);
                }
            });
            //// Removing Vendor Marketplace product for Vendor & Other OrgTypes
            var disableusermanagementOrgtype = _productInternalSettingRepository.GetProductSettingByType("DisableUserManagementForOrgType");
            var organizationsTypeName =  _userClaim.OrganizationType;
            foreach (var items in disableusermanagementOrgtype)
            {
                if (items.Value != String.Empty)
                {
                    string[] types = items.Value.Split(',');
                    if (types.Contains(organizationsTypeName))
                    {
                        productFamilyList.ToList().ForEach(x =>
                        {
                            Solution solution = new Solution();
                            if (x.Name.Equals("Administration", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var soln in x.Solutions.ToList())
                                {
                                    if (soln.ProductId == items.ProductId)
                                    {
                                        x.Solutions.Remove(soln);
                                    }
                                }
                            }
                        });
                    }
                }
            }
            try
            {
                // add benchmarking as sub product
                if (aoUserProducts != null && aoUserProducts.Contains("BM"))
                {
                    var biProduct = personaProductUserDetails.FirstOrDefault(x => x.ProductId == (int)ProductEnum.AoPerformanceAnalytics);
                    if (biProduct != null)
                    {
                        biProduct.SubSolution = "Benchmarking";
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception can occur if AO is down, in such case don't impact other products
                // Nav bar will not show any AO products

                string message = "Exception while getting AO products.";

                Log.Write(LogEventLevel.Error, ex, message);
            }

            //Sort products by name
            productFamilyList.ToList().ForEach(x =>
            {
                x.Solutions = x.Solutions.OrderBy(y => y.ProductName).ToList();
            });

            return productFamilyList;

        }

        /// <summary>
		/// List of Roles by Party ID, Product List and Product ID
		/// </summary>
		/// <param name="partyId">Party ID</param>
		/// <param name="productIdList">List of product ids for the party</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>List of Roles by PartyId and Product</returns>
		public List<ProductRole> ListRolesForProductByParty(long partyId, IList<int> productIdList, int productId)
        {
            var procName = StoredProcNameConstants.SP_ListRolesForProductsByPartyId;

            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                };

                List<ProductRole> rolesList = new List<ProductRole>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result == null) return rolesList;

                foreach (var item in result)
                {
                    rolesList.Add(new ProductRole { ID = item.RoleId.ToString(), Name = item.value, IsAssigned = false, Roletype = item.RoleType, DefaultRole = item.DefaultRole.ToString(), Alias = item.RoleNickName, Description = item.Description , accessAllProperties = IsAccessToAllProperties(ListRoleAttributes(item.RoleAttribute.ToString())) });
                }
                return rolesList;
            }
        }

        /// <summary>
        /// List role attributes
        /// </summary>
        /// <param name="inp"></param>		
        /// <returns></returns>
        public IList<ProductRoleAttribute> ListRoleAttributes(string inp)
        {
            if (string.IsNullOrEmpty(inp)) { return new List<ProductRoleAttribute>(); }

            return JsonConvert.DeserializeObject<IList<ProductRoleAttribute>>(inp);
        }

        /// <summary>
        /// Is Access All Properties
        /// </summary>
        /// <param name="roleAttributes"></param>		
        /// <returns></returns>
        public bool IsAccessToAllProperties(IList<ProductRoleAttribute> roleAttributes)
        {
            return roleAttributes.Any(p => p.AttributeName.Equals("AccessAllProperties", StringComparison.OrdinalIgnoreCase) && p.AttributeValue.Equals("1", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// List of Roles with Rights count
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <param name="productIdList">Product ID's by Org</param>  
        /// <returns>List of Roles and rights count by PartyId and Product</returns>
        public IList<RightRoleDetail> ListRoleWithRights(long partyId, int productId, List<int> productIdList)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRolesAssociatedWithRights;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                };

                List<RightRoleDetail> rolesList = new List<RightRoleDetail>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rolesList.Add(
                            new RightRoleDetail
                            {
                                RoleId = item.RoleId,
                                RoleName = item.Role,
                                IsAssigned = false,
                                RoleType = item.RoleType,
                                RightName = item.Right,
                                RightId = item.RightId,
                                RightValueTypeId = item.RightValueTypeId,
                                RightNickName = item.RightNickName
                            }
                        ); //RightsAssigned = item.count
                    }
                }
                return rolesList;
            }
        }

        /// <summary>
        /// List GB products
        /// </summary>
        public IList<GbProductMap> ListProducts(int? productId, Guid? guid, string name, string booksProductCode)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    ProductId = productId,
                    ProductGUID = guid,
                    Name = name,
                    BooksProductCode = booksProductCode
                };

                List<GbProductMap> rolesList = new List<GbProductMap>();
                return repository.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, param);
            }
        }

        /// <summary>
        /// Returns product details for given product code.
        /// This will get replaced with Blue book call in future
        /// </summary> 
        public GbProductMap GetBooksMasterProductDetail(int gbProductId)
        {
            var gbProductMap = GetAllProducts().FirstOrDefault(x => x.ProductId == gbProductId);
            return gbProductMap;
        }

        /// <summary>
        /// Returns product details for given blue book product code.
        /// This will get replaced with Blue book call in future
        /// </summary> 
        public GbProductMap GetBooksMasterProductDetail(string blueBookProductCode)
        {
            var gbProductMap = GetAllProducts();
            return gbProductMap.FirstOrDefault(x => x.BooksProductCode.ToUpper() == blueBookProductCode.ToUpper());
        }

        /// <summary>
        /// Returns product propertyId roles details for given product code and persona.		
        /// </summary> 
        public RolePropertyList GetUserProductDataFromProductBatch(long personaId, int productId)
        {
            RolePropertyList propertyrolelist = new RolePropertyList();
            using (var repository = GetRepository())
            {
                string productUserInputJson = repository.GetOne<string>(StoredProcNameConstants.SP_GetUserProductBatchJsonData, new { ProductId = productId, PersonaId = personaId });
                if (!string.IsNullOrEmpty(productUserInputJson))
                {
                    propertyrolelist = JsonConvert.DeserializeObject<RolePropertyList>(productUserInputJson.Trim());
                }
            }

            return propertyrolelist;
        }

        /// <summary>
        /// Returns all the products
        /// </summary>
        /// <returns></returns>
        public IList<GbProductMap> GetAllProducts()
        {
            // Get products
            if (_repository != null)
            {
                // unit test
                return ListProducts(null, null, null, null);
            }

            var rpcache = new RPObjectCache();
            var cacheKey = "GB-BB-ProductMap";

            var products = rpcache.GetFromCache(cacheKey, 300,
                () => ListProducts(null, null, null, null));

            return products;
        }

        /// <summary>
        /// Search by company and product ids and returns userlist
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="products"></param>
        /// <param name="upfmId"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="pageNumber"></param>
        /// <param name="roles"></param>
        /// <param name="rights"></param>
        /// <param name="propertyIds"></param>
        /// <param name="companyDomain"></param>
        /// <returns>List of Users by product or company</returns>
        public IList<EnterpriseProductUser> GetUsersByCompanyorProducts(string companyId, string upfmId, IList<int> products, int rowsPerPage, int pageNumber,
                                                                IList<string> roles, IList<string> rights, List<string> propertyIds = null, string companyDomain = null)
        {

            dynamic param = new
            {
                CompanyId = companyId,
                UPFMId = upfmId,
                ProductId = products.Any() ? string.Join(",", products) : null,
                RowsPerPage = rowsPerPage,
                PageNumber = pageNumber,
                Roles = roles.Any() ? string.Join(",", roles) : null,
                Rights = rights.Any() ? string.Join(",", rights) : null,
                Properties = propertyIds.Any() ? string.Join(",", propertyIds) : null,
                CompanyDomain = companyDomain
            };

            using (var repository = GetRepository())
            {
                return repository.GetMany<EnterpriseProductUser>(EnterpriseStoredProcNameConstants.SP_ListUsersWithCompanyId_Ver3, param, 0);
            }
        }

        /// <summary>
        /// Get Unified Login mapping PersonaId for Product UserId by company or upfmId and product id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="upfmId"></param>
        /// <param name="productId"></param>
        /// <param name="productUserIds"></param>
        /// <returns>List of Unified Login mapping UserId by product and company</returns>
        public List<ULMappedPersonaIds> GetULMappingPersonaIDsByCompanyAndProducts(int companyId, string upfmId, int productId, List<string> productUserIds)
        {
            List<ULMappedPersonaIds> mappingUserList = new List<ULMappedPersonaIds>();

            dynamic param = new
            {
                CompanyId = companyId,
                UPFMId = upfmId,
                ProductId = productId,
                TargetProductUserIds = productUserIds.Count > 0 ? string.Join(",", productUserIds) : string.Empty,
            };

            using (var repository = GetRepository())
            {
                mappingUserList = repository.GetMany<ULMappedPersonaIds>(EnterpriseStoredProcNameConstants.SP_ListULMappingPersonaIdForProductUserId, param);
            }

            return mappingUserList;
        }

        /// <summary>
        /// Search by company and product ids and returns userlist
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="products"></param>
        /// <returns>List of Users</returns>
        public IList<EnterpriseProductUser> GetUsersByCompanyorProducts(string companyId, IList<int?> products)
        {
            dynamic param = new
            {
                CompanyId = companyId,
                ProductId = products.Any() ? string.Join(",", products) : null
            };

            using (var repository = GetRepository())
            {
                return repository.GetMany<EnterpriseProductUser>(EnterpriseStoredProcNameConstants.SP_ListUsersWithCompanyId_Ver3, param);
            }
        }

        /// <summary>
        /// Returns  persona enterprise role template details.		
        /// </summary> 
        public RoleTemplate GetEnterpriseRoleForPersona(long personaId)
        {
            RoleTemplate roleTemplate = new RoleTemplate();
            using (var repository = GetRepository())
            {
                roleTemplate = repository.GetOne<RoleTemplate>(StoredProcNameConstants.SP_GetUserRoleTemplate, new { PersonaId = personaId });
            }
            return roleTemplate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyId"></param>
        /// <returns></returns>
        public List<RoleTemplate> GetRoleTemplateList(long partyId)
        {
            List<RoleTemplate> roleTemplate = new List<RoleTemplate>();
            using (var repository = GetRepository())
            {
                object param = new
                {
                    PartyId = partyId
                };
                roleTemplate = repository.GetMany<RoleTemplate>(StoredProcNameConstants.SP_GetRoleTemplate, param).ToList();
            }
            return roleTemplate;
        }

        /// <summary>
        /// Returns  persona has any product error.		
        /// </summary> 
        public bool GetPersonaHasProductError(long personaId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<bool>(StoredProcNameConstants.SP_GetPersonaProductError, new { PersonaId = personaId });
            }
        }

        /// <summary>
		/// GetRoleTemplateProductRoleMapping
		/// </summary>
		/// <param name="roleTemplateId"></param>
		/// <param name="partyId"></param>
		/// <returns></returns>
		public List<RoleTemplateProductRole> GetRoleTemplateProductRoleMapping(int roleTemplateId, long partyId)
        {
            object param = new
            {
                RoleTemplateId = roleTemplateId,
                PartyId = partyId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<RoleTemplateProductRole>(StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings, param).ToList();

            }
        }

        public List<int> GetEnterpriseRoleProductsByOrganization(int roleTemplateId, Guid organizationRealPageId)
        {
            object param = new
            {
                RoleTemplateId = roleTemplateId,
                OrganizationRealPageId = organizationRealPageId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleProductsByOrganization, param).ToList();

            }
        }

        public List<int> GetEnterpriseRoleNewProductsByRoleTemplateId(int roleTemplateId, DateTime createdDateTime)
        {
            object param = new
            {
                RoleTemplateId = roleTemplateId,
                CreatedDateTime = createdDateTime
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, param).ToList();

            }
        }

        public List<int> GetEnterpriseRoleUpdatedProductsByRoleTemplateId(int roleTemplateId, DateTime createdDateTime)
        {
            object param = new
            {
                RoleTemplateId = roleTemplateId,
                CreatedDateTime = createdDateTime
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleUpdatedProductsByRoleTemplateId, param).ToList();

            }
        }

        public List<int> GetEnterpriseRoleProductsByRoleTemplateId(int roleTemplateId, long organizationPartyId)
        {
            object param = new
            {
                RoleTemplateId = roleTemplateId,
                PartyId = organizationPartyId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleProductsByOrganization, param).ToList();
            }
        }

        public List<int> GetEnterpriseRoleDeletedProductsByRoleTemplateId(int roleTemplateId, DateTime createdDateTime)
        {
            object param = new
            {
                RoleTemplateId = roleTemplateId,
                CreatedDateTime = createdDateTime
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleDeletedProductsByRoleTemplateId, param).ToList();

            }
        }

        public List<PersonaProductProperty> GetPersonaProductPrimaryProperties(long personaId)
        {
            dynamic param = new
            {
                PersonaId = personaId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<PersonaProductProperty>(StoredProcNameConstants.SP_GetPersonaProductPrimaryProperties, param);
            }
        }

        public List<AdGroupProduct> GetAdGroupsForProduct(int productId)
        {
            dynamic param = new
            {
                ProductId = productId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<AdGroupProduct>(StoredProcNameConstants.SP_GetADGroupsForProduct, param);
            }
        }

        public List<AdGroup> GetAdGroupsForUser(long personaId)
        {
            dynamic param = new
            {
                PersonaId = personaId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<AdGroup>(StoredProcNameConstants.SP_GetADGroupsForUser, param);
            }
        }

        public List<ProductAdGroupsCount> GetPersonaProductsAdGroupsCount(long personaId)
        {
            dynamic param = new
            {
                PersonaId = personaId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<ProductAdGroupsCount>(StoredProcNameConstants.SP_GetPersonaProductADGroupCount, param);
            }
        }

        public List<AdGroup> GetUserManagementADGroupsByProduct(long productId)
        {
            dynamic param = new
            {
                ProductId = productId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<AdGroup>(StoredProcNameConstants.SP_GetUserManagementADGroupsByProduct, param);
            }
        }

        public List<AdGroupRole> GetAdGroupRolesByPersona(long personaId)
        {
            dynamic param = new
            {
                PersonaId = personaId
            };
            using (var repository = GetRepository())
            {
                return repository.GetMany<AdGroupRole>(StoredProcNameConstants.SP_GetRolesForADGroupByPersona, param);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            string correlationId = "";
            if (_userClaim != null)
            {
                correlationId = (_userClaim.CorrelationId != Guid.Empty) ? _userClaim.CorrelationId.ToString() : "";
            }
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }

        private void CheckProductRight(ref ProductFamily productFamily, IList<ProductInternalSettingByType> lockOnProductAccessRights
                                , IList<ProductInternalSettingByType> checkADGroupUserMgmt, List<AdGroup> adGroupsForPersona, long impersonatePersonaId)
        {
            // check with logged in editors rights
            List<string> editorRights = _userClaim.Rights;
            foreach (var s in productFamily.Solutions)
            {
                //s.LockOnProductAccess = false; don't override the setting from the database
                var productAccessRight = lockOnProductAccessRights.FirstOrDefault(f => f.ProductId == s.ProductId)?.Value;
                var productcheckADGroupUserMgmt = checkADGroupUserMgmt.FirstOrDefault(p => p.ProductId == s.ProductId);
                if (productcheckADGroupUserMgmt != null && productcheckADGroupUserMgmt.Value == "1" && impersonatePersonaId > 0)
                {
                    if (adGroupsForPersona?.Count == 0)
                    {
                        s.LockOnProductAccess = true;
                    }
                    if (adGroupsForPersona != null && adGroupsForPersona.Count > 0)
                    {
                        var userManagementAdGroupsForProduct = GetUserManagementADGroupsByProduct(s.ProductId);
                        if (userManagementAdGroupsForProduct?.Count == 0)
                        {
                            s.LockOnProductAccess = true;
                        }
                        if (userManagementAdGroupsForProduct.Count > 0)
                        {
                            if ((userManagementAdGroupsForProduct.Select(p => p.ADGroupId).Intersect(adGroupsForPersona.Select(a => a.ADGroupId))).ToList().Count == 0)
                            {
                                s.LockOnProductAccess = true;
                            }
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(productAccessRight))
                {
                    s.LockOnProductAccess = !editorRights.Contains(productAccessRight, StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        private string getRoleRightsSchemaName()
        {
            RPObjectCache rpcache = new RPObjectCache();

            var cacheKey = "getRoleRightsSchemaName_" + (int)ProductEnum.UnifiedPlatform;
            string schemaName = rpcache.GetFromCache<string>(cacheKey, 60, () =>
            {
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                return productInternalSettingList.FirstOrDefault(s => s.Name.Equals("RolesRightsSchemaName", StringComparison.OrdinalIgnoreCase))?.Value;
            });

            return schemaName;
        }

        private dynamic CompanyProductParam(string companyId, IList<int?> products, ProductProcVersion version, int rowsPerPage, int pageNumber,
                                            IList<string> roles, IList<string> rights, List<string> propertyIds)
        {
            switch ((int)version)
            {
                case 3:
                    return new
                    {
                        CompanyId = companyId,
                        ProductId = products.Count > 0 ? string.Join(",", products) : null,
                        RowsPerPage = rowsPerPage,
                        PageNumber = pageNumber,
                        Roles = roles.Count > 0 ? string.Join(",", roles) : null,
                        Rights = rights.Count > 0 ? string.Join(",", rights) : null,
                        Properties = propertyIds.Count> 0 ? string.Join("," , propertyIds) : null
                    };

                default:
                    return new
                    {
                        CompanyId = companyId,
                        ProductId = products.Count > 0 ? string.Join(",", products) : null,
                        RowsPerPage = rowsPerPage,
                        PageNumber = pageNumber
                    };
            }
        }

        public bool isProductAssigned(long personaId, int productStatus, int productId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = personaId, ProductStatusValue = productStatus.ToString() }).ToList().Any(x => x.ProductId == productId);
            }
        }

        public IList<UserBatchProductDetail> GetUserBatchDetails(int batchGroupId, long editorUserPersonId, long subjectUserPersonId)
        {
            using (var repo = GetRepository())
            {
                var data = repo.GetMany<UserBatchProductDetail>(StoredProcNameConstants.SP_GetUserBatchRecords, new
                {
                    batchProcessorGroupId = batchGroupId,
                    editorUserPersonId = editorUserPersonId,
                    subjectUserPersonId = subjectUserPersonId

                }).ToList();
                return data;
            }
        }

        public void UpdateBatchGroupStatus(int groupId, bool isLogged)
        {
            using (var repo = GetRepository())
            {
                repo.ExecuteNonQuery(StoredProcNameConstants.SP_UpdateProcessorGroupStatus, new
                {
                    groupId = groupId,
                    activiityLogged = isLogged
                });
            }
        }

        public void UpdateBatchProcessorLog(int batchProcessorId, DateTime? startDateTime, DateTime? endDateTime)
        {
            using (var repo = GetRepository())
            {
                repo.ExecuteNonQuery(StoredProcNameConstants.SP_InsertBatchProcessorLog, new
                {
                    batchProcessorId = batchProcessorId,
                    startDateTime = startDateTime,
                    endDateTime = endDateTime
                });
            }
        }

        public void InsertProductLoginActivitybyUser(int productId, long personaId, long UserId)
        {
            using (var repo = GetRepository())
            {
                repo.ExecuteNonQuery(StoredProcNameConstants.SP_InsertProductLoginActivitybyUser, new
                {
                    productId = productId,
                    personaId = personaId,
                    impersonatorUserId = UserId
                });
            }
        }
        #endregion
    }
}