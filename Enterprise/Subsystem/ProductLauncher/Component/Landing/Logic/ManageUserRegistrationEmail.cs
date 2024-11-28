using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage User Registration Email
    /// </summary>
    public class ManageUserRegistrationEmail : IManageUserRegistrationEmail
    {
        #region Private Variables
        private DefaultUserClaim _userClaim;
        private IManageEmail _emailLogic;
        private IContactMechanismRepository _contactMechanismRepository;
        private IManageCommunicationEvents _communicationEventsLogic;
        private IUserTokenRepository _userTokenRepository;
        private IManagePerson _personManager;
        private IUserLoginRepository _userLoginRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        #endregion

        #region Constructors
        /// <summary>
        /// Manage user registration Email Constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public ManageUserRegistrationEmail(DefaultUserClaim userClaim)
        {
            _emailLogic = new ManageEmail(userClaim);
            _contactMechanismRepository = new ContactMechanismRepository();
            _communicationEventsLogic = new ManageCommunicationEvents();
            _userTokenRepository = new UserTokenRepository();
            _personManager = new ManagePerson();
            _userLoginRepository = new UserLoginRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _userClaim = userClaim;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="manageEmail"></param>
        /// <param name="contactMechanismRepository"></param>
        /// <param name="manageCommunicationEvents"></param>
        /// <param name="userTokenRepository"></param>
        /// <param name="managePerson"></param>
        /// <param name="userLoginRepository"></param>
        /// <param name="productInternalSettingRepository"></param>
        public ManageUserRegistrationEmail(DefaultUserClaim userClaim, IManageEmail manageEmail, IContactMechanismRepository contactMechanismRepository, IManageCommunicationEvents manageCommunicationEvents, IUserTokenRepository userTokenRepository, IManagePerson managePerson, IUserLoginRepository userLoginRepository, IProductInternalSettingRepository productInternalSettingRepository)
        {
            _emailLogic = manageEmail;
            _contactMechanismRepository = contactMechanismRepository;
            _communicationEventsLogic = manageCommunicationEvents;
            _userTokenRepository = userTokenRepository;
            _personManager = managePerson;
            _userLoginRepository = userLoginRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _userClaim = userClaim;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="repository"></param>
        public ManageUserRegistrationEmail(DefaultUserClaim userClaim, IRepository repository)
        {
            _emailLogic = new ManageEmail(userClaim, repository);
            _contactMechanismRepository = new ContactMechanismRepository(repository);
            _communicationEventsLogic = new ManageCommunicationEvents(repository);
            _userTokenRepository = new UserTokenRepository(repository);
            _personManager = new ManagePerson(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _userClaim = userClaim;
        }

        #endregion

        #region Public methods
        public bool SendNewUserRegistrationEmail(IProfileDetail profile)
        {
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                Is3rdPartyIDP = profile.userLogin.Is3rdPartyIDP,
                LoginName = profile.userLogin.LoginName,
                RealPageId = profile.RealPageId,
                UserId = profile.userLogin.UserId,
                LastLogin = profile.userLogin.LastLogin
            };

            return SendNewUserRegistrationEmail(userLoginOnly, profile.organization[0].Name, profile.UserTypeId, profile.organization[0].PartyId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLoginOnly"></param>
        /// <param name="companyName"></param>
        /// <param name="userTypeId"></param>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        public bool SendNewUserRegistrationEmail(UserLoginOnly userLoginOnly, string companyName, int userTypeId, long organizationPartyId)
        {
            bool IsSendGridEnabled = false;
            bool IsUnifiedEmailEnabled = false;
            var userPerson = _personManager.GetPerson(userLoginOnly.RealPageId);
            var firstName = userPerson.FirstName;

            var emailAddress = userLoginOnly.LoginName;
            string correlationId = "";
            if (_userClaim != null)
            {
                correlationId = (_userClaim.CorrelationId != Guid.Empty) ? _userClaim.CorrelationId.ToString() : "";

            }

            if (EmailFormatValidation.IsValidEmail(emailAddress) // the email address appears to be valid
                && (userTypeId != UserTypeConstants.RegularUserNoEmail) // not UserNoEmailRole
                && (((userTypeId == UserTypeConstants.SuperUser || userTypeId == UserTypeConstants.RegularUser) && !userLoginOnly.Is3rdPartyIDP)
                || userTypeId == UserTypeConstants.ExternalUser))
            {
                try
                {
                    string userToken = "";
                    var emailTemplate = new CommunicationEmail();
                    var cesEmail = new Email();
                    int audienceTypeId = 0, purposeTypeId = 0;

                    //Generate a Token, build the Email, then Send if UserLogin is an Email
                    //Create an activity token for user validation
                    int activityId = (int)ActivityType.NewUserRegistration;

                    switch (userTypeId)
                    {
                        case (int)UserRoleType.RealPageEmployee:
                        case (int)UserRoleType.SuperUser:
                            audienceTypeId = (int)CommunicationEventAudienceType.SuperUser;
                            break;

                        case (int)UserRoleType.User:
                            audienceTypeId = (int)CommunicationEventAudienceType.RegularUser;
                            break;

                        case (int)UserRoleType.ExternalUser:
                            audienceTypeId = (int)CommunicationEventAudienceType.ExternalUser;
                            break;
                    }

                    var organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(userLoginOnly.RealPageId, null);

                    if (organizationList.FirstOrDefault(p => p != null && p.PartyId == organizationPartyId).PrimaryOrganization)
                    {
                        var primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);
                        if (primaryOrgStatus.IsPending.Value || primaryOrgStatus.IsExpired.Value || primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.Disabled)
                        {
                            userToken = _userTokenRepository.GetUserActivityToken(userLoginOnly.RealPageId, activityId, organizationPartyId);
                        }
                        else
                        {
                            audienceTypeId = (int)CommunicationEventAudienceType.RegularUser;
                        }
                    }
                    else
                    {
                        audienceTypeId = (int)CommunicationEventAudienceType.MultiCompanyUser;
                    }

                    Guid orgRealPageId = organizationList.FirstOrDefault(p => p.PartyId == organizationPartyId).RealPageId;

                    purposeTypeId = (int)CommunicationEventPurposeType.NewUserSetup;
                    emailTemplate = _emailLogic.GetEmailTemplate(audienceTypeId, purposeTypeId);

                    var logger = Log.Logger;
                    logger = logger.ForContext("CorrelationId", correlationId);
                    logger.Write(LogEventLevel.Information, "{ActionName} - {state}", propertyValue0: "SendNewUserRegistrationEmail", propertyValue1: $"email template generated - {userLoginOnly.RealPageId}");

                    IList<CommonAddress> contactMechanismList = _contactMechanismRepository.ListContactMechanismForPerson(orgRealPageId, "Email Notification");
                    IList<CommonAddress> contactMechanismToList = _contactMechanismRepository.ListContactMechanismForPerson(userLoginOnly.RealPageId, "Email Notification");
                    var senderEmailAddress = contactMechanismList[0].AddressString;
                    var PartyContactMechanismIdFrom = contactMechanismList[0].PartyContactMechanismId;
                    var PartyContactMechanismIdTo = contactMechanismToList[0].PartyContactMechanismId;

                    cesEmail = _emailLogic.CreateWelcomeEmail(userLoginOnly.LoginName, firstName, companyName, organizationPartyId, emailTemplate, userToken, senderEmailAddress, emailAddress);
                    Dictionary<string, object> logData = new Dictionary<string, object> { { "userToken", userToken }, { "cesEmail", cesEmail }, { "audienceTypeId", audienceTypeId } };
                    if (cesEmail.EmailBody != null)
                    {
                        if (logData?.Keys != null)
                        {
                            logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                        }
                        logger = logger.ForContext("ProductModule", this.GetType());
                        logger = logger.ForContext("CorrelationId", correlationId);
                        logger.Write(LogEventLevel.Information, "{ActionName} - {state}", propertyValue0: "SendNewUserRegistrationEmail", propertyValue1: $"email body generated - {userLoginOnly.RealPageId}");
                    }

                    DateTime utcStarted = DateTime.UtcNow;
                    var message = "";
                    RepositoryResponse communicationEventResponse = new RepositoryResponse();
                    try
                    {
                        string emailStatus = "";
#if (DEBUG)
                    emailStatus = "success";
#endif
                    if (string.IsNullOrEmpty(emailStatus))
                    {
                        emailStatus = EmailStatus(userLoginOnly, IsUnifiedEmailEnabled, cesEmail, firstName, ref IsSendGridEnabled);
                    }

                    DateTime utcEnded = DateTime.UtcNow;
             
                    //Save Communication Event
            
                   

                        if (emailStatus.Contains("success"))
                        {
                            communicationEventResponse = _communicationEventsLogic.CreateCommunicationEvent((int)EmailStatusType.EmailSuccess, PartyContactMechanismIdFrom, PartyContactMechanismIdTo, utcStarted, utcEnded, emailStatus);
                            message = $"SendNewUserRegistrationEmail - email sent - {userLoginOnly.RealPageId}";
                        }
                        else
                        {
                            communicationEventResponse = _communicationEventsLogic.CreateCommunicationEvent((int)EmailStatusType.EmailError, PartyContactMechanismIdFrom, PartyContactMechanismIdTo, utcStarted, utcEnded, emailStatus);
                            message = $"SendNewUserRegistrationEmail - email generation failed - {userLoginOnly.RealPageId}";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "SendNewUserRegistrationEmail Error", propertyValue1: $"email generation failed - {userLoginOnly.RealPageId}, Error message is {ex.Message}");

                    }

                    logger = logger.ForContext("CorrelationId", correlationId);
                    logger.Write(LogEventLevel.Information, "{ActionName} - {state}", propertyValue0: "SendNewUserRegistrationEmail", propertyValue1: message);

                   
                        long communicationEventId = communicationEventResponse.Id;
                        if (communicationEventResponse.Id != 0)
                        {
                            communicationEventResponse = _communicationEventsLogic.CreateCommunicationEventEmail(emailTemplate.CommunicationEmailTemplateId, communicationEventId);
                        }

                        if ((communicationEventResponse.Id != 0) && (!IsSendGridEnabled))
                        {
                            communicationEventResponse = _communicationEventsLogic.CreateCESCommunicationEventEmail(cesEmail.ClientUniqueID.ToString().ToUpper(), communicationEventId);
                        }
               


                    return true;
                }
                catch (Exception ex)
                {
                    var logger = Log.Logger;
                    logger = logger.ForContext("CorrelationId", correlationId);
                    logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "SendNewUserRegistrationEmail", propertyValue1: $"email generation failed - {userLoginOnly.RealPageId}");

                    return false;
                }
            }

            return true;
        }

        private string EmailStatus(UserLoginOnly userLoginOnly, bool IsUnifiedEmailEnabled, Email cesEmail, string firstName, ref bool isSendGridEnabled)
        {
            string emailStatus;
            var productSettingList = _productInternalSettingRepository.GetProductInternalSettings(productId: (int)ProductEnum.UnifiedPlatform);
            if ((productSettingList.Count > 0) && (productSettingList.ToList().Any(s => s.Name.Equals("IsSendGridEnabled", StringComparison.OrdinalIgnoreCase))))
            {
                isSendGridEnabled = productSettingList.ToList().FirstOrDefault(s => s.Name.Equals("IsSendGridEnabled", StringComparison.OrdinalIgnoreCase)).Value.Equals("1");
            }

            if ((productSettingList.Count > 0) && productSettingList.ToList().Any(s => s.Name.Equals("IsUnifiedEmailEnabled", StringComparison.OrdinalIgnoreCase)))
            {
                var UnifiedEmailSettings = productSettingList.FirstOrDefault(s => s.Name.Equals("IsUnifiedEmailEnabled", StringComparison.OrdinalIgnoreCase));
                IsUnifiedEmailEnabled = (UnifiedEmailSettings != null) ? UnifiedEmailSettings.Value.Trim() == "1" : true;
            }

            if (IsUnifiedEmailEnabled)
            {
                var emailModel = new EmailModel
                {
                    Subject = cesEmail.EmailSubject,
                    To = new List<UserEmail>
                {
                    new UserEmail
                    {
                        Email = cesEmail.EmailTo,
                        Name = firstName
                    }
                },

                    Body = cesEmail.EmailBody,
                    Bcc = new List<UserEmail>()
                };
                emailStatus = _emailLogic.SendEmailAsync(emailModel) ? "success" : "";
            }
            else
            {
                if (isSendGridEnabled)
                {
                    ISendGridEmail sendGridEmail = new SendGridEmail()
                    {
                        emailSubject = cesEmail.EmailSubject,
                        fromAddress = new EmailAddress()
                        {
                            email = cesEmail.EmailFrom,
                            name = cesEmail.EmailFrom
                        },
                        toAddress = new List<EmailAddress>()
                        {
                            new EmailAddress()
                            {
                                email = cesEmail.EmailTo,
                                name = cesEmail.EmailTo
                            }
                        },
                        message = cesEmail.EmailBody,
                        transId = userLoginOnly.UserId.ToString(),
                        category = "RegistrationEmail"
                    };
                    emailStatus = _emailLogic.SendGridEmail(sendGridEmail);
                }
                else
                {
                    emailStatus = _emailLogic.SendEmail(cesEmail);
                }
            }

            return emailStatus;
        }

        /// <summary>
        /// Used to send the password reset email to the given user
        /// </summary>
        /// <param name="profileDetail"></param>
        /// <returns></returns>
        public bool SendPasswordResetEmail(ProfileDetail profileDetail)
        {
            bool IsSendGridEnabled = false;
            bool IsUnifiedEmailEnabled = false;
            var userPerson = _personManager.GetPerson(profileDetail.RealPageId);
            var userLoginOnly = _userLoginRepository.GetUserLoginOnly(profileDetail.RealPageId);

            var firstName = userPerson.FirstName;

            var emailAddress = userLoginOnly.LoginName;

            if (profileDetail.UserTypeId != UserTypeConstants.RegularUserNoEmail // not UserNoEmailRole
                && !userLoginOnly.Is3rdPartyIDP // Not a user using 3rd party IDP
                && EmailFormatValidation.IsValidEmail(emailAddress) // the email address appears to be valid
            )
            {
                try
                {
                    //Generate a Token, build the Email, then Send if UserLogin is an Email
                    //Create an activity token for user validation
                    int activityId = (int)ActivityType.NewUserRegistration;

                    var organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(userLoginOnly.RealPageId, null);

                    var primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, profileDetail.userLogin.LastLogin, 0, true);
                    var userToken = _userTokenRepository.GetUserActivityToken(userLoginOnly.RealPageId, activityId, _userClaim.OrganizationPartyId);

                    var audienceTypeId = (int)CommunicationEventAudienceType.RegularUser;
                    var purposeTypeId = (int)CommunicationEventPurposeType.PasswordReset;
                    var emailTemplate = _emailLogic.GetEmailTemplate(audienceTypeId, purposeTypeId);

                    var logger = Log.Logger;
                    logger = logger.ForContext("CorrelationId", _userClaim.CorrelationId);
                    logger.Write(LogEventLevel.Information, "{ActionName} - {state}", propertyValue0: "SendPasswordResetEmail", propertyValue1: $"email template generated - {userLoginOnly.RealPageId}");

                    IList<CommonAddress> contactMechanismList = _contactMechanismRepository.ListContactMechanismForPerson(_userClaim.OrganizationRealPageGuid, "Email Notification");
                    IList<CommonAddress> contactMechanismToList = _contactMechanismRepository.ListContactMechanismForPerson(userLoginOnly.RealPageId, "Email Notification");
                    var senderEmailAddress = contactMechanismList[0].AddressString;
                    var PartyContactMechanismIdFrom = contactMechanismList[0].PartyContactMechanismId;
                    var PartyContactMechanismIdTo = contactMechanismToList[0].PartyContactMechanismId;

                    var cesEmail = _emailLogic.CreateWelcomeEmail(userLoginOnly.LoginName, firstName, _userClaim.OrganizationName, _userClaim.OrganizationPartyId, emailTemplate, userToken, senderEmailAddress, emailAddress);
                    Dictionary<string, object> logData = new Dictionary<string, object> { { "userToken", userToken }, { "cesEmail", cesEmail }, { "audienceTypeId", audienceTypeId } };
                    if (cesEmail.EmailBody != null)
                    {
                        logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                        logger = logger.ForContext("ProductModule", this.GetType());
                        logger = logger.ForContext("CorrelationId", _userClaim.CorrelationId);
                        logger.Write(LogEventLevel.Information, "{ActionName} - {state}", propertyValue0: "SendPasswordResetEmail", propertyValue1: $"email body generated - {userLoginOnly.RealPageId}");
                    }

                    DateTime utcStarted = DateTime.UtcNow;
                    string emailStatus = "";
#if (DEBUG)
                    emailStatus = "success";
#endif
                    if (string.IsNullOrEmpty(emailStatus))
                    {
                        emailStatus = EmailStatus(userLoginOnly, IsUnifiedEmailEnabled, cesEmail, firstName, ref IsSendGridEnabled);
                    }

                    DateTime utcEnded = DateTime.UtcNow;

                    //Save Communication Event
                    RepositoryResponse communicationEventResponse = new RepositoryResponse();
                    var message = "";
                    if (emailStatus.Contains("success"))
                    {
                        communicationEventResponse = _communicationEventsLogic.CreateCommunicationEvent((int)EmailStatusType.EmailSuccess, PartyContactMechanismIdFrom, PartyContactMechanismIdTo, utcStarted, utcEnded, emailStatus);
                        message = $"SendPasswordResetEmail - email sent - {userLoginOnly.RealPageId}";
                    }
                    else
                    {
                        communicationEventResponse = _communicationEventsLogic.CreateCommunicationEvent((int)EmailStatusType.EmailError, PartyContactMechanismIdFrom, PartyContactMechanismIdTo, utcStarted, utcEnded, emailStatus);
                        message = $"SendPasswordResetEmail - email generation failed - {userLoginOnly.RealPageId}";
                        return false;
                    }

                    logger = logger.ForContext("CorrelationId", _userClaim.CorrelationId);
                    logger.Write(LogEventLevel.Information, "{ActionName} - {state}", propertyValue0: "SendPasswordResetEmail", propertyValue1: message);

                    long communicationEventId = communicationEventResponse.Id;
                    if (communicationEventResponse.Id != 0)
                    {
                        communicationEventResponse = _communicationEventsLogic.CreateCommunicationEventEmail(emailTemplate.CommunicationEmailTemplateId, communicationEventId);
                    }

                    if ((communicationEventResponse.Id != 0) && (!IsSendGridEnabled))
                    {
                        communicationEventResponse = _communicationEventsLogic.CreateCESCommunicationEventEmail(cesEmail.ClientUniqueID.ToString().ToUpper(), communicationEventId);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    var logger = Log.Logger;
                    logger = logger.ForContext("CorrelationId", _userClaim.CorrelationId);
                    logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "SendPasswordResetEmail", propertyValue1: $"email generation failed - {userLoginOnly.RealPageId} error: {ex.Message}");

                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}