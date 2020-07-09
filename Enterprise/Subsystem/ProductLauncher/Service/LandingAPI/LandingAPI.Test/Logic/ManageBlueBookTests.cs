using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using JsonApiSerializer;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using Xunit;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
	public class ManageBlueBookTests
	{
        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static Guid _adminRealPageId = new Guid("C802694D-1111-2222-3333-3C0F434AE62D");
        private static Guid _invalidRealPageId = new Guid("11111111-1111-2222-3333-3C0F434AE62D");
        private static string _CompanyName = "CF Real Estate Services";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 2116;
        private static long _BooksCompanyMasterId = 379;
        private static int _organizationTypeId = 6;
        private static int _organizationDomainId = 1;


		[Fact]
		public void GetCustomerProperty_InvalidbooksCompanyMasterId_ExceptionThrown()
        {
            IManageBlueBook _manageBlueBook;

			DefaultUserClaim _userClaims = new DefaultUserClaim()
			{
				LoginName = "MocTest",
				CorrelationId = Guid.NewGuid(),
				OrganizationName = "MocTest",
				OrganizationPartyId = 1,
				OrganizationRealPageGuid = Guid.NewGuid(),
				OrganizationMasterId = 1,
				UserRealPageGuid = Guid.NewGuid(),
				PersonaId = 33
			};

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"}, 
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
            };

            Mock<IRepository> _mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(_mockRepository.Object);

			long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;

            _manageBlueBook = new ManageBlueBook(_userClaims, productInternalSettingRepository, _mockHttpMessageHandler.Object);

            //Act
            Exception exception = Record.Exception(() => _manageBlueBook.GetCustomerProperty(booksCompanyMasterId, include, filter));

			//Assert
			Assert.IsType<Exception>(exception);
			Assert.Equal("Invalid parameter booksCompanyMasterId.", exception.Message);
		}
        
        [Fact]
        public void GetCustomer_Valid()
        {
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

            DefaultUserClaim _userClaims = new DefaultUserClaim()
            {
                LoginName = "MocTest",
                CorrelationId = Guid.NewGuid(),
                OrganizationName = "MocTest",
                OrganizationPartyId = 1,
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationMasterId = 1,
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = 33
            };
            
            TranslateCompanyInstance translate = new TranslateCompanyInstance()
            {
                Data = new TranslateCompanyInstanceData()
                {
                    Type = "companyinstanceids",
                    Attributes = new TranslateCompanyInstanceAttributes()
                    {
                        Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform), 
                        CompanyInstanceSourceId = organizationList[0].RealPageId.ToString(),
                        TranslatedCompanyInstances = new List<TranslatedCompanyInstanceData>()
                        {
                            new TranslatedCompanyInstanceData()
                            {
                                Source = ProductEnumHelper.StringValueOf(ProductEnum.OneSite),
                                CompanyInstanceSourceId = "1051412",
                                CustomerEnvironment = "Primary",
                                Domain = "Primary"
                            }
                        }
                    }
                }
            };

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"}, 
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
            };

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(translate);
            responseMapResource.Content = new StringContent(jsonToSave);

            Mock<IRepository> _mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange
            long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;
            string productSource = "OS";
            string domain = "Primary";

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/translate/companyinstance/{organizationList[0].RealPageId}/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}?filter[customerEnvironment]={domain}", responseMapResource);

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(_mockRepository.Object);

            IManageBlueBook _manageBlueBook = new ManageBlueBook(_userClaims, productInternalSettingRepository, _mockHttpMessageHandler.Object);

            //Act
            var result = _manageBlueBook.GetCompanyMap(organizationList[0].RealPageId, 0, ProductEnum.OneSite.ToEnumDescription(), domain);

            //Assert
            Assert.True(result.Count == 1);
            Assert.Equal(ProductEnum.OneSite.ToEnumDescription(), result[0].Source);
            Assert.Equal("1051412", result[0].CompanyInstanceSourceId);
        }
        
    }
}
