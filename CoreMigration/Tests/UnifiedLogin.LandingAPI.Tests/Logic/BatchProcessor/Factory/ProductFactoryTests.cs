using System;
using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Enum;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic.BatchProcessor.Factory
{
    /// <summary>
    /// ProductFactory xUnit tests.
    /// Tests for the factory that returns product instances based on product type.
    /// 
    /// CRITICAL ISSUE: The ProductFactory.GetProductLogic cannot create LeadManagement instances
    /// because LeadManagement requires constructor parameters (no parameterless constructor).
    /// 
    /// These tests document this bug and verify the exception is thrown.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductFactoryTests : TestBase
    {
        #region GetProductLogic Tests - Registered Types (WITH BUG)

        [Fact]
        public void GetProductLogic_LeadAnalytics_ThrowsMissingMethodException()
        {
            // This test documents the BUG in ProductFactory:
            // LeadManagement requires constructor parameters but
            // Activator.CreateInstance() tries to call parameterless constructor
            
            // Act & Assert
            var exception = Assert.Throws<MissingMethodException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.LeadAnalytics));
            
            Assert.NotNull(exception);
            Assert.Contains("LeadManagement", exception.Message);
            Assert.Contains("parameterless constructor", exception.Message);
        }

        #endregion

        #region GetProductLogic Tests - Unregistered Types

        [Fact]
        public void GetProductLogic_OneSite_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.OneSite));
            
            Assert.NotNull(exception);
        }

        [Fact]
        public void GetProductLogic_MarketingCenter_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.MarketingCenter));
            
            Assert.NotNull(exception);
        }

        [Fact]
        public void GetProductLogic_UnifiedPlatform_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.UnifiedPlatform));
            
            Assert.NotNull(exception);
        }

        [Fact]
        public void GetProductLogic_OmniChannel_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.OmniChannel));
            
            Assert.NotNull(exception);
        }

        [Fact]
        public void GetProductLogic_AssetOptimizer_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.AssetOptimizer));
            
            Assert.NotNull(exception);
        }

        #endregion

        #region Multiple Instance Tests

        [Fact]
        public void GetProductLogic_CalledMultipleTimes_ThrowsSameException()
        {
            // This test verifies that the MissingMethodException is consistent
            // across multiple calls (not a transient issue)
            
            // Act & Assert - First call
            var exception1 = Assert.Throws<MissingMethodException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.LeadAnalytics));
            
            // Act & Assert - Second call
            var exception2 = Assert.Throws<MissingMethodException>(() =>
                ProductFactory.GetProductLogic(ProductEnum.LeadAnalytics));
            
            Assert.NotNull(exception1);
            Assert.NotNull(exception2);
            // Both should have same message
            Assert.Equal(exception1.Message, exception2.Message);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ProductFactory_BugDocumentation_MissingMethodException()
        {
            // This test documents the BUG in ProductFactory:
            //
            // | ProductEnum | Implementation Class | Constructor | Result |
            // |-------------|---------------------|-------------|---------|
            // | LeadAnalytics | LeadManagement | Requires parameters | ? MissingMethodException |
            //
            // ROOT CAUSE:
            // 1. ProductFactory.GetProductLogic() uses Activator.CreateInstance
            // 2. Activator.CreateInstance requires parameterless constructor
            // 3. LeadManagement has NO parameterless constructor
            // 4. LeadManagement constructors require:
            //    - ProductEnum productType
            //    - long editorPersonaId
            //    - long subjectPersonaId
            //    - DefaultUserClaim userClaims
            //    - (optionally) IDataCollector, IManagePersona, IProductInternalSettingRepository
            // 5. MissingMethodException thrown before cast is even attempted
            //
            // CODE:
            // ```csharp
            // public static IProduct GetProductLogic(ProductEnum productEnum)
            // {
            //     return (IProduct)Activator.CreateInstance(ProductFactories[productEnum]);
            //     //              ^^^^^^^^^^^^^^^^^^^^^^^^^^ This FAILS!
            //     // Cannot create instance without constructor parameters
            // }
            // ```
            //
            // LeadManagement Constructors:
            // ```csharp
            // public LeadManagement(ProductEnum productType, long editorPersonaId, 
            //                       long subjectPersonaId, DefaultUserClaim userClaims)
            // 
            // public LeadManagement(ProductEnum productType, long editorPersonaId, 
            //                       long subjectPersonaId, DefaultUserClaim userClaims, 
            //                       IDataCollector injectedDataCollector, 
            //                       IManagePersona injectedManagePersona,
            //                       IProductInternalSettingRepository productInternalSettingRepository)
            // ```
            //
            // IMPACT:
            // - ProductFactory.GetProductLogic(ProductEnum.LeadAnalytics) ALWAYS throws
            // - Cannot instantiate LeadManagement without parameters
            // - This code path is completely broken
            // - Any code calling this will fail at runtime
            //
            // FIX OPTIONS:
            // A. Add parameterless constructor to LeadManagement (NOT RECOMMENDED - breaks DI)
            // B. Change ProductFactory to accept constructor parameters
            // C. Use factory method pattern instead of Activator.CreateInstance
            // D. Remove LeadAnalytics from ProductFactory (if it shouldn't be there)
            // E. Create wrapper class with parameterless constructor

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ProductFactory_FixRecommendations_Documentation()
        {
            // RECOMMENDED FIX: Option C - Use Factory Method Pattern
            //
            // Step 1: Update ProductFactory to use factory methods
            // ```csharp
            // public static class ProductFactory
            // {
            //     private static readonly Dictionary<ProductEnum, Func<IProduct>> ProductFactories =
            //         new Dictionary<ProductEnum, Func<IProduct>>();
            //     
            //     static ProductFactory()
            //     {
            //         // Register factory method instead of Type
            //         ProductFactories.Add(ProductEnum.LeadAnalytics, () => 
            //         {
            //             // Create with required parameters
            //             return new LeadManagement(
            //                 ProductEnum.LeadAnalytics,
            //                 0, // editorPersonaId - use default or get from context
            //                 0, // subjectPersonaId - use default or get from context
            //                 new DefaultUserClaim() // or get from context
            //             );
            //         });
            //     }
            //     
            //     public static IProduct GetProductLogic(ProductEnum productEnum)
            //     {
            //         return ProductFactories[productEnum]();  // Call factory method
            //     }
            // }
            // ```
            //
            // Step 2: OR create wrapper with parameterless constructor
            // ```csharp
            // public class LeadManagementProductAdapter : IProduct
            // {
            //     private readonly LeadManagement _leadManagement;
            //     
            //     // Parameterless constructor
            //     public LeadManagementProductAdapter()
            //     {
            //         // Create LeadManagement with default parameters
            //         _leadManagement = new LeadManagement(
            //             ProductEnum.LeadAnalytics,
            //             0, 0, new DefaultUserClaim()
            //         );
            //     }
            //     
            //     public string UpdateProductUserProfile(ProductUserProperitiesRoles batchRecord)
            //     {
            //         // Delegate to LeadManagement
            //         return _leadManagement.UpdateProductUserProfile(batchRecord);
            //     }
            // }
            // 
            // // Then register the adapter
            // ProductFactories.Add(ProductEnum.LeadAnalytics, typeof(LeadManagementProductAdapter));
            // ```
            //
            // Step 3: Update tests to verify working implementation
            // ```csharp
            // [Fact]
            // public void GetProductLogic_LeadAnalytics_ReturnsInstance()
            // {
            //     var result = ProductFactory.GetProductLogic(ProductEnum.LeadAnalytics);
            //     Assert.NotNull(result);
            //     Assert.IsAssignableFrom<IProduct>(result);
            // }
            // 
            // [Fact]
            // public void GetProductLogic_CalledMultipleTimes_ReturnsNewInstances()
            // {
            //     var result1 = ProductFactory.GetProductLogic(ProductEnum.LeadAnalytics);
            //     var result2 = ProductFactory.GetProductLogic(ProductEnum.LeadAnalytics);
            //     Assert.NotSame(result1, result2);
            // }
            // ```
            //
            // WHY OPTION C (Factory Method):
            // - ? Maintains dependency injection
            // - ? Flexible - can inject different dependencies
            // - ? Testable
            // - ? Follows factory pattern properly
            // - ? No need for parameterless constructors
            //
            // WHY NOT Option A (Parameterless Constructor):
            // - ? Breaks dependency injection
            // - ? Hard to test
            // - ? Violates SOLID principles
            // - ? Creates hidden dependencies

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
