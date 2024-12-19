using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
using Xunit;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;


namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    /// <summary>
    /// Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageOneSiteAccountingProductTests : ManageProductBaseTests
    {
        private static string _companyName = "TESTCOMPANY";

        private static LocationID[] _emptyProductPropertyList = new LocationID[0];
        private static LocationID[] _productPropertyList;
        private static RoleName[] _emptyProductRoleList = new RoleName[0];
        private static RoleName[] _productRoleList;
        private static NameValuePair[] _productUser;

        private GbProductMap _gbProductMap = new GbProductMap();

        /// <summary>
        /// 
        /// </summary>
        public ManageOneSiteAccountingProductTests() : base((int)ProductEnum.FinancialSuite)
        {
            _editorSamlAttributes = new List<SamlAttributes>();
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567|username" });

            _userSamlAttributes = new List<SamlAttributes>();
            _userSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567|username2" });
            _userSamlAttributes.Add(new SamlAttributes() { Name = "ProductUserName", Value = "username2" });

            _emptySamlAttributes = new List<SamlAttributes>();

            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });
            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "AllProperties", Value = "1", ProductSettingId = 1234 });

            _repositoryResponseProductStatus.ErrorMessage = "";

            List<LocationID> propertyList = new List<LocationID>();
            propertyList.Add(new LocationID() { LocationID1 = "1234567", Name = "Property 1", Address1 = "Address 1", Address2 = "Address 2", City = "Some City", State = "Some State", Zip = "12345", Assigned = "true" });
            propertyList.Add(new LocationID() { LocationID1 = "3333333", Name = "Property 2", Address1 = "Address 1", Address2 = "Address 2", City = "Another City", State = "Another State", Zip = "54321", Assigned = "false" });
            propertyList.Add(new LocationID() { LocationID1 = "4444444", Name = "Property 3", Address1 = "Address 1", Address2 = "Address 2", City = "No City", State = "No State", Zip = "77777", Assigned = "true" });
            _productPropertyList = propertyList.ToArray();

            List<RoleName> roleList = new List<RoleName>();
            roleList.Add(new RoleName() { Recordno = "1", Name = "Role 1", Description = "Role 1", Assigned = "true" });
            roleList.Add(new RoleName() { Recordno = "2", Name = "Role 2", Description = "Role 2", Assigned = "false" });
            roleList.Add(new RoleName() { Recordno = "3", Name = "Role 3", Description = "Role 3", Assigned = "true" });
            _productRoleList = roleList.ToArray();

            List<NameValuePair> user = new List<NameValuePair>();
            user.Add(new NameValuePair { Name = "FirstName", Value = "First name" });
            user.Add(new NameValuePair { Name = "LastName", Value = "Last name" });
            user.Add(new NameValuePair { Name = "UserId", Value = "12345" });

            _productUser = user.ToArray();

            _resultPropertyList = new List<ProductProperty>();
            _resultPropertyList.Add(new ProductProperty() { ID = "1234567", Name = "Property 1", Street1 = "Address 1", Street2 = "Address 2", City = "Some City", State = "Some State", Zip = "12345", IsAssigned = true });
            _resultPropertyList.Add(new ProductProperty() { ID = "3333333", Name = "Property 2", Street1 = "Address 1", Street2 = "Address 2", City = "Another City", State = "Another State", Zip = "54321", IsAssigned = false });

            _resultPropertyListFinSuite = new List<ACProperty>();
            _resultPropertyListFinSuite.Add(new ACProperty() { PropertyId = "1234567", PropertyName = "Property 1", IsAssigned = true });
            _resultPropertyListFinSuite.Add(new ACProperty() { PropertyId = "3333333", PropertyName = "Property 2", IsAssigned = false });


            _resultRoleList = new List<ProductRole>();
            _resultRoleList.Add(new ProductRole() { ID = "1", Name = "Role 1", Description = "Role 1", Roletype = "Default", RightsAssigned = "12", IsAssigned = true });
            _resultRoleList.Add(new ProductRole() { ID = "2", Name = "Role 2", Description = "Role 2", Roletype = "Custom", RightsAssigned = "2", IsAssigned = false });
            _resultRoleList.Add(new ProductRole() { ID = "3", Name = "Role 3", Description = "Role 3", Roletype = "Default", RightsAssigned = "312", IsAssigned = true });

            _gbProductMap = new GbProductMap { ProductId = 8, BooksProductCode = "ACCT", Name = "OneSite Accounting", UDMSourceCode = "ACCT" };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.FinancialSuite))))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(new List<GbProductMap>() { _gbProductMap });
        }

        [Fact]
        public void Get_ValidatePersonas()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            reqParameter.SortBy = new Dictionary<string, string>();
            reqParameter.SortBy.Add("SiteName", "ASC");
            reqParameter.FilterBy = new Dictionary<string, string>();
            reqParameter.FilterBy.Add("SiteName", "Test Property");

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            List<SamlAttributes> attributes = new List<SamlAttributes>();
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _companyName, Source = BlueBookProductConstants.FinancialSuite };
            mapResource.Add(resource);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                ))
                .Returns(attributes);

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(mapResource);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, null, mockHttpMessageHandler.Object, mockRepository.Object);

            ListResponse resp = manageProduct.GetUserProperties(_editorPersonaId, 10, reqParameter);
            Assert.True(resp.IsError && resp.ErrorReason == "Invalid user persona");

            resp = manageProduct.GetUserProperties(_editorPersonaId, 33, reqParameter);
            Assert.True(resp.IsError && resp.ErrorReason == "Invalid user persona");

            resp = manageProduct.GetUserProperties(_editorPersonaId, 0, reqParameter);
            Assert.True(resp.TotalRows == 0);

        }

        [Fact]
        public void Get_UserProperties()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            reqParameter.SortBy = new Dictionary<string, string>();
            reqParameter.SortBy.Add("SiteName", "ASC");
            reqParameter.FilterBy = new Dictionary<string, string>();
            reqParameter.FilterBy.Add("SiteName", "Test Property");

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            List<SamlAttributes> attributes = new List<SamlAttributes>();
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _companyName, Source = BlueBookProductConstants.FinancialSuite };
            mapResource.Add(resource);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                ))
                .Returns(attributes);

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(mapResource);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            ListResponse resp = manageProduct.GetUserProperties(0, 0, reqParameter);
            Assert.True(resp.TotalRows == 0);

            resp = manageProduct.GetUserProperties(_editorPersonaId, 0, reqParameter);
            Assert.True(resp.TotalRows == 0);
        }


        #region Property

        [Fact]
        public void Get_UserProperties_WithNoFilterAndNoSort()
        {
            //Arrange
            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            TotalRows[] totalRows = new TotalRows[0];

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockService
                .Setup(m => m.GetAllProperties(
                    It.IsAny<Property[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Throws(new Exception("Service exception"));

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            ListResponse resp = manageProduct.GetUserProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && resp.ErrorReason == "Service exception");

            // now return an empty property list
            mockService
                .Setup(m => m.GetAllProperties(
                    It.IsAny<Property[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_emptyProductPropertyList);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            resp = manageProduct.GetUserProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.TotalRows == 0);
            _emptyProductPropertyList = null;

            // now return a null list
            mockService
                .Setup(m => m.GetAllProperties(
                    It.IsAny<Property[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_emptyProductPropertyList);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            resp = manageProduct.GetUserProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.TotalRows == 0);

            // now return a list of properties
            mockService
                .Setup(m => m.GetAllProperties(
                    It.IsAny<Property[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_productPropertyList);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            resp = manageProduct.GetUserProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.TotalRows == 3);

        }

        [Fact]
        public void Get_UserProperties_WithNoFilterAndNoSort_ExceptionThrown()
        {
            //Arrange
            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            GbProductMap productMap = new GbProductMap()
            {
                ProductId = (int)ProductEnum.FinancialSuite,
                Name = "Financial Suit",
                BooksProductCode = "ACCT",
                UDMSourceCode = "Acct"
            };
            SamlRepository samlRepository = new SamlRepository();
            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository.Setup(m => m.GetBooksMasterProductDetail((int)ProductEnum.FinancialSuite)).Returns(productMap);
            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            Persona persona = new Persona() { Organization = new Organization() { BooksMasterId = 1234 } };
            ListResponse resp = manageProduct.GetUserProperties(0, 0, null);
            Assert.True(resp.IsError == true && resp.ErrorReason.ToUpper() == "INVALID PERSONA");
        }

        #endregion

        #region Roles

        [Fact]
        public void Get_UserRoles_WithNoFilterAndNoSort_ExceptionThrown()
        {
            //Arrange
            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            SamlRepository samlRepository = new SamlRepository();
            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            Persona persona = new Persona() { Organization = new Organization() { BooksMasterId = 1234 } };
            ListResponse resp = manageProduct.GetUserRoles(0, 0, null);
            Assert.True(resp.IsError == true && resp.ErrorReason.ToUpper() == "INVALID PERSONA");
        }

        [Fact]
        public void Get_UserRoles_WithFilterAndSort_NoData()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            reqParameter.SortBy = new Dictionary<string, string>();
            reqParameter.SortBy.Add("SiteName", "ASC");
            reqParameter.FilterBy = new Dictionary<string, string>();
            reqParameter.FilterBy.Add("SiteName", "Test Property");

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            TotalRows[] totalRows = new TotalRows[0];

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockService
                .Setup(m => m.GetAllRoles(
                    It.IsAny<Role[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Throws(new Exception("Service exception"));

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            ListResponse resp = manageProduct.GetUserRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && (resp.ErrorReason == "Service exception" || resp.ErrorReason == CommonMessageConstants.RoleErrorMessage));

            resp = manageProduct.GetUserRoles(_editorPersonaId, 33, null);
            Assert.True(resp.IsError && (resp.ErrorReason == "Invalid user persona" || resp.ErrorReason == CommonMessageConstants.RoleErrorMessage));

            // now return an empty property list
            mockService
                .Setup(m => m.GetAllRoles(
                    It.IsAny<Role[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_emptyProductRoleList);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            resp = manageProduct.GetUserRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.TotalRows == 0);
            _emptyProductRoleList = null;

            // now return a null list
            mockService
                .Setup(m => m.GetAllRoles(
                    It.IsAny<Role[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_emptyProductRoleList);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            resp = manageProduct.GetUserRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.TotalRows == 0);

            // now return a list of properties
            mockService
                .Setup(m => m.GetAllRoles(
                    It.IsAny<Role[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_productRoleList);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, managePartyRelationship: null, httpMessageHandler: mockHttpMessageHandler.Object, repository: mockRepository.Object);

            resp = manageProduct.GetUserRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.TotalRows == 3);
        }

        #endregion

        #region User properties

        [Fact]
        public void Update_PropertiesToUser()
        {
            //Arrange

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            TotalRows[] totalRows = new TotalRows[0];

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockService
                .Setup(m => m.GetAllProperties(
                    It.IsAny<Property[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_productPropertyList);

            mockService
                .Setup(m => m.AssignPropertiesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Throws(new Exception("Service exception"));

            mockService
                .Setup(m => m.RemovePropertiesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Throws(new Exception("Service exception"));

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
                ))
                .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            List<string> propertiesToAssign = new List<string>();
            propertiesToAssign.Add("1234567");
            propertiesToAssign.Add("2222222");
            propertiesToAssign.Add("3333333");

            string result = manageProduct.UpdatePropertiesToUser(_editorPersonaId, _userPersonaId, propertiesToAssign, false, out var additionalParameters);
            Assert.True(result == "An error occurred. Service exception");

            result = manageProduct.UpdatePropertiesToUser(_editorPersonaId, 33, propertiesToAssign, false, out var additionalParametersInvalid);
            Assert.True(result == "Invalid user persona");

            // update successfully
            mockService
                .Setup(m => m.RemovePropertiesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("PROVIDED USER PROPERTIES REMOVED SUCCESSFULLY");

            mockService
                .Setup(m => m.AssignPropertiesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("PROVIDED USER PROPERTIES ADDED SUCCESSFULLY");

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.UpdatePropertiesToUser(_editorPersonaId, _userPersonaId, propertiesToAssign, false, out var additionalParametersSuccess);
            Assert.True(string.IsNullOrEmpty(result));

            // update failed
            mockService
                .Setup(m => m.RemovePropertiesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("SOME ERROR");

            mockService
                .Setup(m => m.AssignPropertiesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("SOME ERROR");

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.UpdatePropertiesToUser(_editorPersonaId, _userPersonaId, propertiesToAssign, false, out var additionalParametersError);
            Assert.True(!string.IsNullOrEmpty(result));

            // added all successfully
            propertiesToAssign = new List<string>();
            propertiesToAssign.Add("ALL");

            mockService
                .Setup(m => m.RemovePropertiesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("PROVIDED USER PROPERTIES REMOVED SUCCESSFULLY");

            mockService
                .Setup(m => m.AssignPropertiesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("PROVIDED USER PROPERTIES ADDED SUCCESSFULLY");

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.UpdatePropertiesToUser(_editorPersonaId, _userPersonaId, propertiesToAssign, false, out var additionalParametersAdded);
            Assert.True(string.IsNullOrEmpty(result));

        }

        #endregion

        #region User roles

        [Fact]
        public void Update_RolesToUser()
        {
            //Arrange

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            TotalRows[] totalRows = new TotalRows[0];

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockService
                .Setup(m => m.GetAllRoles(
                    It.IsAny<Role[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_productRoleList);

            mockService
                .Setup(m => m.AssignRolesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Throws(new Exception("Service exception"));

            mockService
                .Setup(m => m.RemoveRolesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Throws(new Exception("Service exception"));

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
                ))
                .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            List<string> rolesToAssign = new List<string>();
            rolesToAssign.Add("1");
            rolesToAssign.Add("3");
            rolesToAssign.Add("4");

            string result = manageProduct.UpdateRolesToUser(_editorPersonaId, _userPersonaId, rolesToAssign, false, out var additionalParametersSuccess);
            Assert.True(result == "An error occurred. Service exception");

            result = manageProduct.UpdateRolesToUser(_editorPersonaId, 33, rolesToAssign, false, out var additionalParametersInvalid);
            Assert.True(result == "Invalid user persona");

            // update successfully
            mockService
                .Setup(m => m.RemoveRolesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("REMOVED PROVIDED ROLES SUCCESSFULLY");

            mockService
                .Setup(m => m.AssignRolesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("PROVIDED USER ROLES ADDED SUCCESSFULLY");

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.UpdateRolesToUser(_editorPersonaId, _userPersonaId, rolesToAssign, false, out var additionalParametersUpdated);
            Assert.True(string.IsNullOrEmpty(result));

            // update failed
            mockService
                .Setup(m => m.RemoveRolesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("SOME ERROR");

            mockService
                .Setup(m => m.AssignRolesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("SOME ERROR");

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.UpdateRolesToUser(_editorPersonaId, _userPersonaId, rolesToAssign, false, out var additionalParametersUpdatedFailed);
            Assert.True(!string.IsNullOrEmpty(result));

            // added all successfully
            rolesToAssign = new List<string>();
            rolesToAssign.Add("ALL");

            mockService
                .Setup(m => m.RemoveRolesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("REMOVED PROVIDED ROLES SUCCESSFULLY");

            mockService
                .Setup(m => m.AssignRolesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("PROVIDED USER ROLES ADDED SUCCESSFULLY");

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.UpdateRolesToUser(_editorPersonaId, _userPersonaId, rolesToAssign, false, out var additionalParametersRemoved);
            Assert.True(string.IsNullOrEmpty(result));

        }

        #endregion

        #region User

        [Fact]
        public void Update_User()
        {
            //Arrange

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            TotalRows[] totalRows = new TotalRows[0];

            IC.Person person = new IC.Person() { FirstName = "Test First", LastName = "Test Last", PartyId = 30, RealPageId = _userRealPageId };
            UserLoginOnly userlogin = new UserLoginOnly() { LoginName = "test@test.com", PartyId = 30, RealPageId = _userRealPageId };

            List<NameValuePair> successAddUser = new List<NameValuePair>();
            successAddUser.Add(new NameValuePair() { Name = "SYSTEMIDENTIFIER", Value = "SOMECOMPANY|USERLOGIN" });
            NameValuePair[] successAddUserResult = successAddUser.ToArray();

            //
            successAddUser = new List<NameValuePair>();
            successAddUser.Add(new NameValuePair() { Name = "Error", Value = "CAN'T CREATE THE USER" });
            NameValuePair[] failAddUserResult = successAddUser.ToArray();

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _newUserPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                ))
                .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _userPersonaId)
                ))
                .Returns(_userPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _newUserPersonaId)
                ))
                .Returns(_newUserPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            mockManagePerson
                .Setup(m => m.GetPerson(
                    It.IsAny<Guid>()
                ))
                .Returns(person);

            mockManageUserLogin
                .Setup(m => m.GetUserLoginOnly(
                    It.IsAny<Guid>()
                ))
                .Returns(userlogin);
            
            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockManageElectronicAddress
                .Setup(m => m.ListElectronicAddressForPerson(
                    It.IsAny<Guid>(),
                    It.IsAny<string>()
                ))
                .Returns(new List<IC.ElectronicAddress>());

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductRepository
                .Setup(m => m.CreateProductSetting(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()
                    , It.IsAny<string>()
                ))
                .Returns(_repositoryResponseProductStatus);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            mockService
                .Setup(m => m.GetAllRoles(
                    It.IsAny<Role[]>()
                    , It.IsAny<FilterSortParameters>()
                    , out totalRows
                ))
                .Returns(_productRoleList);

            mockService
                .Setup(m => m.RemoveRolesFromUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("REMOVED PROVIDED ROLES SUCCESSFULLY");

            mockService
                .Setup(m => m.AssignRolesToUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("PROVIDED USER ROLES ADDED SUCCESSFULLY");

            mockService
                .SetupSequence(m => m.CheckIfUserIDIsUsed(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("YES")
                .Returns("NO");

            mockService
                .Setup(m => m.CreateUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns(successAddUserResult);

            mockService
                .Setup(m => m.UpdateUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("Success");

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
                ))
                .Returns(_partyRelationShip);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object, mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            List<string> rolesToAssign = new List<string>();
            rolesToAssign.Add("1");
            rolesToAssign.Add("3");
            rolesToAssign.Add("4");

            List<string> propertiesToAssign = new List<string>();
            propertiesToAssign.Add("1234567");
            propertiesToAssign.Add("2222222");
            propertiesToAssign.Add("3333333");

            List<string> companiesToAssign = new List<string>();
            companiesToAssign.Add("111111");


            string result = manageProduct.ManageAccountingUser(_editorPersonaId, _userPersonaId, rolesToAssign, propertiesToAssign, companiesToAssign, false, false, false, out List<AdditionalParameters> additionalParametersTest1);
            Assert.True(string.IsNullOrEmpty(result)); // success creating user

            // Test 2
            mockService
                .SetupSequence(m => m.CheckIfUserIDIsUsed(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("YES")
                .Returns("NO");

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object, mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.ManageAccountingUser(_editorPersonaId, _userPersonaId, null, null, null, false, false, false, out List<AdditionalParameters> additionalParametersTest2);
            Assert.True(string.IsNullOrEmpty(result)); // success creating user

            // Test 3
            mockService
                .SetupSequence(m => m.CheckIfUserIDIsUsed(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("YES")
                .Returns("NO");

            mockService
                .Setup(m => m.CreateUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns(failAddUserResult);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object, mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.ManageAccountingUser(_editorPersonaId, _newUserPersonaId, null, null, null, false, false, false, out List<AdditionalParameters> additionalParametersTest3);
            Assert.True(result == "CAN'T CREATE THE USER"); // fail creating user

            // Test 4 - update user
            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object, mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.ManageAccountingUser(_editorPersonaId, _userPersonaId, null, null, null, false, false, false, out List<AdditionalParameters> additionalParametersTest4);
            Assert.True(string.IsNullOrEmpty(result)); // success creating user

            // Test 5 - Get email

            IList<ElectronicAddress> emailList = new List<ElectronicAddress>();
            ElectronicAddress ea = new ElectronicAddress() { AddressType = "Email", AddressString = "some@email.com" };
            ea.contactMechanismUsageType = new ContactMechanismUsageType() { Name = "Primary" };

            emailList.Add(ea);
            mockManageElectronicAddress
                .Setup(m => m.ListElectronicAddressForPerson(
                    It.IsAny<Guid>(),
                    It.IsAny<string>()
                ))
                .Returns(emailList);

            mockService
                .SetupSequence(m => m.CheckIfUserIDIsUsed(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns("YES")
                .Returns("NO");

            mockService
                .Setup(m => m.CreateUser(
                    It.IsAny<NameValuePair[]>()
                ))
                .Returns(successAddUserResult);

            manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object, mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            result = manageProduct.ManageAccountingUser(_editorPersonaId, _userPersonaId, null, null, null, false, false, false, out List<AdditionalParameters> additionalParametersTest5);
            Assert.True(string.IsNullOrEmpty(result)); // success creating user
        }

        #endregion

        #region ChangeAccountingUserClaimStatus

        [Fact]
        public void ChangeAccountingUserClaimStatus_Given_EditorPersonaIdUserPersonaIdAndFalg_Should_SetSSOFlagForAUser()
        {
            //Arrange

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _userPersonaId)
                ))
                .Returns(_userPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            var isLinked = true;
            var systemIdentifier = "SOMECOMPANY|USERLOGIN";
            var login = "dummy";
            var password = "dummy";
            var federatedId = "USERLOGIN";
            var expected = true;
            mockService
                .Setup(m => m.ChangeClaimStatus(systemIdentifier, isLinked, login, password, federatedId));

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object,
                mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object,
                mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            var actual = manageProduct.ChangeAccountingUserClaimStatus(_editorPersonaId, _userPersonaId, isLinked);

            Assert.Equal(expected, actual);
            Assert.True(actual);
        }

        #endregion

        #region Migration

        [Fact]
        public void GetMigrationUsers_Given_EditorPersonaIdDataFilter_Should_ReturnAccountingUserList()
        {
            //Arrange

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _userPersonaId)
                ))
                .Returns(_userPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            var expected = new List<UserName>()
            {
                new UserName() { FirstName = "Person", LastName = "1", EmailAddress = "person1@test.com", UserID = "person1" },
                new UserName() { FirstName = "Person", LastName = "2", EmailAddress = "person2@test.com", UserID = "person2" },
                new UserName() { FirstName = "Person", LastName = "3", EmailAddress = "person3@test.com", UserID = "person3" },
                new UserName() { FirstName = "Person", LastName = "4", EmailAddress = "person4@test.com", UserID = "person4", USERSTATUS = "F" },
                new UserName() { FirstName = "Person", LastName = "5", EmailAddress = "person5@test.com", UserID = "person5", USERSTATUS = "T" }
            };

            var startRow = 0;
            var resultsPerPage = 1000;
            TotalRows[] totalRows = new TotalRows[1] { new TotalRows() { TotalRows1 = "5" } };
            RequestParameter dataFilter = new RequestParameter()
            {
                Pages = new PageRequest()
                {
                    ResultsPerPage = resultsPerPage,
                    StartRow = startRow
                }
            };
            mockService
                .Setup(m => m.GetAllUsers(
                    It.IsAny<Component.SharedObjects.Product.OneSiteAccounting.User[]>(),
                    It.IsAny<FilterSortParameters>(),
                    out totalRows))
                .Returns(expected.ToArray());

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object,
                mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object,
                mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            var actual = manageProduct.GetMigrationUsers(_editorPersonaId, dataFilter);

            //Assert
            Assert.IsType<MigrationUser>(actual.Records[0]);
            Assert.True(expected.Count == actual.Records.Count);
            Assert.Same(actual.ErrorReason, "");
        }

        [Fact]
        public void GetMigrationUsers_Given_EditorPersonaIdDataFilter_Should_ReturnErrorResponse()
        {
            //Arrange

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _userPersonaId)
                ))
                .Returns(_userPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            var startRow = 0;
            var resultsPerPage = 1000;
            TotalRows[] totalRows = new[] { new TotalRows() { TotalRows1 = "Not a Valid UserId" } };
            RequestParameter dataFilter = new RequestParameter()
            {
                Pages = new PageRequest()
                {
                    ResultsPerPage = resultsPerPage,
                    StartRow = startRow
                }
            };
            var users = new List<UserName>();

            mockService
                .Setup(m => m.GetAllUsers(
                    It.IsAny<Component.SharedObjects.Product.OneSiteAccounting.User[]>(),
                    It.IsAny<FilterSortParameters>(),
                    out totalRows))
                .Returns((UserName[])null);

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object,
                mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object,
                mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            var actual = manageProduct.GetMigrationUsers(_editorPersonaId, dataFilter);

            //Assert
            Assert.True(actual.IsError);
            Assert.Same("Invalid user.", actual.ErrorReason);
        }

        [Fact]
        public void UpdateUsersMigrationStatus_Given_EditorPersonaIdAndMigrateUser_Should_ReturnSuccessMessage()
        {
            //Arrange

            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _userPersonaId)
                ))
                .Returns(_userPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser() { UserId = "person1", UsingUnifiedLogin = true, UnifiedLoginUserName = "person1@test.com" },
                new MigrateUser() { UserId = "person2", UsingUnifiedLogin = false, UnifiedLoginUserName = "person2@test.com" },
            };

            mockService
                .Setup(m => m.EnableGreenBookUser(It.IsAny<NameValuePair[]>()))
                .Returns("UserID is Green Book Enabled.");
            mockService
                .Setup(m => m.DisableGreenBookUser(It.IsAny<NameValuePair[]>()))
                .Returns("UserID is Green Book Disabled.");

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object,
                mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object,
                mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            var actual = manageProduct.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal("UserID is Green Book Enabled.UserID is Green Book Disabled.", actual.Message);
            Assert.True(actual.Status);
        }

        #region User-Status

        [Fact]
        public void ChangeUserStatus_Given_UserName_Disable_ShouldReturn_True()
        {
            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _userPersonaId)
                ))
                .Returns(_userPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);


            mockService
                .Setup(m => m.DisableUser(It.IsAny<NameValuePair[]>()))
                .Returns("INACTIVATED");

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            var username = "123";
            var isActive = false;

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object,
                mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object,
                mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            var actual = manageProduct.ChangeUserStatus(_editorPersonaId, username, isActive);

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public void ChangeUserStatus_Given_Wrong_UserName_Disable_ShouldReturn_False()
        {
            var mockService = new Mock<IOneSiteAccountingProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _userPersonaId)
                ))
                .Returns(_userPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            mockService
                .Setup(m => m.DisableUser(It.IsAny<NameValuePair[]>()))
                .Returns("error");

            var username = "123";
            var isActive = false;

            //Act
            IManageProductOneSiteAccounting manageProduct = new ManageProductOneSiteAccounting(_editorRealPageId, _userUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object,
                mockProductInternalSettingRepository.Object, mockManageElectronicAddress.Object, mockManagePerson.Object,
                mockManageUserLogin.Object, mockManagePartyRelationship.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            var actual = manageProduct.ChangeUserStatus(_editorPersonaId, username, isActive);

            //Assert
            Assert.False(actual);
        }

        #endregion

        #endregion

        #region Controller

        [Fact]
        public void Test_Controllers()
        {
            var mockMangeProductOneSiteAccounting = new Mock<IManageProductOneSiteAccounting>();
            ListResponse propertyResult = new ListResponse()
            {
                Records = _resultPropertyListFinSuite.Cast<object>().ToList(),
                TotalRows = _resultPropertyListFinSuite.Count,
                RowsPerPage = _resultPropertyListFinSuite.Count,
                TotalPages = 1,
                ErrorReason = ""
            };

            ListResponse roleResult = new ListResponse()
            {
                Records = _resultRoleList.Cast<object>().ToList(),
                TotalRows = _resultRoleList.Count,
                RowsPerPage = _resultRoleList.Count,
                TotalPages = 1,
                ErrorReason = ""
            };

            List<string> propList = new List<string>();
            propList.Add("1234567");

            List<string> roleList = new List<string>();
            roleList.Add("4");

            AccountingRoleAndPropertyList roleandpropList = new AccountingRoleAndPropertyList();
            roleandpropList.RoleList = roleList;
            roleandpropList.PropertyList = propList;


            mockMangeProductOneSiteAccounting
                .Setup(m => m.GetUserProperties(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<RequestParameter>()
                ))
                .Returns(propertyResult);

            mockMangeProductOneSiteAccounting
                .Setup(m => m.GetUserPropertiesNew(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<RequestParameter>()
                ))
                .Returns(propertyResult);

            mockMangeProductOneSiteAccounting
                .Setup(m => m.GetUserRoles(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<RequestParameter>()
                ))
                .Returns(roleResult);

            mockMangeProductOneSiteAccounting
                .Setup(m => m.UpdatePropertiesToUser(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<List<string>>(),
                    false,
                    out It.Ref<List<AdditionalParameters>>.IsAny,
                    batchProcessTypeCreUpd
                ))
                .Returns("");

            mockMangeProductOneSiteAccounting
                .Setup(m => m.UpdateRolesToUser(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<List<string>>(),
                    false,
                    out It.Ref<List<AdditionalParameters>>.IsAny,
                    batchProcessTypeCreUpd
                ))
                .Returns("");

            mockMangeProductOneSiteAccounting
                .Setup(m => m.ManageAccountingUser(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<List<string>>()
                    , It.IsAny<List<string>>()
                    , It.IsAny<List<string>>()
                    , false
                    , false
                    , false
                    , out It.Ref<List<AdditionalParameters>>.IsAny
                    , batchProcessTypeCreUpd
                ))
                .Returns("");

            mockMangeProductOneSiteAccounting
                .SetupSequence(m => m.DeleteAccountingUser(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                ))
                .Returns("")
                .Returns("Error");

            #region Properties

            ProductOneSiteAccountingController controller = new ProductOneSiteAccountingController(mockMangeProductOneSiteAccounting.Object);
            ListResponse controllerResponse = controller.GetUserProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(controllerResponse.TotalRows == 2);

            var message = controller.UpdateUserProperties(_editorPersonaId, _userPersonaId, propList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.OK);

            message = controller.UpdateUserProperties(_editorPersonaId, _userPersonaId, new List<string>());
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.BadRequest);

            var exception = Record.Exception(() => controller.UpdateUserProperties(0, _userPersonaId, new List<string>()));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            exception = Record.Exception(() => controller.UpdateUserProperties(_editorPersonaId, 0, new List<string>()));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            #endregion

            #region Roles

            controllerResponse = controller.GetUserRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(controllerResponse.TotalRows == 3);

            message = controller.UpdateUserRoles(_editorPersonaId, _userPersonaId, roleList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.OK);

            message = controller.UpdateUserRoles(_editorPersonaId, _userPersonaId, new List<string>());
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.BadRequest);

            exception = Record.Exception(() => controller.UpdateUserRoles(0, _userPersonaId, new List<string>()));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            exception = Record.Exception(() => controller.UpdateUserRoles(_editorPersonaId, 0, new List<string>()));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            #endregion

            mockMangeProductOneSiteAccounting
                .Setup(m => m.UpdatePropertiesToUser(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<List<string>>(),
                    false,
                    out It.Ref<List<AdditionalParameters>>.IsAny, batchProcessTypeCreUpd

                ))
                .Returns("Error");

            mockMangeProductOneSiteAccounting
                .Setup(m => m.UpdateRolesToUser(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<List<string>>(),
                    false,
                    out It.Ref<List<AdditionalParameters>>.IsAny,
                    batchProcessTypeCreUpd
                ))
                .Returns("Error");

            controller = new ProductOneSiteAccountingController(mockMangeProductOneSiteAccounting.Object);

            message = controller.UpdateUserProperties(_editorPersonaId, _userPersonaId, propList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.NoContent);

            message = controller.UpdateUserRoles(_editorPersonaId, _userPersonaId, propList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.NoContent);

            #region User

            message = controller.CreateAccountingUser(_editorPersonaId, _userPersonaId, roleandpropList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.Created);

            message = controller.UpdateAccountingUser(_editorPersonaId, _userPersonaId, roleandpropList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.OK);

            message = controller.CreateAccountingUser(_editorPersonaId, _userPersonaId, null);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.Created);

            message = controller.UpdateAccountingUser(_editorPersonaId, _userPersonaId, null);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.OK);

            exception = Record.Exception(() => controller.CreateAccountingUser(0, _userPersonaId, roleandpropList));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            exception = Record.Exception(() => controller.CreateAccountingUser(_editorPersonaId, 0, roleandpropList));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            exception = Record.Exception(() => controller.UpdateAccountingUser(0, _userPersonaId, roleandpropList));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            exception = Record.Exception(() => controller.UpdateAccountingUser(_editorPersonaId, 0, roleandpropList));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            message = controller.DeleteAccountingUser(_editorPersonaId, _userPersonaId);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.NoContent);

            // calling again should result in an error response
            message = controller.DeleteAccountingUser(_editorPersonaId, _userPersonaId);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.BadRequest);

            exception = Record.Exception(() => controller.DeleteAccountingUser(0, _userPersonaId));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            exception = Record.Exception(() => controller.DeleteAccountingUser(_editorPersonaId, 0));
            Assert.NotNull(exception);
            Assert.IsType<HttpResponseException>(exception);

            mockMangeProductOneSiteAccounting
                .Setup(m => m.ManageAccountingUser(
                    It.IsAny<long>()
                    , It.IsAny<long>()
                    , It.IsAny<List<string>>()
                    , It.IsAny<List<string>>()
                    , It.IsAny<List<string>>(),
                    false, false, false,
                    out It.Ref<List<AdditionalParameters>>.IsAny,
                    batchProcessTypeCreUpd
                ))
                .Returns("Error");

            controller = new ProductOneSiteAccountingController(mockMangeProductOneSiteAccounting.Object);

            message = controller.CreateAccountingUser(_editorPersonaId, _userPersonaId, roleandpropList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.BadRequest);

            message = controller.UpdateAccountingUser(_editorPersonaId, _userPersonaId, roleandpropList);
            Assert.True(message.StatusCode == System.Net.HttpStatusCode.BadRequest);

            #endregion

        }

        #endregion
    }
}
