using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageCredentialTest : IDisposable
	{
		#region Setup
		Mock<ICredentialRepository> _mockCredentialRepository = new Mock<ICredentialRepository>();
		Mock<IPasswordPolicyRepository> _mockPasswordPolicyRepository = new Mock<IPasswordPolicyRepository>();
		Mock<IUserLoginRepository> _mockUserLoginRepository = new Mock<IUserLoginRepository>();
		Mock<IManageUserLogin> _mockManageUserLogin = new Mock<IManageUserLogin>();
		Mock<IManagePerson> _mockManagePerson = new Mock<IManagePerson>();
		UserDeviceDetails _mockUserdDetails = new UserDeviceDetails();
        Mock<IRepository> _mockRepository = new Mock<IRepository>();
        private Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
        IManageCredential _manageCredential;
        CredentialRepository _credentialRepository;

        private string _enterpriseUserName = string.Empty;
		private long _organizationPartyId = 350;
		private Guid _realPageId = new Guid("4ACE5DAE-7DD3-451F-B782-CB13A407D16A");
        private DefaultUserClaim _defaultUserClaim = new DefaultUserClaim();
		private UserDeviceDetails _userDeviceDetails;
		ActivityAttemptDetails _activityAttemptDetails = new ActivityAttemptDetails();
		UserLoginOnly _userlogin = new UserLoginOnly();
		TokenDetail _tokenDetail = new TokenDetail();

		List<SecurityQuestionAnswer> _securityQuestionAnswerList = new List<SecurityQuestionAnswer>();
		IList<SecurityQuestion> _securityQuestion = new List<SecurityQuestion>();
		UserResetPassword _userResetPassword = new UserResetPassword();
		IList<Organization> _organizationList = new List<Organization>();

        OrganizationStatus primaryOrganizationStatus = new OrganizationStatus();

		IdentityProviderType _expectedIdentityProviderType = new IdentityProviderType()
		{
			AuthenticationType = "local"
		};

		public ManageCredentialTest()
		{
			_enterpriseUserName = "user@example.com";

			_userDeviceDetails = new UserDeviceDetails
			{
				BrowserName = "Chrome",
				DeviceType = "Desktop",
				IpAddress = "127.0.0.1",
				Platform = "Windows"
			};

            _userlogin = new UserLoginOnly()
            {
                UserId = 1,
                LoginName = _enterpriseUserName,
                PartyId = 30,
                RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                LastLogin = DateTime.Now,
                PasswordModifiedDate = DateTime.Now.AddDays(-25)
            };

            _mockUserLoginRepository
                .Setup(m => m.GetPrimaryOrgIdByUserId(It.IsAny<long>()))
                .Returns(_organizationPartyId);

            primaryOrganizationStatus = new OrganizationStatus() {PartyId = _organizationPartyId, IsTainted = false, IsActive = true};

            _organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = _realPageId,
                    CreateDate = DateTime.MaxValue.ToUniversalTime(),
                    Name = "CF Res",
                    PartyId = _organizationPartyId,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = 1
                    }
                }
            };

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    It.IsAny<long>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<long>(),
                    It.IsAny<bool>()
                ))
                .Returns(primaryOrganizationStatus);

            _mockRepository.Setup(m => m.GetOne<IdentityProviderType>(StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName, It.IsAny<object>()))
                .Returns(_expectedIdentityProviderType);

