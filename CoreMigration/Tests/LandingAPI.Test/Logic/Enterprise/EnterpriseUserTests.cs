using JsonApiSerializer;
using Moq;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.SharedObjects.Product.OneSite;
using Xunit;
using UserController = UnifiedLogin.LandingAPIEnterprise.Controllers.UserController;

namespace UnifiedLogin.LandingAPI.Test.Logic.Enterprise
{
    [ExcludeFromCodeCoverage]
    public class EnterpriseUserTests : EnterpriseBase
    {
        #region Constructor
        public EnterpriseUserTests()
        {
        }
        #endregion

        private UserController MakeInstance(
            Mock<IRepository> repository = null,
            Mock<HttpMessageHandler> httpMessageHandler = null,
            Mock<IOneSiteProductService> onesiteProductService = null)
        {
            repository = repository != null ? repository : new Mock<IRepository>();
            httpMessageHandler = httpMessageHandler ?? new Mock<HttpMessageHandler>();
            onesiteProductService = onesiteProductService ?? new Mock<IOneSiteProductService>();

            var userClaim = new DefaultUserClaim
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationPartyId = 1234,
                PersonaId = _multifamilyUserPersonaId,
                UserRealPageGuid = _vendorUserRealpageId
            };

            Mock<IUnitOfWork> mockUnitofWork = new Mock<IUnitOfWork>();
            repository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitofWork.Object);

            repository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            repository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            repository.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypes);

            repository.Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            return new UserController(
                repository.Object,
                httpMessageHandler.Object,
                userClaim,
                onesiteProductService.Object
            )
            { Request = new HttpRequestMessage(), Configuration = new HttpClient() };
        }

        //[Fact(Skip = "Issues with route tests")]
        [Fact]
        public void GetUserProductsByPersonaId_ValidPersonaId_ReturnProductList()
        {
            //Arrange
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceId = 54321,
                    CompanyInstanceSourceId = _multifamilyCompanyRealPageId.ToString(), Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                    CompanyInstance = new List<CompanyInstance>()
                    {
                        new CompanyInstance() {CustomerEnvironment = "Primary", IsActive = true}
                    }
                }
            };

            Persona persona = new Persona()
            {
                PersonaId = _multifamilyUserPersonaId,
                RealPageId = _multifamilyCompanyRealPageId,
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

            mockRepository
                .Setup(m => m.GetMany<PersonaProduct>(StoredProcNameConstants.SP_GetProductsByPersonaId, It.Is<object>(
                    d => d.ToString().Contains($"PersonaId = {persona.PersonaId}, StatusTypeId = 8"))))
                .Returns(productList);
            
            var userController = MakeInstance(repository: mockRepository, httpMessageHandler: mockHttpMessageHandler);

            //Act
            HttpResponseMessage response = userController.GetUserProductsByPersonaId(_multifamilyUserPersonaId);
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
