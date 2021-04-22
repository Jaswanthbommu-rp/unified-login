using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Xunit;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;

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
		Mock<IProductRepository> _mockProductRepository = new Mock<IProductRepository>();
		Mock<IProductInternalSettingRepository> _mockPoductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
		Mock<IUnifiedLoginRepository> _unifiedLoginRepository = new Mock<IUnifiedLoginRepository>();
		Mock<IManagePersona> _mockManagePersona = new Mock<IManagePersona>();
		Mock<IManageProfile> _mockManageProfile = new Mock<IManageProfile>();
		Mock<IManageBlueBook> _mockManageBlueBook = new Mock<IManageBlueBook>();
		Mock<IManagePartyRelationship> _mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
		Mock<IManageOrganization> _mockManageOrganization = new Mock<IManageOrganization>();
		Mock<IManageUserRoleRight> _mockManageUserRoleRight = new Mock<IManageUserRoleRight>();
		protected DefaultUserClaim editorUserClaim = new DefaultUserClaim();
		#endregion

		[Fact]
		public void GetUserAssignedProductsByPersona_InvalidPersona_ExceptionThrown()
		{
			//Arrange
			Persona persona = null;

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetUserAssignedProductsByPersona(persona));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetUserAssignedProductsByPersona_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");
			long personaId = 33;
			Persona persona = new Persona()
			{
				FromDate = DateTime.UtcNow,
				Name = "Super User",
				Organization = new Organization() { RealPageId = realPageId },
				PersonaId = personaId,
				RealPageId = realPageId,
				ThruDate = DateTime.UtcNow.AddDays(1)
			};
			IList<PersonaProductUserDetails> expectedUserProducts = new List<PersonaProductUserDetails>();
			expectedUserProducts.Add(new PersonaProductUserDetails()
			{
				PersonaId = 33,
				OrganizationPartyId = 3,
				OrganizationName = "RealPage",
				ProductId = 1,
				ProductName = "OneSite",
				IsFavorite = true,
				HasAccess = true
			});

			_mockProductRepository
				.Setup(m => m.GetAssignedProductsByPersona(It.IsAny<Persona>(), null, null))
				.Returns(expectedUserProducts);

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

			//Act
			var userProducts = _manageProduct.GetUserAssignedProductsByPersona(persona);

			//Assert
			Assert.True(userProducts != null
				&& userProducts.Count == 1
			);
		}

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

			_mockProductRepository
				.Setup(m => m.GetProductTypes())
				.Returns(expectedProductTypes);

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

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
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

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

			_mockManagePersona
				.Setup(m => m.GetPersona(It.IsAny<long>()))
				.Returns(persona);

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

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
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

			//Act
			response = _manageProduct.GetProducts(realPageId, personaId);
			numberOfProperties = type.GetProperties().Length;

			//Assert
			Assert.True(response.Count == 1
				&& response.Where(p => p.ProductName == "OneSite").ToList().Count == 1
				&& response.Where(p => p.IsFavorite == true).ToList().Count == 1
				&& numberOfProperties == 29);
		}

		[Fact]
		public void GetProducts_InvalidRealPageId_ThrowsException()
		{
			//Arrange
			int personaId = 1;
			Guid realPageId = Guid.Empty;

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

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

			_mockManagePersona
				.Setup(m => m.GetPersona(It.IsAny<long>()))
				.Returns(persona);

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetProducts(realPageId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
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
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

			//Act
			response = _manageProduct.GetProducts(realPageId, personaId);
			numberOfProperties = type.GetProperties().Length;

			//Assert
			Assert.True(response.Count == 1
				&& response.Where(p => p.ProductName == "OneSite").ToList().Count == 1
				&& response.Where(p => p.IsFavorite == true).ToList().Count == 1
				&& numberOfProperties == 29);
		}

		[Fact]
		public void GetProductUsers_InvalidProductId_ExceptionThrown()
		{
			//Arrange
			int productId = 0;
			long blueBookCompanyInstanceId = -1;
			long personaId = 0;

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

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
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

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
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

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

            Organization organization = new Organization()
            {
                PartyId = 10639,
                Name = "RealPage Employee",
                RealPageId = new Guid("9A97CCA3-F7FB-400B-AE53-CE601C623031"),
                CreateDate = DateTime.Parse("2018-01-16 16:51:40.277"),
                BooksMasterId = -1,
                OrganizationDomain = new OrganizationDomain() {OrganizationDomainId = 1, Name = "Primary"}
            };

            UnifiedLoginCompany ulc = new UnifiedLoginCompany(){ CompanyRealPageId = organization.RealPageId.ToString(), CompanyName = organization.Name, BooksCustomerMasterId = blueBookCompanyInstanceId, Domain = organization.OrganizationDomain.Name};

            List<UnifiedLoginCompany> unifiedLoginCompanyList = new List<UnifiedLoginCompany>() {ulc};

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
					Right = listRight
				}
			};

			Persona persona = new Persona()
			{
				PersonPartyId = 10662,
				RealPageId = new Guid("0200E973-3D92-4DD3-BEC5-A094C7D98E08"),
				OrganizationPartyId = 10639,
				PersonaTypeId = 3,
				PersonaEnvironmentTypeId = 1,
				Name = "primary",
				FromDate = DateTime.Parse("2018-01-26 03:01:33.763"),
				ThruDate = null,
				IsDefault = false,
				UserId = 499
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

			IList<Persona> listPersona = new List<Persona>();
			listPersona.Add(persona);

			IList<int> productIdList = new List<int>();
			productIdList.Add((int)ProductEnum.OneSite);
			productIdList.Add((int)ProductEnum.FinancialSuite);
			productIdList.Add((int)ProductEnum.ProspectContactCenter);

			_mockManageOrganization
				.Setup(m => m.GetOrganization(It.IsAny<Guid>(), null))
				.Returns(organization);

            _mockManageOrganization
                .Setup(m => m.GetUnifiedLoginCompanyList())
                .Returns(unifiedLoginCompanyList);

			_mockProductRepository
				.Setup(m => m.GetProductIdsByCompany(
						It.IsAny<Guid>()
					)
				)
				.Returns(productIdList);

			_mockProductRepository
				.Setup(m => m.ListRoleWithRights(
					It.IsAny<long>(),
					It.IsAny<int>(),
					It.IsAny<List<int>>()
					)
				)
				.Returns(listRightRoleDetail);

			_mockManagePersona
				.Setup(m => m.GetPersona(
					It.IsAny<long>()
					)
				)
				.Returns(persona);

			_mockManageUserRoleRight
				.Setup(m => m.GetAssignedRoleForPersona(
					It.IsAny<ProductEnum>(),
					It.IsAny<long>(),
					It.IsAny<long>()
					)
				)
				.Returns(listRole);

			_mockManageProfile
				.Setup(m => m.ListPersonsByProductId(
					It.IsAny<int>(),
					It.IsAny<Guid>(),
					It.IsAny<long>()
					)
				)
				.Returns(expectedListProductUsers);

			_manageProduct = new ManageProduct(
				_mockProductRepository.Object,
				_mockPoductInternalSettingRepository.Object,
				_mockManagePersona.Object,
				_mockManageBlueBook.Object,
				_mockManagePartyRelationship.Object,
				_mockManageOrganization.Object,
				_mockManageProfile.Object,
				_mockManageUserRoleRight.Object, editorUserClaim);

			//Act
			IList<ProductUsers> listProductUsers = _manageProduct.GetProductUsers(productId, blueBookCompanyInstanceId, personaId);
			List<ProductUsers> compareResult = listProductUsers.Where(item => expectedListProductUsers.Select(eItem => eItem.PartyId).Contains(item.PartyId)).ToList();

			//Assert
			Assert.True(listProductUsers.Count == expectedListProductUsers.Count);
			Assert.True(compareResult.Count == expectedListProductUsers.Count);
			Assert.True(listProductUsers.SequenceEqual(expectedListProductUsers));
		}
	}
}
