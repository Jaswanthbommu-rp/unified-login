using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Xunit;
using ProductUsers = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ProductUsers;
using Role = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement.Role;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// ManageProduct xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageProductTest
	{
		#region Private Variables
		ManageProduct _manageProduct;
		Mock<IRepository> _mockRepository = new Mock<IRepository>();
		Mock<IProductRepository> _mockProductRepository = new Mock<IProductRepository>();
		Mock<IProductInternalSettingRepository> _mockPoductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
		Mock<IUnifiedLoginRepository> _unifiedLoginRepository = new Mock<IUnifiedLoginRepository>();
		Mock<IManagePersona> _mockManagePersona = new Mock<IManagePersona>();
		Mock<IManageProfile> _mockManageProfile = new Mock<IManageProfile>();
		Mock<IManageBlueBook> _mockManageBlueBook = new Mock<IManageBlueBook>();
		Mock<IManagePartyRelationship> _mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
		Mock<IManageOrganization> _mockManageOrganization = new Mock<IManageOrganization>();
		Mock<IManageUserRoleRight> _mockManageUserRoleRight = new Mock<IManageUserRoleRight>();
		Mock<HttpMessageHandler> _mockMessageHandler = new Mock<HttpMessageHandler>();

		protected DefaultUserClaim editorUserClaim = new DefaultUserClaim();

		List<OrganizationType> _organizationTypeList;
		List<OrganizationDomain> _organizationDomainList;

		#endregion

		public ManageProductTest()
		{
			_organizationTypeList = new List<OrganizationType>()
			{
				new OrganizationType()
				{
					OrganizationTypeId = 6,
					Name = "Multifamily",
					CreateDate = new DateTime()
				},
				new OrganizationType()
				{
					OrganizationTypeId = 14,
					Name = "Vendor",
					CreateDate = new DateTime()
				},
				new OrganizationType()
				{
					OrganizationTypeId = 7,
					Name = "Other",
					CreateDate = new DateTime()
				}
			};

			_organizationDomainList = new List<OrganizationDomain>()
			{
				new OrganizationDomain()
				{
					OrganizationDomainId = 1,
					Name = "Primary",
					CreateDate = new DateTime()
				}
			};
		}

		[Fact]
		public void GetUserAssignedProductsByPersona_InvalidPersona_ExceptionThrown()
		{
			//Arrange
			Persona persona = null;

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetUserAssignedProductsByPersona(persona));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		//[Fact]
		//public void GetUserAssignedProductsByPersona_MockInputData_ReturnValidRepositoryResponseObject()
		//{
		//	//Arrange
		//	Guid realPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");
		//	long personaId = 33;
		//	Persona persona = new Persona()
		//	{
		//		FromDate = DateTime.UtcNow,
		//		Name = "Super User",
		//		Organization = new Organization() { RealPageId = realPageId },
		//		PersonaId = personaId,
		//		RealPageId = realPageId,
		//		ThruDate = DateTime.UtcNow.AddDays(1)
		//	};
		//	IList<PersonaProductUserDetails> expectedUserProducts = new List<PersonaProductUserDetails>();
		//	expectedUserProducts.Add(new PersonaProductUserDetails()
		//	{
		//		PersonaId = 33,
		//		OrganizationPartyId = 3,
		//		OrganizationName = "RealPage",
		//		ProductId = 1,
		//		ProductName = "OneSite",
		//		IsFavorite = true,
		//		HasAccess = true
		//	});
		//
		//	_mockProductRepository
		//		.Setup(m => m.GetAssignedProductsByPersona(It.IsAny<Persona>(), null, null))
		//		.Returns(expectedUserProducts);
		//
		//	_manageProduct = new ManageProduct(
		//		_mockRepository.Object,
		//		editorUserClaim,
		//		_mockMessageHandler.Object);
		//
		//	//Act
		//	var userProducts = _manageProduct.GetUserAssignedProductsByPersona(persona);
		//
		//	//Assert
		//	Assert.True(userProducts != null
		//		&& userProducts.Count == 1
		//	);
		//}

		[Fact]
		public void GetProductTypes_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Type type = typeof(ProductType);
			int numberOfProperties;
			IList<ProductType> expectedProductTypes = new List<ProductType>();
			IList<ProductType> productTypesResponse = new List<ProductType>();

			expectedProductTypes.Add(
				new ProductType
				{
					ProductTypeGuid = new Guid("82f7c646-599d-4aa5-a4d1-d951cce21280"),
					ProductTypeId = 100,
					ParentProductTypeId = null,
					Name = "Property Management",
					Description = "Property Management",
					ParentProductTypeName = null
				});

			expectedProductTypes.Add(
				new ProductType
				{
					ProductTypeGuid = new Guid("d2df04e5-c635-4f9e-90a5-2b3157795f2d"),
					ProductTypeId = 102,
					ParentProductTypeId = 100,
					Name = "Accounting",
					Description = "Accounting (Property, Corporate, Job Cost)",
					ParentProductTypeName = "Property Management"
				});

			Mock<IRepository> mockRepository = new Mock<IRepository>();

			mockRepository.Setup(m => m.GetMany<ProductType>(StoredProcNameConstants.SP_ListProductTypes, null))
				.Returns(expectedProductTypes);

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			productTypesResponse = _manageProduct.GetProductTypes();
			numberOfProperties = type.GetProperties().Length;

			//Assert
			Assert.True(
				productTypesResponse.Count == 2
				&& productTypesResponse.Where(p => p.ParentProductTypeId == null).Count() == 1
				&& productTypesResponse.Where(p => p.ParentProductTypeId != null).Count() == 1
				&& numberOfProperties == 6
			);
		}

		[Fact]
		public void GetProducts_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			int personaId = 1;
			Guid realPageId = Guid.Empty;

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProducts(realPageId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetProducts_InvalidPersona_ExceptionThrown()
		{
			//Arrange
			int personaId = 1;
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Persona persona = null;

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProducts(realPageId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetProducts_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Type type = typeof(ProductUI);
			int numberOfProperties;
			int personaId = 1;
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");

			IList<PersonaProductUserDetails> personaProducts = new List<PersonaProductUserDetails>();
			IList<ProductUI> expectedProductList = new List<ProductUI>();
			IList<ProductUI> response = new List<ProductUI>();

			Organization organization = new Organization()
			{
				PartyId = 10639,
				Name = "RealPage Employee",
				RealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99"),
				CreateDate = DateTime.Parse("2018-01-16 16:51:40.277"),
				BooksMasterId = -1,
				OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" }
			};

			Persona persona = new Persona()
			{
				PersonaId = personaId,
				PersonPartyId = 10662,
				RealPageId = new Guid("0200E973-3D92-4DD3-BEC5-A094C7D98E08"),
				OrganizationPartyId = organization.PartyId,
				PersonaTypeId = 3,
				PersonaEnvironmentTypeId = 1,
				Name = "primary",
				FromDate = DateTime.Parse("2018-01-26 03:01:33.763"),
				ThruDate = null,
				IsDefault = false,
				UserId = 499,
				Organization = organization
			};

			personaProducts.Add(new PersonaProductUserDetails
			{
				ProductId = 1,
				ProductName = "OneSite",
				IsFavorite = true
			});

			expectedProductList.Add(new ProductUI
			{
				Family = "Property Management",
				FamilyId = 100,
				IsFavorite = true,
				LearnMore = "https://www.realpage.com",
				ProductId = 1,
				ProductName = "OneSite",
				TitleId = "OneSite",
				TitleUniqueId = new Guid("0c9da909-71fa-4807-ba36-7ccde6e580ec")
			});

			Mock<IRepository> mockRepository = new Mock<IRepository>();

			mockRepository.Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
					It.Is<object>(
						d => TestIsRealPageId(d, realPageId))))
				.Returns(expectedProductList);

			mockRepository
				.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + personaId + " }"))))
				.Returns(persona);

			mockRepository
				.Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
					It.Is<object>(
						d => TestIsPartyId(d, organization.PartyId))))
				.Returns(organization);

			mockRepository
				.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
				.Returns(_organizationTypeList);

			mockRepository
				.Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
				.Returns(_organizationDomainList);

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			response = _manageProduct.GetProducts(realPageId, personaId);
			numberOfProperties = type.GetProperties().Length;

			//Assert
			Assert.True(response.Count == 1
				&& response.Where(p => p.ProductName == "OneSite").ToList().Count == 1
				&& response.Where(p => p.IsFavorite == true).ToList().Count == 1
				&& numberOfProperties == 31);
		}

		[Fact]
		public void GetProducts_InvalidRealPageId_ThrowsException()
		{
			//Arrange
			int personaId = 1;
			Guid realPageId = Guid.Empty;

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProducts(realPageId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetProducts_InvalidPersona_ThrowsException()
		{
			//Arrange
			int personaId = 1;
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Persona persona = null;

			Mock<IRepository> mockRepository = new Mock<IRepository>();

			mockRepository
				.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + personaId + " }"))))
				.Returns(persona);

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProducts(realPageId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		//[Fact]
		public void GetProducts_MockInputData_ReturnValidProductUIList()
		{
			//Arrange
			Type type = typeof(ProductUI);
			int numberOfProperties;
			int personaId = 1;
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Persona persona = new Persona();
			IList<PersonaProductUserDetails> personaProducts = new List<PersonaProductUserDetails>();
			IList<ProductUI> expectedProductList = new List<ProductUI>();
			IList<ProductUI> response = new List<ProductUI>();

			personaProducts.Add(new PersonaProductUserDetails
			{
				ProductId = 1,
				ProductName = "OneSite",
				IsFavorite = true
			});

			expectedProductList.Add(new ProductUI
			{
				Family = "Property Management",
				FamilyId = 100,
				IsFavorite = true,
				LearnMore = "https://www.realpage.com",
				ProductId = 1,
				ProductName = "OneSite",
				TitleId = "OneSite",
				TitleUniqueId = new Guid("0c9da909-71fa-4807-ba36-7ccde6e580ec")
			});

			_mockProductRepository
				.Setup(m => m.GetProducts(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(expectedProductList);

			_mockManagePersona
				.Setup(m => m.GetPersona(It.IsAny<long>()))
				.Returns(persona);

			_mockProductRepository
				.Setup(m => m.GetAssignedProductsByPersona(It.IsAny<Persona>(), null, null))
				.Returns(personaProducts);

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			response = _manageProduct.GetProducts(realPageId, personaId);
			numberOfProperties = type.GetProperties().Length;

			//Assert
			Assert.True(response.Count == 1
				&& response.Where(p => p.ProductName == "OneSite").ToList().Count == 1
				&& response.Where(p => p.IsFavorite == true).ToList().Count == 1
				&& numberOfProperties == 31);
		}

		[Fact]
		public void GetProductUsers_InvalidProductId_ExceptionThrown()
		{
			//Arrange
			int productId = 0;
			long blueBookCompanyInstanceId = -1;
			long personaId = 0;

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProductUsers(productId, blueBookCompanyInstanceId, personaId));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void GetProductUsers_InvalidblueBookCompanyInstanceId_ExceptionThrown()
		{
			//Arrange
			int productId = (int)ProductEnum.ResearchApplication;
			long blueBookCompanyInstanceId = 0;
			long personaId = 0;

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProductUsers(productId, blueBookCompanyInstanceId, personaId));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void GetProductUsers_InvalidPersonaId_ExceptionThrown()
		{
			//Arrange
			int productId = (int)ProductEnum.ResearchApplication;
			long blueBookCompanyInstanceId = -1;
			long personaId = -1;

			_manageProduct = new ManageProduct(
				_mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProductUsers(productId, blueBookCompanyInstanceId, personaId));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void GetProductUsers_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			int productId = (int)ProductEnum.ResearchApplication;
			long blueBookCompanyInstanceId = -1;
			long personaId = 486;
			int userPersonPartyId = 10662;

			Organization organization = new Organization()
			{
				PartyId = 10639,
				Name = "RealPage Employee",
				RealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99"),
				CreateDate = DateTime.Parse("2018-01-16 16:51:40.277"),
				BooksMasterId = -1,
				OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" }
			};

			UnifiedLoginCompany ulc = new UnifiedLoginCompany() { CompanyRealPageId = organization.RealPageId.ToString(), CompanyName = organization.Name, BooksCustomerMasterId = blueBookCompanyInstanceId, Domain = organization.OrganizationDomain.Name };

			List<UnifiedLoginCompany> unifiedLoginCompanyList = new List<UnifiedLoginCompany>() { ulc };

			IList<RightRoleDetail> listRightRoleDetail = new List<RightRoleDetail>()
			{
				new RightRoleDetail()
				{
					RoleId = 81,
					RoleName = "Black-Book Director",
					IsAssigned = true,
					RoleType = "Default",
					RightName = "Access Reports",
					RightId = 672,
					RightValueTypeId = 47
				},
				new RightRoleDetail()
				{
					RoleId = 81,
					RoleName = "Black-Book Director",
					IsAssigned = true,
					RoleType = "Default",
					RightName = "Create master properties",
					RightId = 667,
					RightValueTypeId = 36
				}
			};

			IList<Right> listRight = new List<Right>()
			{
				new Right()
				{
					RightId = 672,
					RightName = "Access Reports",
					RightValueTypeId = 47,
					RightNickName = "reports.view"
				},
				new Right()
				{
					RightId = 667,
					RightName = "Create master properties",
					RightValueTypeId = 36,
					RightNickName = "property.create"
				}
			};

			IList<Role> listRole = new List<Role>()
			{
				new Role()
				{
					RoleID = 81,
					Name = "director",
					PersonaId = "486",
					Right = listRight,
					RoleNickName = "director"
				}
			};

			Persona persona = new Persona()
			{
				PersonaId = personaId,
				PersonPartyId = userPersonPartyId,
				RealPageId = new Guid("0200E973-3D92-4DD3-BEC5-A094C7D98E08"),
				OrganizationPartyId = organization.PartyId,
				PersonaTypeId = 3,
				PersonaEnvironmentTypeId = 1,
				Name = "primary",
				FromDate = DateTime.Parse("2018-01-26 03:01:33.763"),
				ThruDate = null,
				IsDefault = false,
				UserId = 499,
				Organization = organization
			};

			IList<PersonaCommon> listPersonaCommon = new List<PersonaCommon>()
			{
				new PersonaCommon()
				{
						PersonaId = 486,
						PersonPartyId = 10662,
						RealPageId = new Guid("0200E973-3D92-4DD3-BEC5-A094C7D98E08"),
						OrganizationPartyId = 10639,
						Name = "primary",
						UserId = 499,
						Role = listRole
				}
			};

			IList<ProductUsers> expectedListProductUsers = new List<ProductUsers>()
			{
				new ProductUsers()
				{
					RealPageId = new Guid("0200E973-3D92-4DD3-BEC5-A094C7D98E08"),
					PartyId = 10662,
					FirstName = "test",
					MiddleName = "",
					LastName = "test",
					Title = null,
					Suffix = null,
					persona = listPersonaCommon
				}
			};

			IList<ProductUI> expectedProductList = new List<ProductUI>() {
				new ProductUI() { ProductId = (int)ProductEnum.OneSite },
				new ProductUI() { ProductId = (int)ProductEnum.FinancialSuite },
				new ProductUI() { ProductId = (int)ProductEnum.ResearchApplication },
			};

			dynamic roleDynamic = new
			{
				RoleId = listRole[0].RoleID,
				Role = listRole[0].Name,
				PersonaId = listRole[0].PersonaId,
				RoleNickName = listRole[0].RoleNickName
			};
			List<dynamic> roleDynamicList = new List<dynamic>() { roleDynamic };
			string roleListText = JsonConvert.SerializeObject(roleDynamicList);
			var roleListDynamic = JsonConvert.DeserializeObject<List<dynamic>>(roleListText);

			List<dynamic> roleRightDynamicList = new List<dynamic>() {
			new
			{
				RoleId = 81,
				Role = "Black-Book Director",
				RoleType = "Default",
				Right = "Access Reports",
				RightId = 672,
				RightValueTypeId = 47,
				RightNickName = "AccessReports"
			},
			new
			{
				RoleId = 81,
				Role = "Black-Book Director",
				RoleType = "Default",
				Right = "Create master properties",
				RightId = 667,
				RightValueTypeId = 36,
				RightNickName = "Createmasterproperties"
			}};
			string roleRightListText = JsonConvert.SerializeObject(roleRightDynamicList);
			var roleRightListDynamic = JsonConvert.DeserializeObject<List<dynamic>>(roleRightListText);

			Mock<IRepository> mockRepository = new Mock<IRepository>();

			mockRepository
				.Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
					It.Is<object>(
						d => TestIsRealPageId(d, organization.RealPageId))))
				.Returns(organization);

			mockRepository
				.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
				.Returns(_organizationTypeList);

			mockRepository
				.Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
				.Returns(_organizationDomainList);

			mockRepository.Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
					It.Is<object>(
						d => TestIsRealPageId(d, organization.RealPageId))))
				.Returns(expectedProductList);

			mockRepository
				.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + personaId + " }"))))
				.Returns(persona);

			mockRepository
				.Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesForProductsByPersonaId, It.IsAny<object>()))
				.Returns(roleListDynamic);

			mockRepository
				.Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesAssociatedWithRights, It.IsAny<object>()))
				.Returns(roleRightListDynamic);

			var funcMock = new Mock<Func<ProductUsers, UserLoginCommon, ProductUsers>>();

			mockRepository
				.Setup(m => m.GetManyWithSpliOn<ProductUsers, UserLoginCommon, ProductUsers>(
					StoredProcNameConstants.SP_ListPersonsByProductId,
                    It.IsAny<Func<ProductUsers, UserLoginCommon, ProductUsers>>(),
					It.IsAny<object>(),
					It.IsAny<string>()
					)
				)
				.Returns(new List<ProductUsers>() { new ProductUsers() { PartyId = userPersonPartyId, userLogin = new UserLoginCommon() { UserId = persona.UserId } } });

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			IList<ProductUsers> listProductUsers = _manageProduct.GetProductUsers(productId, blueBookCompanyInstanceId, personaId);
			List<ProductUsers> compareResult = listProductUsers.Where(item => expectedListProductUsers.Select(eItem => eItem.PartyId).Contains(item.PartyId)).ToList();

			//Assert
			Assert.True(listProductUsers.Count == expectedListProductUsers.Count);
			Assert.True(compareResult.Count == expectedListProductUsers.Count);
		}

		public bool TestSqlParameter(object p, string value)
		{
			return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		private bool TestIsRealPageId(object obj, Guid? realPageId)
		{
			if (obj == null && realPageId == null)
			{
				return true;
			}

			if (obj == null)
			{
				return false;
			}

			return obj.ToString().ToLower().Contains($"realpageid = {realPageId}");
		}

		private bool TestIsPartyId(object obj, long partyid)
		{
			if (obj == null)
			{
				return false;
			}

			return obj.ToString().ToLower().Contains($"partyid = {partyid}");
		}
	}
}
