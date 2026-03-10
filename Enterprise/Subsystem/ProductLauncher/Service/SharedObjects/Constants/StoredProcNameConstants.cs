using System.Web.SessionState;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    public static class StoredProcNameConstants
    {
        // WebApi Settings
        public const string SP_GetGlobalSettings = "Enterprise.ListGlobalSettings";

        // Forgot / Reset password
        public const string SP_GetUserSecurityQuestionAnswers = "Ident.GetUserSecurityQuestionAnswer";
        public const string SP_CreateActivityToken = "Ident.CreateActivityToken";
        public const string SP_UpdateActivityAttempt = "Ident.UpdateActivityAttempt";
        public const string SP_GetActivityToken = "Ident.GetActivityToken";
        public const string SP_UpdateEnterpriseUserCredential = "Ident.UpdateEnterpriseUserCredential";
        public const string SP_GetUserSecurityQuestionAnswer = "Ident.GetUserSecurityQuestionAnswer";
        public const string SP_GetActivityAttemptExceeds = "Ident.GetActivityAttemptExceeds";
        public const string SP_GetUserSelectedSecurityQuestions = "Ident.GetUserSelectedSecurityQuestions";
        public const string SP_CreateUserSelectedSecurityQuestions = "Ident.CreateUserSelectedSecurityQuestions";
        public const string SP_GetPasswordHistory = "Ident.GetPasswordHistory";

        public const string SP_CreateUpdateUserTokenDetail = "Ident.CreateUpdateUserTokenDetail";
        public const string SP_UpdateUserLoginTwoFactor = "Ident.UpdateUserLoginTwoFactor";

        public const string SP_ResetEnterpriseUserCredential = "Ident.ResetEnterpriseUserCredential";
        public const string SP_GetAllSecurityQuestions = "Ident.GetAllSecurityQuestions";

        //PasswordPolicy
        public const string SP_CreatePasswordPolicy = "Ident.CreatePasswordPolicy";
        public const string SP_GetPasswordPolicy = "Ident.GetPasswordPolicy";
        public const string SP_UpdatePasswordPolicy = "Ident.UpdatePasswordPolicy";

        //User
        public const string SP_GetUserByLoginId = "Ident.GetUserByLoginId";
        public const string SP_GetExternalUserRelationship = "Enterprise.GetExternalUserRelationship";
        public const string SP_UpdateExternalUserRelationship = "Enterprise.UpdateExternalUserRelationship";
        public const string SP_DeleteExternalUserRelationship = "Enterprise.DeleteExternalUserRelationship";
        public const string SP_EnterpriseCheckOrgAdmin = "Enterprise.CheckOrgAdmin";
        /// <summary>
        /// Stored procedure for retrieving blacklisted domains.
        /// </summary>
        public const string SP_GetBlacklistedDomains = "Enterprise.GetBlacklistedDomains";

        //UserLogin
        public const string SP_CreateUserLogin = "Ident.CreateUserLogin";
        public const string SP_GetUserLogin = "Ident.GetUserLogin";
        public const string SP_GetUserLoginOnly = "Ident.GetUserLoginOnly";
        public const string SP_UpdateUserLogin = "Ident.UpdateUserLogin";
        public const string SP_UpdateLastLogin = "Ident.UpdateLastLogin";
        public const string SP_UpdateUserStatusByCompany = "Ident.UpdateUserStatusByCompany";
        public const string SP_LinkIdentityProviderToUserLogin = "Ident.LinkIdentityProviderToUserLogin";
        public const string SP_UpdateBulkUserStatus = "Ident.UpdateBulkUserStatus";

        //UserLoginPersona
        public const string SP_CreateUserLoginPersona = "Ident.CreateUserLoginPersona";
        public const string SP_GetUserLoginPersona = "Ident.GetUserLoginPersona";
		public const string SP_UpdateUserLoginPersona = "Ident.UpdateUserLoginPersona";

		//Activity Configuration
		public const string SP_ListActivity = "Ident.ListActivity";
        public const string SP_UpdateActivityConfiguration = "Ident.UpdateActivityConfiguration";

        //Person
        public const string SP_CreatePerson = "Person.CreatePerson";
        public const string SP_GetPerson = "Person.GetPerson";
        public const string SP_UpdatePerson = "Person.UpdatePerson";
        public const string SP_ListPersons = "Person.ListPersons_Ver04";
        public const string SP_ListPersonsExport = "Person.ListPersons_Export";
        public const string SP_ListPersonsByProductId = "Person.ListPersonsByProductId";
        public const string SP_GetDefaultPersona = "Person.GetDefaultPersona";
        public const string SP_ListUsers = "Person.ListPersonsForSupportTool";
		public const string SP_GetNotificationEmailForPerson = "Person.GetNotificationEmailForPerson";
        public const string SP_GetPersonaProductError = "Person.GetPersonaProductError";
        public const string SP_GetOrganizationHasPersonaProductError = "Enterprise.GetOrganizationHasPersonaProductError";
        //Persona
        public const string SP_GetPersona = "Person.GetPersona";
        public const string SP_GetPersonaEnvironment = "Person.ListPersonaEnvironmentType";
        public const string SP_CreatePersona = "Person.CreatePersona";
        public const string SP_CreateAdditionalPersona = "Person.CreateAdditionalPersona";
        public const string SP_CreatePersonaType = "Person.CreatePersonaType";
        public const string SP_ListPersona = "Person.ListPersona";
        public const string SP_ListEmployeePersonas = "Person.ListEmployeePersonas";
        public const string SP_ListActivePersona = "Person.ListActivePersona";
        public const string SP_ListPersonaByOrganizationPartyId = "Person.ListPersonaByOrganizationPartyId_Ver01";
        public const string SP_GetActivePersona = "Person.GetActivePersona";
        public const string SP_UpdateActivePersona = "Person.UpdateActivePersona";
        public const string SP_RemovePersona = "Person.RemovePersona";
        public const string SP_UpdatePersona = "Person.UpdatePersona";
        public const string SP_CreatePersonaConfiguration = "Enterprise.CreatePersonaConfiguration";
		public const string SP_ListPersonaToDisableUserProduct = "Person.ListPersonaToDisableUserProduct";        

		//Set Password
		public const string SP_SaveSecurityQuestionAnswers = "Ident.CreateSecurityQuestionAnswers";

        //BlueBook
        public const string SP_MapBlueBookIdtoPartyId = "Enterprise.MapBlueBookIdtoPartyId";
        //public const string SP_GetBlueBookIdByOrganization = "Enterprise.GetBlueBookIdByOrganization";
        public const string SP_GetBookIdByOrganization = "Enterprise.GetBookIdByOrganization";
        public const string SP_DataImportMappingUpdate = "Enterprise.DataImportMappingUpdate";
        //Organization
        //public const string SP_SetupOrganization = "Enterprise.SetupOrganization";
        public const string SP_SetupOrganization = "Enterprise.SetupOrganization_Ver01";
        public const string SP_InsertOrganization = "Enterprise.CreateOrganization";
        public const string SP_UpdateOrganization = "Enterprise.UpdateOrganization";
        public const string SP_UpdateOrganizationThirdPartyIDP = "Enterprise.UpdateOrganizationThirdPartyIDP";
        public const string SP_UpdateUsersIDP = "Ident.UpdateUsersIDP";
        public const string SP_GetUserProfileByUserIds = "Ident.GetUserProfileByUserIds";
        public const string SP_OrganizationIDPList = "Enterprise.OrganizationIDPList";
        public const string SP_InsertBatchCompanyJob = "Batch.InsertBatchCompanyJob";
        //public const string SP_GetOrganization = "Enterprise.GetOrganization"; 
        public const string SP_GetOrganization = "Enterprise.GetOrganization_Ver03";
        public const string SP_LinkPersonToOrganization = "Person.LinkPersonToOrganization";
        public const string SP_UnlinkPersonToOrganization = "Person.UnlinkPersonToOrganization";
        public const string SP_UpdatePersonToOrganization = "Person.UpdatePersonToOrganization";
        public const string SP_LinkOrganizationToOrganization = "Enterprise.LinkOrganizationToOrganization";
        //public const string SP_ListOrganizationByRealPageId = "Enterprise.ListOrganizationByRealPageId";
        public const string SP_ListOrganizationByRealPageId = "Enterprise.ListOrganizationByRealPageId_Ver02";
        public const string SP_GetOrganizationIdentityProviderType = "Enterprise.GetOrganizationIdentityProviderType";
        //public const string SP_GetOrganizationByBlueBookId = "Enterprise.GetOrganizationByBlueBookId";
        public const string SP_GetOrganizationByBlueBookId = "Enterprise.GetOrganizationByBlueBookId_Ver01";

        public const string SP_CreateOrganizationProduct = "Enterprise.CreateOrganizationProduct";
        public const string SP_DeleteOrganizationProduct = "Enterprise.DeleteOrganizationProduct";
        public const string SP_SetupSuperUser = "Security.SetupSuperUser";
        public const string SP_ListOrganizationByLoginName = "Enterprise.ListOrganizationByLoginName";
        public const string SP_ListAllOrganizationByLoginName = "Enterprise.ListAllOrganizationByLoginName";
        public const string SP_ListOrganizationStatusByUserId = "Enterprise.ListOrganizationStatusByUserId";
        public const string SP_ListOrganizationType = "Enterprise.ListOrganizationType";
        public const string SP_ListOrganizationDomain = "Enterprise.ListOrganizationDomain";
        public const string SP_CreateOrganizationDomain = "Enterprise.CreateOrganizationDomain";
		public const string SP_ListProductUsersForOrganization = "Enterprise.ListProductUsersForOrganization";
        public const string SP_ListCompanySetup = "Enterprise.GetCompanyList";
        public const string SP_DisableUsersForProduct = "Enterprise.DisableUsersForProduct";
        public const string SP_CreateOrganizationProductConfiguration = "Enterprise.CreateOrganizationProductConfiguration";
        public const string SP_CreateOrganizationProductConfigurationbyPartyId = "Enterprise.CreateOrganizationProductConfigurationbyPartyId";
        public const string SP_GetOrganizationSettingValue = "Enterprise.GetOrganizationSettingValue";
        public const string SP_GetOrganizationSettingValueByPersonaId = "Enterprise.GetOrganizationSettingValueByPersonaId";
        //PartyRelationship
        public const string SP_GetPartyRelationshipByRealPageId = "Enterprise.GetPartyRelationshipByRealPageId";

        //PartyRole
        public const string SP_GetPartyRoleByRealPageId = "Enterprise.GetPartyRoleByRealPageId";
        public const string SP_CreatePartyRoleByRealPageId = "Enterprise.CreatePartyRoleByRealPageId";
        public const string SP_UpdatePartyRoleByRealPageId = "Enterprise.UpdatePartyRoleByRealPageId";
        public const string SP_GetPartyRole = "Person.GetPartyRole";

        //RelationshipType
        public const string SP_ListRelationshipType = "Enterprise.ListRelationshipType";
        public const string SP_ListUserRelationshipTypes = "[Enterprise].[ListUserRelationshipTypes]";

        //RoleType
        public const string SP_ListRoleType = "Enterprise.ListRoleType";
        public const string SP_ListRoleTypeDependency = "Enterprise.ListRoleTypeDependency";

        //Products
        public const string SP_UpdateProductSettingByPersona = "Enterprise.UpdateProductSettingByPersona";
        public const string SP_ListProductsByOrganization = "Enterprise.ListProductsByOrganization";
        public const string SP_ListProductsByOrganizationForAdminUser = "Enterprise.ListProductsByOrganizationForAdminUser";
        public const string SP_ListProductSettingsByOrganization = "Enterprise.ListProductSettingsByOrganization";
        public const string SP_ListProductSettingsByPersona = "Enterprise.ListProductSettingsByPersona";
        public const string SP_ListProductSettingsByPersonaId = "Enterprise.ListProductSettingsByPersonaId";
        public const string SP_ListGlobalSettingsForProduct = "Enterprise.ListGlobalSettingsForProduct";
        public const string SP_ListProductGlobalSettingsBySettingType = "Enterprise.ListProductGlobalSettingsBySettingType";
        public const string SP_CreateProductSetting = "Enterprise.CreateProductSetting";
        public const string SP_CreateProductSettingType = "Enterprise.CreateProductSettingType";
        public const string SP_LinkProductSettingToConfiguration = "Enterprise.LinkProductSettingToConfiguration";

        public const string SP_CreateProductConfigurationbyPersonaId = "Enterprise.CreateProductConfigurationbyPersonaId";
        public const string SP_UpdatePersonaConfiguration = "Enterprise.UpdatePersonaConfiguration";
        public const string SP_ListProductBatchStatusesByRealPageId = "Enterprise.ListProductBatchByRealPageId";
        public const string SP_GetProductSettingType = "Enterprise.GetProductSettingType";
        public const string SP_ListProductFamilies = "Enterprise.ListProductFamilies";
        public const string SP_ListProduct = "Enterprise.ListProduct";
        public const string SP_ListProductsByPersonaId = "Enterprise.ListProductsByPersonaId";
        public const string SP_GetUserProductBatchJsonData = "Enterprise.GetUserProductBatchJsonData";
        public const string SP_ManagePersonaProductError = "Enterprise.ManagePersonaProductError";

        public const string SP_InsertProductLoginActivitybyUser = "Enterprise.InsertProductLoginActivitybyUser";
        public const string SP_GetProductsByPersonaId = "Security.GetProductsByPersonaId";
        public const string SP_GetPersonaProductPrimaryProperties = "Enterprise.GetPersonaProductPrimaryProperties";

        //Remove
        public const string SP_ListProductTypes = "Enterprise.ListProductTypes";

        //ProductSettingType
        public const string SP_ListProductSettingType = "Enterprise.ListProductSettingType";


        //Contact Mechanism
        public const string SP_CreateContactMechanism = "Person.CreateContactMechanism";
        public const string SP_LinkUsageTypeToPartyContactMechanism = "Person.LinkUsageTypeToPartyContactMechanism";
        public const string SP_UpdateContactMechanismUsageForParty = "Person.UpdateContactMechanismUsageForParty";

        //ContactMechanism To Party
        public const string SP_LinkContactMechanismToParty = "Person.LinkContactMechanismToParty";

        //Expire ContactMechanism to Party
        public const string SP_ExpirePartyContactMechanism = "Person.ExpirePartyContactMechanism";

        //Contact Mechanism for a Person (Electronic, Postal, & Telecommunication)
        public const string SP_ListContactMechanismsForPerson = "Person.ListContactMechanismsForPerson";

        //Contact Mechanism UsageType
        public const string SP_ListContactMechanismUsageType = "Enterprise.ListContactMechanismUsageType";

        //Contact Preference
        public const string SP_AddUpdateContactMechanismPreference = "Enterprise.AddUpdateContactMechanismPreference";
        public const string SP_DeleteContactMechanismPreference = "Enterprise.DeleteContactMechanismPreference";

        //Electronic Address Contact Mechanism
        public const string SP_CreateElectronicAddress = "Person.CreateElectronicAddress";
        public const string SP_ListEmailsForPerson = "Person.ListEmailsForPerson";

        //Geographic Boundary
        public const string SP_CreateGeographicBoundary = "Person.CreateGeographicBoundary";
        public const string SP_LinkGeographicBoundaryToContactMechanism = "Person.LinkGeographicBoundaryToContactMechanism";

        //Telecommunication Number Contact Mechanism
        public const string SP_CreateTelecommunicationNumber = "Person.CreateTelecommunicationsNumber";
        public const string SP_ListTelecommunicationNumbersForPerson = "Person.ListTelecommunicationNumbersForPerson";

        //Postal Address
        public const string SP_CreateStreetAddress = "Person.CreateStreetAddress";

        //Postal Address Contact Mechanism
        public const string SP_ListPostalAddressesForPerson = "Person.ListPostalAddressesForPerson";

        //Preferred Contact Method
        public const string SP_ListPreferredContactMethods = "Person.ListPreferredContactMethods";
        public const string SP_GetIdentityProviderTypeByLoginName = "Ident.GetIdentityProviderTypeByLoginName";

        //SAML
        public const string SP_GetProductSamlDetails = "Ident.GetProductSamlDetails";
        public const string SP_GetProductSamlSettings = "Ident.GetProductSamlSettings";
        public const string SP_ListPersonaProductsSamlDetails = "Ident.ListPersonaProductsSamlDetails";
        public const string SP_GetSamlProductAttributes = "Ident.GetSamlProductAttributes";

        public const string SP_CreateSamlUserAttribute = "Ident.CreateSamlUserAttribute";
        public const string SP_DeleteSamlUserProductInfoAndStatus = "Enterprise.UnLinkProductFromPersona";
        public const string SP_CreateSamlAttribute = "Ident.CreateSamlAttribute";
        public const string SP_CreateSamlAttributeType = "Ident.CreateSamlAttributeType";
        public const string SP_UpdateSamlUserAttribute = "Ident.UpdateSamlUserAttribute";
        public const string SP_RemoveSamlUserAttributeBySamlAttributeId = "Ident.RemoveSamlUserAttributeBySamlAttributeId";

        //Communication
        public const string SP_GetEmailTemplateByParty = "Enterprise.GetEmailTemplateByParty";
        public const string SP_CreateCESCommunicationEvent = "Enterprise.CreateCESCommunicationEvent";
        public const string SP_CreateCommunicationEmailTemplate = "Enterprise.CreateCommunicationEmailTemplate";
        public const string SP_CreateCommunicationEvent = "Enterprise.CreateCommunicationEvent";
        public const string SP_CreateCommunicationEventEmail = "Enterprise.CreateCommunicationEventEmail";
        public const string SP_CreateCommunicationEventPurposeType = "Enterprise.CreateCommunicationEventPurposeType";
        public const string SP_CreateCommunicationEventRole = "Enterprise.CreateCommunicationEventRole";
        public const string SP_CreateCommunicationEventRoleType = "Enterprise.CreateCommunicationEventRoleType";
        public const string SP_ListCommunicationEmailTemplates = "Enterprise.ListCommunicationEmailTemplates";
        public const string SP_ListCommunicationEventPurposeTypes = "Enterprise.ListCommunicationEventPurposeTypes";
        public const string SP_ListCommunicationEventPurposeUsages = "Enterprise.ListCommunicationEventPurposeUsages";
        public const string SP_ListCommunicationRoleTypes = "Enterprise.ListCommunicationRoleTypes";
        public const string SP_ListCommunicationEvents = "Enterprise.ListCommunicationEvents";

        //ProductBatch
        public const string SP_CreateProductBatch = "Batch.CreateProductBatch";
        public const string SP_UpdateProductBatch = "Batch.UpdateProductBatch";
        public const string SP_SavePersonaProductProperties = "Enterprise.SavePersonaProductProperties";
        public const string SP_SaveProductActivityLog = "Batch.SaveProductActivityLog";
        public const string SP_DeleteProductActivityLog = "Batch.DeleteProductActivityLog";
        public const string SP_GetProductActivityLog = "Batch.GetProductActivityLog";

        //BatchProcessor
        public const string SP_GetUserBatchRecords = "Batch.GetUserBatchRecords";
        public const string SP_CreateBatchProcessorGroup = "Batch.CreateBatchProcessorGroup";
        public const string SP_UpdateProcessorGroupStatus = "Batch.UpdateBatchProcessorGroupStatus";
        public const string SP_UpdateEnterpriseRoleProductBatch = "Batch.UpdateEnterpriseRoleProductBatch";
        public const string SP_UpdatePrimaryPropertyProductBatch = "Batch.UpdatePrimaryPropertiesProductBatch";
        public const string SP_InsertBatchProcessorLog = "Batch.InsertBatchProcessorLog";
        public const string SP_GetBulkUserBatchRecords = "Batch.GetBulkUserBatchRecords";
        public const string SP_UpdateBulkUserProductBatch = "Batch.UpdateBulkUserProductBatch";
        public const string SP_UpdateCompanyStatus = "Batch.UpdateCompanyStatus";

        //Green Book
        public const string SP_CreatePropertyMapping = "Enterprise.CreatePropertyMapping";
        public const string SP_ListPropertyMapping = "Enterprise.ListPropertyMapping";
        public const string SP_UpdatePropertyMappingReMap = "Enterprise.UpdatePropertyMappingReMap";
        public const string SP_AddUpdatePropertyMapping = "Enterprise.AddUpdatePropertyMapping";

        public const string SP_CreatePropertyInstance = "Enterprise.CreatePropertyInstance";
        public const string SP_CreatePropertyInstanceMapping = "Enterprise.CreatePropertyInstanceMapping";
        public const string SP_AddUpdatePropertyInstanceMapping = "Enterprise.AddUpdatePropertyInstanceMapping";
        public const string SP_DeletePropertyInstanceMapping = "Enterprise.DeletePropertyInstanceMapping";
        
        public const string SP_GetPropertyInstanceByPersonaId = "Enterprise.GetPropertyInstanceByPersonaId";
        public const string SP_GetPropertyInstanceIdsByPersonaId = "Enterprise.GetPropertyInstanceIdsByPersonaId";
        public const string SP_GetPropertyInstanceListById = "Enterprise.GetPropertyInstanceListById";
        public const string SP_GetPropertyInstanceListByIdWithPaging = "Enterprise.GetPropertyInstanceListByIdWithPaging";
        public const string SP_UpdatePropertyInstance = "Enterprise.UpdatePropertyInstance";
        public const string SP_UpdatePropertyInstances =  "Enterprise.UpdateUPFMPropertyInstances";
        public const string SP_DeletePropertyInstance = "Enterprise.DeletePropertyInstance";
        


        public const string SP_ListRolesByParty = "Security.ListRolesByParty";
        public const string SP_ListRolesForProductsByPersonaId = "Security.ListRolesForProductsByPersonaId";
        public const string SP_ListRolesForProductsByPartyId = "Security.ListRolesForProductsByPartyId";
        public const string SP_ListRolesAssociatedWithRights = "Security.ListRolesAssociatedWithRights";
        public const string SP_ListRightsAssociatedWithRoles = "Security.ListRightsAssociatedWithRoles";
        public const string SP_ListRightForProductsByPartyId = "Security.ListRightForProductsByPartyId";
        public const string SP_CreateRole = "Security.CreateRole";
        public const string SP_UpdateRole = "Security.UpdateRole";
        public const string SP_DeleteRole = "Security.DeleteRole";
        public const string SP_LinkRightsToRoles = "Security.LinkRightsToRoles";
        public const string SP_ListAllRights = "Security.ListAllRights";
        public const string SP_ListSecurityStatus = "Security.ListSecurityStatus";
        public const string SP_SetDefaulteRole = "Security.SetDefaultRole";
        public const string SP_InsertUpdateRoleTemplateUserMapping = "Security.InsertUpdateRoleTemplateUserMapping";
        public const string SP_UnassignEnterpriseRoleFromUser = "Security.DeleteRoleTemplateUserMappingByPersona";
        public const string SP_GetPersistRights = "Security.GetPersistRights";
        public const string SP_GetADGroupRightsByPersona = "Security.GetADGroupRightsByPersona";
        public const string SP_GetRolesForADGroupByPersona = "Security.GetRolesForADGroupByPersona";

        //Configuration Settings
        public const string SP_ListUserLoginSettings = "Enterprise.ListUserLoginSettings";
        public const string SP_UpdateMasterConfigurationSetting = "Enterprise.UpdateMasterConfigurationSetting";
        public const string SP_ListOrganizationSettings = "Enterprise.ListOrganizationSettings";
        public const string SP_ListOrganizations = "Enterprise.ListOrganizations_Ver01";
        public const string SP_CreateMasterConfigurationSetting = "Enterprise.CreateMasterConfigurationSetting";
        public const string SP_GetOrganizationMasterConfiguration = "Enterprise.GetOrganizationMasterConfiguration";
        public const string SP_CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting = "Enterprise.CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting";
     

        #region Persona Security
        public const string SP_SecurityListRolesByRealPageID = "Security.ListRolesByRealPageID";
        public const string SP_LinkPersonaToRole = "Security.LinkPersonaToRole";
        public const string SP_ListRightsByPersonaID = "Security.ListPersonaRightsAndActionsByRoute";
        public const string SP_ListPersonaRightsAndActionsByRoute = "Security.ListPersonaRightsAndActionsByRoute";
        #endregion

        public const string SP_GetUserDetails = "Enterprise.GetUserDetails_Ver01";
        public const string SP_GetUnifiedLoginDefaultRole = "Security.GetUnifiedLoginDefaultRole";
        public const string SP_GetSuperUsersCountByOrganization = "Person.GetSuperUsersCountByOrganization";

        //StatusType
        public const string SP_GetStatusTypes = "Enterprise.GetStatusTypes";

        //Security Setting
        public const string SP_GetSecuritySetting = "Ident.GetSecuritySetting";
        public const string SP_UpdateSecuritySetting = "Ident.UpdateSecuritySetting";

        //Unified Setting
        public const string SP_GetUnifiedSetting = "Settings.GetUnifiedSetting";
        public const string SP_UpdateUnifiedSetting = "Ident.UpdateUnifiedSetting";

        //Accessability Setting
        public const string SP_GetNavigationMenuSettingUnaccessable = "Enterprise.GetNavigationMenuSettingUnaccessable";

        //Custom Fields
        public const string SP_GetFieldsByPartyId = "Settings.GetFieldsByPartyId";
        public const string SP_GetFieldsValuesByUserLoginPersonaId = "Settings.GetFieldsValuesByUserLoginPersonaId";
        public const string SP_AddUpdateFieldValue = "Settings.AddUpdateFieldValue";

        //EmployeeId
        public const string SP_CreateEmployeeId = "Enterprise.CreateUserEmployeeId";
        public const string SP_UpdateEmployeeId = "Enterprise.UpdateUserEmployeeId";
        public const string SP_GetEmployeeId = "Enterprise.GetUserEmployeeIdByUserLoginPersonaId";

        //SuperVisorId
        public const string SP_GetSuperVisorId = "Enterprise.GetSupervisor";
        public const string SP_InsertUpdateSuperVisor = "Enterprise.InsertUpdateSuperVisor";

        //Maintenance
        public const string SP_InsertOrganizationRemovalQueue = "Maintenance.InsertOrganizationRemovalQueue";
        public const string SP_ListOrganizationToDelete = "Maintenance.ListOrganizationToDelete";
        public const string SP_DeleteOrganization = "Maintenance.DeleteOrganization";
        public const string SP_UpdateOrganizationRemovalQueueStatus = "Maintenance.UpdateOrganizationRemovalQueueStatus";

        //HOTS
        public const string SP_GetBaseCompanyUPFMId = "Hots.GetBaseCompanyRealpageId";
        public const string SP_ListHotsBaseOrganizationUsers = "Hots.ListHotsBaseOrganizationUsers";
        public const string SP_InsertHotsCompanyRelationship = "Hots.InsertHotsCompanyRelationship";
        public const string SP_InsertHotsPropertyRelationship = "Hots.InsertHotsPropertyRelationship";
        public const string SP_UpdateHotsCloneUserPassword = "Hots.UpdateHotsCloneUserPassword";

        // Navigation Menu
        public const string SP_GetNavigationMenu = "Enterprise.GetNavigationMenu";
        public const string SP_GetNavigationMenuRights = "Enterprise.GetNavigationMenuRights";

        //Enterprise role
        public const string SP_GetRoleTemplateProductRoleMappings = "Security.GetRoleTemplateProductRolesMappings";
        public const string SP_GetEnterpriseRoleProductsByOrganization = "Security.GetEnterpriseRoleProductsByOrganization";
        public const string SP_GetEnterpriseRoleUpdatedProductsByRoleTemplateId = "Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId";
        public const string SP_GetEnterpriseRoleDeletedProductsByRoleTemplateId = "Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId";
        public const string SP_GetUserRoleTemplate = "Security.GetUserRoleTemplate";
        public const string SP_GetEnterpriseRoleNewProductsByRoleTemplateId = "Security.GetEnterpriseRoleNewProductsByRoleTemplateId";
        public const string SP_GetEnterpriseDelegateRole = "Security.GetDelegateAdminRoleTemaplte";
        public const string SP_InsertUpdateDelegateAdminTemplate = "Security.InsertUpdateDelegateAdminRoleTemplate";
        public const string SP_UpdateDelegateAdminStatus = "Security.UpdateDelegateAdminStatus";
        public const string SP_GetRoleTemplate = "Security.GetRoleTemplate";

        //AD groups
        public const string SP_GetADGroupsForUser = "Security.GetADGroupsByPersona";
        public const string SP_GetADGroupsForProduct = "Security.GetADGroupsByProductId";
        public const string SP_GetUserManagementADGroupsByProduct = "Security.GetUserManagementADGroupsByProduct";
        public const string SP_GetPersonaProductADGroupCount = "Security.GetPersonaProductsADGroupsCount";

        //AD User Details
        public const string SP_GetADDetailsForUser = "Security.GetADDetailsForUser";
        public const string SP_GetADGroupsByPersonaId = "Security.GetADGroupsProductRightByPersonaId";
        public const string SP_GetOrganizationTypeADGroups = "Enterprise.GetOrganizationTypeADGroups";
        public const string SP_GetEmployeeProductADGroupMapping = "Enterprise.GetEmployeeProductADGroupMapping";
        public const string SP_AddUpdateEmployeeProductADGroupMapping = "Enterprise.AddUpdateEmployeeProductADGroupMapping";

        //User Sync Properties
        public const string SP_GetUserSyncJobTaskDetails = "Enterprise.GetUserSyncJobTaskDetails";
        public const string SP_AddPersonaProductMatchedPrimaryProperties = "Enterprise.AddPersonaProductMatchedPrimaryProperties";
        public const string SP_AddUpdatePersonaProductPropertyInstanceMapping = "Enterprise.AddUpdatePersonaProductPropertyInstanceMapping";
        public const string SP_DeletePersonaProductMatchedPrimaryProperties = "Enterprise.DeletePersonaProductMatchedPrimaryProperties";
        /// <summary>
        /// Bulk insert/delete property instance mappings for a user using TVP
        /// </summary>
        public const string SP_BulkCreateDeleteUPFMPropertyInstanceMapping = "Enterprise.BulkCreateDeleteUPFMPropertyInstanceMapping";

        /// <summary>
        /// Insert company address for organization
        /// </summary>
        public const string SP_InsertCompanyAddress = "Enterprise.InsertCompanyAddress";
    }

    public static class EnterpriseStoredProcNameConstants
    {
        public const string SP_ListUserInformation = "Person.GetUserInformation_Ver02";
        public const string SP_CreateUnityUser = "Ident.CreateUser_Ver01";
        public const string SP_ListUsersWithCompanyId = "Person.ListUsersWithCompanyId";
        public const string SP_ListUsersWithCompanyId_Ver2 = "Person.ListUsersWithCompanyId_Ver2";
        public const string SP_ListUsersWithCompanyId_Ver3 = "Person.ListUsersWithCompanyId_Ver3";
        public const string SP_ListUsersWithCompanyId_Ver4 = "Person.ListUsersWithCompanyId_Ver4";
        public const string SP_ListULMappingPersonaIdForProductUserId = "Enterprise.ListULMappingPersonaIdForProductUserId";
        public const string SP_ListULMappingPersonaIdForProductUserId_v2 = "Enterprise.ListULMappingPersonaIdForProductUserId_v2";
        public const string SP_ListUsersProductsDetailsLoginByPersonaId = "Person.ListUsersProductDetailsLoginByPersonaId";
        public const string SP_ListUsersProductsDetailsLoginByLoginName = "Person.ListUsersProductDetailsLoginByLoginName";
    }
}
