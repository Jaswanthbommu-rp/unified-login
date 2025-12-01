using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Types;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Test.Logic.ProductIntegration.Factory
{
    public class DefaultIntegrationTypeFactoryTests
    {
        private IIntegrationTypeFactory MakeInstance(
            Mock<IManageProduct> manageProductMock = null,
            Mock<IManageUnifiedLogin> manageUnifiedLoginMock = null,
            Mock<IManageProductOneSite> manageProductOneSiteMock = null,
            Mock<IProductRepository> productRepositoryMock = null,
            Mock<IProductInternalSettingRepository> productInternalSettingRepositoryMock = null,
            DefaultUserClaim defaultUserClaim = null)
        {
            ArgumentNullException.ThrowIfNull(manageProductOneSiteMock);
            manageProductMock = manageProductMock ?? new Mock<IManageProduct>();
            manageUnifiedLoginMock = manageUnifiedLoginMock ?? new Mock<IManageUnifiedLogin>();
            manageProductOneSiteMock = manageProductOneSiteMock ?? new Mock<IManageProductOneSite>();
            productRepositoryMock = productRepositoryMock ?? new Mock<IProductRepository>();
            productInternalSettingRepositoryMock = productInternalSettingRepositoryMock ?? new Mock<IProductInternalSettingRepository>();

            defaultUserClaim = defaultUserClaim ?? new DefaultUserClaim()
            {
                CorrelationId = new Guid()
            };

            return new IntegrationTypeFactory(manageProductMock.Object, manageUnifiedLoginMock.Object, manageProductOneSiteMock.Object, productRepositoryMock.Object,
                productInternalSettingRepositoryMock.Object, defaultUserClaim);
        }

        [Fact]
        public void GetIntegration_GeneratesUPFMInstance()
        {
            var productId = 1;

            var manageProductMock = new Mock<IManageProduct>();
            manageProductMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType", null)).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "UPFM"
                }
            });

            var instance = MakeInstance(manageProductMock: manageProductMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<UPFMIntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_GeneratesLegacyInstance()
        {
            var productId = 1;

            var manageProductMock = new Mock<IManageProduct>();
            manageProductMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType", null)).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "Legacy"
                }
            });

            var instance = MakeInstance(manageProductMock: manageProductMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<LegacyIntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_GeneratesStandardV1Instance()
        {
            var productId = 1;

            var manageProductMock = new Mock<IManageProduct>();
            manageProductMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType", null)).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "Standard v1"
                }
            });

            var instance = MakeInstance(manageProductMock: manageProductMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<StandardV1IntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_CaseInsensitiveTypeValue()
        {
            var productId = 1;

            var manageProductMock = new Mock<IManageProduct>();
            manageProductMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType",null)).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "uPfM"
                }
            });

            var instance = MakeInstance(manageProductMock: manageProductMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<UPFMIntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_InvalidTypeValueDefaultsToLegacy()
        {
            var productId = 1;

            var manageProductMock = new Mock<IManageProduct>();
            manageProductMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType",null)).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "invalid"
                }
            });

            var instance = MakeInstance(manageProductMock: manageProductMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<LegacyIntegrationType>(result);
        }
    }
}