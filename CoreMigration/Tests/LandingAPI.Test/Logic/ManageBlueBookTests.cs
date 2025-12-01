using Moq;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using UnifiedLogin.SharedObjects.Base;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
	public class ManageBlueBookTests : TestBase
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
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseTranslatev2", Value = "0"}
            };

            Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

			long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;

            _manageBlueBook = new ManageBlueBook(_userClaims, mockRepository.Object, productInternalSettingRepository, _mockHttpMessageHandler.Object);

            new RPObjectCache().BustCache();

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

            Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange
            long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;
            string productSource = "OS";
            string domain = "Primary";

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/translate/v2/companyinstance/{organizationList[0].RealPageId}/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}?filter[greenbookCares]=true", responseMapResource);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

            IManageBlueBook _manageBlueBook = new ManageBlueBook(_userClaims, mockRepository.Object, productInternalSettingRepository, _mockHttpMessageHandler.Object);

            new RPObjectCache().BustCache();

            //Act
            var result = _manageBlueBook.GetCompanyMap(organizationList[0].RealPageId, 0, ProductEnum.OneSite.ToEnumDescription(), domain);

            //Assert
            Assert.True(result.Count == 1);
            Assert.Equal(ProductEnum.OneSite.ToEnumDescription(), result[0].Source);
            Assert.Equal("1051412", result[0].CompanyInstanceSourceId);
        }

        [Fact]
        public void GetUPFMPropertyInstances_Valid()
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
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseTranslatev2", Value = "1"}
            };

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(translate);
            responseMapResource.Content = new StringContent(jsonToSave);

            List<PropertyInstance> propertyInstances = new List<PropertyInstance>()
            {
                new PropertyInstance() {PropertyInstanceSourceId = "cb1f5a51-56cc-415c-9d8e-3d5e3f0f8b68"},
                new PropertyInstance() {PropertyInstanceSourceId = "b6f475fc-7408-424b-a749-129035dcf57b"},
                new PropertyInstance() {PropertyInstanceSourceId = "a61481fc-5779-4546-8d5a-b29ecf139095"},
                new PropertyInstance() {PropertyInstanceSourceId = "d0ab0e33-4c04-4028-97f8-cda5a8423a30"},
            };

            UPFMPropertyInstanceRootObject propertyInstanceRoot = new UPFMPropertyInstanceRootObject(){ data = new List<UPFMPropertyInstanceData>() { new UPFMPropertyInstanceData() { attributes = new UPFMPropertyInstanceAttributes() { propertyInstance = propertyInstances }}}};
            
            jsonToSave = JsonConvert.SerializeObject(propertyInstanceRoot);
            HttpResponseMessage responsePropertyInstance = new HttpResponseMessage(HttpStatusCode.OK);
            responsePropertyInstance.Content = new StringContent(jsonToSave);

            Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange
            long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;
            string productSource = "OS";
            string domain = "Primary";

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/translate/v2/companyinstance/{organizationList[0].RealPageId}/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}?filter[greenbookCares]=true", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]={organizationList[0].RealPageId}&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true&page[size]=9999", responsePropertyInstance);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

            IManageBlueBook _manageBlueBook = new ManageBlueBook(_userClaims, mockRepository.Object, productInternalSettingRepository, _mockHttpMessageHandler.Object);

            new RPObjectCache().BustCache();

            //Act
            var result = _manageBlueBook.GetCompanyMap(organizationList[0].RealPageId, 0, ProductEnum.OneSite.ToEnumDescription(), domain);

            //Assert
            Assert.True(result.Count == 1);
            Assert.Equal(ProductEnum.OneSite.ToEnumDescription(), result[0].Source);
            Assert.Equal("1051412", result[0].CompanyInstanceSourceId);

            var propertyListResult = _manageBlueBook.GetUPFMPropertyInstances(organizationList[0].RealPageId.ToString());

            Assert.True(propertyListResult.Count == 4);

        }

        [Fact]
        public void GetUPFMOperators_Valid()
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

            UDMOperatorsRootObject operatorsRoot = new UDMOperatorsRootObject()
            {
                Data = new UDMOperatorsDataObject()
                {
                    type = "Operators",
                    attributes = new UDMOperatorsAttributesObject()
                    {
                        booksOperators = new List<UDMOperators>()
                        {
                            new UDMOperators()
                            {
                                Origin = new UDMOperatorsDetails()
                                {
                                    Source = "AO", CompanyName = "AO Company", CompanyInstanceSourceId = "1234"
                                },
                                Translations = new List<UDMOperatorsDetails>()
                                {
                                    new UDMOperatorsDetails()
                                    {
                                        Source = "UPFM", CompanyName = "UPFM Operator Company", CompanyInstanceSourceId = "cb1f5a51-56cc-415c-9d8e-3d5e3f0f8b68"
                                    }
                                }
                            },
                            new UDMOperators()
                            {
                                Origin = new UDMOperatorsDetails()
                                {
                                    Source = "AO", CompanyName = "AO Company 2", CompanyInstanceSourceId = "5432"
                                },
                                Translations = new List<UDMOperatorsDetails>()
                                {
                                    new UDMOperatorsDetails()
                                    {
                                        Source = "UPFM", CompanyName = "UPFM Operator Company 2", CompanyInstanceSourceId = "d0ab0e33-4c04-4028-97f8-cda5a8423a30"
                                    }
                                }
                            },
                            new UDMOperators()
                            {
                                Origin = new UDMOperatorsDetails()
                                {
                                    Source = "AO", CompanyName = "AO Company 3 no translate", CompanyInstanceSourceId = "6666"
                                }
                            }
                        }
                    }
                }
            };

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseTranslatev2", Value = "1"}
            };

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(operatorsRoot);
            responseMapResource.Content = new StringContent(jsonToSave);

            Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange
            long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;
            string productSource = "OS";
            string domain = "Primary";

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/operators/{organizationList[0].RealPageId}/UPFM", responseMapResource);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 1))))
                .Returns(productInternalSettings);

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

            IManageBlueBook _manageBlueBook = new ManageBlueBook(_userClaims, mockRepository.Object, productInternalSettingRepository, _mockHttpMessageHandler.Object);

            new RPObjectCache().BustCache();

            //Act
            var result = _manageBlueBook.GetOperatorListForUPFMCompany(organizationList[0].RealPageId, "UPFM");

            //Assert
            Assert.True(result.Count() == 2);

        }

        [Fact]
        public void GetUPFMOperators_NotFound()
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
            
            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseTranslatev2", Value = "1"}
            };

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.NotFound);

            Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange
            long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;
            string productSource = "OS";
            string domain = "Primary";

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/operators/{organizationList[0].RealPageId}/UPFM", responseMapResource);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

            IManageBlueBook _manageBlueBook = new ManageBlueBook(_userClaims, mockRepository.Object, productInternalSettingRepository, _mockHttpMessageHandler.Object);

            new RPObjectCache().BustCache();

            //Act
            var result = _manageBlueBook.GetOperatorListForUPFMCompany(organizationList[0].RealPageId, "UPFM");

            //Assert
            Assert.True(!result.Any());

        }

        public bool TestIsProductId(object obj, int productId)
        {
            return obj.ToString().Contains($"ProductId = {productId}");
        }
    }
}
