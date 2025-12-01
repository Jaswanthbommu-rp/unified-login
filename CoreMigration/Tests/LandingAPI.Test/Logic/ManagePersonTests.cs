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
	/// Person Unit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManagePersonTests
	{
		[Fact]
		public void CreatePerson_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManagePerson managePerson = new ManagePerson();

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => managePerson.CreatePerson(null));
		}

		[Fact]
		public void CreatePerson_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			IPerson person = new Person() {
				FirstName = "Jack",
				MiddleName = "",
				LastName = "Bauer",
				Title = "CTU Agent",
				Suffix = "",
				PreferredContactMethodId = 1
			};
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			var mockObject = new Mock<IPersonRepository>();
			mockObject
				.Setup(m => m.CreatePerson(person))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManagePerson managePerson = new ManagePerson(mockObject.Object);
			IRepositoryResponse repositoryResponse = managePerson.CreatePerson(person);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == realPageId
			);
		}

		[Fact]
		public void GetPerson_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			IManagePerson managePerson = new ManagePerson();

			//Act
			Guid realPageId = new Guid();

			//Assert
			Assert.Throws<Exception>(() => managePerson.GetPerson(realPageId));
		}

public void GetPerson_RealPageIdNotExistinDatabase_ReturnEmptyObject()
		{
			//Arrange
			Type type = typeof(Person);
			IPerson expectedPerson = new Person() {};
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			var mockObject = new Mock<IPersonRepository>();
			mockObject.Setup(m => m.GetPerson(realPageId)).Returns(new Person {});

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			IManagePerson managePerson = new ManagePerson(mockObject.Object);
			IPerson person = managePerson.GetPerson(realPageId);
            //Assert
            Assert.True(
				person.FirstName == expectedPerson.FirstName
				&& person.MiddleName == expectedPerson.MiddleName
				&& person.LastName == expectedPerson.LastName
				&& person.PartyId == expectedPerson.PartyId
				&& person.Suffix == expectedPerson.Suffix
				&& person.Title == expectedPerson.Title
				&& person.RealPageId == expectedPerson.RealPageId
				&& person.PreferredContactMethodId == expectedPerson.PreferredContactMethodId
                && NumberOfProperties == 10
			);
		}

		[Fact]
		public void UpdatePerson_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			IManagePerson managePerson = new ManagePerson();
			Guid realPageId = new Guid();

			//Act
			IPerson person = new Person()
			{
				PartyId = 1,
				Title = "Property Manager",
				FirstName = "John",
				MiddleName = "X",
				LastName = "doe",
				Suffix = "Mr",
				PreferredContactMethodId = 1
			};

			//Assert
			Assert.Throws<Exception>(() => managePerson.UpdatePerson(realPageId, person));
		}

		[Fact]
		public void UpdatePerson_InvalidPersonOnject_ExceptionThrown()
		{
			//Arrange
			IManagePerson managePerson = new ManagePerson();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");

			//Act
			Person person = null;

			//Assert
			Assert.Throws<ArgumentNullException>(() => managePerson.UpdatePerson(realPageId, person));
		}

		[Fact]
		public void UpdatePerson_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			IPerson person = new Person()
			{
				FirstName = "Jack",
				MiddleName = "",
				LastName = "Bauer",
				Title = "CTU Agent",
				Suffix = "",
				PreferredContactMethodId = 1
			};
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			var mockObject = new Mock<IPersonRepository>();
			mockObject
				.Setup(m => m.UpdatePerson(realPageId, person))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManagePerson managePerson = new ManagePerson(mockObject.Object);
			IRepositoryResponse repositoryResponse = managePerson.UpdatePerson(realPageId, person);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == realPageId
			);
		}
	}
}
