using UnifiedLogin.SharedObjects.IdentityConfig;
using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public interface IUserLoginOnly : IUserLoginCommon
    {
        bool Is3rdPartyIDP { get; set; }
        bool IsLoginNameNullOrWhiteSpace { get; }
        DateTime? LastLogin { get; set; }
        string LoginNameType { get; set; }
        long PartyId { get; set; }
        string Password { get; set; }
        string PasswordHash { get; set; }
        DateTime? PasswordModifiedDate { get; set; }
        string PasswordSalt { get; set; }
        long PersonaId { get; set; }
        Guid RealPageId { get; set; }
    }
}