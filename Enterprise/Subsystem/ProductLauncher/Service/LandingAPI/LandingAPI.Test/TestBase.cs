using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Text;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Net.Http;
using LaunchDarkly.Sdk.Server;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test
{
    public class TestBase
    {
        protected readonly Mock<IRepository> mockRepository;
        protected readonly List<ProductInternalSetting> _upfmProductInternalSettings;

        protected readonly string _mockTiboWebHookSigningSecret = "1234567890";
        protected long _editorPersonaId = 4;
        protected int _editorUserId = 14;
        protected Guid _editorRealPageId = new Guid("523C6677-C20D-4E6A-A4CC-0DE5781F0D5C");
        protected int _editorOrganizationPartyId = 1234;
        private Guid _editorOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
        private Guid _editorCorrelationId = new Guid("8C5F223C-169A-44BD-9844-F925B5F0C332");
        protected DefaultUserClaim _editorUserClaim;
        protected Persona _editorPersona;
        protected Mock<HttpMessageHandler> mockHttpMessageHandler;
        protected HttpClient client;
        protected long _userPersonaId = 5;
        protected int _userUserId = 15;
        protected Guid _userRealPageId = new Guid("623C6677-D20D-5E6A-B4CC-1DE5781F0D5C");
        protected Guid _userOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
        protected int _userOrganizationPartyId = 1234;
        private Guid _userCorrelationId = new Guid("078724B2-D381-4E45-9EE9-6DD6D9B9B74B");
        protected DefaultUserClaim _userUserClaim;

        public TestBase()
        {
            mockRepository = new Mock<IRepository>();
            _editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId, OrganizationPartyId = _editorOrganizationPartyId, UserId = _editorUserId };
            _editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId, RealPageId = _editorOrganizationRealPageId, Name = "RealPage", BooksMasterId = 1234, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" } };
            mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            client = new HttpClient(mockHttpMessageHandler.Object, false);
            _upfmProductInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "SettingsApiEndPoint", Value = "http://localhost" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientName", Value = "unifiedlogin-server" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientSecret", Value = "abcdefgh" },
                new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
                new ProductInternalSetting() { Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret },
                new ProductInternalSetting() { Name = "IsCloneUsersProcessEnabledForHOTS", Value = "1" },
                new ProductInternalSetting() { Name = "ExcludeProductFromOrgSupportUser", Value = "3,4,8,14,28,36,56" }
            };


            // default UPFM product settings
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.UnifiedPlatform))))
                .Returns(_upfmProductInternalSettings);

            // OneSite default settings
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.OneSite))))
                .Returns(new List<ProductInternalSetting>()
                {
                    new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                    new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                    new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                    new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
                });

            // Insurance default settings
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.AssetOptimizer))))
                .Returns(new List<ProductInternalSetting>()
                {
                    new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                    new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                    new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                    new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
                });

            // Insurance default settings
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.ResidentPortal))))
                .Returns(new List<ProductInternalSetting>()
                {
                    new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                    new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                    new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                    new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
                });

            // Insurance default settings
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.Insurance))))
                .Returns(new List<ProductInternalSetting>()
                {
                    new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                    new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                    new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                    new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
                });

            // HOTS
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.HandsOnTrainingSystem))))
                .Returns(new List<ProductInternalSetting>()
                {
                    new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                    new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                    new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                    new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
                });

            // Vendor
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.VendorMarketplace))))
                .Returns(new List<ProductInternalSetting>()
                {
                    new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                    new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                    new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                    new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
                });

        }

        public string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
        public bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool TestIs(string propertyName, object obj, Guid? realPageId)
        {
            if (obj == null && realPageId == null)
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            return obj.ToString().ToLower().Contains($"{propertyName} = {realPageId}");
        }

        public bool TestIsProductId(object obj, int productId)
        {
            return obj.ToString().ToLower().Contains($"productid = {productId}");
        }
    }
}
