using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Xunit;
using ProductUsers = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ProductUsers;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// ManageProduct xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageProductTest : TestBase
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
		Mock<HttpMessageHandler> _mockMessageHandler = new Mock<HttpMessageHandler>();

		protected DefaultUserClaim editorUserClaim = new DefaultUserClaim();
        private static List<GbProductMap> _gbProductMap;

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

            _gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() { BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UI", Name = "UnifiedUI", ProductId = 2, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AO", Name = "Asset Optimization", ProductId = 4, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PW", Name = "Propertyware", ProductId = 5, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "L2L", Name = "Lead2Lease", ProductId = 6, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "YS", Name = "YieldStar", ProductId = 7, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ACCT", Name = "Financial Suite", ProductId = 8, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LS", Name = "Marketing Center", ProductId = 9, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LVL1", Name = "Prospect Contact Center", ProductId = 10, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "??", Name = "Social", ProductId = 11, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPSB", Name = "Ops Bid", ProductId = 12, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPS", Name = "Spend Management", ProductId = 13, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OMS", Name = "Client Portal", ProductId = 14, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LD", Name = "Renters Insurance", ProductId = 15, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CD", Name = "Vendor Credentialing", ProductId = 16, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AB", Name = "Resident Portals", ProductId = 17, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "NWP", Name = "Utility Management", ProductId = 18, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LP", Name = "Product Learning Portal", ProductId = 19, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DOC", Name = "Document Director", ProductId = 20, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OSC", Name = "L&R Conversion Utility", ProductId = 21, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OC", Name = "OmniChannel", ProductId = 22, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ONST", Name = "On-Site", ProductId = 23, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RA", Name = "Unified Data Management", ProductId = 24, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SP", Name = "Self-provisioning portal", ProductId = 25, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UA", Name = "Unified Amenities", ProductId = 26, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "MT", Name = "Migration Tool Application", ProductId = 27, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PUPDATE", Name = "Product Updates", ProductId = 28, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "BI", Name = "Business Intelligence", ProductId = 29, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PA", Name = "Performance Analytics", ProductId = 30, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "MA", Name = "Investment Analytics", ProductId = 31, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PO", Name = "YieldStar", ProductId = 32, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AX", Name = "Axiometrics", ProductId = 33, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "BM", Name = "Benchmarking", ProductId = 34, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "null", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ELMS", Name = "EasyLMS", ProductId = 36, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PHOTO", Name = "Property Photos", ProductId = 37, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "VMP", Name = "Vendor Marketplace", ProductId = 38, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "IMP", Name = "Integration Marketplace", ProductId = 39, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLM", Name = "ILM Lead Management", ProductId = 40, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLA", Name = "ILM Leasing Analytics", ProductId = 41, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SM", Name = "Settings Management", ProductId = 43, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPM", Name = "Portfolio Management", ProductId = 44, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CIMPL", Name = "CIMPL", ProductId = 45, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SSM", Name = "Site Spend Management Portal", ProductId = 46, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DIQ", Name = "Deposit Alternative", ProductId = 47, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CPAY", Name = "ClickPay", ProductId = 48, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HLP", Name = "Simon Help Center", ProductId = 49, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SLM", Name = "Senior Lead Management", ProductId = 50, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LRO", Name = "LRO", ProductId = 51, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AA", Name = "Amenity Optimization", ProductId = 52, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AIRM", Name = "AI Revenue Management", ProductId = 53, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RC", Name = "Rent Control", ProductId = 54, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RENO", Name = "Renovation Manager", ProductId = 55, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SET", Name = "Unified Settings", ProductId = 56, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SMS-T", Name = "Intelligent Building", ProductId = 57, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-E", Name = "Intelligent Building Energy", ProductId = 58, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-W", Name = "Intelligent Building Water", ProductId = 59, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "HAAS", Name = "Home Sharing", ProductId = 60, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PME", Name = "PME Dashboard", ProductId = 62, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RMA", Name = "Market Analytics", ProductId = 66, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ST", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HOTS", Name = "Hands On Training System", ProductId = 63, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PEQ", Name = "P2 Engagement Queue", ProductId = 64, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LeaseLabs", Name = "LeaseLabs", ProductId = 68, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPT", Name = "Reporting", ProductId = 67, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "6247", Name = "Self-Guided Tour", ProductId = 65, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LST", Name = "Lead Scoring", ProductId = 69, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SMS-TC", Name = "Smart Waste Commercial", ProductId = 70, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "OS", Name = "Facilities", ProductId = 75, UDMSourceCode = null }
            };

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);
        }

		[Fact]
		public void GetUserAssignedProductsByPersona_InvalidPersona_ExceptionThrown()
		{
			//Arrange
			Persona persona = null;

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

			//Act
			Exception exception = Record.Exception(() => _manageProduct.GetUserAssignedProductsByPersona(persona));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
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

			mockRepository.Setup(m => m.GetMany<ProductType>(StoredProcNameConstants.SP_ListProductTypes, null))
				.Returns(expectedProductTypes);

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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
				TitleUniqueId = new Guid("0c9da909-71fa-4807-ba36-7ccde6e580ec"),
				ProductCode = "OS",
				UDMSourceCode = null
			});

            expectedProductList.Add(new ProductUI
            {
                Family = "Property Management",
                FamilyId = 100,
                IsFavorite = true,
                LearnMore = "https://www.realpage.com",
                ProductId = 57,
                ProductName = "Smart Waste",
                TitleId = "Smart Waste",
                TitleUniqueId = new Guid("D8DD4D6E-00F6-4453-8AF2-6EFF7E3F87B5"),
                ProductCode = "SMS-T",
                UDMSourceCode = "IB"
			});

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

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

			//Act
			response = _manageProduct.GetProducts(realPageId, personaId, allProducts: false, replaceProductCodeWithUDMIfExists: true);
			numberOfProperties = type.GetProperties().Length;

			//Assert
			Assert.True(response.Count == 2
				&& response.Where(p => p.ProductName == "OneSite").ToList().Count == 1
				&& response.Where(p => (p.ProductCode == "IB" && p.ProductCode == p.UDMSourceCode)).ToList().Count == 1
				&& numberOfProperties == 31);

            //Act
            // the response here should still have the correct BooksProductCode and UDMSourceCode so they can be used correctly.
            response = _manageProduct.GetProducts(realPageId, personaId, allProducts: false, replaceProductCodeWithUDMIfExists: false);

            //Assert
            Assert.True(response.Count == 2
                        && response.Where(p => (p.ProductCode == "SMS-T" && p.UDMSourceCode == "IB")).ToList().Count == 1);
		}

		[Fact]
		public void GetProducts_InvalidRealPageId_ThrowsException()
		{
			//Arrange
			int personaId = 1;
			Guid realPageId = Guid.Empty;

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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

			mockRepository
				.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + personaId + " }"))))
				.Returns(persona);

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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
				.Setup(m => m.GetProducts(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(expectedProductList);

			_mockManagePersona
				.Setup(m => m.GetPersona(It.IsAny<long>()))
				.Returns(persona);

			_mockProductRepository
				.Setup(m => m.GetAssignedProductsByPersona(It.IsAny<Persona>(), null, null))
				.Returns(personaProducts);

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
		public void GetProductUsers_InvalidProductId_ExceptionThrown()
		{
			//Arrange
			int productId = 0;
			long blueBookCompanyInstanceId = -1;
			long personaId = 0;

			_manageProduct = new ManageProduct(
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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
				mockRepository.Object,
				editorUserClaim,
				_mockMessageHandler.Object);

            new RPObjectCache().BustCache();

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
				//Organization = organization
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
						//Role = listRole
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
				RoleId = 81,
				Role = "director",
				PersonaId = 486,
				RoleNickName = "director"
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

            new RPObjectCache().BustCache();

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
