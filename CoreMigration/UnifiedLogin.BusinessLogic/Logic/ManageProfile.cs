using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage Profile repository calls
    /// </summary>
    public class ManageProfile : IManageProfile
    {
		#region Private Variables
		IProfileRepository _profileRepository;
		IProductRepository _productRepository;
        IManagePerson _personLogic;
        IManageUserLogin _userLoginLogic;
        IManagePartyRelationship _partyRelationshipLogic;
        IManageContactMechanism _contactMechanismLogic;
        IManagePartyRole _partyRoleLogic;
		int? _parentPartyRoleTypeId = null;
	    private DefaultUserClaim _userClaim;

        #endregion

        #region Constructors
        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaim"></param>
        /// <param name="messageHandler"></param>
        public ManageProfile(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _profileRepository = new ProfileRepository(repository, userClaim, messageHandler);
            _productRepository = new ProductRepository(repository, userClaim);
            _personLogic = new ManagePerson(repository);
            _userLoginLogic = new ManageUserLogin(repository, userClaim, messageHandler);
            _partyRelationshipLogic = new ManagePartyRelationship(repository);
            _contactMechanismLogic = new ManageContactMechanism(repository);
            _partyRoleLogic = new ManagePartyRole(repository);
            //For list Persons, return users of RoleType Parent = User Role (400)
            _parentPartyRoleTypeId = (int)ParentUserRoleType.UserRole;
            _userClaim = userClaim;
        }

        /// <summary>
        /// Create a basic instance of the ManageProfile Controller class
        /// </summary>
        /// <param name="userClaim">Information about the user</param>
        public ManageProfile(DefaultUserClaim userClaim)
	    {
		    _profileRepository = new ProfileRepository(userClaim);
			_productRepository = new ProductRepository(userClaim);
		    _personLogic = new ManagePerson();
		    _userLoginLogic = new ManageUserLogin(userClaim);
		    _partyRelationshipLogic = new ManagePartyRelationship();
		    _contactMechanismLogic = new ManageContactMechanism();
		    _partyRoleLogic = new ManagePartyRole();
			//For list Persons, return users of RoleType Parent = User Role (400)
			_parentPartyRoleTypeId = (int)ParentUserRoleType.UserRole;
		    _userClaim = userClaim;
	    }
        #endregion

        #region Public methods
        /// <summary>
        /// Get Profile Detail for a person
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="orgPartyId"></param>
        /// <param name="roleTypeFrom"></param>
        /// <param name="roleTypeTo"></param>
        /// <param name="relationshipType"></param>
        /// <param name="contactMechanismUsageTypeName"></param>
        /// <returns></returns>
        public IProfileDetail GetProfileDetail(Guid realPageId, long orgPartyId, string roleTypeFrom = null, string roleTypeTo = null, string relationshipType = null, string contactMechanismUsageTypeName = null)
        {
            IProfileDetail profileDetail = new ProfileDetail();

            IList<Organization> organizationList = new List<Organization>();
            IList<CommonAddress> contactMechansimList = new List<CommonAddress>();
            List<OrganizationSetting> organizationSettings = new List<OrganizationSetting>();

            IPerson person = _personLogic.GetPerson(realPageId);
            // TODO FIX!
            var userLogin = _userLoginLogic.GetUserLogin(realPageId, orgPartyId); // keep for now
            IManageConfigurationSetting configurationSettingLogic = new ManageConfigurationSetting();

            organizationList = _userLoginLogic.ListOrganizationByEnterpriseUserId(realPageId, relationshipType).Where(p => p.PartyId == orgPartyId).ToList();

            foreach (var organization in organizationList)
            {
                relationshipType = organization.RelationshipType;
				PartyRelationship partyRelationship = _partyRelationshipLogic.GetPartyRelationship(realPageId, organization.RealPageId, roleTypeFrom, roleTypeTo, relationshipType);
                if (partyRelationship != null)
                {
                    organization.partyRelationship = partyRelationship;
                }
            }

            //var orgPartyId = organizationList.Select(p => p.PartyId).FirstOrDefault();
            var orgSettings = configurationSettingLogic.ListOrganizationConfigurationSetting(orgPartyId, null);

            if (orgSettings?.Count > 0)
            {
                foreach (var orgsetting in orgSettings)
                {
                    OrganizationSetting setting = new OrganizationSetting
                    {
                        Value = orgsetting.Value,
                        Name = orgsetting.SettingName
                    };
                    organizationSettings.Add(setting);
                }
            }

            contactMechansimList = _contactMechanismLogic.ListContactMechanismForPerson(realPageId, contactMechanismUsageTypeName);

            profileDetail.PartyId = person.PartyId;
            profileDetail.RealPageId = person.RealPageId;
            profileDetail.Title = person.Title;
            profileDetail.FirstName = person.FirstName;
            profileDetail.MiddleName = person.MiddleName;
            profileDetail.LastName = person.LastName;
            profileDetail.Suffix = person.Suffix;
            profileDetail.PreferredContactMethodId = person.PreferredContactMethodId;
            profileDetail.userLogin = userLogin;
            profileDetail.organization = organizationList;
            profileDetail.contactMechanism = contactMechansimList;
            profileDetail.OrganizationSettings = organizationSettings;
            profileDetail.UserTypeId = (int)userLogin.UserRoleType;

			var notificationEmail = profileDetail.contactMechanism.Where<CommonAddress>(p => p.contactMechanismUsageType.ContactMechanismUsageTypeId == 301).ToList();
            if (notificationEmail.Count > 0)
            {
                var userTypes = new List<UserRoleType>() { UserRoleType.UserNoEmail };
                if(profileDetail.organization.HasAnyUserRole(userTypes))
                {
                    profileDetail.NotificationEmail = notificationEmail[0].AddressString;
                }
            }
            
            //IManagePartyRole managePartyRole = new ManagePartyRole();
            profileDetail.PartyRole =  _partyRoleLogic.GetPartyRole(realPageId);

            return profileDetail;
        }

        public bool GetOrganizationHasProductAssignmentError(long orgPartyId)
        {
            return _profileRepository.GetOrganizationHasAnyProductAssignmentError(orgPartyId);
		}
        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">Profile object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateProfile(Guid realPageId, IProfile profile)
        {
            if (realPageId == Guid.Empty)
            {
				throw new Exception("Invalid parameter realPageId.");
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile), "Null Profile.");
            }

            return _profileRepository.UpdateProfile(realPageId, profile);
        }

        /// <summary>
        /// Get a list of persons 
        /// </summary>
        /// <param name="globals">Parameter for filter and sort</param>
        /// <param name="organizationRealPageId">Organization's realPageId</param>
        /// <returns>List of Persons</returns>
        public IList<ProfileDetail> ListProfileDetails(IDictionary<object, object> globals, Guid? organizationRealPageId = null)
        {
			IList<ProfileDetail> profileDetailList = new List<ProfileDetail>();
            RequestParameter dataFilter = new RequestParameter();
            bool isExport = false;
            // for now, get the current user organization to filter the list of users
            //ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            //if (currentClaimPrincipal.Identity.IsAuthenticated)
            //{
            //    Persona persona = _personaLogic.GetActivePersona(_userClaim.UserRealPageGuid);
            if (organizationRealPageId == null || !_userClaim.RealPageEmployee)
            {
                organizationRealPageId = _userClaim.OrganizationRealPageGuid;
            }

			if (globals.ContainsKey(BaseType.RequestParameter))
			{
				dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
			}
            if (globals.ContainsKey("isExport"))
            {
                isExport = true;
            }
            IList<int> organizationActiveProductIdList = _productRepository.GetProductIdsByCompany(_userClaim.OrganizationRealPageGuid);
            if (organizationActiveProductIdList.Contains((int)ProductEnum.AssetOptimizer))
            {
                var allProducts = _productRepository.GetAllProducts();
                organizationActiveProductIdList.Remove((int)ProductEnum.AssetOptimizer);
                foreach (var product in allProducts)
                {
                    if (ProductEnumHelper.CheckAoProductSupportedByGreenBook(product.BooksProductCode))
                    {
                        if (!organizationActiveProductIdList.Contains(product.ProductId))
                        {
                            organizationActiveProductIdList.Add(product.ProductId);
                        }
                    }
                }
            }
            profileDetailList = _profileRepository.ListPersons(
				organizationActiveProductIdList: organizationActiveProductIdList,
				realPageId: organizationRealPageId,
				parentPartyRoleTypeId: _parentPartyRoleTypeId,
				dataFilterSort: dataFilter,
                isExport: isExport);
	
            return profileDetailList;
        }

		/// <summary>
		/// Returns a list of persons by ProductId
		/// </summary>
		/// <param name="productId">Single product to search by product id</param>
		/// <param name="organizationRealPageId">Optional Organization realpage uniqueidentifier</param>
		/// <param name="personaId">Optional personaId</param>
		/// <returns>List of Person</returns>
		public IList<ProductUsers> ListPersonsByProductId(int productId, Guid? organizationRealPageId = null, long? personaId = null)
		{
			IList<ProductUsers> productUsersList = new List<ProductUsers>();

			productUsersList = _profileRepository.ListPersonsByProductId(productId, organizationRealPageId, personaId);

			return productUsersList;
		}
		#endregion
	}
}