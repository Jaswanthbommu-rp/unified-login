using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing.UserUpdate
{
    public class UpdateUserProfileEntity
    {
        public Guid LoggedInUserRealPageId { get; set; }
        public IProfileDetail NewProfile { get; set; }
        public IProfileDetail OldProfile { get; set; }
        public long CreateUserPersonaId { get; set; }
        public string SaveProductBatchError { get; set; }
        public bool IsCurrentOrgThePrimaryOrg { get; set; }
        public IList<IdentityProviderType> IdentityProviderTypeList { get; set; }
        public IList<ProductBatch> ProductBatchData { get; set; }
        public IList<ContactMechanismUsageType> EmailUsageType { get; set; }
        public Organization OrganizationExternalUser { get; set; }
        public IUserLoginOnly UserLoginOnly { get; set; }
        public IList<UserOrganization> UserPersonaOrganizationList { get; set; }
        public List<long> ExistingRoleIds { get; set; }
        public OrganizationStatus CurrentPrimaryOrgStatus { get; set; }
        public OrganizationStatus CurrentOrgStatus { get; set; }
        public IList<Persona> PersonaList { get; set; }
        public IList<string> AoProductsAvailableForUser { get; set; }
        public Organization CurrentOrg { get; set; }
        public IList<EditorAssignedPersona> EditorAssignedPersonaList { get; set; }
        public bool UserIsExternalEverywhere { get; set; }
    }
}
