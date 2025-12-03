using Moq;
using System;
using System.Collections.Generic;
using System.Text;
// TODO: Update these using statements once CoreMigration project references are available
// using UnifiedLogin.DataAccess;
// using UnifiedLogin.SharedObjects;
// using UnifiedLogin.SharedObjects.Enums;

namespace UnifiedLogin.LandingAPI.Tests.Helpers
{
    /// <summary>
    /// Base class for test classes providing common test infrastructure and mock setups.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.TestBase
    /// </summary>
    public class TestBase
    {
        // TODO: Uncomment once IRepository interface is available in CoreMigration
        // protected readonly Mock<IRepository> mockRepository;

        // TODO: Replace with actual types once SharedObjects are migrated
        // protected readonly List<ProductInternalSetting> _upfmProductInternalSettings;

        protected readonly string _mockTiboWebHookSigningSecret = "1234567890";

        public TestBase()
        {
            // TODO: Uncomment and update once CoreMigration dependencies are available
            // mockRepository = new Mock<IRepository>();

            // _upfmProductInternalSettings = new List<ProductInternalSetting>()
            // {
            //     new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
            //     new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
            //     new ProductInternalSetting() { Name = "SettingsApiEndPoint", Value = "http://localhost" },
            //     new ProductInternalSetting() { Name = "UnifiedLoginServerClientName", Value = "unifiedlogin-server" },
            //     new ProductInternalSetting() { Name = "UnifiedLoginServerClientSecret", Value = "abcdefgh" },
            //     new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
            //     new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
            //     new ProductInternalSetting() { Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret },
            //     new ProductInternalSetting() { Name = "IsCloneUsersProcessEnabledForHOTS", Value = "1" },
            //     new ProductInternalSetting() { Name = "ExcludeProductFromOrgSupportUser", Value = "3,4,8,14,28,36,56" },
            //     new ProductInternalSetting() { Name = "PlatformAdminRole", Value = "Platform Administrator" }
            // };

            // TODO: Setup mock repository for various products
            // SetupProductMocks();
        }

        /// <summary>
        /// Encodes a string value to Base64.
        /// </summary>
        /// <param name="value">The string to encode</param>
        /// <returns>Base64 encoded string</returns>
        public string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Tests if a SQL parameter matches the expected value.
        /// </summary>
        /// <param name="p">The parameter object</param>
        /// <param name="value">The expected value</param>
        /// <returns>True if the parameter matches the value</returns>
        public bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Tests if an object contains a specific property-value pair.
        /// </summary>
        /// <param name="propertyName">The property name to check</param>
        /// <param name="obj">The object to inspect</param>
        /// <param name="realPageId">The expected RealPageId value</param>
        /// <returns>True if the object contains the expected property-value pair</returns>
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

        /// <summary>
        /// Tests if an object contains a ProductId property with the specified value.
        /// </summary>
        /// <param name="obj">The object to inspect</param>
        /// <param name="productId">The expected ProductId value</param>
        /// <returns>True if the object contains the expected ProductId</returns>
        public bool TestIsProductId(object obj, int productId)
        {
            return obj.ToString().ToLower().Contains($"productid = {productId}");
        }

        // TODO: Add this method once ProductInternalSetting and ProductEnum are available
        // /// <summary>
        // /// Sets up mock repository responses for various products.
        // /// </summary>
        // private void SetupProductMocks()
        // {
        //     // UPFM product settings
        //     mockRepository
        //         .Setup(m => m.GetMany<ProductInternalSetting>(
        //             StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
        //             It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.UnifiedPlatform))))
        //         .Returns(_upfmProductInternalSettings);
        //
        //     // OneSite default settings
        //     mockRepository
        //         .Setup(m => m.GetMany<ProductInternalSetting>(
        //             StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
        //             It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.OneSite))))
        //         .Returns(GetDefaultProductSettings());
        //
        //     // Add similar setups for other products...
        // }

        // TODO: Add this method once ProductInternalSetting is available
        // private List<ProductInternalSetting> GetDefaultProductSettings()
        // {
        //     return new List<ProductInternalSetting>()
        //     {
        //         new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
        //         new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
        //         new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
        //         new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
        //     };
        // }
    }
}
