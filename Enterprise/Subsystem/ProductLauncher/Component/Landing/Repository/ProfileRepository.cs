using Dapper;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;


namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    /// <summary>
    /// Profile Repository
    /// </summary>
    public class ProfileRepository : BaseRepository, IProfileRepository
    {
        private DefaultUserClaim _userClaim;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IPartyRelationshipRepository _partyRelationshipRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IManagePerson _managePerson;
        IUserRepository _userRepository;

        /// <summary>
        /// Used to filter user list results
        /// </summary>
        public enum UserListTypeFilter
        {
            /// <summary>
            /// View All Users
            /// </summary>
            ViewAllUsers = 0,

            /// <summary>
            /// Exclude Support Users
            /// </summary>
            ExcludeSupportUsers = 1,

            /// <summary>
            /// Exclude Support And SuperUsers
            /// </summary>
            ExcludeSupportAndSuperUsers = 2,

            /// <summary>
            /// Only return users with the same operator
            /// </summary>
            OperatorUsers = 3
        }

        #region Constructor
        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        public ProfileRepository(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler) : base(repository)
        {
            _userClaim = userClaim;
            _manageUserLogin = new ManageUserLogin(repository, userClaim, messageHandler);
            _partyRelationshipRepository = new PartyRelationshipRepository(repository);
            _productRepository = new ProductRepository(repository, userClaim);
            _personaRepository = new PersonaRepository(repository, userClaim);
            _organizationRepository = new OrganizationRepository(repository);
            _managePerson = new ManagePerson(repository);
        }

        /// <summary>
        /// Profile base Constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public ProfileRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _userClaim = userClaim;
            _manageUserLogin = new ManageUserLogin(_userClaim);
            _partyRelationshipRepository = new PartyRelationshipRepository();
            _productRepository = new ProductRepository(_userClaim);
            _personaRepository = new PersonaRepository(_userClaim);
            _organizationRepository = new OrganizationRepository();
            _userRepository = new UserRepository(userClaim);
            _managePerson = new ManagePerson();
        }
        #endregion

        #region public Profile methods
        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">profile object of the parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateProfile(Guid realPageId, IProfile profile)
        {
            int roleTypeIdFrom = 0;
            bool industryStandardJobChanged = false;
            bool residentPortalAssignedToUser = false;
            bool IsSuperUser = false;
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            //IPersonaRepository personaRepository = new PersonaRepository();
            bool customJobTitleChanged = false;
            bool isKnockProductAssignedToUser = false;
            bool isPhoneNumberChange = false;

            //get Organization Enterprise guid from Persona
            Guid organizationRealPageId = Guid.Empty;
            long personaId = _personaRepository.GetActivePersonaId(realPageId);
            Persona persona = _personaRepository.GetPersona(personaId);
            var toUserLogInfo = GetUserActivityLogInfo(personaId);
            UserDetails impersonatorUserInfo = _userClaim.ImpersonatedBy == Guid.Empty ? null : _userRepository.GetUserDetails(null, _userClaim.ImpersonatedBy.ToString());
            if (persona != null)
            {
                organizationRealPageId = persona.Organization.RealPageId;
            }

            //Find the User Type
            PartyRelationship partyRelationship = _partyRelationshipRepository.GetPartyRelationship(realPageId, organizationRealPageId, null, null, "User Type");
            if (partyRelationship != null)
            {
                IsSuperUser = (partyRelationship.RoleTypeIdFrom == (int)UserRoleType.SuperUser) ? true : false;
                if (!IsSuperUser)
                {
                    //Regular user has access to Resident Portal?
                    IList<PersonaProductUserDetails> personaProductUserDetailsList = _productRepository.GetAssignedProductsByPersona(persona, null, null);
                    if (personaProductUserDetailsList != null)
                    {
                        residentPortalAssignedToUser = personaProductUserDetailsList.Any(p => p.ProductId == (int)ProductEnum.ResidentPortal);
                        isKnockProductAssignedToUser = personaProductUserDetailsList.Any(p => p.ProductId == (int)ProductEnum.KnockCRM);
                    }
                }
            }

            using (var repository = GetRepository())
            {
                //Begin the transaction
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    UserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
                    if (_userClaim.ImpersonatedBy != Guid.Empty)
                    {
                        impersonatorUserLoginOnly = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = _userClaim.ImpersonatedBy });
                    }

                    var oldPerson = repository.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, new { realPageId });
                    customJobTitleChanged = oldPerson.Title != profile.Title ? true : false;

                    //Setup the parameter values to update the person's info
                    dynamic param = new
                    {
                        RealPageId = realPageId,
                        Title = profile.Title,
                        FirstName = profile.FirstName,
                        MiddleName = profile.MiddleName,
                        LastName = profile.LastName,
                        Suffix = profile.Suffix,
                        PreferredContactMethodId = profile.PreferredContactMethodId
                    };
                    //Update the person's info
                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, param);
                    if (repositoryResponse.Id == 0)
                    {
                        repositoryResponse.ErrorMessage = "Update profile Error: Update person failed.";
                    }
                    else
                    {
                        IList<PreferredContactMethod>  preferredContactMethodList = repository.GetMany<PreferredContactMethod>(StoredProcNameConstants.SP_ListPreferredContactMethods, null).ToList();
                    
                        if (oldPerson.Title != profile.Title)
                        {
                            AuditActivityLog(oldPerson.Title, profile.Title, "Company Job Title", toUserLogInfo, impersonatorUserInfo);
                        }

                        if (oldPerson.PreferredContactMethodId != profile.PreferredContactMethodId && profile.PreferredContactMethodId > 0)
                        {
                            string oldValue = preferredContactMethodList.Where(p => p.PreferredContactMethodId == oldPerson.PreferredContactMethodId).Select(p => p.Name).FirstOrDefault();
                            string newValue = preferredContactMethodList.Where(p => p.PreferredContactMethodId == profile.PreferredContactMethodId).Select(p => p.Name).FirstOrDefault();
                            AuditActivityLog(oldValue, newValue, "Preferred Contact Method", toUserLogInfo, impersonatorUserInfo);
                        }
                        //get the Person Employment RoleTypeId
                        string roleTypeName = null;
                        string relationshipTypeName = "Employment";
                        dynamic paramRelType = new
                        {
                            RealPageIdFrom = realPageId,
                            RealPageIdTo = organizationRealPageId,
                            RoleTypeName = roleTypeName,
                            RelationshipTypeName = relationshipTypeName
                        };
                        PartyRelationship relationshipType = repository.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, paramRelType);
                        //get the person to organization Job title relationship roletype if exists
                        roleTypeIdFrom = ((relationshipType != null) && (relationshipType.RoleTypeIdFrom > 0)) ? relationshipType.RoleTypeIdFrom : 0;
                        var userpersona = repository.GetOne<PartyRole>(StoredProcNameConstants.SP_GetPartyRoleByRealPageId, new { realPageId });

                        //Job Title
                        if ((profile.PartyRole != null) && (profile.PartyRole.PartyRoleId > 0))
                        {
                            //get the Organization Employer RoleTypeId
                            dynamic paramOrgRole = new
                            {
                                RoleTypeName = "Organization Role"
                            };
                            IList<RoleType> roleTypes = repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, paramOrgRole);
                            var Employer = roleTypes.SingleOrDefault<RoleType>(p => p.Name == "Employer");
                            int roleTypeIdTo = (int)Employer.PartyRoleTypeId; //Employer

                            //Unlink the person to organization Job title relationship if exists
                            //Link the person to organization Job title relationship
                            dynamic paramRole = new
                            {
                                PersonRealPageId = realPageId,
                                OrganizationRealPageId = organizationRealPageId,
                                UnlinkRoleTypeIdFrom = roleTypeIdFrom,
                                LinkRoleTypeIdFrom = profile.PartyRole.RoleTypeId,
                                RoleTypeIdTo = roleTypeIdTo
                            };
                            RepositoryResponse RoleId = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, paramRole);

                            //Update Job title parameter values
                            param = new
                            {
                                PartyRoleId = profile.PartyRole.PartyRoleId,
                                RoleTypeID = profile.PartyRole.RoleTypeId
                            };
                            //Update the person's Job title
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePartyRoleByRealPageId, param);
                        }
                        else
                        {
                            //Add Job title parameter values
                            param = new
                            {
                                RealPageId = realPageId,
                                RoleTypeId = profile.PartyRole.RoleTypeId
                            };
                            //Assign Job tile to person
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePartyRoleByRealPageId, param);
                        }
                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = "Update profile Error: Job Title failed.";
                        }
                        else
                        {
                            if (userpersona != null && userpersona.RoleTypeId != profile.PartyRole.RoleTypeId)
                            {
                                var roleTypeList = new List<RoleType>();
                                int? organizationPartyID = null;
                                param = new
                                {
                                    RoleTypeName = "person role",
                                    OrganizationPartyID = organizationPartyID
                                };
                                roleTypeList = repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, param);
                                string oldValue = roleTypeList.Where(r => r.PartyRoleTypeId == roleTypeIdFrom).Select(r => r.Name).FirstOrDefault();
                                string newValue = roleTypeList.Where(r => r.PartyRoleTypeId == profile.PartyRole.RoleTypeId).Select(r => r.Name).FirstOrDefault();
                                AuditActivityLog(oldValue, newValue, "Industry Job Title", toUserLogInfo, impersonatorUserInfo);
                            }

                            IList<TelecommunicationNumber> telecommunicationslists = repository.GetMany<TelecommunicationNumber>(StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson, new { realPageId }).ToList();
                            //isPhoneNumberChange = isPhoneNumberUpdated(profile.TelecommunicationNumber.ToList(), telecommunicationslists.ToList());

                            industryStandardJobChanged = profile.PartyRole.RoleTypeId != roleTypeIdFrom ? true : false;
                            ITelecommunicationNumberRepository telecommunicationNumberRepository = new TelecommunicationNumberRepository();
                            ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
                            IContactMechanismRepository contactMechanismRepository = new ContactMechanismRepository();
                            IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
                            string ContactMechanismUsageTypeName = "phone type";
                            dynamic paramEmail = new
                            {
                                ContactMechanismUsageTypeName
                            };
                            IList<ContactMechanismUsageType> ContactMechanismUsageTypes = repository.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, paramEmail);
                            //Loop through all the Telecommunication numbers
                            foreach (ITelecommunicationNumber phone in profile.TelecommunicationNumber.ToList())
                            {
                                telecommunicationNumber.ContactMechanismId = phone.ContactMechanismId;
                                telecommunicationNumber.AreaCode = phone.AreaCode;
                                telecommunicationNumber.CountryCode = phone.CountryCode;
                                telecommunicationNumber.PhoneNumber = phone.PhoneNumber;
                                telecommunicationNumber.ISOCode = phone.ISOCode;
                                telecommunicationNumber.IsDefault = phone.IsDefault;
                                if (phone.IsDeleted && !string.IsNullOrEmpty(phone.PhoneNumber))
                                {
                                    var phoneType = profile.TelecommunicationNumber.FirstOrDefault(t => t.ContactMechanismId == phone.ContactMechanismId);
                                    string newPhoneType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == phoneType.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                    AuditActivityLog($"{phone.ISOCode}({phone.CountryCode}) {phone.PhoneNumber},{newPhoneType}", " ", "Deleted Phone Number", toUserLogInfo, impersonatorUserInfo);
                                }
                                if (phone.ContactMechanismId == 0 && !string.IsNullOrEmpty(phone.PhoneNumber))
                                {
                                    var phoneType = profile.TelecommunicationNumber.FirstOrDefault(t => t.ContactMechanismId == phone.ContactMechanismId);
                                    string PhoneNumberType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == phoneType.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                    AuditActivityLog($"{phone.ISOCode}({phone.CountryCode}) {phone.PhoneNumber},{PhoneNumberType}", " ", "Added Phone Number", toUserLogInfo, impersonatorUserInfo);
                                } //New Telecommunication number
                                if (phone.ContactMechanismId == 0)
                                {
                                    if (phone.IsDeleted)
                                    {
                                        profile.TelecommunicationNumber.Remove((TelecommunicationNumber)phone);
                                    }
                                    else
                                    {
                                        //Is the Phone and Type valid
                                        if ((phone.PhoneNumber.Trim().Length > 0) && (phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0))
                                        {
                                            //Add a new phone
                                            //Create the Contact Mechanism
                                            param = new
                                            {
                                                ContactMechanismId = 0
                                            };
                                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);

                                            if (repositoryResponse.Id == 0)
                                            {
                                                repositoryResponse.ErrorMessage = "Update profile Error: Create Contact Mechanism failed.";
                                            }
                                            else
                                            {
                                                phone.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);
                                                telecommunicationNumber.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);
                                                //Associate the Contact Mechanism to a Party
                                                param = new
                                                {
                                                    RealPageId = realPageId,
                                                    PartyContactMechanismId = 0,
                                                    ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                                                    FromDate = utcNow,
                                                    ThruDate = utcMaxValue
                                                };
                                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                                if (repositoryResponse.Id == 0)
                                                {
                                                    repositoryResponse.ErrorMessage = "Update profile Error: Create Contact Mechanism failed.";
                                                }
                                                else
                                                {
                                                    phone.PartyContactMechanismId = repositoryResponse.Id;
                                                    //Assign a usage type to the Contact Mechanism
                                                    partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                                                    param = new
                                                    {
                                                        PartyContactMechanismId = partyContactMechanism.PartyContactMechanismId,
                                                        ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                                                    };
                                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
                                                    if (repositoryResponse.Id == 0)
                                                    {
                                                        repositoryResponse.ErrorMessage = "Update profile Error: Link UsageType to Party Contact Mechanism failed.";
                                                    }
                                                    isPhoneNumberChange = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (phone.IsDeleted)
                                    {
                                        param = new
                                        {
                                            PartyContactMechanismID = phone.PartyContactMechanismId,
                                            RealPageId = profile.RealPageId

                                        };
                                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_ExpirePartyContactMechanism, param);
                                        if (repositoryResponse.Id == 0)
                                        {
                                            repositoryResponse.ErrorMessage = "Update profile Error: Link Contact Mechanism Expire for a Party failed.";
                                        }
                                        else
                                        {
                                            profile.TelecommunicationNumber.Remove((TelecommunicationNumber)phone);
                                            isPhoneNumberChange = true;
                                        }
                                    }
                                    else
                                    {
                                        if ((phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0) && (phone.PhoneNumber.Trim().Length > 0))
                                        {
                                            //Set the PhoneType for the added/updated Telecommunication number
                                            param = new
                                            {
                                                PartyContactMechanismID = phone.PartyContactMechanismId,
                                                ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                                            };

                                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateContactMechanismUsageForParty, param);
                                            if (repositoryResponse.Id == 0)
                                            {
                                                repositoryResponse.ErrorMessage = "Update profile Error: Update Contact Mechanism Usage For Party failed.";
                                            }
                                        }
                                        else
                                        {
                                            //Expire the Telecommunication number is the phone number or type is cleared
                                            param = new
                                            {
                                                RealPageId = realPageId,
                                                PartyContactMechanismId = phone.PartyContactMechanismId,
                                                ContactMechanismId = phone.ContactMechanismId,
                                                FromDate = utcNow,
                                                ThruDate = utcNow
                                            };
                                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                        }
                                        if (repositoryResponse.Id == 0)
                                        {
                                            repositoryResponse.ErrorMessage = "Update profile Error: Link Contact Mechanism To a Party failed.";
                                        }
                                        isPhoneNumberChange = true;
                                    }
                                }

                                if (!phone.IsDeleted && telecommunicationNumber.ContactMechanismId > 0 && phone.PhoneNumber.Trim().Length > 0)
                                {
                                    param = new
                                    {
                                        ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                                        AreaCode = telecommunicationNumber.AreaCode,
                                        CountryCode = telecommunicationNumber.CountryCode,
                                        PhoneNumber = telecommunicationNumber.PhoneNumber,
                                        ISOCode = telecommunicationNumber.ISOCode,
                                        Default = telecommunicationNumber.IsDefault
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateTelecommunicationNumber, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update profile Error: Link a telecommunication number details for a person failed.";
                                    }
                                    foreach (var existingPhone in telecommunicationslists.ToList())
                                    {

                                        if (existingPhone.ContactMechanismId == telecommunicationNumber.ContactMechanismId)
                                        {
                                            if (existingPhone.PhoneNumber != telecommunicationNumber.PhoneNumber)
                                            {
                                                isPhoneNumberChange = true;
                                            }
                                            var phoneType = profile.TelecommunicationNumber.FirstOrDefault(t => t.ContactMechanismId == phone.ContactMechanismId);
                                            string oldPhoneType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == existingPhone.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                            string newPhoneType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == phoneType.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                            if (existingPhone.PhoneNumber != telecommunicationNumber.PhoneNumber || oldPhoneType != newPhoneType || existingPhone.CountryCode != telecommunicationNumber.CountryCode)
                                            {
                                                var newValue = $"{telecommunicationNumber.ISOCode}({telecommunicationNumber.CountryCode})  {telecommunicationNumber.PhoneNumber},{newPhoneType}";
                                                var oldValue = $"{existingPhone.ISOCode}({existingPhone.CountryCode}) {existingPhone.PhoneNumber},{oldPhoneType}";
                                                AuditActivityLog(oldValue,newValue, "Phone Number",toUserLogInfo,impersonatorUserInfo);
                                            }
                                        }
                                    }
                                    }
                                }
                            
                            if (profile.TelecommunicationNumber.Any())
                            {
                                bool response = UpdateContactPreference(repository, profile.RealPageId, profile.TelecommunicationNumber.ToList());
                                if (!response)
                                {
                                    repositoryResponse.ErrorMessage = "Update profile Error: Create Contact Mechanism Preference failed.";
                                }
                            }
                            if(profile.TelecommunicationNumber.Any() && profile.TelecommunicationNumber.Any())
                            {
                                var oldDefault = telecommunicationslists.FirstOrDefault(t => t.IsDefault);
                                var newDefault = profile.TelecommunicationNumber.FirstOrDefault(t => t.IsDefault);
                                if (oldDefault != null && newDefault != null)
                                {
                                    string oldPhoneType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == oldDefault.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                    string newPhoneType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == newDefault.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                    if (oldDefault.PhoneNumber != newDefault.PhoneNumber)
                                    {
                                        AuditActivityLog($"{oldDefault.ISOCode}({oldDefault.CountryCode})  {oldDefault.PhoneNumber},{oldPhoneType}", $"{newDefault.ISOCode}({newDefault.CountryCode})  {newDefault.PhoneNumber},{newPhoneType}", "Default Phone Number", toUserLogInfo, impersonatorUserInfo);
                                    }
                                }
                            }

                            IList<ElectronicAddress> oldEmailContact = repository.GetMany<ElectronicAddress>(StoredProcNameConstants.SP_ListEmailsForPerson, new { realPageId }).ToList();
                            if (!oldEmailContact.Any())
                            {
                                ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType();
                                contactMechanismUsageType.ParentContactMechanismUsageTypeId = 300;
                                contactMechanismUsageType.ContactMechanismUsageTypeId = 302;
                                contactMechanismUsageType.Name = "Email";
                                ElectronicAddress electronicAdd = new ElectronicAddress();
                                electronicAdd.AddressType = "Email";
                                electronicAdd.AddressString = "";
                                electronicAdd.contactMechanismUsageType = contactMechanismUsageType;
                                oldEmailContact.Add(electronicAdd);
                            }

                            IManageElectronicAddress electronicAddressLogic = new ManageElectronicAddress();
                            IElectronicAddress electronicAddress = new ElectronicAddress();
                            electronicAddress = profile.EmailContacts[0];

                            if (electronicAddress.ContactMechanismId == 0 && profile.EmailContacts[0].AddressString.Trim().Length > 0)
                            {
                                param = new
                                {
                                    ContactMechanismId = 0
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);

                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update profile Error: Create Contact Mechanism failed for Electronic Email Address.";
                                }
                                else
                                {
                                    electronicAddress.ContactMechanismId = (int)repositoryResponse.Id;
                                    //Associate the Contact Mechanism to a Party
                                    param = new
                                    {
                                        RealPageId = realPageId,
                                        PartyContactMechanismId = 0,
                                        ContactMechanismId = repositoryResponse.Id,
                                        FromDate = utcNow,
                                        ThruDate = utcMaxValue
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update profile Error: Create Contact Mechanism failed for Electronic Email Address.";
                                    }
                                    else
                                    {
                                        electronicAddress.PartyContactMechanismId = (int)repositoryResponse.Id;
                                        param = new
                                        {
                                            PartyContactMechanismId = repositoryResponse.Id,
                                            ContactMechanismUsageTypeId = profile.EmailContacts[0].contactMechanismUsageType.ContactMechanismUsageTypeId
                                        };
                                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
                                        if (repositoryResponse.Id == 0)
                                        {
                                            repositoryResponse.ErrorMessage = "Update profile Error: Link UsageType to Party Contact Mechanism failed for Electronic Email Address.";
                                        }
                                        else
                                        {
                                            electronicAddress.ContactMechanismUsageTypeId = (int)repositoryResponse.Id;
                                        }
                                    }
                                }
                            }

                            if (profile.EmailContacts[0].AddressString.Trim().Length > 0)
                            {
                                param = new
                                {
                                    ContactMechanismId = profile.EmailContacts[0].ContactMechanismId,
                                    ElectronicAddressString = profile.EmailContacts[0].AddressString,
                                    ElectronicAddressType = profile.EmailContacts[0].AddressType
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update profile Error: Link a Electronic Email Address details for a person failed.";
                                }
                            }
                            else
                            {
                                if (electronicAddress.PartyContactMechanismId != 0)
                                {
                                    param = new
                                    {
                                        PartyContactMechanismID = electronicAddress.PartyContactMechanismId,
                                        RealPageId = profile.RealPageId

                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_ExpirePartyContactMechanism, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update profile Error: Link Contact Mechanism Expire for a Party failed for Electronic Email Address.";
                                    }
                                    else
                                    {
                                        electronicAddress.ContactMechanismId = 0;
                                        electronicAddress.ContactMechanismUsageTypeId = 0;
                                        electronicAddress.PartyContactMechanismId = 0;
                                    }
                                }
                            }
                            var OldSecondaryEmail = oldEmailContact.Where(r => r.ContactMechanismUsageTypeId == 302).Select(r => r.AddressString).FirstOrDefault();
                            if(OldSecondaryEmail == null)
                            {
                                OldSecondaryEmail = "";
                            }
                            if (OldSecondaryEmail != profile.EmailContacts[0].AddressString)
                            {
                                AuditActivityLog(OldSecondaryEmail, profile.EmailContacts[0].AddressString, "Secondary Email", toUserLogInfo, impersonatorUserInfo);
                            }
                        }
                    }

                    if (!IsSuperUser && (industryStandardJobChanged || customJobTitleChanged) && residentPortalAssignedToUser)
                    {
                        string saveProductBatchError = "Save Product User Profile/Type Error: ";
                        //Industry Standard Job title got Set/Updated and the Regular user (Staff Role) has access to Resident Portal
                        ProductBatch productBatch = new ProductBatch()
                        {
                            ProductId = (int)ProductEnum.ResidentPortal,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true }
                        };
                        SaveProductBatch(repository, productBatch, null, saveProductBatchError, _userClaim.PersonaId, personaId, _userClaim.UserRealPageGuid, null, JsonConvert.SerializeObject(productBatch.InputJson), impersonatorUserLoginOnly.UserId, (int)BatchProcessType.ProfileUpdate);
                    }
                    if (isPhoneNumberChange && isKnockProductAssignedToUser)
                    {
                        string saveProductBatchError = "Save Product User Profile/Type Error: ";

                        ProductBatch productBatch = new ProductBatch()
                        {
                            ProductId = (int)ProductEnum.KnockCRM,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true }
                        };
                        SaveProductBatch(repository, productBatch, null, saveProductBatchError, _userClaim.PersonaId, personaId, _userClaim.UserRealPageGuid, null, JsonConvert.SerializeObject(productBatch.InputJson), impersonatorUserLoginOnly.UserId, (int)BatchProcessType.ProfileUpdate);
                    }
                }
                catch (Exception exception)
                {
                    repositoryResponse.Id = 0;
                    repositoryResponse.ErrorMessage = "Update profile Error: " + exception.Message;
                }
                finally
                {
                    if (repositoryResponse.ErrorMessage.Length == 0)
                    {
                        //Commit and end transaction.
                        repository.UnitOfWork.Commit();
                    }
                    else
                    {
                        //Rollback transaction and dispose it.
                        repository.UnitOfWork.Rollback();
                    }
                }
                return repositoryResponse;
            }
        }

        /// <summary>
        /// Returns a list of persons 
        /// </summary>
        /// <param name="organizationActiveProductIdList">List of product ids</param>
        /// <param name="realPageId">Organization realpage uniqueidentifier</param>
        /// <param name="parentPartyRoleTypeId">PartyRole parentId</param>
        /// <param name="dataFilterSort">Data Filtering and Sorting</param>
        /// <param name="isExport">flag to check user list export</param>

        /// <returns>List of Person</returns>
        public IList<ProfileDetail> ListPersons(IList<int> organizationActiveProductIdList, Guid? realPageId = null, int? parentPartyRoleTypeId = null, RequestParameter dataFilterSort = null, bool isExport = false)
        {
            var filterUserList = UserListTypeFilter.ExcludeSupportAndSuperUsers;
            var productInternalSettingRepository = new ProductInternalSettingRepository();
            var lstProductsWithDatasharedProduct = productInternalSettingRepository.GetProductSettingByType(SettingConstants.SharedProductSettingName);
            if (lstProductsWithDatasharedProduct != null && lstProductsWithDatasharedProduct.Any())
            {
                foreach (var product in lstProductsWithDatasharedProduct)
                {
                    if (organizationActiveProductIdList.Contains(product.ProductId) && !organizationActiveProductIdList.Contains(int.Parse(product.Value)))
                    {
                        organizationActiveProductIdList.Add(int.Parse(product.Value));
                    }
                }
            }
            if (_userClaim.UserRealPageGuid != Guid.Empty)
            {
                var partyRelationship = _partyRelationshipRepository.GetPartyRelationship(_userClaim.UserRealPageGuid, _userClaim.OrganizationRealPageGuid, null, null, "User Type");
                bool isSuperUser = false;
                if (partyRelationship != null)
                {
                    isSuperUser = partyRelationship.RoleTypeIdFrom == (int)UserRoleType.SuperUser;
                }

                //UserLogin ul = _manageUserLogin.GetUserLogin(_userClaim.UserRealPageGuid);
                if (isSuperUser)
                {
                    filterUserList = UserListTypeFilter.ExcludeSupportUsers;
                }
                if (_userClaim.RealPageEmployee || (_userClaim.IsRPEmployee && _userClaim.OrganizationRealPageGuid != DefaultUserClaim.EmployeeCompanyRealPageId) || _userClaim.ImpersonatedBy != Guid.Empty)
                {
                    filterUserList = UserListTypeFilter.ViewAllUsers;
                }

                // should we use this or always filter?
                //var organizationDetails = _organizationRepository.GetOrganization(null, _userClaim.OrganizationPartyId);
                //if (organizationDetails.EnablePrimaryPropertiesAndEnterpriseRoles == 1)
                {
                    var externalUserRelationship = GetExternalUserRelationship(_userClaim.OrganizationPartyId, _userClaim.UserId);
                    if (externalUserRelationship != null && !string.IsNullOrEmpty(externalUserRelationship.OperatorCode) && !string.IsNullOrEmpty(externalUserRelationship.OperatorValue))
                    {
                        filterUserList = UserListTypeFilter.OperatorUsers;
                        // filter to just this operator and external users only
                        if (dataFilterSort != null)
                        {
                            if (dataFilterSort.FilterBy != null)
                            {
                                if (dataFilterSort.FilterBy.Keys.Any(p => p.Equals("Operator", StringComparison.OrdinalIgnoreCase)))
                                {
                                    var keyName = dataFilterSort.FilterBy.Keys.First(p => p.Equals("Operator", StringComparison.OrdinalIgnoreCase));
                                    dataFilterSort.FilterBy.Remove(keyName);
                                    dataFilterSort.FilterBy.Add("Operator", $"{externalUserRelationship.OperatorCode}|{externalUserRelationship.OperatorValue}");
                                }
                                else
                                {
                                    dataFilterSort.FilterBy.Add("Operator", $"{externalUserRelationship.OperatorCode}|{externalUserRelationship.OperatorValue}");
                                }
                                if (dataFilterSort.FilterBy.Keys.Any(p => p.Equals("UserType", StringComparison.OrdinalIgnoreCase)))
                                {
                                    var keyName = dataFilterSort.FilterBy.Keys.First(p => p.Equals("UserType", StringComparison.OrdinalIgnoreCase));
                                    dataFilterSort.FilterBy.Remove(keyName);
                                    dataFilterSort.FilterBy.Add("userType", "405");
                                }
                                else
                                {
                                    dataFilterSort.FilterBy.Add("userType", "405");
                                }
                            }
                            else
                            {
                                dataFilterSort.FilterBy = new Dictionary<string, string> { { "Operator", $"{externalUserRelationship.OperatorCode}|{externalUserRelationship.OperatorValue}" }, { "userType", "405" } };
                            }
                        }
                        else
                        {
                            dataFilterSort = new RequestParameter
                            {
                                FilterBy = new Dictionary<string, string> { { "Operator", $"{externalUserRelationship.OperatorCode}|{externalUserRelationship.OperatorValue}" }, {"userType", "405"} }
                            };
                        }
                    }

                    if (externalUserRelationship != null && externalUserRelationship.ThirdPartyRelationShipId == 10)
                    {
                        if (dataFilterSort.FilterBy.Keys.Contains("userType"))
                        {
                            dataFilterSort.FilterBy["userType"] = "404";
                        }
                    }

                }
            }

            string organizationActiveProducts = string.Join(",", organizationActiveProductIdList);
            IList<FilterTableType> assignedProducts = new List<FilterTableType>()
            {
                new FilterTableType()
                {
                    ColumnName = "ProductId",
                    SearchValue = organizationActiveProducts
                }
            };
            string assignedProductsJson = string.Empty;
            if (assignedProducts.Count > 0)
            {
                assignedProductsJson = JsonConvert.SerializeObject(
                    new
                    {
                        assignedProducts
                    }
                );
            }

            IList<FilterTableType> filterBy = new List<FilterTableType>();
            dataFilterSort.FilterBy.ToList().ForEach(f =>
            {
                if (
                    (f.Key.Equals("Name", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("ProductId", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("Status", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("UserType", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("OffsetMinutes", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("RoleTemplateId", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("PrimaryProperties", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("PersonaHasProductError", StringComparison.OrdinalIgnoreCase)) ||
                    (f.Key.Equals("Operator", StringComparison.OrdinalIgnoreCase))
                    )
                {
                    if (lstProductsWithDatasharedProduct != null && lstProductsWithDatasharedProduct.Any() && f.Key.Equals("ProductId", StringComparison.OrdinalIgnoreCase))
                    {
                        var sharedProduct = lstProductsWithDatasharedProduct.FirstOrDefault(m => m.ProductId == int.Parse(f.Value));
                        filterBy.Add(
                           new FilterTableType()
                           {
                               ColumnName = f.Key,
                               SearchValue = sharedProduct != null ? sharedProduct.Value : f.Value
                           }
                       );
                    }
                    else
                    {
                        filterBy.Add(
                            new FilterTableType()
                            {
                                ColumnName = f.Key,
                                SearchValue = f.Value
                            }
                        );
                    }
                }
            });
            string filterByJson = string.Empty;
            if (filterBy.Count > 0)
            {
                filterByJson = JsonConvert.SerializeObject(
                    new
                    {
                        filterBy
                    }
                );
            }

            IList<SortTableType> sortBy = new List<SortTableType>();
            dataFilterSort.SortBy.ToList().ForEach(s =>
            {
                sortBy.Add(
                    new SortTableType()
                    {
                        ColumnName = s.Key,
                        SortDirection = s.Value.Substring(0, Math.Min(128, s.Value.Length))
                    }
                );
            });
            string sortByJson = string.Empty;
            if (sortBy.Count > 0)
            {
                sortByJson = JsonConvert.SerializeObject(
                    new
                    {
                        sortBy
                    }
                );
            }

            using (var repository = GetRepository())
            {
                var items = repository.GetManyWithSpliOn<ProfileDetail, UserLogin, int, string, ProfileDetail>(
                    isExport ? StoredProcNameConstants.SP_ListPersonsExport : StoredProcNameConstants.SP_ListPersons,
                    (profiledetail, userlogin, userproductcount, userType) =>
                    {
                        profiledetail.userLogin = userlogin;
                        profiledetail.userLogin.PartyId = profiledetail.PartyId;
                        profiledetail.userLogin.RealPageId = profiledetail.RealPageId;
                        profiledetail.userLogin.LoginNameType = EmailFormatValidation.IsValidEmail(profiledetail.userLogin.LoginName) ? "email" : "";
                        profiledetail.SummaryCount.TotalAssignedProducts = userproductcount;
                        profiledetail.AssignedProducts = null;
                        profiledetail.contactMechanism = null;
                        profiledetail.organization = null;
                        profiledetail.PartyRole = null;
                        profiledetail.TelecommunicationNumber = null;
                        profiledetail.InactivePersona = null;
                        profiledetail.Persona = null;
                        profiledetail.Operator = profiledetail.Operator;
                        //profiledetail.OperatorRealPageId = profiledetail.OperatorRealPageId;
                        profiledetail.UserRelationshipType = profiledetail.UserRelationshipType;
                        profiledetail.CompanyName = profiledetail.CompanyName;

                        if (userType != null)
                        {
                            var userTypeEnum = Regex.Replace(userType, @"[^A-Za-z0-9]+", "");

                            if (Enum.TryParse(userTypeEnum, true, out UserRoleType userRoleType))
                            {
                                profiledetail.userLogin.UserRoleType = userRoleType;
                            }
                        }

                        profiledetail.userLogin.IsPending = false;
                        profiledetail.userLogin.IsExpired = false;
                        profiledetail.userLogin.IsActive = true;
                        profiledetail.userLogin.IsLocked = false;
                        profiledetail.userLogin.Status = UserUiStatusType.Active;

                        if (isExport)
                        {
                            profiledetail.userLogin.IsSuperUser = profiledetail.userLogin.UserRoleType == UserRoleType.SuperUser;
                            profiledetail.userLogin = UserLoginStatus.SetUserLoginStatus((UserLogin)profiledetail.userLogin);
                            profiledetail.SuperVisorUser = new UserInfoLite
                            {
                                FirstName = profiledetail.SupervisorFirstName,
                                LastName = profiledetail.SupervisorLastName,
                                LoginName = profiledetail.SupervisorLoginName
                            };
                            profiledetail.PhoneNumber = profiledetail.PhoneNumber;
                            profiledetail.PhoneNumberType = profiledetail.PhoneNumberType;
                        }
                        else
                        {
                            profiledetail.userLogin = _manageUserLogin.GetUserLogin((UserLogin)profiledetail.userLogin, _userClaim.OrganizationPartyId);
                            var superVisorInfo = _userRepository.GetSuperVisorInformation(profiledetail.userLogin.UserId, _userClaim.OrganizationPartyId);
                            profiledetail.SuperVisorUser = (superVisorInfo != null) ? superVisorInfo : new UserInfoLite();
                            IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();
                            var phoneLists = telecommunicationNumberLogic.ListTelecommunicationNumberForPerson(profiledetail.RealPageId, null);
                            foreach (var item in phoneLists.ToList().Where(x => x.IsDefault == true))
                            {
                                profiledetail.PhoneNumber = item.PhoneNumber;
                                profiledetail.PhoneNumberType = item.contactMechanismUsageType.Name;
                            }
                        }
                        return profiledetail;
                    },
                    new
                    {
                        RealPageId = realPageId,
                        ParentPartyRoleTypeId = parentPartyRoleTypeId,
                        UserListFilterType = (int)filterUserList,
                        AssignedProducts = assignedProductsJson,
                        FilterBy = filterByJson,
                        SortBy = sortByJson,
                        RowsPerPage = dataFilterSort.Pages.ResultsPerPage == 100 ? 0 : dataFilterSort.Pages.ResultsPerPage, //ResultsPerPage == 100 ? Current Shell : New Shell
                        PageNumber = ((dataFilterSort.Pages.ResultsPerPage == 100) || (dataFilterSort.Pages.StartRow <= 0)) ? 1 : dataFilterSort.Pages.StartRow
                    },
                    splitOn: "UserId, Products, UserType");

                //Set the product count to 0 when the user status is disabled.
                items.ToList().FindAll(i => i.userLogin.Status == UserUiStatusType.Disabled).ForEach(d =>
                {
                    d.SummaryCount.TotalAssignedProducts = 0;
                    d.userLogin.Status = UserUiStatusType.Deactivated;
                });

                return items.ToList();
            }
        }

        public bool GetOrganizationHasAnyProductAssignmentError(long orgPartyId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<bool>(StoredProcNameConstants.SP_GetOrganizationHasPersonaProductError, new { PartyId = orgPartyId });
            }

        }

        /// <summary>
        /// Returns a list of persons by ProductId
        /// </summary>
        /// <param name="productId">Single product to search by product id</param>
        /// <param name="realPageId">Optional Organization realpage uniqueidentifier</param>
        /// <param name="personaId">Optional personaId</param>
        /// <returns>List of Person</returns>
        public IList<ProductUsers> ListPersonsByProductId(int productId, Guid? realPageId = null, long? personaId = null)
        {
            dynamic param = new
            {
                ProductId = productId,
                RealPageId = realPageId,
                PersonaId = personaId
            };

            using (var repository = GetRepository())
            {
                var items = repository.GetManyWithSpliOn<ProductUsers, UserLoginCommon, ProductUsers>(
                    StoredProcNameConstants.SP_ListPersonsByProductId,
                    (productUsers, userlogin) =>
                    {
                        productUsers.userLogin = userlogin;

                        return productUsers;
                    },
                    param: (object)param,
                    splitOn: "UserId");

                return items.ToList();
            }
        }


        public ExternalUserRelationship GetExternalUserRelationship(long organizationPartyId, long userId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    UserLoginId = userId,
                    OrganizationPartyId = organizationPartyId
                };
                List<UserLoginPersona> userLoginPersonaList = repository.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona, param);
                return repository.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship, new { UserLoginPersonaId = userLoginPersonaList.First().UserLoginPersonaId });
            }
        }

        #endregion

        #region Private Profile methods
        /// <summary>
        /// Save Product Batch
        /// </summary>
        /// <param name="repository">Dapper Repository</param>
        /// <param name="product">Product Batch object</param>
        /// <param name="createUserResponse">Response when creating a new user</param>
        /// <param name="saveProductBatchError"></param>
        /// <param name="CreateUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="AssignUserPersonaId">Assigned to user PersonaId</param>
        /// <param name="realPageId">The enterprise User Id of the person editing the product user</param>
        /// <param name="errorStatus">Error Status</param>
        /// <param name="inputJson">Product Batch Input JSON</param>
        /// <param name="batchProcessTypeId">Batch Process Type</param>
        /// <param name="impersonatorUserId"></param>
        private void SaveProductBatch(IRepository repository, IProductBatch product, CreateUserResponse<IErrorData> createUserResponse, string saveProductBatchError, long CreateUserPersonaId, long AssignUserPersonaId, Guid realPageId, Status<IErrorData> errorStatus, string inputJson, long impersonatorUserId, int batchProcessTypeId = 1)
        {
            try
            {
                var batchGroup = CreateBatchProcessGroup(repository);

                //Set the Logged-in and New User PeronaIds
                product.CreateUserPersonaId = CreateUserPersonaId;
                product.AssignUserPersonaId = AssignUserPersonaId;
                //Assign the 
                //product.InputJson.IsAssigned = true;
                //Create the Product Batch data to be processed by the Queuing
                dynamic param = new
                {
                    PersonRealPageId = realPageId,
                    CreateUserPersonaId = product.CreateUserPersonaId,
                    AssignUserPersonaId = product.AssignUserPersonaId,
                    ProductId = product.ProductId,
                    StatusTypeId = product.StatusTypeId,
                    RetryCount = product.RetryCount,
                    InputJson = inputJson,
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BatchProcessTypeId = batchProcessTypeId,
                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                    ImpersonatorUserId = impersonatorUserId
                };

                RepositoryResponse repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductBatch, param);
                //In-case of an error creating a product batch record, append the ProductId to the ErrorReason
                if (repositoryResponse.Id == 0)
                {
                    if (errorStatus != null)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.21";
                    }
                    if (createUserResponse != null)
                    {
                        createUserResponse.Status = errorStatus;
                    }
                    if (saveProductBatchError != "Save Product(s) Error: ")
                    {
                        saveProductBatchError += ", ";
                    }
                    saveProductBatchError += product.ProductId.ToString();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
            }
        }

        private BatchProcessorGroup CreateBatchProcessGroup(IRepository repo)
        {
            {
                DynamicParameters param = new DynamicParameters();
                int groupID = 0;
                param.Add("@BatchProcessorGroupID", groupID, dbType: DbType.Int32, direction: ParameterDirection.Output);

                try
                {
                    var a = repo.Execute(StoredProcNameConstants.SP_CreateBatchProcessorGroup, param);
                    groupID = param.Get<int>("@BatchProcessorGroupID");
                }
                catch (Exception ex)
                {
                }

                return new BatchProcessorGroup()
                {
                    BatchProcessorGroupId = groupID,
                    BatchProcessorGroupActivityLogged = false
                };
            }
        }

        /// <summary>
        /// Update Contact Preference
        /// </summary>
        /// <param name="repository">Dapper Repository</param>
        /// <param name="realPageId">The enterprise User Id</param>
        /// <param name="telecommunicationNumbers">telecommunicationNumbers list</param>
        /// <returns>Success/Fail</returns>
        private bool UpdateContactPreference(IRepository repository, Guid realPageId, List<TelecommunicationNumber> telecommunicationNumbers)
        {
            var preferredContact = telecommunicationNumbers
                        .Where(tc => tc.IsDeleted == false && tc.IsPreferred == true).FirstOrDefault();
            IList<TelecommunicationNumber> telecommunications = repository.GetMany<TelecommunicationNumber>(StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson, new { realPageId }).ToList();
            var currentContactMechanismId = telecommunications?.Where(tc => tc.IsPreferred == true).FirstOrDefault()?.ContactMechanismId;
            if ((currentContactMechanismId == null && preferredContact != null) ||
                (preferredContact != null && currentContactMechanismId != null
                    && (currentContactMechanismId != preferredContact.ContactMechanismId)))
            {
                dynamic param = new
                {
                    CurrentContactMechanismId = preferredContact.ContactMechanismId,
                    PreviousPreferenceId = currentContactMechanismId ?? 0
                };
                RepositoryResponse repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateContactMechanismPreference, param);
                if (repositoryResponse.Id == 0)
                {
                    return false;
                }
            }
            else if (preferredContact == null && currentContactMechanismId != null)
            {
                dynamic param = new
                {
                    ContactMechanismId = currentContactMechanismId
                };
                RepositoryResponse repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DeleteContactMechanismPreference, param);
                if (repositoryResponse.Id == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public UserActivityLogInfo GetUserActivityLogInfo(long personaId)
        {
            var persona = _personaRepository.GetPersona(personaId);
            var userLogin = _manageUserLogin.GetUserLoginOnly(persona.RealPageId);
            var person = _managePerson.GetPerson(persona.RealPageId);

            return new UserActivityLogInfo
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                RealPageId = userLogin.RealPageId,
                LoginName = userLogin.LoginName,
                BooksOrganizationMasterId = persona.Organization.BooksMasterId,
                OrganizationPartyId = persona.OrganizationPartyId,
                UserId = userLogin.UserId
            };
        }

        public void AuditActivityLog(String oldValue, string newValue, string fieldName, UserActivityLogInfo toUserLogInfo, UserDetails impersonatorUserInfo)
        {
            try
            {
                string message = "";
                List<AdditionalParameters> additionalInfo = new List<AdditionalParameters>();
                if (fieldName != "Deleted Phone Number" && fieldName != "Added Phone Number")
                {
                    if (string.IsNullOrEmpty(oldValue))
                    {
                        oldValue = "Blank Value";
                    }
                    if (string.IsNullOrEmpty(newValue))
                    {
                        newValue = "Blank Value";
                    }
                    message = impersonatorUserInfo != null
                    ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated {fieldName} from {oldValue} to {newValue}."
                    : $"{_userClaim.FirstName} {_userClaim.LastName} updated {fieldName} from {oldValue} to {newValue}.";

                    additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = "{\"action\" : \"Updated To\", \"value\" : \"" + (newValue == "Blank Value" ? " " : newValue) + "\"}" });
                    additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = "{\"action\" : \"Updated From\", \"value\" :  \"" + (oldValue == "Blank Value" ? " " : oldValue) + "\" }" });
                }
                else if(fieldName == "Deleted Phone Number")
                {
                    message = impersonatorUserInfo != null
                                       ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Deleted Phone Number {oldValue}."
                                       : $"{_userClaim.FirstName} {_userClaim.LastName} Deleted Phone Number {oldValue}.";
                    additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = "{\"action\" : \"Deleted\", \"value\" : \"" + oldValue + "\"}" });
                }
                else if (fieldName == "Added Phone Number")
                {
                    message = impersonatorUserInfo != null
                                       ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added Phone Number {oldValue}."
                                       : $"{_userClaim.FirstName} {_userClaim.LastName} Added Phone Number {oldValue}.";
                    additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = "{\"action\" : \"Added\", \"value\" : \"" + oldValue + "\"}" });
                }

                var activityDetails = new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.UPDATE_USER,
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = _userClaim.LoginName,
                    FromUserLoginId = _userClaim.UserId,
                    FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                    FromUserFirstName = _userClaim.FirstName,
                    FromUserLastName = _userClaim.LastName,

                    ToUserLoginName = toUserLogInfo.LoginName,
                    ToUserLoginId = toUserLogInfo.UserId,
                    ToUserFirstName = toUserLogInfo.FirstName,
                    ToUserLastName = toUserLogInfo.LastName,
                    ToUserRealpageId = toUserLogInfo.RealPageId.ToString(),
                };

                if (additionalInfo != null)
                {
                    activityDetails.AdditionalInformation = additionalInfo;
                }

                LogActivity.WriteActivity(activityDetails);
            }
            catch(Exception ex)
            {  }
        }
        #endregion
    }
}