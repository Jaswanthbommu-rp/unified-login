using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	/// <summary>
	/// Manage Product repository calls
	/// </summary>
	public class ManageProduct : IManageProduct
    {
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
        
        IProductRepository _productRepository;
        IProductInternalSettingRepository _productInternalSettingRepository;
        IManagePersona _managePersona;
        IManageBlueBook _manageBlueBook;
        IManagePartyRelationship _managePartyRelationship;
        IOrganizationRepository _organizationRepository;
        IManageProfile _manageProfile;
        IManageUserRoleRight _manageUserRoleRight;
	    DefaultUserClaim _defaultUserClaim;

        private readonly object rightLock = new object();

		#region Ctor

		/// <summary>
		/// Used for dependency injection
		/// </summary> 
		public ManageProduct(IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository, IManagePersona managePersona,
            IManageBlueBook manageBlueBook, IManagePartyRelationship managePartyRelationship,
            IManageOrganization manageOrganization, IManageProfile manageProfile,
            IManageUserRoleRight manageUserRoleRight, DefaultUserClaim userClaim)
        {
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePersona = managePersona;
            _manageBlueBook = manageBlueBook;
            _managePartyRelationship = managePartyRelationship;
            _organizationRepository = new OrganizationRepository();
            _manageProfile = manageProfile;
            _manageUserRoleRight = manageUserRoleRight;
            _defaultUserClaim = userClaim;
        }

        /// <summary>
        /// Repository test Constructor
        /// </summary>
        public ManageProduct(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _productRepository = new ProductRepository(repository, userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _manageBlueBook = new ManageBlueBook(userClaim, repository, _productInternalSettingRepository, messageHandler);
            _managePartyRelationship = new ManagePartyRelationship(repository);
            _organizationRepository = new OrganizationRepository(repository);
            _manageProfile = new ManageProfile(userClaim);
            _manageUserRoleRight = new ManageUserRoleRight();
            _defaultUserClaim = userClaim;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageProduct(DefaultUserClaim userClaim)
        {
            _productRepository = new ProductRepository(userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _managePersona = new ManagePersona(userClaim);
            _manageBlueBook = new ManageBlueBook(userClaim);
            _managePartyRelationship = new ManagePartyRelationship();
            _organizationRepository = new OrganizationRepository();
            _manageProfile = new ManageProfile(userClaim);
            _manageUserRoleRight = new ManageUserRoleRight();
	        _defaultUserClaim = userClaim;
        }

        #endregion

        #region Public Method

        /// <summary>
        /// List user(s)
        /// </summary>
        /// <param name="productId">Unique ProductId</param>
        /// <param name="blueBookCompanyInstanceId">Unique blueBook CompanyInstanceId</param>
        /// <param name="personaId">Optional Unique PersonaId</param>
        /// <returns>ProductUsers Object</returns>
        public IList<ProductUsers> GetProductUsers(int productId, long blueBookCompanyInstanceId, long personaId = 0)
        {
            if (!Enum.IsDefined(typeof(ProductEnum), productId))
            {
                throw new Exception("Invalid parameter ProductId.");
            }

            if ((blueBookCompanyInstanceId < -1) || (blueBookCompanyInstanceId == 0))
            {
                throw new Exception("Invalid parameter blueBook Company InstanceId.");
            }

            if (personaId < 0)
            {
                throw new Exception("Invalid parameter PersonaId.");
            }

            Persona persona = new Persona();
            PersonaCommon personaCommon = new PersonaCommon();
            IList<Persona> listPersona = new List<Persona>();
            IList<Persona> listPersonaByUser = new List<Persona>();
            IList<ProductUsers> listProductUsers = new List<ProductUsers>();
            IList<RightRoleDetail> listRightRoleDetail = new List<RightRoleDetail>();
            IList<RightRoleDetail> listRightByRole = new List<RightRoleDetail>();
            IList<Role> roleList = new List<Role>();
            Organization organization = new Organization();
            long? userPersonaId = null;
            Guid companyRealPageId = Guid.Empty;
            ;

            //Get the UL Organization details by the CompanyInstanceId from BlackBook
            if (blueBookCompanyInstanceId != -1)
            {
                var orgList = _organizationRepository.GetUnifiedLoginCompanyList();
                UnifiedLoginCompany ulc = orgList.FirstOrDefault(p => p.Domain.Equals("Primary") && p.BooksCustomerMasterId == blueBookCompanyInstanceId);
                if (ulc == null)
                {
                    throw new Exception("No company could be found.");
                }
                companyRealPageId = new Guid(ulc.CompanyRealPageId);
            }
            else
            {
                companyRealPageId = EmployeeCompanyRealPageId;
            }

            if (organization != null)
            {
				if (_defaultUserClaim.OrganizationRealPageGuid == Guid.Empty) {
					_defaultUserClaim.OrganizationRealPageGuid = organization.RealPageId;
				}
				List<int> productIds = _productRepository.GetProductIdsByCompany(organization.RealPageId).ToList();

                //Get the Organization PartyId from the call above and the ProductId from blackBook
                listRightRoleDetail = _productRepository.ListRoleWithRights(organization.PartyId, productId, productIds);
            }

            //If we got a PersonaId from the Product (e.g. BlackBook Research Application)
            if (personaId > 0)
            {
                userPersonaId = personaId;
                persona = _managePersona.GetPersona(personaId);
                listPersona.Add(persona);
            }
            else
            {
                listPersona = _managePersona.ListPersonaByOrganizationPartyId(organization.PartyId, null);
            }

            listProductUsers = _manageProfile.ListPersonsByProductId(productId, organization.RealPageId, userPersonaId);
            roleList = _manageUserRoleRight.GetAssignedRoleForPersona((ProductEnum)productId, userPersonaId, organization.PartyId);

            //Build a list of Right by Role
            roleList.ToList().ForEach(ro =>
            {
                listRightByRole = listRightRoleDetail.ToList().FindAll(rrd => rrd.RoleId == ro.RoleID);
                listRightByRole.ToList().ForEach(r =>
                {
                    lock(rightLock){
                        if (!ro.Right.Any(rr => rr.RightId == r.RightId && rr.RightName.Equals(r.RightName, StringComparison.OrdinalIgnoreCase) && rr.RightValueTypeId == r.RightValueTypeId && rr.RightNickName.Equals(r.RightNickName, StringComparison.OrdinalIgnoreCase)))
                        {
                            ro.Right.Add(
                                new Right()
                                {
                                    RightId = r.RightId,
                                    RightName = r.RightName,
                                    RightValueTypeId = r.RightValueTypeId,
                                    RightNickName = r.RightNickName
                                }
                            );
                        }
                    }
                });
            });

            //Build list of Personas by User
            listProductUsers.ToList().ForEach(u =>
            {
                listPersonaByUser = listPersona.ToList().FindAll(up => up.UserId == u.userLogin.UserId);
                listPersonaByUser.ToList().ForEach(p =>
                {
                    u.persona.Add(
                        new PersonaCommon()
                        {
                            PersonaId = p.PersonaId,
                            PersonPartyId = p.PersonaTypeId,
                            RealPageId = p.RealPageId,
                            OrganizationPartyId = p.OrganizationPartyId,
                            Name = p.Name,
                            UserId = p.UserId,
                            Role = roleList.ToList().FindAll(r => Convert.ToInt64(r.PersonaId) == p.PersonaId)
                        }
                    );
                });
            });

            return listProductUsers;
        }

        /// <summary>
        /// Used to return a list of products an organization has with option to merge access to persona access
        /// </summary>
        /// <param name="realPageId">realPageId</param>
        /// <param name="personaId">personaId</param>
        /// <param name="allProducts">Return all product types</param>
        /// <returns></returns>
        public IList<ProductUI> GetProducts(Guid realPageId, long personaId = 0, bool allProducts = false)
        {
            var listResponse = new ListResponse();

            if (realPageId == null || realPageId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(realPageId), "Null realPageId.");
            }

            IList<ProductUI> productList = _productRepository.GetProducts(organizationRealPageId: realPageId, personaId:personaId, allProducts:allProducts);

            if (personaId > 0)
            {
                Persona persona = _managePersona.GetPersona(personaId);

                if (persona == null)
                {
                    throw new ArgumentNullException(nameof(persona), "Null persona.");
                }

                var personaProducts = _productRepository.GetAssignedProductsByPersona(persona);

                // remove any deleted or inactive products from the tile page
                //productList = productList.Where(p => (!(p.ProductStatus == (int)ProductBatchStatusType.Deleted || p.ProductStatus == (int)ProductBatchStatusType.Inactive))).ToList();

                foreach (ProductUI product in productList)
                {
                    var personaProduct = personaProducts.SingleOrDefault(u => u.ProductId == product.ProductId);

                    if (personaProduct != null)
                    {
                        product.HasAccess = personaProduct.HasAccess;
                        product.ProductUrl = personaProduct.ProductUrl;
                    }

                    var favoriteProduct =
                        personaProducts.SingleOrDefault(u => u.ProductId == product.ProductId && u.IsFavorite == true);

                    if (favoriteProduct != null)
                    {
                        product.IsFavorite = favoriteProduct.IsFavorite;
                    }
                }

                try
                {
                    // if AO products then include from personaProducts
                    if (productList.Any(x => x.ProductId == (int) ProductEnum.AssetOptimizer))
                    {
                        var aoProductList =
                            personaProducts.Where(
                                y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum) y.ProductId)).ToList();
                        if (aoProductList.Any())
                        {
                            foreach (var aoProduct in aoProductList)
                            {
                                productList.Add(new ProductUI
                                    {
                                        ProductId = aoProduct.ProductId,
                                        ProductName = aoProduct.ProductName,
                                        FamilyId = aoProduct.FamilyId,
                                        Solution = aoProduct.Solution,
                                        ProductTypeId = aoProduct.ProductTypeId,
                                        IsFavorite = aoProduct.IsFavorite,
                                        SolutionId = aoProduct.SolutionId,
                                        ActivitiesList = aoProduct.ActivitiesList,
                                        ClassName = aoProduct.ClassName,
                                        ClientId = aoProduct.ClientId,
                                        Family = aoProduct.Family,
                                        HasAccess = true,
                                        IsAllowFavorite = aoProduct.IsAllowFavorite,
                                        IsNewTab = aoProduct.IsNewTab,
                                        IsResource = aoProduct.IsResource,
                                        LearnMore = aoProduct.LearnMore,
                                        ProductDescription = aoProduct.ProductDescription,
                                        ProductStatus = aoProduct.ProductStatus,
                                        ProductUrl = aoProduct.ProductUrl,
                                        SettingsUrl = aoProduct.SettingsUrl,
                                        Subsolution = aoProduct.Subsolution,
                                        TitleId = aoProduct.TitleId,
                                        TitleUniqueId = aoProduct.TitleUniqueId,
										ShowInAppSwitcher = aoProduct.ShowInAppSwitcher,
                                        ShowInUserListFilter = aoProduct.ShowInUserListFilter 
                                    }
                                );
                            }
                        }
                    }

                    // remove AO (umbrella) from products
                    var ao = productList.FirstOrDefault(x => x.ProductId == (int) ProductEnum.AssetOptimizer);
                    productList.Remove(ao);
                }
                catch
                {
                    // Skip AO specific exception to avoid impact on other products
                }

                productList = productList.OrderBy(e => e.IsFavorite ? 0 : 1).ThenBy(e => e.TitleId).ToList();
            }

            return productList;
        }

        /// <summary>
        /// Used to return a list of productfamilies
        /// </summary>
        /// <param name="organizationRealPageId">The unique identitifier for the organization</param>
        /// <param name="editorRealPageId">Populates while creating new user</param>
        /// <param name="personRealPageId">Populates when updating user</param>
        /// <param name="accessFilter">Filter Products</param>
		/// <param name="loginName">User Login Name</param>
        /// <returns>List of Product Families</returns>
        public IList<ProductFamily> GetProductFamilies(Guid organizationRealPageId, Guid editorRealPageId, Guid? personRealPageId, string accessFilter = null, string loginName = null)
        {
            //IProductRepository productRepository = new ProductRepository();
            var productFamilyList = _productRepository.GetProductFamilies(organizationRealPageId, editorRealPageId, personRealPageId, accessFilter, loginName);

            return productFamilyList;
        }

        /// <summary>
        /// Used to return a list of products user has access to, filterable by favorites and resouce only
        /// </summary>
        /// <param name="persona">persona</param>
        /// <param name="productSelectType">productSelectType</param>     
        /// <param name="security">security</param>
        /// <returns></returns>
        public IList<PersonaProductUserDetails> GetUserAssignedProductsByPersona(Persona persona, ProductSelectType? productSelectType = null, RouteSecurity security = null)
        {
            if (persona == null)
            {
                throw new ArgumentNullException(nameof(persona), "Null persona.");
            }

			//List<ProductRight> userRights = new List<ProductRight>();
			//List<Role> propRole = _userRoleRightRepository.ListRoleByPersona((int)ProductEnum.UnifiedLogin, persona.PersonaId, null);
			//foreach (Role r in propRole)
			//{
			//	List<ProductRight> roleRights = _unifiedLoginRepository.ListRightsByRole(persona.OrganizationPartyId, (int)ProductEnum.UnifiedLogin, r.RoleID) ?? new List<ProductRight>();
			//	foreach (ProductRight rght in roleRights)
			//	{
			//		if (!userRights.Any(p => p.ID == rght.ID))
			//		{
			//			userRights.Add(rght);
			//		}
			//	}
			//}

			//bool superUser = false;
			// TODO
			// need to see if the user is a SuperUser or not
			PartyRelationship partyRelationship = _managePartyRelationship.GetPartyRelationship(persona.RealPageId, persona.Organization.RealPageId, roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type");
            //if (partyRelationship?.RoleTypeFrom.Name.ToUpper() == "SUPERUSER")
            //{
            //    superUser = true;
            //}
            List<PersonaProductUserDetails> userProducts = new List<PersonaProductUserDetails>();

            userProducts = _productRepository.GetAssignedProductsByPersona(persona, productSelectType, security).ToList();

            // check in BlueBook to see which products are active and can be shown
            //IList<CustomerCompanyMap> availableProductList = _manageBlueBook.GetCompanyMap(persona.Organization.BooksCustomerMasterId);

            //if (availableProductList != null)
            //{
				// remove any products that are not assigned to the company in BlueBook
				// don't do this right now because some products are not yet in BlueBook
				//userProducts = ProcessProductListFromBooks(availableProductList, userProducts);
			//}

            // Remove AO & BM product from list
            userProducts.RemoveAll(x =>
                    x.ProductId == (int)ProductEnum.AssetOptimizer || x.ProductId == (int)ProductEnum.AoBenchmarking);

            // remove any deleted or inactive products from the tile page
            userProducts = userProducts.Where(p => (!(p.ProductStatus == (int)ProductBatchStatusType.Deleted || p.ProductStatus == (int)ProductBatchStatusType.Inactive))).ToList();

            userProducts = userProducts.OrderBy(e => e.IsFavorite ? 0 : 1).ThenBy(e => e.TitleId).ToList();

            return userProducts;
        }

        /// <summary>
        /// Expire and create a product setting of a persona
        /// </summary>        
        /// <param name="productSetting">productSetting</param>        
        /// <param name="personaId">personaId</param>        
        /// <returns></returns>
        public RepositoryResponse UpdateProductSetting(ProductSetting productSetting, long? personaId)
        {
            if (productSetting == null)
            {
                throw new ArgumentNullException(nameof(productSetting), "Null productSetting");
            }

            if (personaId == null)
            {
                throw new ArgumentNullException(nameof(personaId), "Null personaId");
            }

            var productSettingTypeId = _productRepository.GetProductSettingType(productSetting.Name.Trim());
            RepositoryResponse response = new RepositoryResponse();

            if (productSettingTypeId > 0)
            {
                response = _productRepository.CreateProductSetting(personaId.Value, productSetting.ProductId, productSettingTypeId, productSetting.Value);
            }
            else
            {
                response.ErrorMessage = $"Unable to get productSettingTypeId for {productSetting.Name}";
                response.Id = 0;
            }

            return response;
        }

	    /// <summary>
	    /// Used to get internal settings for a product
	    /// </summary>
	    /// <param name="productId">The id of the product to get the settings for</param>
	    /// <returns>The list of settings</returns>
	    public IList<ProductInternalSetting> GetProductInternalSettings(int productId)
	    {
		    RPObjectCache rpcache = new RPObjectCache();
		    var cacheKey = "productInternalSetting_" + productId;
		    var productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60, () =>
		    {
			    // load from database
			    return _productInternalSettingRepository.GetProductInternalSettings(productId);
		    });
		    return productInternalSettingList;
	    }

        /// <summary>
        /// Used to get all internal settings by product setting type
        /// </summary>
        /// <param name="productSettingType">The type of the product type to get the settings for</param>
        /// <returns>The list of settings</returns>
        public IList<ProductInternalSettingByType> GetProductSettingByType(string productSettingType)
        {
            if (!ListProductSettingType().Any(pst => pst.Name.Equals(productSettingType, StringComparison.OrdinalIgnoreCase)))
            {
                return new List<ProductInternalSettingByType>();
            }

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSettingByType_" + productSettingType;
            var productInternalSettingByTypeList = rpcache.GetFromCache<IList<ProductInternalSettingByType>>(cacheKey, 60, () =>
            {
                // load from database
                return _productInternalSettingRepository.GetProductSettingByType(productSettingType);
            });
            return productInternalSettingByTypeList;
        }

        /// <summary>
        /// Used to add or update a product setting for the given configuration
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productInternalSetting"></param>
        /// <returns></returns>
        public RepositoryResponse CreateProductSettingAndLinkToConfiguration(int productId, ProductInternalSetting productInternalSetting)
        {
            RepositoryResponse response = _productInternalSettingRepository.CreateProductSettingAndLinkToConfiguration(productId, productInternalSetting);
            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                MemoryCache.Default.Remove("productInternalSetting_" + productId);
            }

            return response;
        }

        /// <summary>
        /// Returns a list of productSettingType
        /// </summary>
        /// <returns></returns>
        public IList<ProductSettingType> ListProductSettingType()
        {
            return _productRepository.ListProductSettingType();
        }

        /// <summary>
        /// Used to return a list of productTypes
        /// </summary>
        /// <returns></returns>
        public IList<ProductType> GetProductTypes()
        {
            var result = _productRepository.GetProductTypes();

            return result;
        }
        #endregion

        #region Privates

        private List<int> GetProductIdsByOrg()
        {
            List<int> productIds = _productRepository.GetProductIdsByCompany(_defaultUserClaim.OrganizationRealPageGuid).ToList();
            return productIds;
        }

        /// <summary>
        /// Used to remove any products that are not currently enabled in BlueBook
        /// </summary>
        /// <param name="availableProductsList"></param>
        /// <param name="userProducts"></param>
        /// <returns></returns>
        private IList<PersonaProductUserDetails> ProcessProductListFromBooks(IList<CustomerCompanyMap> availableProductsList, IList<PersonaProductUserDetails> userProducts)
        {
            var bbConstants = typeof(BlueBookProductConstants).GetFields();

            // add products that do not exist in BlueBook so they aren't removed
            List<ProductEnum> prodList = new List<ProductEnum>()
            {
                ProductEnum.ResearchApplication,
                ProductEnum.ProductLearningPortal,
            };

            foreach (CustomerCompanyMap cmr in availableProductsList)
            {
                foreach (var pi in bbConstants)
                {
                    if (cmr.Source.ToUpper() == pi.GetValue(pi).ToString())
                    {
                        prodList.Add((ProductEnum)Enum.Parse(typeof(ProductEnum), pi.Name));
                        break;
                    }
                }
            }

            foreach (PersonaProductUserDetails pud in userProducts.ToList())
            {
                // if the product wasn't found in BlueBook, remove it
                if (!(prodList.Any(p => (int)p == pud.ProductId)))
                {
                    // remove it
                    userProducts.Remove(pud);
                }
            }

            return userProducts;
        }

        /// <summary>
        /// List GB products
        /// </summary>
        public IList<GbProductMap> ListProducts(int? productId = null, Guid? productGuid = null, string name = null, string booksProductCode = null)
        {
            var result = _productRepository.ListProducts(productId, productGuid, name, booksProductCode);
            return result;
        }

        /// <summary>
        /// Used to get a list of products for the given persona id
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="statusType"></param>
        /// <returns></returns>
        public IList<PersonaProduct> GetAllProductsByPersona(long personaId, ProductBatchStatusType statusType)
        {
            return _productRepository.GetAllProductsByPersona(personaId, statusType);
        }

        public IList<ProductUI> AddProductSourceAndGreenBookCareFlagToProducts(Guid upfmCompanyId, IList<ProductUI> products)
        {
            var booksCompanyInstance = _manageBlueBook.GetCompanyInstanceByUPFMCompanyId(upfmCompanyId.ToString().ToLower());
            int customerCompanyId = booksCompanyInstance?.Attributes?.CustomerCompanyMap.FirstOrDefault()?.CustomerCompanyId ?? 0;
            string domain = booksCompanyInstance?.Attributes?.Domain;
            var productInternalSettingType = _productInternalSettingRepository.GetProductSettingByType("UsePrimaryProperties");
            foreach (var product in products)
            {
                //Assign PrimaryProperty flag for Product               
                string UsePrimaryProperties = productInternalSettingType?
                                                        .Where(p => p.BooksProductCode == product.ProductCode
                                                            && p.ProductId == product.ProductId
                                                            && p.Name.ToLower() == "useprimaryproperties")
                                                            ?.FirstOrDefault()?.Value;
                product.UsePrimaryProperties = UsePrimaryProperties != null && (UsePrimaryProperties !="0");
                /*
                 * Not found" when:
                    Product instance does not exist in UDM
                    Product instance does not have an assigned domain(this check is not required as we are getting all th products of same domain of company)
                    Product instance domain is not the same as the UPFM company instance
                    If we have more then one product instance with same product code irrespective of greenbookcare flag
                 */
                if (!string.IsNullOrEmpty(domain) && customerCompanyId != 0)
                {
                    var booksCustomerCompanyMap = _manageBlueBook.GetCustomerCompanyMapByCustomerCompanyId(customerCompanyId, domain);
                    var findBooksProductCode = booksCustomerCompanyMap?.Where(p => p.Source == product.ProductCode);
                    if (findBooksProductCode != null && findBooksProductCode.Count() == 1)
                    {
                        product.ProductInstance = findBooksProductCode.FirstOrDefault().CompanyInstanceSourceId;
                        product.GreenBookCares = findBooksProductCode.FirstOrDefault().CompanyInstance.FirstOrDefault().GreenBookCares;
                    }
                }
            }
            return products;
        }

        #endregion
    }
}