using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.UserUpdate
{
    public class UpdateUserProfileEntity
    {
        public Guid LoggedInUserRealPageId { get; set; }
        public IProfileDetail NewProfile { get; set; }
        public IProfileDetail OldProfile { get; set; }
        public long CreateUserPersonaId { get; set; }
        public int GreenBookRole { get; set; }
        public string SaveProductBatchError { get; set; }
        public long? ContactMechanismId { get; set; }
        public bool UserIsActive { get; set; }
        public bool IsFeatureUser { get; set; }
        public bool IsCurrentOrgThePrimaryOrg { get; set; }
        public IList<IdentityProviderType> IdentityProviderTypeList { get; set; }
        public IList<ProductBatch> ProductBatchData { get; set; }
        public IList<ContactMechanismUsageType> EmailUsageType { get; set; }
        public IOrganization OrganizationExternalUser { get; set; }
        public IUserLoginOnly UserLoginOnly { get; set; }
        public IList<UserOrganization> UserPersonaOrganizationList { get; set; }
        public Persona Persona { get; set; }
        public long CurrentOrgPartyId { get; set; }
        public long AssignUserPersonaId { get; set; }
        public long ExistingRoleId { get; set; }
        public OrganizationStatus CurrentPrimaryOrgStatus { get; set; }
        public OrganizationStatus CurrentOrgStatus { get; set; }
        public IList<Persona> PersonaList { get; set; }
        public IList<string> AoProductsAvailableForUser { get; set; }
        public UserDetails UserDetails { get; set; }
        public bool ProfileChanged { get; set; }
        public bool LoginNamechanged { get; set; }
        public Organization CurrentOrg { get; set; }
        public IList<EditorAssignedPersona> EditorAssignedPersonaList { get; set; }
        public bool UserIsExternalEverywhere { get; set; }
    }
}
