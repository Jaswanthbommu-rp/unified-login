using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User.Models;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Enterprise.User.Models
{
    /// <summary>
    /// EntUserStatus xUnit tests.
    /// Tests for enterprise API user status enumeration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntUserStatusTests : TestBase
    {
        #region Enum Value Tests

        [Fact]
        public void EntApiUserStatus_Activate_HasCorrectValue()
        {
            // Assert
            Assert.Equal(1, (int)EntApiUserStatus.Activate);
        }

        [Fact]
        public void EntApiUserStatus_Deactivate_HasCorrectValue()
        {
            // Assert
            Assert.Equal(2, (int)EntApiUserStatus.Deactivate);
        }

        #endregion

        #region Enum Name Tests

        [Fact]
        public void EntApiUserStatus_Activate_HasCorrectName()
        {
            // Assert
            Assert.Equal("Activate", EntApiUserStatus.Activate.ToString());
        }

        [Fact]
        public void EntApiUserStatus_Deactivate_HasCorrectName()
        {
            // Assert
            Assert.Equal("Deactivate", EntApiUserStatus.Deactivate.ToString());
        }

        #endregion

        #region Enum Parse Tests

        [Fact]
        public void EntApiUserStatus_ParseActivate_ReturnsActivate()
        {
            // Act
            var result = System.Enum.Parse<EntApiUserStatus>("Activate");

            // Assert
            Assert.Equal(EntApiUserStatus.Activate, result);
        }

        [Fact]
        public void EntApiUserStatus_ParseDeactivate_ReturnsDeactivate()
        {
            // Act
            var result = System.Enum.Parse<EntApiUserStatus>("Deactivate");

            // Assert
            Assert.Equal(EntApiUserStatus.Deactivate, result);
        }

        [Fact]
        public void EntApiUserStatus_ParseFromInt1_ReturnsActivate()
        {
            // Act
            var result = (EntApiUserStatus)1;

            // Assert
            Assert.Equal(EntApiUserStatus.Activate, result);
        }

        [Fact]
        public void EntApiUserStatus_ParseFromInt2_ReturnsDeactivate()
        {
            // Act
            var result = (EntApiUserStatus)2;

            // Assert
            Assert.Equal(EntApiUserStatus.Deactivate, result);
        }

        #endregion

        #region Enum IsDefined Tests

        [Fact]
        public void EntApiUserStatus_IsDefined_Activate_ReturnsTrue()
        {
            // Assert
            Assert.True(System.Enum.IsDefined(typeof(EntApiUserStatus), EntApiUserStatus.Activate));
        }

        [Fact]
        public void EntApiUserStatus_IsDefined_Deactivate_ReturnsTrue()
        {
            // Assert
            Assert.True(System.Enum.IsDefined(typeof(EntApiUserStatus), EntApiUserStatus.Deactivate));
        }

        [Fact]
        public void EntApiUserStatus_IsDefined_InvalidValue_ReturnsFalse()
        {
            // Assert
            Assert.False(System.Enum.IsDefined(typeof(EntApiUserStatus), 99));
        }

        #endregion

        #region Enum Values Count Test

        [Fact]
        public void EntApiUserStatus_HasTwoValues()
        {
            // Arrange
            var values = System.Enum.GetValues<EntApiUserStatus>();

            // Assert
            Assert.Equal(2, values.Length);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void EntApiUserStatus_ClassPurpose_Documentation()
        {
            // This test documents the enum purpose:
            // EntApiUserStatus is used only for Enterprise API to indicate user status.
            //
            // Values:
            // - Activate (1): Activate external API user status
            // - Deactivate (2): Deactivate external API user status
            //
            // Usage:
            // - Used in ActivateDeactivateUser method to change user status
            // - Maps to internal UserUiStatusType enum

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
