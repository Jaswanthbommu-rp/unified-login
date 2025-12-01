using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
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
		Mock<IEmailRepository> _mockEmailRepository = new Mock<IEmailRepository>();
		Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
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

		[Fact]
		public void SendGridEmail_SendGridDisabled_ExceptionThrown()
		{
			//Arrange
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
					email = "noreply@realpage.com",
					name = "RealPage"
				},
				category = "xUnit Test",
				message = "Email Body",
				transId = "12345"
			};

			List<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>()
			{
				new ProductInternalSetting()
				{
					Name = "IsSendGridEnabled",
					Value = "0"
				},
				new ProductInternalSetting()
				{
					Name = "SendGridApiEndPoint",
					Value = "https://ueapi-dev.realpage.com"
				},
				new ProductInternalSetting()
				{
					Name = "SendGridSendEmailEndPoint",
					Value = "/emails/api/v1/sendEmail/'"
				}
			};

			_mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
				.Returns(productInternalSettingList);

			_manageEmail = new ManageEmail(_userClaims, _mockEmailRepository.Object, _mockProductInternalSettingRepository.Object);

			//Act
			string response = _manageEmail.SendGridEmail(sendGridEmail);

			//Assert
			Assert.Equal("SendGrid emails is disabled.", response, true, true, true);
		}

		[Fact]
		public void SendGridEmail_InvalidProductSetting_ExceptionThrown()
		{
			//Arrange
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
					email = "noreply@realpage.com",
					name = "RealPage"
				},
				category = "xUnit Test",
				message = "Email Body",
				transId = "12345"
			};

			List<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>();
			productInternalSettingList = null;

			_mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
				.Returns(productInternalSettingList);

			_manageEmail = new ManageEmail(_userClaims, _mockEmailRepository.Object, _mockProductInternalSettingRepository.Object);

			//Act
			string response = _manageEmail.SendGridEmail(sendGridEmail);

			//Assert
			Assert.Equal("An error occured when sending the email.", response, true, true, true);
		}

		[Fact]
		public void SendGridEmail_InvalidProductSetting_NoProductSetting()
		{
			//Arrange
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
					email = "noreply@realpage.com",
					name = "RealPage"
				},
				category = "xUnit Test",
				message = "Email Body",
				transId = "12345"
			};

			var productInternalSettingList = new List<ProductInternalSetting>();

			_mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
				.Returns(productInternalSettingList);

			_manageEmail = new ManageEmail(_userClaims, _mockEmailRepository.Object, _mockProductInternalSettingRepository.Object);

			//Act
			string response = _manageEmail.SendGridEmail(sendGridEmail);

			//Assert
			Assert.Equal("Invalid product settings for Unified Platform.", response, true, true, true);
		}
		#endregion
	}
}
