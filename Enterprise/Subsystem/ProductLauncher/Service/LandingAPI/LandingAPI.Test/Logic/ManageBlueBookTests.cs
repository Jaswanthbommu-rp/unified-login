using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageBlueBookTests
	{
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

			Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

			//Arrange
			long booksCompanyMasterId = 0;
            string include = null;
            string filter = null;

            _manageBlueBook = new ManageBlueBook(_userClaims, mockProductInternalSettingRepository.Object);

            //Act
            Exception exception = Record.Exception(() => _manageBlueBook.GetCustomerProperty(booksCompanyMasterId, include, filter));

			//Assert
			//Assert
			Assert.IsType<Exception>(exception);
			Assert.Equal("Invalid parameter booksCompanyMasterId.", exception.Message);
		}
    }
}
