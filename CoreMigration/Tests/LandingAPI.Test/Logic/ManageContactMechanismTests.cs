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
	/// Contact Mechanism xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageContactMechanismTests
	{
		[Fact]
		public void CreateContactMechanism_NoInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			var mockObject = new Mock<IContactMechanismRepository>();
			mockObject
				.Setup(m => m.CreateContactMechanism())
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism(mockObject.Object);
			IRepositoryResponse repositoryResponse = manageContactMechanism.CreateContactMechanism();

			//Assert
			Assert.True(repositoryResponse.Id == 1);
			Assert.True(repositoryResponse.ErrorMessage == "");
			Assert.True(repositoryResponse.RealPageId == realPageId);
		}

		[Fact]
		public void LinkContactMechanismToParty_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism();
			IPartyContactMechanism partyContactMechanism = new PartyContactMechanism()
			{
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime(),
				ContactMechanismId = 1
			};

			//Act
			Guid realPageId = new Guid();

			//Assert
			Assert.Throws<Exception>(() => manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism));
		}

		[Fact]
		public void LinkContactMechanismToParty_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism();

			//Act
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");

			//Assert
			Assert.Throws<ArgumentNullException>(() => manageContactMechanism.LinkContactMechanismToParty(realPageId, null));
		}

		[Fact]
		public void LinkContactMechanismToParty_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			IPartyContactMechanism partyContactMechanism = new PartyContactMechanism()
			{
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime(),
				ContactMechanismId = 1
			};
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			var mockObject = new Mock<IContactMechanismRepository>();
			mockObject
				.Setup(m => m.LinkContactMechanismToParty(realPageId, partyContactMechanism))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism(mockObject.Object);
			IRepositoryResponse repositoryResponse = manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism);

			//Assert
			Assert.True(repositoryResponse.Id == 1);
			Assert.True(repositoryResponse.ErrorMessage == "");
			Assert.True(repositoryResponse.RealPageId == realPageId);
		}

		[Fact]
		public void LinkGeographicBoundaryToContactMechanism_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism();

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => manageContactMechanism.LinkGeographicBoundaryToContactMechanism(null));
		}

		[Fact]
		public void LinkGeographicBoundaryToContactMechanism_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			IContactMechanismBoundary contactMechanismBoundary = new ContactMechanismBoundary()
			{
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime(),
				ContactMechanismId = 1
			};
			var mockObject = new Mock<IContactMechanismRepository>();
			mockObject
				.Setup(m => m.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism(mockObject.Object);
			IRepositoryResponse repositoryResponse = manageContactMechanism.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary);

			//Assert
			Assert.True(repositoryResponse.Id == 1);
			Assert.True(repositoryResponse.ErrorMessage == "");
			Assert.True(repositoryResponse.RealPageId == realPageId);
		}

		[Fact]
		public void LinkUsageTypeToPartyContactMechanism_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			int partyContactMechanismID = 0;
			int ContactMechanismUsageTypeId = 1;
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism();

			//Act
			Exception exception = Assert.Throws<Exception>(() => manageContactMechanism.LinkUsageTypeToPartyContactMechanism(partyContactMechanismID, ContactMechanismUsageTypeId));

			//Assert
			Assert.Equal(exception.Message, "Missing Party Contact Mechanism Id.");
		}

		[Fact]
		public void LinkUsageTypeToPartyContactMechanism_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			int partyContactMechanismID = 1;
			int ContactMechanismUsageTypeId = 1;
			var mockObject = new Mock<IContactMechanismRepository>();
			mockObject
				.Setup(m => m.LinkUsageTypeToPartyContactMechanism(partyContactMechanismID, ContactMechanismUsageTypeId))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism(mockObject.Object);
			IRepositoryResponse repositoryResponse = manageContactMechanism.LinkUsageTypeToPartyContactMechanism(partyContactMechanismID, ContactMechanismUsageTypeId);

			//Assert
			Assert.True(repositoryResponse.Id == 1);
			Assert.True(repositoryResponse.ErrorMessage == "");
			Assert.True(repositoryResponse.RealPageId == realPageId);
		}

		[Fact]
		public void ListContactMechanismForPerson_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism();

			//Act
			Guid realPageId = new Guid();
			Exception exception = Assert.Throws<Exception>(() => manageContactMechanism.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName));

			//Assert
			Assert.Equal(exception.Message, "Invalid parameter realPageId.");
		}

		[Fact]
		public void ListContactMechanismForPerson_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";

			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Type type = typeof(CommonAddress);
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};

			CommonAddress commonAddress = new CommonAddress()
			{
				AddressString = "none@nowhere.com",
				AddressType = "Email",
				ContactMechanismId = 1,
				PartyContactMechanismId = 1,
				ContactMechanismUsageTypeId = 1,
				contactMechanismUsageType = contactMechanismUsageType
			};
			List<CommonAddress> expectedCommonAddressList = new List<CommonAddress>();
			expectedCommonAddressList.Add(commonAddress);

			var mockObject = new Mock<IContactMechanismRepository>();
			mockObject
				.Setup(m => m.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName))
				.Returns(() => expectedCommonAddressList);

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			IManageContactMechanism manageContactMechanism = new ManageContactMechanism(mockObject.Object);
			IList<CommonAddress> commonAddressList = manageContactMechanism.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName);

			//Assert
			Assert.True(commonAddressList.Count == expectedCommonAddressList.Count);
			Assert.True(
				commonAddressList.All(
					o => expectedCommonAddressList.Any(
						w => w.AddressString == o.AddressString
						&&
						w.AddressType == o.AddressType
						&&
						w.ContactMechanismId == o.ContactMechanismId
						&&
						w.PartyContactMechanismId == o.PartyContactMechanismId
						&&
						w.contactMechanismUsageType.ContactMechanismUsageTypeId == o.contactMechanismUsageType.ContactMechanismUsageTypeId
						&&
						w.contactMechanismUsageType.ParentContactMechanismUsageTypeId == o.contactMechanismUsageType.ParentContactMechanismUsageTypeId
						&&
						w.contactMechanismUsageType.Name == o.contactMechanismUsageType.Name
					)
				) == true
			);
			Assert.True(NumberOfProperties == 6);
		}

		[Fact]
		public void ListContactMechanismForPerson_ObjectProperties_Verify()
		{
			//Arrange
			Type type = typeof(CommonAddress);

			//Act
			int NumberOfProperties = type.GetProperties().Length;

			//Assert
			Assert.True(NumberOfProperties == 6);
		}
	}
}
