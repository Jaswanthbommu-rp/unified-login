using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public interface IIdentityServerRepository
    {
        Client GetClient(string clientCode);
        IEnumerable<Token> GetTokensBySubject(string subjectCode, int tokenType);
        Token GetIdentityToken(string tokenKey, int tokenType);
        void DeleteIdentityTokenByKey(string tokenKey, int tokenType);
        void DeleteIdentityTokenBySubjectAndClient(string subjectCode, string clientCode, int tokenType);
        void InsertIdentityToken(Token token);
        Consent GetConsentBySubjectAndClient(string subjectCode, string clientCode);
        void DeleteConsentBySubjectAndClient(string subjectCode, string clientCode);
        void InsertConsent(Consent consent);
        void UpdateConsent(Consent consent);
        IEnumerable<Consent> GetConsentsBySubject(string subjectCode);
        IEnumerable<Scope> GetAllScopes();
        IEnumerable<ScopeClaim> GetAllScopeClaims();
        IEnumerable<ScopeSecret> GetAllScopeSecrets();
        void UpdateIdentityTokenExpiry(string tokenKey, DateTimeOffset expiry);
        //IEnumerable<OrganizationClientUserClaim> GetAllOrganizationClientUserClaims(int organizationId, string clientCode, long userId);
        IEnumerable<PortfolioProductUserClaims> GetAllPortfolioProductUserClaims(int portfolioId, string clientCode, long userId);
        List<ClientUserClaim> GetUserClaimTypesForClient(string clientId);
    }
}