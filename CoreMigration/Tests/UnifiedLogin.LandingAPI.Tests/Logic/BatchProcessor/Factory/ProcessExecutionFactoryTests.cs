using System;
using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory;
using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Process;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Enum;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic.BatchProcessor.Factory
{
    /// <summary>
    /// ProcessExecutionFactory xUnit tests.
    /// Tests for the factory that returns process execution instances based on batch process type.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProcessExecutionFactoryTests : TestBase
    {
        #region GetProductLogic Tests - All Registered Types

        [Fact]
        public void GetProductLogic_CreateUpdateProductUser_ReturnsCreateUpdateProductUser()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.CreateUpdateProductUser);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CreateUpdateProductUser>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_ProfileUpdate_ReturnsUpdateProductUserProfile()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.ProfileUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UpdateProductUserProfile>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_DeactivateProductUser_ReturnsDeactivateProductUser()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.DeactivateProductUser);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DeactivateProductUser>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_UserTypeAdminToRegular_ReturnsChangeProductUserType()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.UserTypeAdminToRegular);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ChangeProductUserType>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_UserTypeRegularToAdmin_ReturnsChangeProductUserType()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.UserTypeRegularToAdmin);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ChangeProductUserType>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_UserTypeAdminToExternal_ReturnsChangeProductUserType()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.UserTypeAdminToExternal);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ChangeProductUserType>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_UserTypeExternalToAdmin_ReturnsChangeProductUserType()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.UserTypeExternalToAdmin);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ChangeProductUserType>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_EnterpriseRoleCreateUpdateProductUser_ReturnsEnterpriseCreateUpdateProductUser()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.EnterpriseRoleCreateUpdateProductUser);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<EnterpriseCreateUpdateProductUser>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        [Fact]
        public void GetProductLogic_PrimaryPropertiesUpdateProductUser_ReturnsCreateUpdateProductUser()
        {
            // Act
            var result = ProcessExecutionFactory.GetProductLogic(BatchProcessType.PrimaryPropertiesUpdateProductUser);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CreateUpdateProductUser>(result);
            Assert.IsAssignableFrom<IProcessExecution>(result);
        }

        #endregion

        #region GetProductLogic Tests - Unregistered Types

        [Fact]
        public void GetProductLogic_ActivateProductUser_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProcessExecutionFactory.GetProductLogic(BatchProcessType.ActivateProductUser));
        }

        [Fact]
        public void GetProductLogic_UnassignUser_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProcessExecutionFactory.GetProductLogic(BatchProcessType.UnassignUser));
        }

        [Fact]
        public void GetProductLogic_BulkAddUpdateEnterpriseRole_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProcessExecutionFactory.GetProductLogic(BatchProcessType.BulkAddUpdateEnterpriseRole));
        }

        [Fact]
        public void GetProductLogic_AssignOrUnasignProductsForBulkUsers_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                ProcessExecutionFactory.GetProductLogic(BatchProcessType.AssignOrUnasignProductsForBulkUsers));
        }

        #endregion

        #region Multiple Instance Tests

        [Fact]
        public void GetProductLogic_CalledMultipleTimes_ReturnsNewInstances()
        {
            // Act
            var result1 = ProcessExecutionFactory.GetProductLogic(BatchProcessType.CreateUpdateProductUser);
            var result2 = ProcessExecutionFactory.GetProductLogic(BatchProcessType.CreateUpdateProductUser);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2); // Activator.CreateInstance creates new instances
        }

        [Fact]
        public void GetProductLogic_DifferentProcessTypes_ReturnsDifferentTypes()
        {
            // Act
            var createUpdate = ProcessExecutionFactory.GetProductLogic(BatchProcessType.CreateUpdateProductUser);
            var profileUpdate = ProcessExecutionFactory.GetProductLogic(BatchProcessType.ProfileUpdate);
            var deactivate = ProcessExecutionFactory.GetProductLogic(BatchProcessType.DeactivateProductUser);
            var changeType = ProcessExecutionFactory.GetProductLogic(BatchProcessType.UserTypeAdminToRegular);

            // Assert
            Assert.IsType<CreateUpdateProductUser>(createUpdate);
            Assert.IsType<UpdateProductUserProfile>(profileUpdate);
            Assert.IsType<DeactivateProductUser>(deactivate);
            Assert.IsType<ChangeProductUserType>(changeType);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ProcessExecutionFactory_RegisteredTypes_Documentation()
        {
            // This test documents all registered process types in the factory:
            //
            // | BatchProcessType | Implementation Class |
            // |------------------|---------------------|
            // | CreateUpdateProductUser | CreateUpdateProductUser |
            // | ProfileUpdate | UpdateProductUserProfile |
            // | DeactivateProductUser | DeactivateProductUser |
            // | UserTypeAdminToRegular | ChangeProductUserType |
            // | UserTypeRegularToAdmin | ChangeProductUserType |
            // | UserTypeAdminToExternal | ChangeProductUserType |
            // | UserTypeExternalToAdmin | ChangeProductUserType |
            // | EnterpriseRoleCreateUpdateProductUser | EnterpriseCreateUpdateProductUser |
            // | PrimaryPropertiesUpdateProductUser | CreateUpdateProductUser |
            //
            // Unregistered types (will throw KeyNotFoundException):
            // - ActivateProductUser
            // - UnassignUser
            // - BulkAddUpdateEnterpriseRole
            // - AssignOrUnasignProductsForBulkUsers

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
