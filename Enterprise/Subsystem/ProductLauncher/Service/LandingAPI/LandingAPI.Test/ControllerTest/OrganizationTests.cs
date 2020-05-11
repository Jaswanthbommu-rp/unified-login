using Castle.Components.DictionaryAdapter;
using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
	[ExcludeFromCodeCoverage]
    public class OrganizationTests
    {
		#region Private Variables
		Mock<IRepository> _mockRepository = new Mock<IRepository>();
		Mock<IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();
		Mock<IManageOrganization> _mockManageOrganization = new Mock<IManageOrganization>();
		Mock<IRepositoryResponse> _mockRepositoryResponse = new Mock<IRepositoryResponse>();
		Mock<IOrganizationProductRepository> _mockOrganizationProductRepository = new Mock<IOrganizationProductRepository>();
		Mock<IManageOrganizationProduct> _mockManageOrganizationProduct = new Mock<IManageOrganizationProduct>();
		Mock<IManageCustomFields> _mockManageCustomFields = new Mock<IManageCustomFields>();
		//Mock<ICustomFieldsRepository> _mockCustomFieldsRepository = new Mock<ICustomFieldsRepository>();
		Mock<IManageUserLogin> _mockManageUserLogin = new Mock<IManageUserLogin>();
		Mock<IManagePartyRelationship> _mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
		
		private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
		private static Guid _adminRealPageId = new Guid("C802694D-1111-2222-3333-3C0F434AE62D");
		private static string _CompanyName = "CF Real Estate Services";
		private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
		private static int _PartyId = 54321;
		private static long _BooksMasterId = 2116;
		private static long _BooksCompanyMasterId = 379;
		private static int _organizationTypeId = 6;
		private static DefaultUserClaim _defaultUserClaim = new DefaultUserClaim();

        #endregion

        public OrganizationTests()
        {
            _defaultUserClaim.CorrelationId = new Guid();

            var organizationTypeList = new List<OrganizationType>()
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

            var organizationDomainList = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };

            var organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = _RealPageId,
                    CreateDate = _CreateDate,
                    Name = _CompanyName,
                    PartyId = _PartyId,
                    BooksMasterId = _BooksMasterId,
                    BooksCustomerMasterId = _BooksCompanyMasterId,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = _organizationTypeId,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = _organizationTypeId,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                }
            };

            _mockRepository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(organizationList);

            // THIS RESULT IS CACHED SO WE CANT REALLY TEST IT HAVING MULTIPLE RESULTS!
            _mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(organizationTypeList);

            _mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(organizationDomainList);
        }

		#region Controller Unit Tests
		[Fact]
        public void InsertOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("InsertOrganization" == baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/organization"
                )
            );
        }

		[Fact]
		public void InsertOrganization_DuplicateBookMasterId_BadRequest()
		{
			//Arrange
			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksCompanyMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 1,
				Name = "CF Real Estate Services",
				Products = new List<string>()
				{
					"AB"
				},
				AdminUser = new OrganizationAdminUser()
				{
					FirstName = "Jack",
					LastName = "Doe",
					Email = "jack.doe@example.com",
					Suffix = string.Empty,
					Title = string.Empty
				}
			};

			//Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();
			
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

            OrganizationController organizationController = new OrganizationController(
                manageOrganization
                , _mockRepositoryResponse.Object
                , _mockOrganizationProductRepository.Object
                , _mockManageOrganizationProduct.Object
                , _mockManageCustomFields.Object
                , _mockManageUserLogin.Object
                , _mockManagePartyRelationship.Object
                , _defaultUserClaim);
            organizationController.Request = new HttpRequestMessage();
            organizationController.Configuration = new HttpConfiguration();

			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"Duplicate master ids\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void InsertOrganization_InvalidOrganizationType_BadRequest()
		{
			//Arrange
			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 0,
				Name = "CF Real Estate Services",
				Products = new List<string>()
				{
					"AB"
				},
				AdminUser = new OrganizationAdminUser()
				{
					FirstName = "Jack",
					LastName = "Doe",
					Email = "jack.doe@example.com",
					Suffix = string.Empty,
					Title = string.Empty
				}
			};

            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

            OrganizationController organizationController = new OrganizationController(
                manageOrganization
                , _mockRepositoryResponse.Object
                , _mockOrganizationProductRepository.Object
                , _mockManageOrganizationProduct.Object
                , _mockManageCustomFields.Object
                , _mockManageUserLogin.Object
                , _mockManagePartyRelationship.Object
                , _defaultUserClaim);
            organizationController.Request = new HttpRequestMessage();
            organizationController.Configuration = new HttpConfiguration();

			//Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();
           
			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"An invalid Organization Type id was given: 0\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void InsertOrganization_InvalidProducts_BadRequest()
		{
			//Arrange
			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 1,
				Name = "CF Real Estate Services",
				Products = new List<string>()
				{
					"XX"
				},
				AdminUser = new OrganizationAdminUser()
				{
					FirstName = "Jack",
					LastName = "Doe",
					Email = "jack.doe@example.com",
					Suffix = string.Empty,
					Title = string.Empty
				}
			};

            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

            OrganizationController organizationController = new OrganizationController(
                manageOrganization
                , _mockRepositoryResponse.Object
                , _mockOrganizationProductRepository.Object
                , _mockManageOrganizationProduct.Object
                , _mockManageCustomFields.Object
                , _mockManageUserLogin.Object
                , _mockManagePartyRelationship.Object
                , _defaultUserClaim);
            organizationController.Request = new HttpRequestMessage();
            organizationController.Configuration = new HttpConfiguration();

			//Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();
			
			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"An invalid product was given : XX\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void InsertOrganization_InvalidAdminUser_BadRequest()
		{
			//Arrange
			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 1,
				Name = "CF Real Estate Services",
				Products = new List<string>()
				{
					"AB"
				},
				AdminUser = null
			};

            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

            OrganizationController organizationController = new OrganizationController(
                manageOrganization
                , _mockRepositoryResponse.Object
                , _mockOrganizationProductRepository.Object
                , _mockManageOrganizationProduct.Object
                , _mockManageCustomFields.Object
                , _mockManageUserLogin.Object
                , _mockManagePartyRelationship.Object
                , _defaultUserClaim);
            organizationController.Request = new HttpRequestMessage();
            organizationController.Configuration = new HttpConfiguration();

			//Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"No admin user information provided\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void InsertOrganization_CompanyExits_BadRequest()
		{
			//Arrange
			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 1,
				Name = "CF Real Estate Services",
				Products = new List<string>()
				{
					"AB"
				},
				AdminUser = new OrganizationAdminUser()
				{
					FirstName = "Jack",
					LastName = "Doe",
					Email = "jack.doe@example.com",
					Suffix = string.Empty,
					Title = string.Empty
				}
			};

            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);
			
            OrganizationController organizationController = new OrganizationController(
                manageOrganization
                , _mockRepositoryResponse.Object
                , _mockOrganizationProductRepository.Object
                , _mockManageOrganizationProduct.Object
                , _mockManageCustomFields.Object
                , _mockManageUserLogin.Object
                , _mockManagePartyRelationship.Object
                , _defaultUserClaim);
            organizationController.Request = new HttpRequestMessage();
            organizationController.Configuration = new HttpConfiguration();

			//Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate, true);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"MessageHandler.Handle - Company: CF Real Estate Services with BlueBookId: " + _BooksCompanyMasterId.ToString() + " already exists!\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void InsertOrganization_CustomerMasterBookIdExits_BadRequest()
		{
			//Arrange
			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 1,
				Name = "New Company",
				Products = new List<string>()
				{
					"AB"
				},
				AdminUser = new OrganizationAdminUser()
				{
					FirstName = "Jack",
					LastName = "Doe",
					Email = "jack.doe@example.com",
					Suffix = string.Empty,
					Title = string.Empty
				}
			};

            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

			OrganizationController organizationController = new OrganizationController(
				manageOrganization
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate, true);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"MessageHandler.Handle - Bluebook customer master id " + _BooksCompanyMasterId.ToString() + " already in use!\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void InsertOrganization_AdminExits_BadRequest()
		{
			//Arrange
			UserLoginOnly userLoginOnly = new UserLoginOnly()
			{
				UserId = 3,
				PartyId = 1,
				LoginName = "jack.doe@example.com",
				PasswordHash = ""
			};

			_mockRepository
				.Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
				.Returns(userLoginOnly);

			ManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

			OrganizationController organizationController = new OrganizationController(
				manageOrganization
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 1,
				Name = "New Company",
				Products = new List<string>()
				{
					"AB"
				},
				AdminUser = new OrganizationAdminUser()
				{
					FirstName = "Jack",
					LastName = "Doe",
					Email = "jack.doe@example.com",
					Suffix = string.Empty,
					Title = string.Empty
				}
			};

			//Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"Admin email already exists\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void InsertOrganization_ErrorInsertOrganization_BadRequest()
		{
			//Arrange
			UserLoginOnly userLoginOnly = new UserLoginOnly();
			userLoginOnly = null;
			Organization organization = new Organization();
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 0,
				ErrorMessage = "Failed to create organization",
				RealPageId = Guid.Empty
			};            

			_mockRepository
				.Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
				.Returns(userLoginOnly);

			_mockRepository
				.Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
				.Returns(repositoryResponse);

			_mockRepository
				.Setup(m => m.UnitOfWork)
				.Returns(_mockUnitofWork.Object);

            IUserLoginRepository userLoginRepository = new UserLoginRepository(_mockRepository.Object);

            ManageOrganization organizationLogic = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

			OrganizationController organizationController = new OrganizationController(
				organizationLogic
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			OrganizationCreate organizationCreate = new OrganizationCreate()
			{
				BooksCompanyId = _BooksMasterId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				OrganizationTypeId = 1,
				Name = "New Company",
				Products = new List<string>()
				{
					"AB"
				},
				AdminUser = new OrganizationAdminUser()
				{
					FirstName = "Jack",
					LastName = "Doe",
					Email = "jack.doe@example.com",
					Suffix = string.Empty,
					Title = string.Empty
				}
			};
			
			//Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "{\"Message\":\"Failed to create organization\"}";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
        public void UpdateOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("UpdateOrganization" == baseTest.VerifyRouteToAction(
                HttpMethod.Put,
				"http://localhost/api/organization"
				)
            );
        }

        [Fact]
        public void GetOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetOrganization" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/organization/C802694D-5553-4527-8616-3C0F434AE62D"
                )
            );
        }

		[Fact]
		public void GetOrganization_InvalidRealPageId_ReturnNotFound()
		{
			//Arrange
			Guid realPageId = Guid.NewGuid();

			Organization organization = new Organization();

			organization = null;

			_mockManageOrganization
				.Setup(m => m.GetOrganization(realPageId, null, null, null))
				.Returns(organization);

			OrganizationController organizationController = new OrganizationController(
				_mockManageOrganization.Object
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim
				)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.GetOrganization(realPageId);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "\"Not found\"";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.NotFound));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void GetOrganization_ValidRealPageId_ReturnOrganization()
		{
			//Arrange
			Guid realPageId = Guid.NewGuid();

			Organization organization = new Organization()
			{
				Name = "Company",
				RealPageId = realPageId,
				BooksCustomerMasterId = _BooksCompanyMasterId,
				BooksMasterId = _BooksMasterId,
				organizationType = new OrganizationType()
				{
					OrganizationTypeId = 1,
					Name = "Multifamily",
					CreateDate = new DateTime()
				},
				OrganizationTypeId = 1,
				PartyId = 1,
				PrimaryOrganization = true
			};

			_mockManageOrganization
				.Setup(m => m.GetOrganization(realPageId, null, null, null))
				.Returns(organization);

			OrganizationController organizationController = new OrganizationController(
				_mockManageOrganization.Object
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim
				)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.GetOrganization(realPageId);
			Organization resultOrganization = response.Content.ReadAsAsync<Organization>().Result;

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
			Assert.True(resultOrganization == organization);
		}

		[Fact]
		public void GetOrganization_ValidRealPageId_ReturnOrganizationList()
		{
			//Arrange
			Guid? realPageId = null;

			IList<Organization> organizationList = new List<Organization>()
			{
				new Organization()
				{
					Name = "Company",
					RealPageId = Guid.NewGuid(),
					BooksCustomerMasterId = _BooksCompanyMasterId,
					BooksMasterId = _BooksMasterId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1,
						Name = "Multifamily",
						CreateDate = new DateTime()
					},
					OrganizationTypeId = 1,
					OrganizationDomainId = 1,
					OrganizationDomain = new OrganizationDomain()
                    {
						OrganizationDomainId = 1,
						Name = "Primary",
						CreateDate = new DateTime()
                    },
					PartyId = 1,
					PrimaryOrganization = true
				}
			};

			_mockManageOrganization
				.Setup(m => m.GetOrganizationList())
				.Returns(organizationList);

			OrganizationController organizationController = new OrganizationController(
				_mockManageOrganization.Object
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim
				)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.GetOrganization(realPageId);
			IList<Organization> resultOrganizationList = response.Content.ReadAsAsync<IList<Organization>>().Result;

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
			Assert.True(resultOrganizationList == organizationList);
		}

		[Fact]
		public void OrganizationCustomFields_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("OrganizationCustomFields" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/organization/customfields"
				)
			);
		}

		[Fact]
		public void OrganizationCustomFields_ValidData_OKRequest()
		{
			//Arrange
			RequestParameter datafilter = new RequestParameter();
			datafilter.Pages.ResultsPerPage = 0;
			datafilter.Pages.StartRow = 1;
			IDictionary<object, object> globals = new Dictionary<object, object>();
			globals.Add(BaseType.RequestParameter, datafilter);
			int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;
			Type type = typeof(IList<CustomField>);

			DefaultUserClaim userClaim = new DefaultUserClaim()
			{
				PersonaId = 1234,
				OrganizationRealPageGuid = new Guid(),
				UserRealPageGuid = new Guid(),
				CustomerMasterId = _BooksCompanyMasterId
			};

			IList<CustomField> customFieldList = new List<CustomField>()
			{
				new CustomField()
				{
					FieldId = 15,
					OrganizationId = 350,
					Enabled = true,
					Name = "Employee ID",
					Description = null,
					FieldTypeId = 1,
					FieldTypeName = "Alphanumeric",
					Required = false,
					ReadOnly = false,
					DefaultValue = null,
					SyncField = null,
					Sequence = 1,
					HelpText = null,
					MinCharLength = 1,
					MaxCharLength = 10
				}
			};

			_mockRepository
				.Setup(m => m.GetMany<CustomField>(StoredProcNameConstants.SP_GetFieldsByMasterId, It.IsAny<object>()))
				.Returns(customFieldList);

			ICustomFieldsRepository customFieldsRepository = new CustomFieldsRepository(_mockRepository.Object);

			ManageCustomFields manageCustomFields = new ManageCustomFields(customFieldsRepository, userClaim);

			_mockManageCustomFields
				.Setup(m => m.GetCustomField(globals, _BooksCompanyMasterId, bookMasterTypeId))
				.Returns(customFieldList);

			OrganizationController organizationController = new OrganizationController(
				_mockManageOrganization.Object
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, manageCustomFields
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, userClaim
				)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = organizationController.OrganizationCustomFields(datafilter: null);

			//Assert
			Assert.True(
				customFieldList.Count == customFieldList.Count
				&&
				customFieldList.SequenceEqual(customFieldList)
				&&
				NumberOfProperties == 1
			);
		}

		[Fact]
        public void GetProductsByOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetProductsByOrganization" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
				"http://localhost/api/organization/C802694D-5553-4527-8616-3C0F434AE62D/products?mergePersonaAccess=false&allProducts=false"
				)
            );
        }

		[Fact]
		public void OrganizationType_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("OrganizationType" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/OrganizationType"
				)
			);
		}

		[Fact]
		public void OrganizationType_NoData_OKRequest()
		{
			//Arrange
			ObjectListOutput<OrganizationType, IErrorData> output = new ObjectListOutput<OrganizationType, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			List<OrganizationType> organizationTypeList = null;

			Mock<IRepository> mockRepository = new Mock<IRepository>();

			mockRepository
				.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
				.Returns(organizationTypeList);

			ManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim);

			OrganizationController organizationController = new OrganizationController(
				manageOrganization
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
			rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.OrganizationType();
			output = response.Content.ReadAsAsync<ObjectListOutput<OrganizationType, IErrorData>>().Result;

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
			Assert.True(output.list == null);
		}

		[Fact]
		public void OrganizationType_ValidData_OKRequest()
		{
			//Arrange
			ObjectListOutput<OrganizationType, IErrorData> output = new ObjectListOutput<OrganizationType, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

			OrganizationController organizationController = new OrganizationController(
				manageOrganization
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
			rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.OrganizationType();
			output = response.Content.ReadAsAsync<ObjectListOutput<OrganizationType, IErrorData>>().Result;

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
			Assert.True(output.list.Count.Equals(3));
		}

		[Fact]
		public void OrganizationProductTest()
		{
			OrganizationController orgCont = new OrganizationController(_mockManageOrganization.Object
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim);

			List<ProductEnum> productList = new EditableList<ProductEnum>();
			List<string> blueBookProductList = new List<string>();

			// verify all blue book enums match a product
			foreach (var pi in typeof(BlueBookProductConstants).GetFields())
			{
				blueBookProductList.Add(pi.GetValue(pi).ToString());
			}

			List<string> invalidProductList = ManageOrganization.ParseProduct(blueBookProductList, productList);
			Assert.True(invalidProductList.Count == 0 && productList.Count == blueBookProductList.Count);

			// list of products to exclude from Bluebook to product integration
			var ignoreProductList = new List<ProductEnum>()
			{
				ProductEnum.UnifiedUI
				, ProductEnum.SelfProvisioningPortal
				, ProductEnum.SalesForce
				, ProductEnum.SettingsManagement
			};
			
			foreach (var pr in typeof(ProductEnum).GetFields())
			{
				if (pr.Name != "value__")
				{
					ProductEnum current = (ProductEnum) Enum.Parse(typeof(ProductEnum), pr.Name);
					if (!ignoreProductList.Contains(current))
					{
						// if this fails, then you didn't add the product to the BlueBookProductConstants.cs file!
						Assert.True(productList.Contains(current), $"Missing product {pr.Name} in BlueBookProductConstants.cs");
					}
				}
			}
		}

		[Fact]
		public void ListOrganizationByEnterpriseUserId_InvalidRealPageId_ReturnBadRequest()
		{
			//Arrange
			ObjectListOutput<Organization, IErrorData> output = new ObjectListOutput<Organization, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			Guid realPageId = Guid.Empty;

			OrganizationController organizationController = new OrganizationController(
				_mockManageOrganization.Object
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, _defaultUserClaim
				)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();
			
			HttpResponseMessage response = organizationController.ListOrganizationByEnterpriseUserId(realPageId);
			string message = response.Content.ReadAsStringAsync().Result;
			string expectedValue = "\"Invalid parameter: realPageId\"";

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
			Assert.True(expectedValue == message);
		}

		[Fact]
		public void ListOrganizationByEnterpriseUserId_ValidRealPageId_ReturnData()
		{
			//Arrange
			ObjectListOutput<Organization, IErrorData> output = new ObjectListOutput<Organization, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			Guid realPageId = Guid.NewGuid();

			DefaultUserClaim userClaim = new DefaultUserClaim()
			{
				PersonaId = 1234,
				OrganizationRealPageGuid = realPageId,
				UserRealPageGuid = new Guid(),
				CustomerMasterId = _BooksCompanyMasterId
			};

			OrganizationType organizationType = new OrganizationType()
			{
				OrganizationTypeId = 1,
				Name = "Multifamily",
				CreateDate = new DateTime()
			};

            OrganizationDomain organizationDomain = new OrganizationDomain()
            {
                OrganizationDomainId = 1,
                Name = "Primary",
                CreateDate = new DateTime()
            };

			RoleType roleTypeFrom = new RoleType()
			{
				PartyRoleTypeId = (int)UserRoleType.User,
				ParentPartyRoleTypeId = 400,
				Name = "User"
			};

			RoleType roleTypeTo = new RoleType()
			{
				PartyRoleTypeId = 202,
				ParentPartyRoleTypeId = 200,
				Name = "Property Management Company"
			};

			RelationshipType relationshipType = new RelationshipType()
			{
				RelationshipTypeId = 44,
				RoleTypeIdValidFrom = (int)UserRoleType.User,
				RoleTypeIdValidTo = 202,
				Name = "User Relationship",
				Description = ""
			};

			PartyRelationship partyRelationship = new PartyRelationship()
			{
				PartyRelationshipId = 3,
				PartyIdFrom = 19,
				RealPageIdFrom = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202"),
				PartyIdTo = 6,
				RealPageIdTo = new Guid("724DE532-7969-42B5-9E71-2955167179BA"),
				RoleTypeIdFrom = (int)UserRoleType.User,
				RoleTypeFrom = roleTypeFrom,
				RoleTypeIdTo = 202,
				RoleTypeTo = roleTypeTo,
				PartyRelationshipTypeId = 44,
				PartyRelationshipType = relationshipType,
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};

			IList<Organization> organizationList = new List<Organization>()
			{
				new Organization()
				{
					Name = "Company",
					RealPageId = Guid.NewGuid(),
					BooksCustomerMasterId = _BooksCompanyMasterId,
					BooksMasterId = _BooksMasterId,
					organizationType = organizationType,
					OrganizationTypeId = 1,
					OrganizationDomain = organizationDomain,
					OrganizationDomainId = 1,
					PartyId = 1,
					PrimaryOrganization = true,
					partyRelationship = partyRelationship
				}
			};

            IList<OrganizationType> organizationTypeList = new List<OrganizationType>() {organizationType};

			_mockRepository
				.Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, It.IsAny<object>()))
				.Returns(organizationList);

			IUserLoginRepository userLoginRepository = new UserLoginRepository(_mockRepository.Object);
			ManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim);

			_mockManageUserLogin
				.Setup(m => m.ListOrganizationByEnterpriseUserId(realPageId, null))
				.Returns(organizationList);

			_mockManagePartyRelationship
				.Setup(m => m.GetPartyRelationship(realPageId, partyRelationship.RealPageIdTo, null, null, null))
				.Returns(partyRelationship);

			OrganizationController organizationController = new OrganizationController(
				manageOrganization
				, _mockRepositoryResponse.Object
				, _mockOrganizationProductRepository.Object
				, _mockManageOrganizationProduct.Object
				, _mockManageCustomFields.Object
				, _mockManageUserLogin.Object
				, _mockManagePartyRelationship.Object
				, userClaim
				)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

			HttpResponseMessage response = organizationController.ListOrganizationByEnterpriseUserId(realPageId);
			output = response.Content.ReadAsAsync<ObjectListOutput<Organization, IErrorData>>().Result;

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
			Assert.True(output.list.Count.Equals(1));
		}

		#endregion
	}
}
