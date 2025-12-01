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
	/// Party Relationship xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManagePartyRelationshipTests
	{
		[Fact]
		public void LinkOrganizationToOrganization_InvalidOrganizationRealPageIdFrom_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid();
			Guid RealPageIdTo = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkOrganizationToOrganization_InvalidOrganizationRealPageIdTo_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid();
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkOrganizationToOrganization_InvalidOrganizationRoleTypeIdFrom_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 0,
				RoleTypeIdTo = 2
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkOrganizationToOrganization_InvalidOrganizationRoleTypeIdTo_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 0
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkOrganizationToOrganization_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};
			var mockObject = new Mock<IPartyRelationshipRepository>();
			mockObject
				.Setup(m => m.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship(mockObject.Object);
			IRepositoryResponse repositoryResponse = managePartyRelationship.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == realPageId
			);
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidPersonRealPageIdFrom_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid();
			Guid RealPageIdTo = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidOrganizationRealPageIdTo_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid();
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidPersonRoleTypeIdFrom_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 0,
				RoleTypeIdTo = 2
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidOrganizationRoleTypeIdTo_ExceptionThrown()
		{
			//Arrange
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();

			//Act
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 0
			};

			//Assert
			Assert.Throws<Exception>(() => managePartyRelationship.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));
		}

		[Fact]
		public void LinkPersonToOrganization_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid();
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};
			var mockObject = new Mock<IPartyRelationshipRepository>();
			mockObject
				.Setup(m => m.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

			//Act
			IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship(mockObject.Object);
			IRepositoryResponse repositoryResponse = managePartyRelationship.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == realPageId
			);
		}

        [Fact]
        public void GetPartyRelationship_InvalidRealPageId_ExceptionThrown()
        {
            //Arrange
            Guid emptyRealPageId = Guid.Empty;
            Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
            Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
            string roleTypeNameFrom = null;
            string roleTypeNameTo = null;
            string relationshipTypeName = null;

            var mockObject = new Mock<IPartyRelationshipRepository>();
            IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship(mockObject.Object);

            //Act
            Exception realPageIdFromException = Record.Exception(() => 
                    managePartyRelationship.GetPartyRelationship(emptyRealPageId, RealPageIdTo, roleTypeNameFrom, roleTypeNameTo, relationshipTypeName
                ));
            Exception realPageIdToException = Record.Exception(() =>
                    managePartyRelationship.GetPartyRelationship(RealPageIdFrom, emptyRealPageId, roleTypeNameFrom, roleTypeNameTo, relationshipTypeName
                ));

            //Assert
            Assert.IsType<Exception>(realPageIdFromException);
            Assert.IsType<Exception>(realPageIdToException);
        }

        [Fact]
        public void GetPartyRelationship_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Type type = typeof(PartyRelationship);
            int numberOfProperties = 0;
            PartyRelationship partyRelationship = new PartyRelationship();
            Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
            Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
            string roleTypeNameFrom = null;
            string roleTypeNameTo = null;
            string relationshipTypeName = null;
            PartyRelationship expectedPartyRelationship = new PartyRelationship()
            {
                RealPageIdFrom = RealPageIdFrom,
                RealPageIdTo = RealPageIdTo,
                PartyIdFrom = 1,
                PartyIdTo = 2
            };

            Mock<IPartyRelationshipRepository> _mockPartyRelationship = new Mock<IPartyRelationshipRepository>();
            _mockPartyRelationship
                .Setup(m => m.GetPartyRelationship(RealPageIdFrom, RealPageIdTo, roleTypeNameFrom, roleTypeNameTo, relationshipTypeName))
                .Returns(expectedPartyRelationship);

            IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationship.Object);

            //Act
            partyRelationship = managePartyRelationship.GetPartyRelationship(RealPageIdFrom, 
                                                                             RealPageIdTo,
                                                                             roleTypeNameFrom,
                                                                             roleTypeNameTo,
                                                                             relationshipTypeName);
            numberOfProperties = type.GetProperties().Length;

            //Assert
            Assert.True(partyRelationship != null
                && partyRelationship.RealPageIdFrom == RealPageIdFrom
                && partyRelationship.RealPageIdTo == RealPageIdTo
                && numberOfProperties == 13);
        }

    }
}
