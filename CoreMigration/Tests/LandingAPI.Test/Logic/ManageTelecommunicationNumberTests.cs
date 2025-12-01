using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// Telecommunication Number Unit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageTelecommunicationNumberTests
	{
		[Fact]
		public void CreateTelecommunicationNumber_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManageTelecommunicationNumber manageTelecommunicationNumber = new ManageTelecommunicationNumber();

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => manageTelecommunicationNumber.CreateTelecommunicationNumber(null));
		}

		[Fact]
		public void CreateTelecommunicationNumber_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber()
			{
				ContactMechanismId = 1,
				CountryCode = "1",
				AreaCode = "972",
				PhoneNumber = "820-3000",
				IsDefault = true
			};
			var mockObject = new Mock<ITelecommunicationNumberRepository>();
			mockObject.Setup(m => m.CreateTelecommunicationNumber(telecommunicationNumber)).Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManageTelecommunicationNumber manageTelecommunicationNumber = new ManageTelecommunicationNumber(mockObject.Object);
			IRepositoryResponse repositoryResponse = manageTelecommunicationNumber.CreateTelecommunicationNumber(telecommunicationNumber);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == realPageId
			);
		}

		[Fact]
		public void ListTelecommunicationNumber_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			IManageTelecommunicationNumber manageTelecommunicationNumber = new ManageTelecommunicationNumber();

			//Act
			Guid realPageId = new Guid();

			//Assert
			Assert.Throws<Exception>(() => manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName));
		}

		[Fact]
		public void ListTelecommunicationNumberForPerson_RealPageIdNotExistinDatabase_ReturnEmptyObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";

			Type type = typeof(TelecommunicationNumber);
			ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
			IList<ITelecommunicationNumber> expectedTelecommunicationNumberList = new List<ITelecommunicationNumber>();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			var mockObject = new Mock<ITelecommunicationNumberRepository>();
			mockObject.Setup(m => m.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName)).Returns(() => new List<TelecommunicationNumber> { new TelecommunicationNumber() { } });

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			expectedTelecommunicationNumberList.Add(telecommunicationNumber);
			IManageTelecommunicationNumber manageTelecommunicationNumber = new ManageTelecommunicationNumber(mockObject.Object);
			IList<TelecommunicationNumber> telecommunicationNumberList = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName);

			//Assert
			Assert.True(
				telecommunicationNumberList.Count == 1
				&& expectedTelecommunicationNumberList.Count == 1
				&& telecommunicationNumberList.All(
					o => expectedTelecommunicationNumberList.Any(
						w => w.CountryCode == o.CountryCode
						&&
						w.AreaCode == o.AreaCode
						&&
						w.PhoneNumber == o.PhoneNumber
						&&
						w.ContactMechanismId == o.ContactMechanismId
						&&
                        w.IsDefault == o.IsDefault
						&&
                        w.contactMechanismUsageType == o.contactMechanismUsageType
						&&
						w.PartyContactMechanismId == o.PartyContactMechanismId
					)
				) == true
				&& NumberOfProperties == 11
			);
		}
	}
}
