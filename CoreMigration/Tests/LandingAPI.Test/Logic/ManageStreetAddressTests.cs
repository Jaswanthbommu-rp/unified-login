using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// Contact Mechanism Unit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageStreetAddressTests
	{
		[Fact]
		public void CreateStreetAddress_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManageStreetAddress manageStreetAddress = new ManageStreetAddress();

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => manageStreetAddress.CreateStreetAddress(null));
		}

		[Fact]
		public void CreateStreetAddress_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			IStreetAddress streetAddress = new StreetAddress()
			{
				ContactMechanismId = 1,
				StreetAddress1 = "2201 Lakeside Blvd",
				StreetAddress2 = null,
				StreetAddress3 = null
			};
			var mockObject = new Mock<IStreetAddressRepository>();
            var mockRepository = new Mock<IRepository>();

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateStreetAddress, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });
            IStreetAddressRepository streenAddressRepository = new StreetAddressRepository(mockRepository.Object);

            //Act
            IManageStreetAddress manageStreetAddress = new ManageStreetAddress(streenAddressRepository);
			IRepositoryResponse repositoryResponse = manageStreetAddress.CreateStreetAddress(streetAddress);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == realPageId
			);
		}
	}
}