_mockRepository.Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, It.IsAny<object>()))
                .Returns(_organizationList);

            _mockRepository.Setup(m => m.GetOne<ActivityAttempt>(StoredProcNameConstants.SP_UpdateActivityAttempt, It.IsAny<object>()))
                .Returns(new ActivityAttempt());

            _credentialRepository = new CredentialRepository(_mockRepository.Object);
        }

        #endregion

        #region Security Questions

        [Fact]
		public void GetSecurityQuestion_Error_NullEnterpriseUserName()
		{
			_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
				_mockUserRepository.Object,
				_defaultUserClaim
			);
			var response = _manageCredential.GetSecurityQuestion(null, _userDeviceDetails);

			Assert.True(response.IsError);
			Assert.Equal("No Username specified.", response.ErrorReason);
		}

		[Fact]
		public void GetSecurityQuestion_Error_EnterpriseUserNameNotExistinDatabase()
		{
			_userlogin = null;

			_mockCredentialRepository
				.Setup(m => m.UpdateUserActivityAttempts(_enterpriseUserName, ActivityType.ForgotPassword, _mockUserdDetails, _organizationPartyId, string.Empty))
				.Returns(new ActivityAttempt());

			_mockCredentialRepository
				.Setup(m => m.GetIdentityProviderTypeByLoginName(_enterpriseUserName))
				.Returns(_expectedIdentityProviderType);

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
				.Returns(_userlogin);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response = _manageCredential.GetSecurityQuestion(_enterpriseUserName, _userDeviceDetails);

			Assert.True(response.IsError);
			Assert.Equal("The Username \"user@example.com\" is incorrect or was not found.", response.ErrorReason);
		}

		[Fact]
		public void GetSecurityQuestion_Error_UserHasNoSecurityQuestions()
		{

			_mockCredentialRepository
				.Setup(m => m.UpdateUserActivityAttempts(_enterpriseUserName, ActivityType.ForgotPassword, _mockUserdDetails, _organizationPartyId, string.Empty))
				.Returns(new ActivityAttempt());

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.ForgotPassword))
				.Returns(_activityAttemptDetails);

			_mockCredentialRepository
				.Setup(m => m.GetUserSecurityQuestionAnswer(_enterpriseUserName))
				.Returns(_securityQuestionAnswerList);

            _mockManageUserLogin
                .Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_userlogin.RealPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.GetIdentityProviderTypeByLoginName(_enterpriseUserName))
				.Returns(_expectedIdentityProviderType);

            _manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response = _manageCredential.GetSecurityQuestion(_enterpriseUserName, _userDeviceDetails);

			Assert.True(response.IsError);
			Assert.Equal("User has no security questions defined.", response.ErrorReason);
		}

		[Fact]
		public void GetSecurityQuestion_Success_SelectRandomQuestions()
		{
			_tokenDetail = new TokenDetail()
			{
				EnterpriseUserId = 1,
				RealPageId = _realPageId,
				Token = "token"
			};

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_securityQuestion = new List<SecurityQuestion>
			{
				new SecurityQuestion {Question = "Question-1", SecurityQuestionId = 1},
				new SecurityQuestion {Question = "Question-2", SecurityQuestionId = 2},
				new SecurityQuestion {Question = "Question-3", SecurityQuestionId = 3}
			};

			_mockCredentialRepository
				.Setup(m => m.UpdateUserActivityAttempts(_enterpriseUserName, ActivityType.ForgotPassword, _mockUserdDetails, _organizationPartyId, string.Empty))
				.Returns(new ActivityAttempt());

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.ForgotPassword))
				.Returns(_activityAttemptDetails);

			_mockCredentialRepository
				.Setup(m => m.GetUserSecurityQuestionAnswer(_enterpriseUserName))
				.Returns(_securityQuestionAnswerList);

			_mockCredentialRepository
				.Setup(m => m.CreateActivityToken(_organizationPartyId, _realPageId, (int)ActivityType.ForgotPassword))
				.Returns("SJSJSJJSJS-TOKEN-SJSJS");

            _mockManageUserLogin
                .Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.GetIdentityProviderTypeByLoginName(_enterpriseUserName))
				.Returns(_expectedIdentityProviderType);

			_mockCredentialRepository
				.Setup(m => m.GetUserSecurityQuestion(_enterpriseUserName))
				.Returns(_securityQuestion);

			_mockCredentialRepository
				.Setup(m => m.GetActivityToken(_enterpriseUserName, "token", (int)ActivityType.ForgotPassword, _organizationPartyId))
				.Returns(_tokenDetail);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response = _manageCredential.GetSecurityQuestion(_enterpriseUserName, _userDeviceDetails);

			Assert.False(response.IsError);
			Assert.Null(response.ErrorReason);
			Assert.Equal(2, response.SecurityQuestions.Count);
		}

		#endregion

		#region Change Password

		[Fact]
		public void ForgotPassword_Error_NoUserName()
		{
			_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "OK",
					ActivityToken = "A",
					EnterpriseUserName = "",
					NewPassword = "newpwd"
				});

			Assert.True(response.IsError);
			Assert.False(response.IsSuccess);
			Assert.Equal("No Username specified.", response.ErrorReason);
		}

		[Fact]
		public void ForgotPassword_Error_NoFPToken()
		{
			_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "OK",
					ActivityToken = "",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "newpwd"
				});

			Assert.True(response.IsError);
			Assert.False(response.IsSuccess);
			Assert.Equal("Forgot Password Activity Token is not specified.", response.ErrorReason);
		}

		[Fact]
		public void ForgotPassword_Error_NoAnswerToken()
		{
			_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "newpwd"
				});

			Assert.True(response.IsError);
			Assert.False(response.IsSuccess);
			Assert.Equal("Correct Answer Token is not specified.", response.ErrorReason);
		}

		[Fact]
		public void ForgotPassword_Error_NoChangePwd()
		{
			_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "AMA",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = ""
				});

			Assert.True(response.IsError);
			Assert.False(response.IsSuccess);
			Assert.Equal("New Password is not specified.", response.ErrorReason);
		}

		[Fact]
		public void ForgotPassword_Error_EnterpriseUserNameNotExistinDatabase()
		{
			_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "AMA",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "new"
				});

			Assert.True(response.IsError);
			Assert.Equal("User name is incorrect or not found.", response.ErrorReason);
		}

		[Fact]
		public void ForgotPassword_Error_IncorrectLengthPasswordValidation()
		{
			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(
					new ChangePassword
					{
						CorrectAnswerToken = "AMA",
						ActivityToken = "ABCD",
						EnterpriseUserName = _enterpriseUserName,
						NewPassword = "new"
					}
				);

			Assert.True(response.IsError);
			Assert.True(response.ErrorReason.Contains("Your password must be at least 8 characters.") == true);
		}

		[Fact]
		public void ForgotPassword_Success_CorrectLengthPasswordValidation()
		{
			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_manageCredential = new ManageCredential(
                _credentialRepository,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "AMA",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "newpassw"
				});

			Assert.True(response.IsError);
			Assert.True(response.ErrorReason.Contains("Your password must include minimum 1 lower case characters and minimum 1 upper case characters. Your password must include 1 numeric characters. Your password must include minimum 1 special characters.") == true);
		}

		[Fact]
		public void ForgotPassword_Error_InCorrectLengthPasswordValidationMaxLengthExceed()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 5,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "AMA",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "abcd1234abcdefgh"
				});

			Assert.True(response.IsError);
			Assert.True(response.ErrorReason.Contains("Your password must be 5 characters or less. Your password must include minimum 1 lower case characters and minimum 1 upper case characters. Your password must include minimum 1 special characters.") == true);
		}

		[Fact]
		public void ForgotPassword_Error_InCorrecUpperCasePassword()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "AMA",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "abcd1234abcdefgh"
				});

			Assert.True(response.IsError);
			Assert.True(response.ErrorReason.Contains("Your password must be 12 characters or less. Your password must include minimum 1 lower case characters and minimum 1 upper case characters. Your password must include minimum 1 special characters.") == true);
		}

		[Fact]
		public void ForgotPassword_Error_InCorrectLowerCasePassword()
		{

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "AMA",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "ABCD1234ABCDEFGH"
				});

			Assert.True(response.IsError);
			Assert.True(response.ErrorReason.Contains("Your password must be 12 characters or less. Your password must include minimum 1 lower case characters and minimum 1 upper case characters. Your password must include minimum 1 special characters.") == true);
		}

		[Fact]
		public void ForgotPassword_Success_CorrectCasePassword()
		{
			string newPassword = "AbCdE1GH@Bcd";

			PasswordDetail passwordDetail = newPassword.PasswordHash();
			string enterpriseUserId = "1";

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_tokenDetail = new TokenDetail()
			{
				EnterpriseUserId = 1,
				RealPageId = _realPageId,
				Token = "ABCD"
			};

			_mockCredentialRepository
				.Setup(m => m.GetActivityToken(_enterpriseUserName, "ABCD", (int)ActivityType.ForgotPassword, _organizationPartyId))
				.Returns(_tokenDetail);

			_tokenDetail = new TokenDetail()
			{
				EnterpriseUserId = 1,
				RealPageId = _realPageId,
				Token = "AMA"
			};

			_mockCredentialRepository
				.Setup(m => m.GetActivityToken(_enterpriseUserName, "AMA", (int)ActivityType.VerifyAnswers, _organizationPartyId))
				.Returns(_tokenDetail);

			_mockCredentialRepository
				.Setup(m => m.UpdateEnterpriseUserCredential(_enterpriseUserName, It.IsAny<string>(), It.IsAny<string>(), "AMA", (int)ActivityType.VerifyAnswers, _organizationPartyId))
				.Returns("12345");

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(
					new ChangePassword()
					{
						CorrectAnswerToken = "AMA",
						ActivityToken = "ABCD",
						EnterpriseUserName = _enterpriseUserName,
						NewPassword = newPassword
					}
				);

			Assert.False(response.IsError);
			Assert.Equal(string.Empty, response.ErrorReason);
		}

		[Fact]
		public void ForgotPassword_Error_InCorrectNumericInPassword()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(new ChangePassword
				{
					CorrectAnswerToken = "AMA",
					ActivityToken = "ABCD",
					EnterpriseUserName = _enterpriseUserName,
					NewPassword = "AbCdEFGHABcd"
				});

			Assert.True(response.IsError);
			Assert.True(response.ErrorReason.Contains("Your password must include 1 numeric characters. Your password must include minimum 1 special characters.") == true);
		}

		[Fact]
		public void ForgotPassword_Error_SpecialCharNotInPassword()
		{

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(
					new ChangePassword()
					{
						CorrectAnswerToken = "AMA",
						ActivityToken = "AB1CDmsmsmsm",
						EnterpriseUserName = _enterpriseUserName,
						NewPassword = "AbCdEF4HABcd"
					}
				);

			Assert.True(response.IsError);
			Assert.True(response.ErrorReason.Contains("Your password must include minimum 1 special characters.") == true);
		}

		[Fact]
		public void ForgotPassword_Error_PasswordContainUserName_Case1()
		{

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.ForgotPassword(
					new ChangePassword()
					{
						CorrectAnswerToken = "AMA",
						ActivityToken = "ABCD",
						EnterpriseUserName = _enterpriseUserName,
						NewPassword = "abcd" + _enterpriseUserName + "test1"
					}
				);

			Assert.True(response.IsError);
			Assert.Equal("Your password cannot be the same as your Username.", response.ErrorReason);
		}
		#endregion

		#region VerifySecurityAnswers

		[Fact]
		public void VerifySecurityAnswers_Error_NoUserName()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
					new UserSecurityAnswer
					{
						EnterpriseUserName = "",
						ActivityToken = "aa",
						SecurityQuestionAnswers = null
					}, _userDeviceDetails
				);

			Assert.True(response.IsError);
			Assert.Equal("No Username specified.", response.ErrorReason);
		}

		[Fact]
		public void VerifySecurityAnswers_Error_NoSecurityQuestion()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
					new UserSecurityAnswer()
					{
						EnterpriseUserName = _enterpriseUserName,
						ActivityToken = "aa",
						SecurityQuestionAnswers = null
					}, _userDeviceDetails
				);

			Assert.True(response.IsError);
			Assert.Equal("No questions received from user.", response.ErrorReason);
		}

		[Fact]
		public void VerifySecurityAnswers_Error_NoToken()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
					new UserSecurityAnswer()
					{
						EnterpriseUserName = _enterpriseUserName,
						ActivityToken = "",
						SecurityQuestionAnswers =
							new List<SecurityQuestionAnswer>
							{
								 new SecurityQuestionAnswer {Answer = "answer", SecurityQuestionId = 1}
							}
					}, _userDeviceDetails
				);

			Assert.True(response.IsError);
			Assert.Equal("Null or empty security Forgot Password Activity Token.", response.ErrorReason);
		}

		[Fact]
		public void VerifySecurityAnswers_Error_EnterpriseUserNameNotExistinDatabase()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_userlogin = null;
			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.QuestionAttempts))
				.Returns(_activityAttemptDetails);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
					new UserSecurityAnswer()
					{
						EnterpriseUserName = _enterpriseUserName,
						ActivityToken = "sometoken",
						SecurityQuestionAnswers =
							new List<SecurityQuestionAnswer>
							{
								 new SecurityQuestionAnswer {Answer = "answer", SecurityQuestionId = 1}
							}
					}, _userDeviceDetails
				);

			Assert.True(response.IsError);
			Assert.Equal("User Name is incorrect or not found.", response.ErrorReason);
		}

		[Fact]
		public void VerifySecurityAnswers_Error_ActivityAttemptsExceed()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.QuestionAttempts))
				.Returns(_activityAttemptDetails);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
					new UserSecurityAnswer()
					{
						EnterpriseUserName = _enterpriseUserName,
						ActivityToken = "sometoken",
						SecurityQuestionAnswers =
							new List<SecurityQuestionAnswer>
							{
								 new SecurityQuestionAnswer {Answer = "answer", SecurityQuestionId = 1}
							}
					}, _userDeviceDetails
				);

			Assert.True(response.IsError);
			Assert.Equal("Max attempts to answer security questions exceeded. Your account is locked", response.ErrorReason);
		}

		[Fact]
		public void VerifySecurityAnswers_Error_ActivityTokenNoMatchorExpired()
		{

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_activityAttemptDetails = new ActivityAttemptDetails()
			{
				ActivityTokenExpirationMinutes = 1,
				AttemptCount = 1,
				MaxActivitycount = 5
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.QuestionAttempts))
				.Returns(_activityAttemptDetails);

			_mockCredentialRepository
				.Setup(m => m.GetActivityToken(_enterpriseUserName, "token", (int)ActivityType.ForgotPassword, _organizationPartyId))
				.Returns(_tokenDetail);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
				new UserSecurityAnswer
				{
					EnterpriseUserName = _enterpriseUserName,
					ActivityToken = "AT",
					SecurityQuestionAnswers =
						new List<SecurityQuestionAnswer>
						{
							 new SecurityQuestionAnswer {Answer = "answer", SecurityQuestionId = 1}
						}
				}, _userDeviceDetails
			);

			Assert.True(response.IsError);
			Assert.Equal("Forgot Password Activity Token is expired.", response.ErrorReason);
		}

		[Fact]
		public void VerifySecurityAnswers_Error_SecurityAnswerNotMatch_ActivityAttemptNotExceeded()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_activityAttemptDetails = new ActivityAttemptDetails()
			{
				ActivityTokenExpirationMinutes = 1,
				AttemptCount = 1,
				MaxActivitycount = 5
			};

			IList<SecurityQuestionAnswer> userSecuAnswers = new List<SecurityQuestionAnswer>
			{
				new SecurityQuestionAnswer {Answer = "wrongAnswer", SecurityQuestionId = 2},
				new SecurityQuestionAnswer {Answer = "Answer-3", SecurityQuestionId = 3}
			};

			_securityQuestionAnswerList = new List<SecurityQuestionAnswer>
			{
				new SecurityQuestionAnswer {Answer = "Answer-1".ToUpper().Sha256(), SecurityQuestionId = 1},
				new SecurityQuestionAnswer {Answer = "Answer-2".ToUpper().Sha256(), SecurityQuestionId = 2},
				new SecurityQuestionAnswer {Answer = "Question-3".ToUpper().Sha256(), SecurityQuestionId = 3}
			};

			_securityQuestion = new List<SecurityQuestion>
			{
				new SecurityQuestion {Question = "Question-1", SecurityQuestionId = 1},
				new SecurityQuestion {Question = "Question-2", SecurityQuestionId = 2},
				new SecurityQuestion {Question = "Question-3", SecurityQuestionId = 3}
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.UpdateUserActivityAttempts(_enterpriseUserName, ActivityType.ForgotPassword, _mockUserdDetails, _organizationPartyId, string.Empty))
				.Returns(new ActivityAttempt());

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.ForgotPassword))
				.Returns(_activityAttemptDetails);

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.QuestionAttempts))
				.Returns(_activityAttemptDetails);

			_mockCredentialRepository
				.Setup(m => m.GetUserSecurityQuestion(_enterpriseUserName))
				.Returns(_securityQuestion);

			_mockCredentialRepository
				.Setup(m => m.GetUserSecurityQuestionAnswer(_enterpriseUserName))
				.Returns(_securityQuestionAnswerList);

			_mockCredentialRepository
				.Setup(m => m.GetActivityToken(_enterpriseUserName, "AT", (int)ActivityType.ForgotPassword, _organizationPartyId))
				.Returns(new TokenDetail { EnterpriseUserId = 123, Token = "ABCD" });

			_mockCredentialRepository
				.Setup(m => m.CreateActivityToken(_organizationPartyId, _realPageId, (int)ActivityType.ForgotPassword))
				.Returns("SJSJSJJSJS-TOKEN-SJSJS");

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
				new UserSecurityAnswer
				{
					EnterpriseUserName = _enterpriseUserName,
					ActivityToken = "AT",
					SecurityQuestionAnswers = userSecuAnswers
				}, _userDeviceDetails
			);

			Assert.True(response.IsError);
			Assert.Equal("One or more of your answers are incorrect. Please try again with a new set of questions.", response.ErrorReason);
			Assert.False(response.IsAnswersCorrect);
			Assert.Equal(2, response.SecurityQuestions.Count); // new set of question
		}

		[Fact]
		public void VerifySecurityAnswers_Success_SecurityAnswerMatch()
		{
			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_activityAttemptDetails = new ActivityAttemptDetails()
			{
				ActivityTokenExpirationMinutes = 1,
				AttemptCount = 0,
				MaxActivitycount = 1
			};

			IList<SecurityQuestionAnswer> userSecuAnswers = new List<SecurityQuestionAnswer>()
			{
				 new SecurityQuestionAnswer {Answer = "Answer-1".ToUpper().Sha256(), SecurityQuestionId = 1},
				 new SecurityQuestionAnswer {Answer = "Answer-2".ToUpper().Sha256(), SecurityQuestionId = 2},
				 new SecurityQuestionAnswer {Answer = "Answer-3".ToUpper().Sha256(), SecurityQuestionId = 3}
			};

			_securityQuestionAnswerList = new List<SecurityQuestionAnswer>
			 {
				 new SecurityQuestionAnswer {Answer = "Answer-1".ToUpper().Sha256(), SecurityQuestionId = 1},
				 new SecurityQuestionAnswer {Answer = "Answer-2".ToUpper().Sha256(), SecurityQuestionId = 2},
				 new SecurityQuestionAnswer {Answer = "Answer-3".ToUpper().Sha256(), SecurityQuestionId = 3}
			 };

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.UpdateUserActivityAttempts(_enterpriseUserName, ActivityType.ForgotPassword, _mockUserdDetails, _organizationPartyId, string.Empty))
				.Returns(new ActivityAttempt());

			_mockCredentialRepository
				.Setup(m => m.GetActivityAttemptExceeds(_organizationPartyId, _enterpriseUserName, (int)ActivityType.QuestionAttempts))
				.Returns(_activityAttemptDetails);

			_mockCredentialRepository
				.Setup(m => m.GetUserSecurityQuestionAnswer(_enterpriseUserName))
				.Returns(_securityQuestionAnswerList);

			_mockCredentialRepository
				.Setup(m => m.GetActivityToken(_enterpriseUserName, "AT", (int)ActivityType.ForgotPassword, _organizationPartyId))
				.Returns(new TokenDetail { EnterpriseUserId = 123, Token = "ABCD" });

			_mockCredentialRepository
				.Setup(m => m.CreateActivityToken(_organizationPartyId, _realPageId, (int)ActivityType.ForgotPassword))
				.Returns("SJSJSJJSJS-TOKEN-SJSJS");

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response =
				_manageCredential.VerifySecurityAnswers(
				new UserSecurityAnswer
				{
					EnterpriseUserName = _enterpriseUserName,
					ActivityToken = "AT",
					SecurityQuestionAnswers = _securityQuestionAnswerList
				}, _userDeviceDetails
			);

			Assert.False(response.IsError);
			Assert.Equal(response.ErrorReason, string.Empty);
			Assert.True(response.IsAnswersCorrect);
			Assert.Equal(response.EnterpriseUserName, _enterpriseUserName);
			Assert.Null(response.SecurityQuestions);
		}
		#endregion

		#region Reset Password

		[Fact]
		public void ResetPassword_Error_NullUserResetPassword()
		{
			_userResetPassword = null;

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			Assert.Throws<ArgumentNullException>(() => _manageCredential.ResetPassword(_realPageId, _userResetPassword));
		}

		[Fact]
		public void ResetPassword_Error_NullRealPageId()
		{
			_realPageId = Guid.Empty;

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(resetPasswordResponse.IsError);
			Assert.Equal("RealPage Id for user not provided.", resetPasswordResponse.ErrorReason);
		}

		[Fact]
		public void ResetPassword_Error_InvalidUserName()
		{
			//_realPageId = Guid.NewGuid();
			_defaultUserClaim = new DefaultUserClaim();
			_defaultUserClaim.UserRealPageGuid = _realPageId;
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				RealPageId = _realPageId,
				ErrorMessage = string.Empty
			};

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "AbCdE1GH@Bcd",
				OldPassword = _realPageId.ToString(),
				RealPageId = _realPageId
			};

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(resetPasswordResponse.IsError);
			Assert.Equal("User Name is incorrect or not found.", resetPasswordResponse.ErrorReason);
		}

		[Fact]
		public void ResetPassword_Error_InValidPassword()
		{
			//_realPageId = Guid.NewGuid();
			_defaultUserClaim = new DefaultUserClaim();
			_defaultUserClaim.UserRealPageGuid = _realPageId;
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				RealPageId = _realPageId,
				ErrorMessage = string.Empty
			};

			_userResetPassword = new UserResetPassword
			{
				NewPassword = string.Empty,
				OldPassword = _realPageId.ToString(),
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                PasswordSalt = It.IsAny<string>(),
				PasswordHash = It.IsAny<string>(),
				PasswordModifiedDate = DateTime.Now.AddDays(-25),
			};

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.ResetEnterpriseUserCredential(_realPageId, It.IsAny<string>(), It.IsAny<string>(), _organizationPartyId))
				.Returns(repositoryResponse);

			_mockCredentialRepository
				.Setup(m => m.UpdateUserStatusByCompany(_realPageId, _organizationPartyId, UserUiStatusType.Active, DateTime.Now, null))
				.Returns(repositoryResponse);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(resetPasswordResponse.IsError);
			Assert.Equal("New Password is not specified.", resetPasswordResponse.ErrorReason);
		}

		[Fact]
		public void ResetPassword_Error_ResetEnterpriseUserCredential()
		{
			//_realPageId = Guid.NewGuid();
			_defaultUserClaim = new DefaultUserClaim();
			_defaultUserClaim.UserRealPageGuid = _realPageId;
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				RealPageId = Guid.Empty,
				ErrorMessage = string.Empty
			};

			RepositoryResponse errorRepositoryResponse = new RepositoryResponse()
			{
				Id = 0,
				RealPageId = Guid.Empty,
				ErrorMessage = "Error Reset Enterprise User Credential."
			};

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "AbCdE1GH@Bcd",
				OldPassword = _realPageId.ToString(),
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                PasswordSalt = It.IsAny<string>(),
				PasswordHash = It.IsAny<string>(),
				PasswordModifiedDate = DateTime.Now.AddDays(-25),
                LastLogin = DateTime.Now
            };

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.ResetEnterpriseUserCredential(_realPageId, _userResetPassword.NewPassword, _userResetPassword.NewPassword, _organizationPartyId))
				.Returns(errorRepositoryResponse);

			_mockCredentialRepository
				.Setup(m => m.UpdateUserStatusByCompany(_realPageId, _organizationPartyId, UserUiStatusType.Active, DateTime.Now, null))
				.Returns(repositoryResponse);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(resetPasswordResponse.IsError);
		}

		[Fact]
		public void ResetPassword_Error_UpdateUserStatus()
		{
			//_realPageId = Guid.NewGuid();
			_defaultUserClaim = new DefaultUserClaim();
			_defaultUserClaim.UserRealPageGuid = _realPageId;
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				RealPageId = _realPageId,
				ErrorMessage = string.Empty
			};

			RepositoryResponse errorRepositoryResponse = new RepositoryResponse()
			{
				Id = 0,
				RealPageId = Guid.Empty,
				ErrorMessage = "Error Update User Status."
			};

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "AbCdE1GH@Bcd",
				OldPassword = _realPageId.ToString(),
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                //IsActive = true,
                //IsExpired = false,
                //IsPending = false,
                //IsLocked = false,
                PasswordSalt = It.IsAny<string>(),
				PasswordHash = It.IsAny<string>(),
				PasswordModifiedDate = DateTime.Now.AddDays(-25),
                LastLogin = DateTime.Now
            };

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.ResetEnterpriseUserCredential(_realPageId, It.IsAny<string>(), It.IsAny<string>(), _organizationPartyId))
				.Returns(repositoryResponse);

			_mockCredentialRepository
				.Setup(m => m.UpdateUserStatusByCompany(_realPageId, _organizationPartyId, UserUiStatusType.Active, It.IsAny<DateTime>(), null))
				.Returns(errorRepositoryResponse);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(resetPasswordResponse.IsError);
		}

		[Fact]
		public void ResetPassword_Error_IncorrectCurrentPassword()
		{
			//_realPageId = Guid.NewGuid();
			_defaultUserClaim = new DefaultUserClaim();
			_defaultUserClaim.UserRealPageGuid = _realPageId;
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				RealPageId = _realPageId,
				ErrorMessage = string.Empty
			};

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "AbCdE1GH@Bcd",
				OldPassword = "AbCdE1GH@Bcd",
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                //IsActive = true,
                //IsExpired = false,
                //IsPending = false,
                //IsLocked = false,
                PasswordSalt = "PasswordSalt",
				PasswordHash = "PasswordHash",
				PasswordModifiedDate = DateTime.Now.AddDays(-25),
				//FromDate = DateTime.Now
			};

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.ResetEnterpriseUserCredential(_realPageId, It.IsAny<string>(), It.IsAny<string>(), _organizationPartyId))
				.Returns(repositoryResponse);

			_mockCredentialRepository
				.Setup(m => m.UpdateUserStatusByCompany(_realPageId, _organizationPartyId, UserUiStatusType.Active, It.IsAny<DateTime>(), null))
				.Returns(repositoryResponse);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(resetPasswordResponse.IsError);
			Assert.Equal("Current password is incorrect.", resetPasswordResponse.ErrorReason);
		}

		[Fact]
		public void ResetPassword_Error_OrganizationPasswordPolicy()
		{
			//_realPageId = Guid.NewGuid();
			_defaultUserClaim = new DefaultUserClaim();
			_defaultUserClaim.UserRealPageGuid = _realPageId;
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				RealPageId = _realPageId,
				ErrorMessage = string.Empty
			};

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "AbCdE1GH@Bcd",
				OldPassword = _realPageId.ToString(),
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                PasswordSalt = It.IsAny<string>(),
				PasswordHash = It.IsAny<string>(),
				PasswordModifiedDate = DateTime.Now.AddDays(-25),
				LastLogin = DateTime.Now
			};

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			PasswordPolicy passwordPolicy = new PasswordPolicy();
			passwordPolicy = null;
			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(passwordPolicy);

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.ResetEnterpriseUserCredential(_realPageId, It.IsAny<string>(), It.IsAny<string>(), _organizationPartyId))
				.Returns(repositoryResponse);

			_mockCredentialRepository
                .Setup(m => m.UpdateUserStatusByCompany(_realPageId, _organizationPartyId, UserUiStatusType.Active, It.IsAny<DateTime>(), null))
                .Returns(repositoryResponse);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(resetPasswordResponse.IsError);
			Assert.Equal($"Unable to find password policy for organization - {_organizationPartyId}", resetPasswordResponse.ErrorReason);
		}

		[Fact]
		public void ResetPassword_Error_Success()
		{
			//_realPageId = Guid.NewGuid();
			_defaultUserClaim = new DefaultUserClaim();
			_defaultUserClaim.UserRealPageGuid = _realPageId;
			RepositoryResponse repositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				RealPageId = _realPageId,
				ErrorMessage = string.Empty
			};

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "AbCdE1GH@Bcd",
				OldPassword = _realPageId.ToString(),
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                //IsActive = true,
                //IsExpired = false,
                //IsPending = false,
                //IsLocked = false,
                PasswordSalt = It.IsAny<string>(),
				PasswordHash = It.IsAny<string>(),
				PasswordModifiedDate = DateTime.Now.AddDays(-25),
                LastLogin = DateTime.Now
            };

			_organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = _realPageId,
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Res",
					PartyId = _organizationPartyId,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 1
					}
				}
			};

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(_realPageId))
				.Returns(_organizationList);

			_mockCredentialRepository
				.Setup(m => m.ResetEnterpriseUserCredential(_realPageId, It.IsAny<string>(), It.IsAny<string>(), _organizationPartyId))
				.Returns(repositoryResponse);

			_mockCredentialRepository
                .Setup(m => m.UpdateUserStatusByCompany(_realPageId, _organizationPartyId, UserUiStatusType.Active, It.IsAny<DateTime>(), null))
                .Returns(repositoryResponse);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ResetPasswordResponse resetPasswordResponse = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.False(resetPasswordResponse.IsError);
		}

		[Fact]
		public void ResetPassword_Error_OldPasswordNotsupplied()
		{
			//_realPageId = Guid.NewGuid();

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "New",
				OldPassword = "",
				RealPageId = _realPageId
			};

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(response.IsError);
			Assert.Equal("Old Password is not specified.", response.ErrorReason);
		}

		[Fact]
		public void ResetPassword_Error_NewPasswordNotsupplied()
		{
			//_realPageId = Guid.NewGuid();

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "",
				OldPassword = "Old",
				RealPageId = _realPageId
			};

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response = _manageCredential.ResetPassword(_realPageId, _userResetPassword);

			Assert.True(response.IsError);
			Assert.Equal("New Password is not specified.", response.ErrorReason);
		}
		#endregion

		#region Password Expiration
		[Fact]
		public void CheckPasswordExpiration_Success_FalseEnablePasswordExpiration()
		{
			//_realPageId = Guid.NewGuid();

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "",
				OldPassword = "Old",
				RealPageId = _realPageId
			};

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = false,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

			var response = _manageCredential.CheckPasswordExpiration(_userlogin.UserId, _realPageId);

			Assert.False(response.IsError);
			Assert.Null(response.ErrorReason);
			Assert.Equal(SeverityLevelType.None, response.SeverityLevel);
		}

		[Fact]
		public void CheckPasswordExpiration_Error_UserNotExist()
		{
			_userResetPassword = new UserResetPassword
			{
				NewPassword = "New",
				OldPassword = "Old",
				RealPageId = _realPageId
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_enterpriseUserName))
				.Returns(_userlogin);
            _mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = true,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

            _manageCredential = new ManageCredential(
                _credentialRepository,
                _mockPasswordPolicyRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserLogin.Object,
                _mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
            );

            _realPageId = Guid.NewGuid();

            var response = _manageCredential.CheckPasswordExpiration(_userlogin.UserId, _realPageId);

            Assert.True(response.IsError);
			Assert.Equal("User Name is incorrect or not found.", response.ErrorReason);
		}

		[Fact]
		public void CheckPasswordExpiration_Success_InformationMessage()
		{
			//_realPageId = Guid.NewGuid();

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "New",
				OldPassword = "Old",
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                PasswordModifiedDate = DateTime.Now.AddDays(-25)
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.GetIdentityProviderTypeByLoginName(_enterpriseUserName))
				.Returns(_expectedIdentityProviderType);

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = true,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

            _manageCredential = new ManageCredential(
                _credentialRepository,
                _mockPasswordPolicyRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserLogin.Object,
                _mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
            );

            var response = _manageCredential.CheckPasswordExpiration(_userlogin.UserId, _realPageId);

            Assert.False(response.IsError);
			Assert.Equal(SeverityLevelType.Information, response.SeverityLevel);
		}

		[Fact]
		public void CheckPasswordExpiration_Success_WarningMessage()
		{
			_userResetPassword = new UserResetPassword
			{
				NewPassword = "New",
				OldPassword = "Old",
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                PasswordModifiedDate = DateTime.Now.AddDays(-28)
			};

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(_realPageId))
                .Returns(_userlogin);

            _mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = true,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

            _manageCredential = new ManageCredential(
                _credentialRepository,
                _mockPasswordPolicyRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserLogin.Object,
                _mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
            );

            //_realPageId = Guid.NewGuid();

            var response = _manageCredential.CheckPasswordExpiration(_userlogin.UserId, _realPageId);

            Assert.False(response.IsError);
			Assert.Equal(SeverityLevelType.Warning, response.SeverityLevel);
		}

		[Fact]
		public void CheckPasswordExpiration_Success_CriticalMessage()
		{
			//_realPageId = Guid.NewGuid();

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "New",
				OldPassword = "Old",
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
                //IsActive = true,
                //IsExpired = false,
                //IsPending = false,
                //IsLocked = false,
                PasswordModifiedDate = DateTime.Now.AddDays(-30)
            };

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(_realPageId))
				.Returns(_userlogin);

			_mockCredentialRepository
				.Setup(m => m.GetIdentityProviderTypeByLoginName(_enterpriseUserName))
				.Returns(_expectedIdentityProviderType);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = true,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

            var response = _manageCredential.CheckPasswordExpiration(_userlogin.UserId, _realPageId);

            Assert.False(response.IsError);
			Assert.Equal(SeverityLevelType.Critical, response.SeverityLevel);
		}

		[Fact]
		public void CheckPasswordExpiration_Error_PasswordAlreadyExpired()
		{
			//_realPageId = Guid.NewGuid();

			_userResetPassword = new UserResetPassword
			{
				NewPassword = "New",
				OldPassword = "Old",
				RealPageId = _realPageId
			};

			_userlogin = new UserLoginOnly()
			{
				LoginName = _enterpriseUserName,
				PartyId = 30,
				RealPageId = _realPageId,
                Is3rdPartyIDP = false,
				//IsActive = true,
				//IsExpired = false,
				//IsPending = false,
				//IsLocked = false,
				PasswordModifiedDate = DateTime.Now.AddDays(-31)
			};

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
				.Returns(_userlogin);

            _mockCredentialRepository
				.Setup(m => m.GetIdentityProviderTypeByLoginName(_enterpriseUserName))
				.Returns(_expectedIdentityProviderType);

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			_mockPasswordPolicyRepository
				.Setup(m => m.GetPasswordPolicy(_organizationPartyId))
				.Returns(new PasswordPolicy
				{
					MinimumSpecialCharacter = 1,
					MaximumLength = 12,
					MinimumNumeric = 1,
					MinimumUppercase = 1,
					MinimumLowercase = 1,
					MinimumLength = 8,
					AllowUsersToChangeOwnPassword = false,
					EnablePasswordExpiration = true,
					NumberOfPasswordsToRemember = 5,
					PasswordExpirationPeriodInDays = 30,
					PasswordPolicyId = 1
				});

            var response = _manageCredential.CheckPasswordExpiration(_userlogin.UserId, _realPageId);

            Assert.False(response.IsError);
			Assert.Equal("User Password is already expired.", response.ErrorReason);
		}
		#endregion

		#region User All Security Questions
		[Fact]
		public void UserAllSecurityQuestions_Error_UserNameNullOrEmpty()
		{
			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response = _manageCredential.UserAllSecurityQuestions(null);

			Assert.True(response.IsError);
			Assert.Equal("User Name is not specified.", response.ErrorReason);
		}

		[Fact]
		public void UserAllSecurityQuestions_Error_NoQuestions()
		{
			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			var response = _manageCredential.UserAllSecurityQuestions(_enterpriseUserName);

			Assert.True(response.IsError);
			Assert.Equal("Error while getting security questions.", response.ErrorReason);
		}

		[Fact]
		public void ValidatePasswordForUser_Error_IncorrectUserName()
		{
			_enterpriseUserName = string.Empty;

			_manageCredential = new ManageCredential(
				_mockCredentialRepository.Object,
				_mockPasswordPolicyRepository.Object,
				_mockUserLoginRepository.Object,
				_mockManageUserLogin.Object,
				_mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim
			);

			ValidatePasswordResponse validatePasswordResponse = _manageCredential.ValidatePasswordForUser(_enterpriseUserName, string.Empty);

			Assert.True(validatePasswordResponse.IsError);
			Assert.Equal("User name is incorrect or not found.", validatePasswordResponse.ErrorReason);
		}

		public void Dispose()
		{
			//tear down
		}
		#endregion
	}
}
