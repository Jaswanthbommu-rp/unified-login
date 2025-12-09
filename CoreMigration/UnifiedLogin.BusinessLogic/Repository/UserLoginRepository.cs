using Dapper;
using UnifiedLogin.DataAccess;
using UnifiedLogin.DataAccess.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// User Login Repository
    /// </summary>
    public class UserLoginRepository : BaseRepository, IUserLoginRepository
    {
        private OrganizationRepository _organizationRepository;
        private CredentialRepository _credentialRepository;
        //private UserRepository _userRepository; // DO NOT ADD REFERENCE TO UserRepository!

        #region Constructor
        /// <summary>
        /// UserRepository Constructor
        /// </summary>
        public UserLoginRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _organizationRepository = new OrganizationRepository();
            _credentialRepository = new CredentialRepository();
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public UserLoginRepository(IRepository repository) : base(repository)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _credentialRepository = new CredentialRepository(repository);
        }
        #endregion

        #region public UserLogin methods
        /// <summary>
        /// Create a new UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateUserLogin(Guid realPageId, IUserLogin userLogin)
        {
            dynamic param = new
            {
                realPageId,
                userLogin.LoginName,
                userLogin.FromDate,
                userLogin.ThruDate
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLogin, param);
                return result;
            }
        }

        /// <summary>
        /// Get the user detail by party unique identifier and company Id
        /// </summary>
        /// <param name="realPageId">unique identifier</param>
        /// <param name="orgPartyId">Organization party id</param>
        /// <returns>User object</returns>
        public UserLogin GetUserLogin(Guid realPageId, long orgPartyId)
        {
            UserLogin userLogin;
            using (var repository = GetRepository())
            {
                userLogin = repository.GetOne<UserLogin>(StoredProcNameConstants.SP_GetUserLogin, new { realPageId });
            }
            
            IIdentityProviderType identityProviderType = _credentialRepository.GetIdentityProviderTypeByLoginName(userLogin.LoginName);
            userLogin.Is3rdPartyIDP = !identityProviderType.IsLocal;

            if (userLogin != null)
            {
                OrganizationStatus orgStatus = GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, orgPartyId, false);
                if (orgStatus == null)
                {
                    return null;
                }

                // remap the orgStatus to the userStatus
                userLogin = MapCompanyStatusToUserStatus(userLogin, orgStatus);
                userLogin = UserLoginStatus.SetUserLoginStatus(userLogin);
            }

            return userLogin;
        }

        private UserLogin MapCompanyStatusToUserStatus ( UserLogin userLogin, OrganizationStatus orgStatus)
        {
            userLogin.StatusId = orgStatus.StatusTypeId;
            userLogin.Status = orgStatus.Status;
            
            userLogin.IsPending = orgStatus.IsPending;
            userLogin.IsExpired = orgStatus.IsExpired;
            userLogin.IsActive = orgStatus.IsActive;
            userLogin.IsLocked = orgStatus.IsLocked;
            userLogin.IsForceReSetPassword = orgStatus.IsForceReSetPassword;
            userLogin.IsTainted = orgStatus.IsTainted;
            userLogin.FromDate = orgStatus.FromDate;
            userLogin.ThruDate = orgStatus.ThruDate;
            userLogin.StatusThruDate = orgStatus.StatusThruDate;
            
            return userLogin;
        }

        /// <summary>
        /// Used to get user information that does not include a status for the company
        /// </summary>
        /// <param name="enterpriseUserName"></param>
        /// <returns></returns>
        public UserLoginOnly GetUserLoginOnly(string enterpriseUserName)
        {
            return GetUserLoginOnly(enterpriseUserName, 0, Guid.Empty);
        }

        /// <summary>
        /// Used to get user information that does not include a status for the company
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserLoginOnly GetUserLoginOnly(long userId)
        {
            return GetUserLoginOnly(null, userId, Guid.Empty);
        }

        /// <summary>
        /// Used to get user information that does not include a status for the company
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns></returns>
        public UserLoginOnly GetUserLoginOnly(Guid realPageId)
        {
            return GetUserLoginOnly(null, 0, realPageId);
        }

        /// <summary>
        /// Get UserLogin
        /// </summary>
        /// <param name="enterpriseUserName">Enterprise User Name</param>
        /// <param name="userId">user id</param>
        /// <param name="realPageId"></param>
        /// <returns>UserLogin object</returns>
        private UserLoginOnly GetUserLoginOnly(string enterpriseUserName, long userId, Guid realPageId)
        {
            UserLoginOnly userLogin = new UserLoginOnly();
            dynamic param = null;
            if (!string.IsNullOrEmpty(enterpriseUserName))
            {
                param = new {EnterpriseUserName = enterpriseUserName};
            }

            if (userId != 0)
            {
                param = new {UserId = userId};
            }

            if (realPageId != Guid.Empty)
            {
                param = new { RealPageId = realPageId };
            }

            using (var repository = GetRepository())
            {
                userLogin = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, param);
            }

            return userLogin;
        }

        /// <summary>
        /// Update User ABulk ctivity Attempts
        /// </summary>
        public int UpdateBulkUserStatus(IList<Guid> userRealPageIdList, int statusTypeId, DateTime fromDate, DateTime? thruDate, long organizationPartyId)
        {
            DynamicParameters param = new DynamicParameters();

            dynamic p = new
            {
                StatusTypeId = statusTypeId,
                FromDate = fromDate,
                StatusThruDate = thruDate,
                PartyId = organizationPartyId
            };
            param.AddDynamicParams(p);

            using (var repository = GetRepository())
            {
                var tvp = new TableValueParmInfo();
                //IEnumerable<string> col = new string[] { "RealPageId" }; 
                //tvp.OrderedColumnName = col;
                tvp.TableVariableName = "RealPageId";
                tvp.TableParamTypeName = "dbo.PARTYGUID";
                tvp.StoredProcedureName = StoredProcNameConstants.SP_UpdateBulkUserStatus;

                var result = repository.ExecuteStoredProcWithTvp<Guid>(tvp, userRealPageIdList, param);

                return result;
            }
        }
        
        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">User object of the parameter values</param>
        /// <param name="organizationPartyId">organizationPartyId</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateUserLogin(Guid realPageId, IUserLogin userLogin, long organizationPartyId)
        {
            dynamic param = new
            {
                realPageId,
                userLogin.LoginName,
                userLogin.PasswordHash,
                userLogin.PasswordSalt,
                userLogin.FromDate,
                userLogin.ThruDate,
                PartyId = organizationPartyId
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, param);
                return result;
            }
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="username">Username</param>        
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateLastLogin(string username)
        {
            dynamic param = new
            {
                LoginName = username
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateLastLogin, param);
                return result;
            }
        }


        /// <summary>
        /// Link Identity Provider to a UserLogin
        /// </summary>
        /// <param name="personaId">PersonaId</param>
        /// <param name="userId">UserLogin unique Id</param>
        /// <param name="contactMechanismId">Contact Mechanism Id</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse LinkIdentityProviderToUserLogin(long personaId, long userId, int contactMechanismId)
        {
            dynamic param = new
            {
                PersonaId = personaId,
                UserId = userId,
                ContactMechanismId = contactMechanismId
            };

            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, param);
            }
        }

        /// <summary>
        /// List Persona and Organization by User LoginName
        /// </summary>
        /// <param name="loginName">User login name</param>
        /// <param name="organizationRealPageId"></param>        
        /// <returns>List of User Persona and Organization detail</returns>
        public IList<UserOrganization> ListOrganizationByLoginName(string loginName, Guid? organizationRealPageId = null)
        {
            dynamic param = new
            {
                LoginName = loginName,
                OrganizationRealPageId = organizationRealPageId
            };

            using (var repository = GetRepository())
            {
                return repository.GetMany<UserOrganization>(StoredProcNameConstants.SP_ListOrganizationByLoginName, param);
            }
        }

        /// <summary>
        /// List all organization for user without thru data condition
        /// </summary>
        /// <param name="loginName">User login name</param>        
        /// <returns>List of User Persona and Organization detail</returns>
        public IList<UserOrganization> ListAllOrganizationByLoginName(string loginName)
        {
            dynamic param = new
            {
                LoginName = loginName
            };

            using (var repository = GetRepository())
            {
                return repository.GetMany<UserOrganization>(StoredProcNameConstants.SP_ListAllOrganizationByLoginName, param);
            }
        }

        /// <summary>
        /// list of Organization By Enterprise User Id
        /// </summary>
        /// <param name="realPageId">Unique Identifier - EnterpriseUserId</param>
        /// <param name="relationshipType">Parties Relationship type name (Optional)</param>
        /// <returns>List of Organization</returns>
        public IList<Organization> ListOrganizationByEnterpriseUserId(Guid realPageId, string relationshipType)
        {
            dynamic param = new
            {
                RealPageId = realPageId,
                RelationshipTypeName = relationshipType
            };
            
            IList<OrganizationType> organizationTypeList = _organizationRepository.ListOrganizationType();

            using (var repository = GetRepository())
            {
                IList<Organization> organizationList = repository.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, param);

                organizationList.ToList().ForEach(org =>
                    {
                        var orgType = _organizationRepository.ListOrganizationType().FirstOrDefault(o => o.OrganizationTypeId == org.OrganizationTypeId);
                        org.organizationType = orgType != null ? new OrganizationType {Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate} : new OrganizationType();
                        var orgDomain = _organizationRepository.ListOrganizationDomain().FirstOrDefault(d => d.OrganizationDomainId == org.OrganizationDomainId);
                        org.OrganizationDomain = orgDomain != null ? new OrganizationDomain {OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate} : new OrganizationDomain();
                    }

                );
                return organizationList;
            }
        }

        /// <summary>
        /// List Organization without status by User id
        /// </summary>
        /// <param name="userId">User id</param>        
        /// <returns>List of Organizations and their status for the user</returns>
        public IList<OrganizationStatus> ListOrganizationWithoutStatusByUserId(long userId)
        {
            dynamic param = new
            {
                UserId = userId
            };

            using (var repository = GetRepository())
            {
                return repository.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, param);
            }
        }

        /// <summary>
        /// The primary org by User id (without status!)
        /// </summary>
        /// <param name="userId">User id</param>        
        /// <returns>The primary organization and its status for the user</returns>
        public OrganizationStatus GetPrimaryOrgWithoutStatusByUserId(long userId)
        {
            return ListOrganizationWithoutStatusByUserId(userId).FirstOrDefault(p => p.PrimaryOrganization);
        }

        /// <summary>
        /// Get Default OrgId For User
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Organization Id</returns>
        public long GetPrimaryOrgIdByUserId(long userId)
        {
            return GetPrimaryOrgWithoutStatusByUserId(userId).PartyId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="enterpriseUserName"></param>
        /// <param name="activityTypeId"></param>
        /// <returns></returns>
        public ActivityAttemptDetails GetActivityAttemptExceeds(long partyId, string enterpriseUserName, int activityTypeId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<ActivityAttemptDetails>(StoredProcNameConstants.SP_GetActivityAttemptExceeds,
                    new { enterpriseUserName, activityTypeId, partyId });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastLogin"></param>
        /// <param name="organizationPartyId"></param>
        /// <param name="getPrimaryOrg"></param>
        /// <returns></returns>
        public OrganizationStatus GetUserOrganizationWithStatus(long userId, DateTime? lastLogin, long organizationPartyId, bool getPrimaryOrg)
        {
            var organizationList = ListOrganizationWithoutStatusByUserId(userId);

            OrganizationStatus orgStatus = null;
            orgStatus = getPrimaryOrg ? organizationList.FirstOrDefault(p => p.PrimaryOrganization) : organizationList.FirstOrDefault(p => p.PartyId == organizationPartyId);

            orgStatus?.SetOrganizationStatus(lastLogin != null);

            return orgStatus;
        }

        /// <summary>
        /// List Blacklisted user domains
        /// </summary>
        public IList<string> GetBlacklistedDomains()
        {
            using (var repository = GetRepository())
            {
                // Specify null for the optional param to resolve ambiguity
                return (IList<string>)repository.GetMany<string>(StoredProcNameConstants.SP_GetBlacklistedDomains, param: null);
            }
        }
        #endregion
    }
}
