using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Text;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test
{
    public class TestBase
    {
        protected readonly Mock<IRepository> mockRepository;
        protected readonly List<ProductInternalSetting> _upfmProductInternalSettings;

        protected readonly string _mockTiboWebHookSigningSecret = "1234567890";

        public TestBase()
        {
            mockRepository = new Mock<IRepository>();

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
                new ProductInternalSetting() { Name = "ExcludeProductFromOrgSupportUser", Value = "3,4,8,14,28,36,56" },
                new ProductInternalSetting() { Name = "PlatformAdminRole", Value = "Platform Administrator" }
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
