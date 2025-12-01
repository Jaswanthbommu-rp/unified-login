using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Product.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedAmenities;
using UnifiedLogin.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;
using UL = UnifiedLogin.SharedObjects.Product.UserManagement;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    /// <summary>
    /// Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
	public class ManageUnifiedAmenitiesTest : ManageProductBaseTests
	{
		private static string _companyName = "TESTCOMPANY";
		private Mock<IManageBlueBook> _mockManageBlueBook = new Mock<IManageBlueBook>();
		private Mock<IManagePersona> _mockManagePersona = new Mock<IManagePersona>();
		private Mock<IProductRepository> _mockProductRepository = new Mock<IProductRepository>();
		private Mock<ISamlRepository> _mockSamlRepository = new Mock<ISamlRepository>();
		private Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
		private Mock<IUserRoleRightRepository> _mockUserRoleRightRepository = new Mock<IUserRoleRightRepository>();
		private Mock<IManagePerson> _mockManagePerson = new Mock<IManagePerson>();
		private Mock<IManageUserLogin> _mockManageUserLogin = new Mock<IManageUserLogin>();
		private Mock<IManagePartyRelationship> _mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
		private Mock<IUnifiedLoginRepository> _mockUnifiedLoginRepository = new Mock<IUnifiedLoginRepository>();
		private Mock<IPropertyRepository> _mockPropertyRepository = new Mock<IPropertyRepository>();
        private Mock<IUserLoginRepository> _mockUserLoginRepository = new Mock<IUserLoginRepository>();

        private RequestParameter _reqParameter = new RequestParameter();

		private IC.Person _person = new IC.Person();
		private UserLoginOnly _userlogin = new UserLoginOnly();
		private List<UL.Role> _roleListByPersona = new List<UL.Role>();
		private IList<CustomerCompanyMap> _mapCompany = new List<CustomerCompanyMap>();
		private IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();

		private List<ProductRole> _productRoles = new List<ProductRole>();
		private List<ProductRight> _productRights = new List<ProductRight>();

		private RepositoryResponse _roleRightResult = new RepositoryResponse();
		private RepositoryResponse _roleRightErrorResult = new RepositoryResponse();

		private IList<PropertyInstance> _propertyInstanceList = new List<PropertyInstance>();
		private IList<CustomerCompanyPropertyMap> _vCompanyPropertyMap = new List<CustomerCompanyPropertyMap>();
		private List<ProductProperty> _productPropertyList = new List<ProductProperty>();

		private Company _company = new Company();
        private CustomerCompany _customercompany = new CustomerCompany();

        private GbProductMap _gbProductMap = new GbProductMap();


		public ManageUnifiedAmenitiesTest() : base((int)ProductEnum.UnifiedAmenities)
		{
			_emptySamlAttributes = new List<SamlAttributes>();
			_productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

			_repositoryResponseProductStatus = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
			_repositoryResponsePropertySuccess = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
			_repositoryResponsePropertyFail = new RepositoryResponse() { ErrorMessage = "error", Id = -1 };
		}

		/// <summary>
		/// used to initialize unit tests
		/// </summary>
		private void AssertInitial()
		{
			//Arrange
			_reqParameter.Pages.ResultsPerPage = 1000;
			_reqParameter.SortBy = new Dictionary<string, string>
			{
				{"SiteName", "ASC"}
			};
			_reqParameter.FilterBy = new Dictionary<string, string>
			{
				{"SiteName", "Test Property"}
			};

			_person = new IC.Person() { FirstName = "Test First", LastName = "Test Last", PartyId = 30 };
			_userlogin = new UserLoginOnly() { UserId = _editorUserId, LoginName = "test", PartyId = 30, RealPageId = new Guid(), LastLogin = DateTime.Now };

			_roleListByPersona = new List<UL.Role>() { new UL.Role() { RoleID = 1, Name = "Test Role" } };

			_partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_SUPERUSER };

			_mapCompany = new List<CustomerCompanyMap>()
			{
				new CustomerCompanyMap() {CompanyInstanceSourceId = _companyName, Source = BlueBookProductConstants.UnifiedAmenities, CompanyInstanceId = 1234},
				new CustomerCompanyMap() {CompanyInstanceSourceId = _companyName, Source = BlueBookProductConstants.Insurance, CompanyInstanceId = 4321}
			};

			_roleRightResult = new RepositoryResponse() { Id = 1, ErrorMessage = "" };

			_productRoles = new List<ProductRole>()
			{
				new ProductRole() {ID = "1", Description = "Amenity.No.Pricing", Name = "Manage Amenity No Pricing", IsAssigned = true},
				new ProductRole() {ID = "2", Description = "Amenity.Status", Name = "Manage Amenity Status", IsAssigned = false},
				new ProductRole() {ID = "3", Description = "Amenity.With.Pricing", Name = "Manage Amenity With Pricing", IsAssigned = true},
				new ProductRole() {ID = "4", Description = "Prop.Amenity.No.Pricing", Name = "Manage Property Amenity No Pricing", IsAssigned = false},
				new ProductRole() {ID = "5", Description = "Prop.Amenity.With.Pricing", Name = "Manage Property Amenity Pricing", IsAssigned = false},
				new ProductRole() {ID = "6", Description = "View.Amenities", Name = "View Amenities", IsAssigned = false},
			};

			_productRights = new List<ProductRight>()
			{
				new ProductRight() {ID = 1, Description = "A right here", Assigned = true},
				new ProductRight() {ID = 2, Description = "Test right", Assigned = false},
				new ProductRight() {ID = 3, Description = "Z right here", Assigned = true},
				new ProductRight() {ID = 4, Description = "My right", Assigned = false},
				new ProductRight() {ID = 5, Description = "Another right", Assigned = false},
			};

			_propertyInstanceList = new List<PropertyInstance>()
			{
				new PropertyInstance() {PropertyName = "Property 1", PropertyInstanceSourceId = "1234567", Address = new InstanceAddress() {State = "TX"}, IsActive = true},
				new PropertyInstance() {PropertyName = "Property 2", PropertyInstanceSourceId = "54321", Address = new InstanceAddress() {State = "TX"}, IsActive = true}
			};

			_vCompanyPropertyMap = new List<CustomerCompanyPropertyMap>()
			{
				new CustomerCompanyPropertyMap() {PropertyName = "Property 1", CustomerPropertyId = 1234567, PropertyAddress = "1234 Main Street", PropertyState = "TX", IsActive = true},
				new CustomerCompanyPropertyMap() {PropertyName = "Property 2", CustomerPropertyId = 54321, PropertyAddress = "4543 Street", PropertyState = "TX", IsActive = true}
			};

			_productPropertyList = new List<ProductProperty>()
			{
				new ProductProperty() {Name = "Property 1", ID = "1234567"},
			};

			IList<GbProductMap> productList = new List<GbProductMap> { new GbProductMap { ProductId = 26, BooksProductCode = "UA", Name = "Unified Amenities" } };
			_customercompany = new CustomerCompany() { CustomerCompanyId = 123456, IsActive = true, CompanyName = "Test Company",  MigrationStatus = "migrated" };//Category = "rpup"

            _gbProductMap = new GbProductMap { ProductId = (int)ProductEnum.UnifiedAmenities, BooksProductCode = "UA", Name = "Unified Amenities" };

			_company = new Company()
			{
				CustomerCompanyId = 123456,
				CompanyName = "Test Company",
				IsActive = true,
				CustomerCompany = new List<CustomerCompany>() { _customercompany }
			};

			_mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
				))
				.Returns(_mapCompany);

			_mockManageBlueBook
				.Setup(m => m.GetVCompanyPropertyMap(
					It.IsAny<long>(),
					It.IsAny<string>()
				))
				.Returns(_vCompanyPropertyMap);

			_mockManageBlueBook
				.Setup(m => m.GetCompanyCustomerInfo(
					It.IsAny<Guid>(),
					It.IsAny<string>(),
					It.IsAny<long>()
				))
				.Returns(_customercompany);

			_mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.IsAny<long>()
					, It.IsAny<int>()
				))
				.Returns(_emptySamlAttributes);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 4)
				))
				.Returns(_editorPersona);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 5)
				))
				.Returns(_userPersona);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 10)
				))
				.Returns(_userInvalidPersona);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 33)
				))
				.Returns(_nullPersona);

			_mockManagePerson
				.Setup(m => m.GetPerson(
					It.IsAny<Guid>()
				))
				.Returns(_person);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(
					It.IsAny<Guid>()
				))
				.Returns(_userlogin);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 5)
				))
				.Returns(_userPersona);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 10)
				))
				.Returns(_userInvalidPersona);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 33)
				))
				.Returns(_nullPersona);

			_mockProductRepository
				.Setup(m => m.GetProductSettingsByPersona(
					It.IsAny<long>()
				))
				.Returns(_userProductSettings);

			_mockProductRepository
				.Setup(m => m.ListProductSettingType(
				))
				.Returns(_productSettingType);

			_mockProductRepository
				.Setup(m => m.ListRolesForProductByParty(
					It.IsAny<long>(),
					It.IsAny<List<int>>(),
					It.IsAny<int>()
				))
				.Returns(_productRoles);

			_mockProductRepository
				.Setup(m => m.ListProducts(
					null, null, null, null
				))
				.Returns(productList);

			_mockProductRepository
				.Setup(m => m.GetBooksMasterProductDetail(
					It.IsAny<int>()
				))
				.Returns(_gbProductMap);

			_mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(
					It.IsAny<int>()
				))
				.Returns(_productInternalSettings);

			_mockManagePartyRelationship
				.Setup(m => m.GetPartyRelationship(
					It.IsAny<Guid>()
					, It.IsAny<Guid>()
					, null
					, null
					, It.IsAny<string>()
				))
				.Returns(_partyRelationShip);

			_mockUserRoleRightRepository
				.Setup(m => m.ListRoleByPersona(
					It.IsAny<int>(),
					It.IsAny<long>(),
					null
				))
				.Returns(_roleListByPersona);

			_mockUserRoleRightRepository
				.Setup(m => m.InsertAssignedRoleToUser(
					It.IsAny<long>(),
					It.IsAny<long>(),
					It.IsAny<int>(),
					It.IsAny<bool>()
				))
				.Returns(_roleRightResult);

			_mockUnifiedLoginRepository
				.Setup(m => m.ListRightsByRole(
					It.IsAny<long>(),
					It.IsAny<List<int>>(),
					It.IsAny<long>(),
					It.IsAny<long>()
                    
				))
				.Returns(_productRights);

			_mockPropertyRepository
				.Setup(m => m.ListPropertiesByPersona(
					It.IsAny<long>(),
					It.IsAny<int>()
				))
				.Returns(_productPropertyList);

			_mockPropertyRepository
				.Setup(m => m.InsertRemoveAssignedPropertyToUser(
					It.IsAny<long>(),
					It.IsAny<ProductEnum>(),
					It.IsAny<long>(),
					It.IsAny<int>()
				))
				.Returns(_repositoryResponsePropertySuccess);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _editorPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusEditorPersona);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _newUserPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusNewUserPersona);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _userPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusUserPersona);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _userInvalidPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusInvalidPersona);
        }

		[Fact]
		public void Put_UnassignUser()
		{
			//Arrange
			AssertInitial();

			//Act
			IManageUnifiedAmenities manageProduct 
                = new ManageUnifiedAmenities(_editorUserClaim, 
                    _mockManagePersona.Object, 
                    _mockManagePerson.Object, 
                    _mockManageBlueBook.Object, 
                    _mockProductRepository.Object, 
                    _mockSamlRepository.Object, 
                    _mockProductInternalSettingRepository.Object, 
                    null,
                    _mockUserRoleRightRepository.Object,
                    _mockManageUserLogin.Object, 
                    null, 
                    _mockPropertyRepository.Object,
                    _mockUserLoginRepository.Object);

			string result = manageProduct.UnassignUser(_editorPersonaId, _userPersonaId, null);
			Assert.True(string.IsNullOrEmpty(result));

			//manageProduct = new ManageUnifiedAmenities(_editorUserClaim, _mockManagePersona.Object, _mockManageBlueBook.Object, _mockProductRepository.Object, _mockSamlRepository.Object, _mockProductInternalSettingRepository.Object, null, _mockUserRoleRightRepository.Object);
			result = manageProduct.UnassignUser(0, 0, null);
			Assert.True(result.ToUpper() == "INVALID PERSONA");

			// error removing role
			_roleRightErrorResult = new RepositoryResponse() { ErrorMessage = "There was an error removing the role", Id = -1 };

			_mockUserRoleRightRepository
				.Setup(m => m.InsertAssignedRoleToUser(
					It.IsAny<long>(),
					It.IsAny<long>(),
					It.IsAny<int>(),
					It.IsAny<bool>()
				))
				.Returns(_roleRightErrorResult);

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim, 
                _mockManagePersona.Object,
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, 
                null, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                null, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			result = manageProduct.UnassignUser(_editorPersonaId, _userPersonaId, null);
			Assert.True(result.ToUpper() == "THERE WAS AN ERROR REMOVING THE ROLE");

			// null role response
			_roleListByPersona = null;
			_mockUserRoleRightRepository
				.Setup(m => m.ListRoleByPersona(
					It.IsAny<int>(),
					It.IsAny<long>(),
					null
				))
				.Returns(_roleListByPersona);

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim, 
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, 
                null, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                null, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);
			result = manageProduct.UnassignUser(_editorPersonaId, _userPersonaId, null);
			Assert.True(string.IsNullOrEmpty(result));
		}

		[Fact]
		public void Put_ManageUnifiedAmenitiesUser()
		{
			//Arrange
			AssertInitial();

			//Act
			IManageUnifiedAmenities manageProduct = new ManageUnifiedAmenities(_editorUserClaim, 
                _mockManagePersona.Object, 
                _mockManagePerson.Object, _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, 
                _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object,
                _mockManageUserLogin.Object, 
                null, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			string result = manageProduct.ManageUnifiedAmenitiesUser(_editorPersonaId, _userPersonaId, null);
			Assert.True(string.IsNullOrEmpty(result));

			result = manageProduct.ManageUnifiedAmenitiesUser(0, 0, null);
			Assert.True(result.ToUpper() == "INVALID PERSONA");

			// success deleting existing role and adding new role
			_roleRightErrorResult = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
			_mockUserRoleRightRepository
				.Setup(m => m.InsertAssignedRoleToUser(
					It.IsAny<long>(),
					It.IsAny<long>(),
					It.IsAny<int>(),
					It.IsAny<bool>()
				))
				.Returns(_roleRightErrorResult);
			UnifiedAmenitiesPropertyRole userAssignProductPropertyRole = new UnifiedAmenitiesPropertyRole() { IsAssigned = true, RoleList = new List<string>() { "10" } };

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim, 
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, _mockProductRepository.Object, _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object, _mockManageUserLogin.Object, null, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);
			result = manageProduct.ManageUnifiedAmenitiesUser(_editorPersonaId, _userPersonaId, userAssignProductPropertyRole);
			Assert.True(string.IsNullOrEmpty(result));

			// fail to remove existing role from a user
			_roleRightErrorResult = new RepositoryResponse() { ErrorMessage = "Problem deleting role from user", Id = -1 };
			_mockUserRoleRightRepository
				.Setup(m => m.InsertAssignedRoleToUser(
					It.IsAny<long>(),
					It.Is<long>(l => l == 1),
					It.IsAny<int>(),
					It.IsAny<bool>()
				))
				.Returns(_roleRightErrorResult);

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim, _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object,
                _mockProductInternalSettingRepository.Object, 
                _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                null, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			result = manageProduct.ManageUnifiedAmenitiesUser(_editorPersonaId, _userPersonaId, userAssignProductPropertyRole);
			Assert.True(result.ToUpper() == "PROBLEM DELETING ROLE FROM USER");

			// fail to add new role to user
			_mockUserRoleRightRepository = new Mock<IUserRoleRightRepository>();
			RepositoryResponse roleRightResultSuccess = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
			_mockUserRoleRightRepository
				.Setup(m => m.InsertAssignedRoleToUser(
					It.IsAny<long>(),
					It.Is<long>(l => l == 1),
					It.IsAny<int>(),
					It.IsAny<bool>()
				))
				.Returns(roleRightResultSuccess);

			RepositoryResponse roleRightResultFail = new RepositoryResponse() { ErrorMessage = "Problem deleting role from user", Id = -1 };
			_mockUserRoleRightRepository
				.Setup(m => m.InsertAssignedRoleToUser(
					It.IsAny<long>(),
					It.Is<long>(l => l == 3),
					It.IsAny<int>(),
					It.IsAny<bool>()
				))
				.Returns(roleRightResultFail);

			_mockUserRoleRightRepository
				.Setup(m => m.ListRoleByPersona(
					It.IsAny<int>(),
					It.IsAny<long>(),
					null
				))
				.Returns(_roleListByPersona);

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim, 
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, 
                _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                null, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			result = manageProduct.ManageUnifiedAmenitiesUser(_editorPersonaId, _userPersonaId, userAssignProductPropertyRole);
			Assert.True(result.ToUpper() == "PROBLEM DELETING ROLE FROM USER");

			// null role response
			_roleListByPersona = null;
			_mockUserRoleRightRepository
				.Setup(m => m.ListRoleByPersona(
					It.IsAny<int>(),
					It.IsAny<long>(),
					It.IsAny<long>()
				))
				.Returns(_roleListByPersona);

			_roleRightErrorResult = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
			_mockUserRoleRightRepository
				.Setup(m => m.InsertAssignedRoleToUser(
					It.IsAny<long>(),
					It.IsAny<long>(),
					It.IsAny<int>(),
					It.IsAny<bool>()
				))
				.Returns(_roleRightErrorResult);

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim, 
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, 
                _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                null, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			result = manageProduct.ManageUnifiedAmenitiesUser(_editorPersonaId, _userPersonaId, null);
			Assert.True(string.IsNullOrEmpty(result));
		}

		[Fact]
		public void Get_GetRoles()
		{
			//Arrange
			AssertInitial();

			IManageUnifiedAmenities manageProduct = new ManageUnifiedAmenities(_editorUserClaim,
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, 
                _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                _mockUnifiedLoginRepository.Object, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			// invalid persona
			ListResponse response = manageProduct.GetRoles(0, 0, 3);
			Assert.True(response.IsError == true);

			// valid data
			response = manageProduct.GetRoles(4, 4, 3);
			Assert.True(response.Records.Count == 6);

			// verify the records are ordered
			List<ProductRole> productRoleResult = response.Records.Cast<ProductRole>().ToList();

			Assert.True(productRoleResult[0].Name == _productRoles[0].Name &&
						productRoleResult[1].Name == _productRoles[1].Name &&
						productRoleResult[2].Name == _productRoles[2].Name &&
						productRoleResult[3].Name == _productRoles[3].Name &&
						productRoleResult[4].Name == _productRoles[4].Name
			);
			List<ProductRole> nullRoleList = null;

			_mockProductRepository
				.Setup(m => m.ListRolesForProductByParty(
					It.IsAny<long>(),
					It.IsAny<List<int>>(),
					It.IsAny<int>()
				))
				.Returns(nullRoleList);

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim, 
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object,
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object, 
                _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                _mockUnifiedLoginRepository.Object, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			response = manageProduct.GetRoles(4, 4, 3);
			Assert.True(response.Records.Count == 0);

			response = manageProduct.GetRoles(4, 0, 3);
			Assert.True(response.Records.Count == 0);

			// exceptions
			_mockProductRepository
				.Setup(m => m.ListRolesForProductByParty(
					It.IsAny<long>(),
					It.IsAny<List<int>>(),
					It.IsAny<int>()
				))
				.Throws(new Exception("Invalid SQL"));

			response = manageProduct.GetRoles(4, 0, 3);
			Assert.True(response.IsError == true && (response.ErrorReason == "There was a problem getting the roles.") ||
													 response.ErrorReason == CommonMessageConstants.CompanyErrorMessage ||
													 response.ErrorReason == CommonMessageConstants.RoleErrorMessage);
		}

		[Fact]
		public void Get_GetRightsByRole()
		{
			//Arrange
			AssertInitial();

			IManageUnifiedAmenities manageProduct = new ManageUnifiedAmenities(_editorUserClaim,
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object, 
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object,
                _mockManagePartyRelationship.Object, 
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                _mockUnifiedLoginRepository.Object, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			// invalid persona
			ListResponse response = manageProduct.GetRightsByRole(0, 3, 3);
			Assert.True(response.IsError == true);

			// valid data
			response = manageProduct.GetRightsByRole(4, 3, 3);
			Assert.True(response.Records.Count == 5);

			// verify the records are ordered
			List<ProductRight> productRightResult = response.Records.Cast<ProductRight>().ToList();

			Assert.True(productRightResult[0].Description == "A right here" &&
						productRightResult[1].Description == "Another right" &&
						productRightResult[2].Description == "My right" &&
						productRightResult[3].Description == "Test right" &&
						productRightResult[4].Description == "Z right here"
			);

			List<ProductRight> nullRightList = null;

			_mockUnifiedLoginRepository
				.Setup(m => m.ListRightsByRole(
					It.IsAny<long>(),
					It.IsAny<List<int>>(),
					It.IsAny<long>(),
					It.IsAny<long>()
                    
                ))
				.Returns(nullRightList);

			manageProduct = new ManageUnifiedAmenities(_editorUserClaim,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageBlueBook.Object, 
                _mockProductRepository.Object,
                _mockSamlRepository.Object, 
                _mockProductInternalSettingRepository.Object,
                _mockManagePartyRelationship.Object,
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object,
                _mockUnifiedLoginRepository.Object, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			response = manageProduct.GetRightsByRole(4, 3, 3);
			Assert.True(response.Records.Count == 0);

			response = manageProduct.GetRightsByRole(4, 0, 3);
			Assert.True(response.Records.Count == 0);

			// exceptions
			_mockUnifiedLoginRepository
				.Setup(m => m.ListRightsByRole(
					It.IsAny<long>(),
					It.IsAny<List<int>>(),
					It.IsAny<long>(),
					It.IsAny<long>()
                    
                ))
				.Throws(new Exception("Invalid SQL"));

			response = manageProduct.GetRightsByRole(4, 0, 3);
			Assert.True(response.IsError == true && (response.ErrorReason == "There was a problem getting the rights." ||
													 response.ErrorReason == CommonMessageConstants.RightErrorMessage ||
													 response.ErrorReason == CommonMessageConstants.CompanyErrorMessage));
		}

		[Fact]
		public void Get_Properties()
		{
			//Arrange
			AssertInitial();

			IManageUnifiedAmenities manageProduct = new ManageUnifiedAmenities(_editorUserClaim,
                _mockManagePersona.Object, 
                _mockManagePerson.Object, 
                _mockManageBlueBook.Object,
                _mockProductRepository.Object, 
                _mockSamlRepository.Object,
                _mockProductInternalSettingRepository.Object,
                _mockManagePartyRelationship.Object,
                _mockUserRoleRightRepository.Object, 
                _mockManageUserLogin.Object, 
                _mockUnifiedLoginRepository.Object, 
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);

			ListResponse response = manageProduct.GetProperties(4, 0, false, null);
			Assert.True(response.Records.Count == 2);

			response = manageProduct.GetProperties(4, 4, false, null);
			List<ProductProperty> propertyAssignedList = response.Records.Cast<ProductProperty>().ToList();

			Assert.True(response.Records.Count == 2 && propertyAssignedList.Count(m => m.IsAssigned == true) == 1);

			_vCompanyPropertyMap = null;

			_mockManageBlueBook
				.Setup(m => m.GetVCompanyPropertyMap(
					It.IsAny<long>(),
					It.IsAny<string>()
				))
				.Returns(_vCompanyPropertyMap);

			response = manageProduct.GetProperties(4, 0, false, null);
			Assert.True(response.IsError == false && response.Records.Count == 0);
		}
	}
}
