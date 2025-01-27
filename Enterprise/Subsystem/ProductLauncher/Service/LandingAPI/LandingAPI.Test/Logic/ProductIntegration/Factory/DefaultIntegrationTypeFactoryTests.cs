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
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ProductIntegration.Factory
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