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

        #region Constructor
        public EnterpriseUserTests()
        {
            _defaultUserClaim.CorrelationId = new Guid();
            _defaultUserClaim.CustomerMasterId = _BooksCompanyMasterId;
            _defaultUserClaim.OrganizationPartyId = 1234;

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
