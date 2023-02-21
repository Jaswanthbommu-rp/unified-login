using JsonApiSerializer;
using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Xunit;
using UserController = RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers.UserController;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.Enterprise
{
    [ExcludeFromCodeCoverage]
    public class EnterpriseUserTests
    {
        private static DefaultUserClaim _defaultUserClaim = new DefaultUserClaim();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 2116;
        private static long _PersonaId = 1234;
        private static long _BooksCompanyMasterId = 379;
        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private List<GbProductMap> _gbProductMap;

        #region Constructor
        public EnterpriseUserTests()
        {
            _defaultUserClaim.CorrelationId = new Guid();
            _defaultUserClaim.CustomerMasterId = _BooksCompanyMasterId;
            _defaultUserClaim.OrganizationPartyId = 1234;

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
        }
        #endregion

        private UserController MakeInstance(
            Mock<IRepository> repository = null,
            Mock<IRepositoryResponse> repositoryResponse = null,
            Mock<HttpMessageHandler> httpMessageHandler = null)
        {
            repository = repository != null ? repository : new Mock<IRepository>();
            repositoryResponse = repositoryResponse != null ? repositoryResponse : new Mock<IRepositoryResponse>();
            httpMessageHandler = httpMessageHandler != null ? httpMessageHandler : new Mock<HttpMessageHandler>();

            Mock<IUnitOfWork> mockUnitofWork = new Mock<IUnitOfWork>();
            repository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitofWork.Object);

            repository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);


            return new UserController(
                repository.Object,
                repositoryResponse.Object,
                httpMessageHandler.Object,
                _defaultUserClaim,
                null
            )
            { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };
        }

        [Fact(Skip = "Issues with route tests")]
        public void GetUserProductsByPersonaId_ValidPersonaId_ReturnProductList()
        {
            //Arrange
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceId = 54321,
                    CompanyInstanceSourceId = _RealPageId.ToString(), Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                    CompanyInstance = new List<CompanyInstance>()
                    {
                        new CompanyInstance() {CustomerEnvironment = "Primary", IsActive = true}
                    }
                }
            };

            Persona persona = new Persona()
            {
                PersonaId = _PersonaId,
                RealPageId = _RealPageId,
                Organization = new Organization() { Name = "Test Company", PartyId = 1234 },
                Name = "Title",
                OrganizationPartyId = 1234
            };

            Person person = new Person()
            {
                FirstName = "First name",
                LastName = "Last name",
                RealPageId = persona.RealPageId
            };

            List<PersonaProduct> productList = new List<PersonaProduct>();
            productList.Add(new PersonaProduct()
            {
                ProductId = (int)ProductEnum.OneSite,
                Name = "OneSIte",
                ShowInAppSwitcher = true,
                IsResource = false,
                BooksProductCode = ProductEnumHelper.StringValueOf(ProductEnum.OneSite),
                IsNewTab = true,
                Url = "product/onesite",
                FamilyName = "Property Management",
                FamilyId = 100,
            });

            productList.Add(new PersonaProduct()
            {
                ProductId = (int)ProductEnum.AoBusinessIntelligence,
                Name = "Business Intelligence",
                Url = "/product/businessintelligence",
                FamilyId = 400,
                FamilyName = "Asset Optimization",
                IsNewTab = true,
                isFavorite = true,
                IsResource = false,
                StatusTypeId = 8,
                BooksProductCode = "BI",
                ShowInAppSwitcher = true
            });

            productList.Add(new PersonaProduct()
            {
                ProductId = (int)ProductEnum.ProductUpdates,
                Name = "Product Updates",
                Url = "http://w2w.realpage.com/products/WNWC.asp",
                FamilyId = 0,
                FamilyName = null,
                IsNewTab = true,
                isFavorite = false,
                IsResource = true,
                StatusTypeId = 8,
                BooksProductCode = "PUPDATE",
                ShowInAppSwitcher = false
            });

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={_BooksCompanyMasterId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);

            var mockRepository = new Mock<IRepository>();
            mockRepository
                .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(
                    d => d.ToString().Contains($"personaId = {persona.PersonaId}"))))
                .Returns(persona);

            mockRepository
                .Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.Is<object>(
                    d => d.ToString().Contains($"{persona.RealPageId}"))))
                .Returns(person);

            var userController = MakeInstance(repository: mockRepository, httpMessageHandler: mockHttpMessageHandler);

            mockRepository
                .Setup(m => m.GetMany<PersonaProduct>(StoredProcNameConstants.SP_GetProductsByPersonaId, It.Is<object>(
                    d => d.ToString().Contains($"PersonaId = {persona.PersonaId}, StatusTypeId = 8"))))
                .Returns(productList);

            //Act
            HttpResponseMessage response = userController.GetUserProductsByPersonaId(_PersonaId);
            UserController.UserProductOutputResultv2 result = response.Content?.ReadAsAsync<UserController.UserProductOutputResultv2>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(result?.Resources.Count == 1 && result?.Resources[0].Id == (int)ProductEnum.ProductUpdates);
            Assert.True(result?.Products.Count == 3 && result.Products.ContainsKey("Favorites") && result?.Products["Favorites"].Count == 1);
            Assert.Equal(result.User.FullName, person.FirstName + " " + person.LastName);
        }

        [Fact]
        public void GetProductUserProperties_InvalidProductCode()
        {
            var userController = MakeInstance();

            var response = userController.GetProductUserProperties("invalid product code", null);
            var result = response.Content?.ReadAsAsync<ListResponse>().Result;

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.True(result.IsError);
        }
    }
}
