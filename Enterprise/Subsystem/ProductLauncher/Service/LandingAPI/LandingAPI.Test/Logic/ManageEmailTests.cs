using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// Email Business logic xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageEmailTests
	{
		#region Private Variables
		DefaultUserClaim _userClaims = new DefaultUserClaim()
		{
			LoginName = "MocTest",
			CorrelationId = Guid.NewGuid(),
			OrganizationName = "MocTest",
			OrganizationPartyId = 1,
			OrganizationRealPageGuid = Guid.NewGuid(),
			OrganizationMasterId = 1,
			UserRealPageGuid = Guid.NewGuid(),
		};

		IManageEmail _manageEmail;
		#endregion

		#region Public xUnit tests
		[Fact]
		public void SendGridEmail_InvalidInputObject_ExceptionThrown()
		{
			//Arrange
			_manageEmail = new ManageEmail(_userClaims);

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => _manageEmail.SendGridEmail(null));
		}

		[Fact]
		public void SendGridEmail_InvalidFromAddress_ExceptionThrown()
		{
			//Arrange
			_manageEmail = new ManageEmail(_userClaims);
			ISendGridEmail sendGridEmail = new SendGridEmail()
			{
				emailSubject = "Email Subject",
				toAddress = new List<EmailAddress>()
				{
					new EmailAddress()
					{
						email = "Joe@Example.com",
						name = "Joe"
					}
				},
				fromAddress = null,
				category = "xUnit Test",
				message = "Email Body",
				transId = "12345"
			};
			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => _manageEmail.SendGridEmail(sendGridEmail));
		}

		[Fact]
		public void SendGridEmail_InvalidFromAddressAddress_ExceptionThrown()
		{
			//Arrange
			_manageEmail = new ManageEmail(_userClaims);
			ISendGridEmail sendGridEmail = new SendGridEmail()
			{
				emailSubject = "Email Subject",
				toAddress = new List<EmailAddress>()
				{
					new EmailAddress()
					{
						email = "Joe@Example.com",
						name = "Joe"
					}
				},
				fromAddress = new EmailAddress()
				{
					email = "noreplyrealpage.com",
					name = "RealPage"
				},
				category = "xUnit Test",
				message = "Email Body",
				transId = "12345"
			};
			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => _manageEmail.SendGridEmail(sendGridEmail));
		}

		[Fact]
		public void SendGridEmail_InvalidToAddress_ExceptionThrown()
		{
			//Arrange
			_manageEmail = new ManageEmail(_userClaims);
			ISendGridEmail sendGridEmail = new SendGridEmail()
			{
				emailSubject = "Email Subject",
				toAddress = null,
				fromAddress = new EmailAddress()
				{
					email = "noreply@realpage.com",
					name = "RealPage"
				},
				category = "xUnit Test",
				message = "Email Body",
				transId = "12345"
			};
			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => _manageEmail.SendGridEmail(sendGridEmail));
		}

		[Fact]
		public void SendGridEmail_InvalidToAddressAddress_ExceptionThrown()
		{
			//Arrange
			_manageEmail = new ManageEmail(_userClaims);
			ISendGridEmail sendGridEmail = new SendGridEmail()
			{
				emailSubject = "Email Subject",
				toAddress = new List<EmailAddress>()
				{
					new EmailAddress()
					{
						email = "JoeExample.com",
						name = "Joe"
					}
				},
				fromAddress = new EmailAddress()
				{
					email = "noreply@realpage.com",
					name = "RealPage"
				},
				category = "xUnit Test",
				message = "Email Body",
				transId = "12345"
			};
			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => _manageEmail.SendGridEmail(sendGridEmail));
		}
		#endregion
	}
}
