using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    /// <summary>
    /// Electronic Address xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
	public class ManageElectronicAddressTests
	{
		[Fact]
		public void CreateElectronicAddress_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManageElectronicAddress manageElectronicAddress = new ManageElectronicAddress();

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => manageElectronicAddress.CreateElectronicAddress(null));
		}

		[Fact]
		public void CreateElectronicAddress_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			IElectronicAddress electronicAddressNumber = new ElectronicAddress()
			{
				ContactMechanismId = 1,
				AddressString = "none@nowhere.com",
				AddressType = "Email"
			};

            //mockObject
            var mockRepository = new Mock<IRepository>();
            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = "", RealPageId = realPageId});
            
            var electronicAddressRepository = new ElectronicAddressRepository(mockRepository.Object);

            //Act

            IManageElectronicAddress manageElectronicAddress = new ManageElectronicAddress(electronicAddressRepository);
			IRepositoryResponse repositoryResponse = manageElectronicAddress.CreateElectronicAddress(electronicAddressNumber);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == realPageId
			);
		}

		[Fact]
		public void ListElectronicAddressForPerson_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			IManageElectronicAddress manageElectronicAddress = new ManageElectronicAddress();

			//Act
			Guid realPageId = new Guid();

			//Assert
			Assert.Throws<Exception>(() => manageElectronicAddress.ListElectronicAddressForPerson(realPageId, ContactMechanismUsageTypeName));
		}

		[Fact]
		public void ListElectronicAddressForPerson_RealPageIdNotExistinDatabase_ReturnEmptyObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			Type type = typeof(ElectronicAddress);
            ElectronicAddress electronicAddress = new ElectronicAddress() {ContactMechanismUsageTypeId = 123};
			IList<IElectronicAddress> expectedElectronicAddressList = new List<IElectronicAddress>();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");

            var mockRepository = new Mock<IRepository>();
            mockRepository.Setup(m => m.GetMany<ElectronicAddress>(StoredProcNameConstants.SP_ListEmailsForPerson, It.IsAny<object>()))
                .Returns(() => new List<ElectronicAddress> { electronicAddress });

            mockRepository.Setup(m => m.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, It.IsAny<object>()))
                .Returns(() => new List<ContactMechanismUsageType> { new ContactMechanismUsageType() { ContactMechanismUsageTypeId = 123, Name = "Test" } });
            
            var electronicAddressRepository = new ElectronicAddressRepository(mockRepository.Object);

            IManageElectronicAddress manageElectronicAddress = new ManageElectronicAddress(electronicAddressRepository);

            //Act
            int NumberOfProperties = type.GetProperties().Length;
			expectedElectronicAddressList.Add(electronicAddress);
			IList<ElectronicAddress> electronicAddressList = manageElectronicAddress.ListElectronicAddressForPerson(realPageId, ContactMechanismUsageTypeName);

			//Assert
			Assert.True(
				electronicAddressList.Count == 1
				&& expectedElectronicAddressList.Count == 1
				&& electronicAddressList.All(
					o => expectedElectronicAddressList.Any(
						w => w.AddressString == o.AddressString
						&&
						w.AddressType == o.AddressType
						&&
						w.ContactMechanismId == o.ContactMechanismId
						&&
						w.contactMechanismUsageType == o.contactMechanismUsageType
						&&
						w.PartyContactMechanismId == o.PartyContactMechanismId
					)
				) == true
				&& NumberOfProperties == 6
			);
		}
	}
}
