using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// Class for Persona Repository
    /// </summary>
    public class PersonaRepository : BaseRepository, IPersonaRepository
    {
        private readonly DefaultUserClaim _userClaim;
        #region Private Variables
        readonly IOrganizationRepository _organizationRepository;
        readonly IUserLoginRepository _userLoginRepository;
        #endregion

        #region Ctor
        /// <summary>
        /// Persona base Constructor
        /// </summary>
        public PersonaRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _userClaim = new DefaultUserClaim();
            _organizationRepository = new OrganizationRepository();
            _userLoginRepository = new UserLoginRepository();
        }

        /// <summary>
        /// Transaction Shared repository constructor
        /// </summary>
        /// <param name="repository"></param>

        public PersonaRepository(IRepository repository) : base(repository)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
        }

        /// <summary>
        /// Unit Test Constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaim"></param>
        public PersonaRepository(IRepository repository, DefaultUserClaim userClaim) : base(repository)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _userClaim = userClaim;
        } 

        /// <summary>
        ///Default Constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public PersonaRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
        {
            if (userClaim == null)
            {
                userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            }
            _userClaim = userClaim;
            _organizationRepository = new OrganizationRepository();
            _userLoginRepository = new UserLoginRepository();
        }
        #endregion

        #region public Persona methods

        /// <summary>
        /// Get Persona Environment Object
        /// </summary>
        /// <returns>Persona Environment Type Object</returns>
        public IList<PersonaEnvironment> GetPersonaEnvironmentType()
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                };
                IList<PersonaEnvironment> personaEnvironmentType = repository.GetMany<PersonaEnvironment>(StoredProcNameConstants.SP_GetPersonaEnvironment, param);

                return personaEnvironmentType;
            }
        }

        /// <summary>
        /// Create a new Persona
        /// </summary>
        /// <param name="personRealPageId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="persona">Persona object of the parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreatePersona(Guid personRealPageId, Guid organizationRealPageId, IPersona persona)
        {
            long PersonaId = 0;
            dynamic param = new
            {
                PersonRealPageId = personRealPageId,
                OrganizationRealPageId = organizationRealPageId,
                persona.PersonaTypeId,
                persona.FromDate,
                persona.ThruDate,
                PersonaId = PersonaId
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, param);
                return result;
            }
        }

        /// <summary>
        /// Get Default Persona by Persona Id
        /// </summary>
        /// <param name="personaId">Persona Id</param>
        /// <param name="withRights">Also merge persona with current user rights</param>
        /// <returns>Persona Object</returns>
        public Persona GetPersona(long personaId, bool withRights = true)
        {
            //var claimDetails = GetClaims();
            Persona persona = null;
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    personaId
                };
                persona = repository.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, param);
            }

            if (persona != null)
            {
                //IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(persona.RealPageId, null);
                //Organization organization = organizationList.FirstOrDefault(i => i.PartyId == persona.OrganizationPartyId);
                Organization organization = _organizationRepository.GetOrganization(organizationPartyId: persona.OrganizationPartyId);
                if (organization != null)
                {
                    persona.Organization = organization;
                }

                if (withRights)
                {
                    persona = AddRightsToPersona(persona);
                }
            }

            return persona;
        }

        private Persona AddRightsToPersona(Persona persona)
        {
            if (persona == null) return null;

            persona.hasViewOnlySupportToolAccess = false;

            System.Security.Claims.ClaimsPrincipal currentClaimPrincipal = System.Security.Claims.ClaimsPrincipal.Current;
            DefaultUserClaim userClaim = new DefaultUserClaim(currentClaimPrincipal);

            //NOT Super user then check for Right
            if (persona.UserTypeId != UserTypeConstants.SuperUser)
            {
                persona.hasResidentPortalUserAccess = CheckUserRight.CheckUserHasAccess(userClaim.Rights, "AddEditResidentPortalUser");

                List<string> editorRights = userClaim.Rights;
                persona.hasManageAccountingProductAccess = editorRights.Contains(ProductRightEnum.ManageAccountingProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageAssetOptimizationProductAccess = editorRights.Contains(ProductRightEnum.ManageAssetOptimizationProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageClientPortalProductAccess = editorRights.Contains(ProductRightEnum.ManageClientPortalProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageDocumentManagementProductAccess = editorRights.Contains(ProductRightEnum.ManageDocumentManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageILMLeadManagemementProductAccess = editorRights.Contains(ProductRightEnum.ManageILMLeadManagemementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageILMLeasingAnalyticsProductAccess = editorRights.Contains(ProductRightEnum.ManageILMLeasingAnalyticsProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageLead2LeaseProductAccess = editorRights.Contains(ProductRightEnum.ManageLead2LeaseProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageMarketingCenterProductAccess = editorRights.Contains(ProductRightEnum.ManageMarketingCenterProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageOneSiteProductAccess = editorRights.Contains(ProductRightEnum.ManageOneSiteProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageOnSiteProductAccess = editorRights.Contains(ProductRightEnum.ManageOnSiteProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasProspectContactCenterProductAccess = editorRights.Contains(ProductRightEnum.ProspectContactCenterProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageRentersInsuranceProductAccess = editorRights.Contains(ProductRightEnum.ManageRentersInsuranceProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageSpendManagementProductAccess = editorRights.Contains(ProductRightEnum.ManageSpendManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageUnifiedAmenitiesProductAccess = editorRights.Contains(ProductRightEnum.ManageUnifiedAmenitiesProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageUtilityManagementProductAccess = editorRights.Contains(ProductRightEnum.ManageUtilityManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageVendorComplianceProductAccess = editorRights.Contains(ProductRightEnum.ManageVendorComplianceProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageRealConnectProductAccess = editorRights.Contains(ProductRightEnum.ManageRealConnectProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManagePortfolioManagementProductAccess = editorRights.Contains(ProductRightEnum.ManagePortfolioManagementProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageIntegrationMarketplaceProductAccess = editorRights.Contains(ProductRightEnum.AccessIntegrationMarketplace.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManagePlatFormSecurity = editorRights.Contains(ProductRightEnum.ManagePlatFormSecurity.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageCustomFields = editorRights.Contains(ProductRightEnum.ManageCustomFields.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageUnifiedSettings = editorRights.Contains(ProductRightEnum.ManageUnifiedSettings.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageClickPayProductAccess = editorRights.Contains(ProductRightEnum.ManageClickPayProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageSeniorLeadManagementProductAccess = editorRights.Contains(ProductRightEnum.ManageSeniorLeadManagement.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageDepositAlternativeProductAccess = editorRights.Contains(ProductRightEnum.ManageDepositAlternativeProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageSettingsTemplates = editorRights.Contains(ProductRightEnum.ManageSettingsTemplates.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasnotificationsAccess = editorRights.Contains("ManageNotifications", StringComparer.OrdinalIgnoreCase);

                persona.hasManageRenovationManagerProductAccess = editorRights.Contains(ProductRightEnum.ManageRenovationManager.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageIntelligentBuildingTrashProductAccess = editorRights.Contains(ProductRightEnum.ManageIntelligentBuildingTrashProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
               
                persona.hasManageIntelligentBuildingEnergyProductAccess = editorRights.Contains(ProductRightEnum.ManageIntelligentBuildingEnergyProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
                
                persona.hasManageIntelligentBuildingWaterProductAccess = editorRights.Contains(ProductRightEnum.ManageIntelligentBuildingWaterProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);
                
                persona.hasManageHandsOnTrainingSystemAccess = editorRights.Contains(ProductRightEnum.ManageHandsOnTrainingSystemProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageLeaseLabsAccess = editorRights.Contains(ProductRightEnum.ManageLeaseLabsProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

              
                persona.hasManageLeadScoringAccess = editorRights.Contains(ProductRightEnum.ManageLeadScoringProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasPlatformAlertsAccess = (editorRights.Contains("CreatePlatformAlerts", StringComparer.OrdinalIgnoreCase) 
                                                || editorRights.Contains("ApprovePlatformAlerts", StringComparer.OrdinalIgnoreCase));

                persona.hasImportUsersAccess = editorRights.Contains("AbilityToImportUsers", StringComparer.OrdinalIgnoreCase);

                persona.hasManageSmartWasteCommercialProductAccess = editorRights.Contains
                    (ProductRightEnum.ManageSmartWasteCommercialProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

                persona.hasManageAdminSupportPortalProductAccess = editorRights.Contains
                   (ProductRightEnum.ManageAdminSupportPortalProductAccess.ToString(), StringComparer.OrdinalIgnoreCase);

            }

            if (currentClaimPrincipal.Identity.IsAuthenticated)
            {
                persona.hasViewOnlySupportToolAccess =  userClaim.Rights.Contains("ViewOnlySupportToolAccess", StringComparer.OrdinalIgnoreCase);
                persona.hasViewOnlySettingsAccess = userClaim.Rights.Contains("ViewUnifiedSettings", StringComparer.OrdinalIgnoreCase);
                persona.hasManageUnifiedSettings = userClaim.Rights.Contains("ManageUnifiedSetting", StringComparer.OrdinalIgnoreCase);
                persona.hasManageCustomFields = userClaim.Rights.Contains("ManageCustomFields", StringComparer.OrdinalIgnoreCase);
                persona.hasManagePlatFormSecurity = userClaim.Rights.Contains("ManagePlatFormSecurity", StringComparer.OrdinalIgnoreCase);
                persona.hasAccessSettingsAdmin = userClaim.Rights.Contains("AccessSettingsAdmin", StringComparer.OrdinalIgnoreCase);
                persona.hasManageSettingsTemplates = userClaim.Rights.Contains("ManageSettingsTemplates", StringComparer.OrdinalIgnoreCase);
                persona.hasnotificationsAccess = userClaim.Rights.Contains("ManageNotifications", StringComparer.OrdinalIgnoreCase);
                persona.hasPlatformAlertsAccess = (userClaim.Rights.Contains("CreatePlatformAlerts", StringComparer.OrdinalIgnoreCase)
                                                || userClaim.Rights.Contains("ApprovePlatformAlerts", StringComparer.OrdinalIgnoreCase));

                persona.hasImportUsersAccess = userClaim.Rights.Contains("AbilityToImportUsers", StringComparer.OrdinalIgnoreCase);

                if (!persona.hasViewOnlySettingsAccess || !persona.hasManageUnifiedSettings || !persona.hasManageCustomFields || !persona.hasManagePlatFormSecurity || !persona.hasManageSettingsTemplates)
                {
                    // check to see impersonating, if they are then check that users rights
                    if (userClaim.ImpersonatedBy != Guid.Empty)
                    {
                        long activePersonaId = GetActivePersonaId(userClaim.ImpersonatedBy);
                        Persona impersonatorPersona = GetPersona(activePersonaId, false);
                        UserRoleRightRepository urr = new UserRoleRightRepository();
                        List<SharedObjects.Product.UnifiedLogin.Role> userRoles = urr.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, impersonatorPersona.PersonaId, impersonatorPersona.OrganizationPartyId);

                        RPObjectCache rpCache = new RPObjectCache();
                        var cacheKey = $"getRolesByParty_{impersonatorPersona.OrganizationPartyId}_{(int)ProductEnum.UnifiedPlatform}";
                        IList<UserRoleRights> roleList = rpCache.GetFromCache<IList<UserRoleRights>>(cacheKey, 180, () =>
                        {
                            SharedDataRepository sdr = new SharedDataRepository();
                            IList<int> productList = sdr.GetProductIdsByCompany(impersonatorPersona.OrganizationPartyId);
                            UserRoleRightRepository urrCache = new UserRoleRightRepository();
                            return urrCache.GetAllRoleRights(impersonatorPersona.OrganizationPartyId, productList, (int)ProductEnum.UnifiedPlatform);
                        });

                        foreach (SharedObjects.Product.UnifiedLogin.Role userRole in userRoles)
                        {
                            if (!persona.hasViewOnlySettingsAccess && roleList.Any(r => r.RoleId == userRole.RoleID))
                            {
                                foreach (Right right in roleList.FirstOrDefault(r => r.RoleId == userRole.RoleID).UserRights)
                                {
                                    if (!string.IsNullOrEmpty(right.RightNickName))
                                    {
                                        if (right.RightNickName.Equals("ViewUnifiedSettings", StringComparison.OrdinalIgnoreCase))
                                        {
                                            persona.hasViewOnlySettingsAccess = true;
                                        }

                                        if (right.RightNickName.Equals("ManageUnifiedSetting", StringComparison.OrdinalIgnoreCase))
                                        {
                                            persona.hasManageUnifiedSettings = true;
                                        }

                                        if (right.RightNickName.Equals("ManageCustomFields", StringComparison.OrdinalIgnoreCase))
                                        {
                                            persona.hasManageCustomFields = true;
                                        }

                                        if (right.RightNickName.Equals("ManagePlatFormSecurity", StringComparison.OrdinalIgnoreCase))
                                        {
                                            persona.hasManagePlatFormSecurity = true;
                                        }

                                        if (right.RightNickName.Equals("ManageSettingsTemplates", StringComparison.OrdinalIgnoreCase))
                                        {
                                            persona.hasManageSettingsTemplates = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return persona;
        }

        /// <summary>
		/// Lists Personas by Enterprise UserId, does NOT include rights correctly!
		/// </summary>
		/// <param name="realPageId">Person Enterprise Id</param>
		/// <returns>A List of Persona Object(s)</returns>
		public IList<Persona> ListPersona(Guid realPageId)
        {
            dynamic param = new
            {
                RealPageId = realPageId
            };

            IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);

            using (var repository = GetRepository())
            {
                IList<Persona> personaList = repository.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, param);
                personaList?.ToList().ForEach(p => p.Organization = organizationList.ToList().FirstOrDefault(o => o.PartyId == p.OrganizationPartyId));

                return personaList;
            }
        }

        /// <summary>
		/// Lists Employee Personas by Enterprise UserId,org partyid
		/// </summary>
		/// <param name="userId">Person Enterprise Id</param>
        /// <param name="orgPartyId">Person org PartyId</param>
		/// <returns>A List of Persona Object(s)</returns>
		public IList<Persona> ListEmployeePersonas(long userId, long orgPartyId)
        {
            dynamic param = new
            {
                UserId = userId,
                OrgPartyId = orgPartyId
            };

            using (var repository = GetRepository())
            {
                IList<Persona> personaList = repository.GetMany<Persona>(StoredProcNameConstants.SP_ListEmployeePersonas, param);
               
                return personaList;
            }
        }

        /// <summary>
        /// Lists only active personas by Enterprise UserId, does NOT include rights correctly!
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="includeOrganization">Include organization details</param>
        /// <returns>A List of Persona Object(s)</returns>
        public IList<Persona> ListActivePersona(Guid realPageId, bool includeOrganization)
        {
            dynamic param = new
            {
                RealPageId = realPageId
            };

            IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);

            using (var repository = GetRepository())
            {
                IList<Persona> personaList = repository.GetMany<Persona>(StoredProcNameConstants.SP_ListActivePersona, param);
                personaList?.ToList().ForEach(p => p.Organization = organizationList.ToList().FirstOrDefault(o => o.PartyId == p.OrganizationPartyId));

                return personaList;
            }
        }

        /// <summary>
        /// List Persona by Enterprise Organization PartyId
        /// </summary>
        /// <param name="organizationPartyId">Organization Party Id</param>
        /// <param name="isDefault">Optional Default persona only</param>
        /// <param name="userRoleType">User Role type (e.g. refer to UserRoleType Enum)</param>
        /// <returns>A List of Persona Object(s)</returns>
        public IList<Persona> ListPersonaByOrganizationPartyId(long organizationPartyId, bool? isDefault = null, int? userRoleType = null)
        {
            dynamic param = new
            {
                OrganizationPartyId = organizationPartyId,
                IsDefault = isDefault,
                UserRoleType = userRoleType
            };

            Organization organization = _organizationRepository.GetOrganization(null, organizationPartyId);

            using (var repository = GetRepository())
            {
                IList<Persona> personaList = repository.GetMany<Persona>(StoredProcNameConstants.SP_ListPersonaByOrganizationPartyId, param);
                personaList?.ToList().ForEach(p => p.Organization = organization);
                return personaList;
            }
        }

        /// <summary>
        /// Get Active PersonaId
        /// </summary>
        /// <param name="realPageId">Enterprise UserId</param>
        /// <returns>Active PersonaId</returns>
        public long GetActivePersonaId(Guid realPageId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = realPageId });
            }
        }

        /// <summary>
        /// Used to update the active persona for a user
        /// </summary>
        /// <param name="personRealPageId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        public RepositoryResponse UpdateActivePersona(Guid personRealPageId, long personaId)
        {

            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateActivePersona, new { RealPageId = personRealPageId, PersonaId = personaId });
            }
        }

        public List<ProductSettingList> GetPersonaProductSettings(long personaId)
        {
            List<ProductSettingList> productSettingList = new List<ProductSettingList>();
            using (var repository = GetRepository())
            {
                productSettingList = repository.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, new { PersonaId = personaId }).ToList();
            }

            return productSettingList;
        }
        /// <summary>
        /// Create a new Secondary Persona
        /// </summary>
        /// <param name="userId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="createdBy">created by</param>
        /// <param name="personaName">Persona Name</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateAdditionalPersona(Guid organizationRealPageId, long userId, long createdBy, string personaName)
        {
            long PersonaId = 0;
            dynamic param = new
            {
                OrganizationRealPageId = organizationRealPageId,
                UserId = userId,
                CreatedBy = createdBy,
                PersonaName = @personaName,
                PersonaId = PersonaId
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateAdditionalPersona, param);
                return result;
            }
        }
        #endregion
    }
}