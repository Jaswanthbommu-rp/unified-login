using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ProductIntegration.Factory
{
    public class DefaultIntegrationTypeFactoryTests
    {
        private IIntegrationTypeFactory MakeInstance(
            Mock<IProductInternalSettingRepository> productInternalSettingRepositoryMock = null,
            Mock<IManageUnifiedLogin> manageUnifiedLoginMock = null,
            Mock<IManageProductOneSite> manageProductOneSiteMock = null,
            DefaultUserClaim defaultUserClaim = null)
        {
            productInternalSettingRepositoryMock = productInternalSettingRepositoryMock ?? new Mock<IProductInternalSettingRepository>();
            manageUnifiedLoginMock = manageUnifiedLoginMock ?? new Mock<IManageUnifiedLogin>();
            manageProductOneSiteMock = manageProductOneSiteMock ?? new Mock<IManageProductOneSite>();

            defaultUserClaim = defaultUserClaim ?? new DefaultUserClaim()
            {
                CorrelationId = new Guid()
            };

            return new DefaultIntegrationTypeFactory(productInternalSettingRepositoryMock.Object, manageUnifiedLoginMock.Object, manageProductOneSiteMock.Object, defaultUserClaim);
        }

        [Fact]
        public void GetIntegration_GeneratesUPFMInstance()
        {
            var productId = 1;

            var productInternalSettingRepositoryMock = new Mock<IProductInternalSettingRepository>();
            productInternalSettingRepositoryMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType")).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "UPFM"
                }
            });

            var instance = MakeInstance(productInternalSettingRepositoryMock: productInternalSettingRepositoryMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<UPFMIntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_GeneratesLegacyInstance()
        {
            var productId = 1;

            var productInternalSettingRepositoryMock = new Mock<IProductInternalSettingRepository>();
            productInternalSettingRepositoryMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType")).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "Legacy"
                }
            });

            var instance = MakeInstance(productInternalSettingRepositoryMock: productInternalSettingRepositoryMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<LegacyIntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_GeneratesStandardV1Instance()
        {
            var productId = 1;

            var productInternalSettingRepositoryMock = new Mock<IProductInternalSettingRepository>();
            productInternalSettingRepositoryMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType")).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "Standard v1"
                }
            });

            var instance = MakeInstance(productInternalSettingRepositoryMock: productInternalSettingRepositoryMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<StandardV1IntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_CaseInsensitiveTypeValue()
        {
            var productId = 1;

            var productInternalSettingRepositoryMock = new Mock<IProductInternalSettingRepository>();
            productInternalSettingRepositoryMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType")).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "uPfM"
                }
            });

            var instance = MakeInstance(productInternalSettingRepositoryMock: productInternalSettingRepositoryMock);

            var result = instance.GetIntegration(productId);

            Assert.NotNull(result);
            Assert.IsType<UPFMIntegrationType>(result);
        }

        [Fact]
        public void GetIntegration_InvalidTypeValue()
        {
            var productId = 1;

            var productInternalSettingRepositoryMock = new Mock<IProductInternalSettingRepository>();
            productInternalSettingRepositoryMock.Setup(s => s.GetProductSettingByType("ProductIntegrationType")).Returns(new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType()
                {
                    Name = "ProductIntegrationType",
                    ProductId = productId,
                    Value = "invalid"
                }
            });

            var instance = MakeInstance(productInternalSettingRepositoryMock: productInternalSettingRepositoryMock);

            var result = instance.GetIntegration(productId);

            Assert.Null(result);
        }
    }
}
