using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// PasswordPolicy XUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManagePasswordPolicyTests
	{
		#region Other Unit Tests
		[Fact]
		public void CreatePasswordPolicy_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManagePasswordPolicy managePasswordPolicy = new ManagePasswordPolicy();

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => managePasswordPolicy.CreatePasswordPolicy(null));
		}

		[Fact]
		public void CreatePasswordPolicy_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			PasswordPolicy passwordPolicy = new PasswordPolicy() {
				PartyId = 1,
				MinimumLength = 8,
				MaximumLength = 25,
				MinimumLowercase = 1,
				MinimumUppercase = 1,
				MinimumNumeric = 1,
				MinimumSpecialCharacter = 1,
				AllowUsersToChangeOwnPassword = false,
				EnablePasswordExpiration = true,
				PasswordExpirationPeriodInDays = 7,
				PreventPasswordReuse = true,
				NumberOfPasswordsToRemember = 5,
				UserId = 1
			};
			//var mockObject = new Mock<IPasswordPolicyRepository>();
			//mockObject
			//	.Setup(m => m.CreatePasswordPolicy(passwordPolicy))
			//	.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

            //repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePasswordPolicy, param);

            Mock<IRepository> mockRepository = new Mock<IRepository>();

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePasswordPolicy, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

            var passwordPolicyRepository = new PasswordPolicyRepository(mockRepository.Object);

            //Act
            IManagePasswordPolicy managePasswordPolicy = new ManagePasswordPolicy(passwordPolicyRepository);
			IRepositoryResponse repositoryResponse = managePasswordPolicy.CreatePasswordPolicy(passwordPolicy);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == Guid.Empty
			);
		}

		[Fact]
		public void GetPasswordPolicy_InvalidPortfolioId_ExceptionThrown()
		{
			//Arrange
			IManagePasswordPolicy managePasswordPolicy = new ManagePasswordPolicy();

			//Act

			//Assert
			Assert.Throws<Exception>(() => managePasswordPolicy.GetPasswordPolicy(-1));
		}

		[Fact]
		public void GetPasswordPolicy_PortfolioIdNotExistinDatabase_ReturnEmptyList()
		{
			//Arrange
			Type type = typeof(PasswordPolicy);
			IPasswordPolicy expectedPasswordPolicy = new PasswordPolicy() { };
			var mockObject = new Mock<IPasswordPolicyRepository>();
			mockObject
				.Setup(m => m.GetPasswordPolicy(1))
				.Returns(new PasswordPolicy());

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			IManagePasswordPolicy managePasswordPolicy = new ManagePasswordPolicy(mockObject.Object);
			PasswordPolicy passwordPolicy = managePasswordPolicy.GetPasswordPolicy(1);

			//Assert
			Assert.True(
				passwordPolicy.PartyId == expectedPasswordPolicy.PartyId
				&& passwordPolicy.MinimumLength == expectedPasswordPolicy.MinimumLength
				&& passwordPolicy.MaximumLength == expectedPasswordPolicy.MaximumLength
				&& passwordPolicy.MinimumLowercase == expectedPasswordPolicy.MinimumLowercase
				&& passwordPolicy.MinimumUppercase == expectedPasswordPolicy.MinimumUppercase
				&& passwordPolicy.MinimumNumeric == expectedPasswordPolicy.MinimumNumeric
				&& passwordPolicy.MinimumSpecialCharacter == expectedPasswordPolicy.MinimumSpecialCharacter
				&& passwordPolicy.AllowUsersToChangeOwnPassword == expectedPasswordPolicy.AllowUsersToChangeOwnPassword
				&& passwordPolicy.EnablePasswordExpiration == expectedPasswordPolicy.EnablePasswordExpiration
				&& passwordPolicy.PasswordExpirationPeriodInDays == expectedPasswordPolicy.PasswordExpirationPeriodInDays
				&& passwordPolicy.PreventPasswordReuse == expectedPasswordPolicy.PreventPasswordReuse
				&& passwordPolicy.NumberOfPasswordsToRemember == expectedPasswordPolicy.NumberOfPasswordsToRemember
				&& passwordPolicy.UserId == expectedPasswordPolicy.UserId
				&& NumberOfProperties == 17
			);
		}

		[Fact]
		public void UpdatePasswordPolicy_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			IManagePasswordPolicy managePasswordPolicy = new ManagePasswordPolicy();

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => managePasswordPolicy.UpdatePasswordPolicy(null));
		}

		[Fact]
		public void UpdatePasswordPolicy_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			PasswordPolicy passwordPolicy = new PasswordPolicy()
			{
				PartyId = 1,
				MinimumLength = 8,
				MaximumLength = 25,
				MinimumLowercase = 1,
				MinimumUppercase = 1,
				MinimumNumeric = 1,
				MinimumSpecialCharacter = 1,
				AllowUsersToChangeOwnPassword = false,
				EnablePasswordExpiration = true,
				PasswordExpirationPeriodInDays = 7,
				PreventPasswordReuse = true,
				NumberOfPasswordsToRemember = 5,
				UserId = 1
			};
			var mockObject = new Mock<IPasswordPolicyRepository>();
			mockObject
				.Setup(m => m.UpdatePasswordPolicy(passwordPolicy))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

			//Act
			IManagePasswordPolicy managePasswordPolicy = new ManagePasswordPolicy(mockObject.Object);
			IRepositoryResponse repositoryResponse = managePasswordPolicy.UpdatePasswordPolicy(passwordPolicy);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == Guid.Empty
			);
		}
		#endregion
	}
}
