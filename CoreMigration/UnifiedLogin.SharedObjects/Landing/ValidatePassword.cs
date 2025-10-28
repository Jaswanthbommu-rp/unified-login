using System;
namespace UnifiedLogin.SharedObjects.Landing
{
    public class ValidatePassword
    {
        public long PartyId { get; set; } //OrganizationId
        public string PasswordToValidate { get; set; }
        public string EnterpriseUserName { get; set; }
        public DateTime? PasswordModifiedDate { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool CheckPasswordHistory { get; set; }
    }
}