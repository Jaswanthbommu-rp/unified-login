using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using DbConnectionEnum = RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum.DbConnectionEnum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public class IdentityServerRepository : BaseRepository, IIdentityServerRepository
    {
        #region Ctor

        public IdentityServerRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        #endregion

        #region IIdentityServerRepository Implementation

        public Client GetClient(string clientCode)
        {
            using (var repository = GetRepository())
            {
                var client = repository.GetOne<Client>(StoredProcNameConstants.SP_GetClientByClientCode, new { clientCode });

                if (client != null)
                {
                    using (var repository2 = GetRepository())
                    {
                        var multiResuts = repository2.QueryMultiple(StoredProcNameConstants.SP_GetClientDetails, new {ClientId = client.ClientId});

                        client.ClientRedirectUris = multiResuts.Read<ClientRedirectUri>();
                        client.ClientScopes = multiResuts.Read<ClientScope>();
                        client.ClientSecrets = multiResuts.Read<ClientSecret>();
                        client.ClientPostLogoutRedirectUris = multiResuts.Read<ClientPostLogoutRedirectUri>();
                        client.ClientClaims = multiResuts.Read<ClientClaim>();
                    }
                }

                return client;
            }
        }

        public IEnumerable<Token> GetTokensBySubject(string subjectCode, int tokenType)
        {
            using (var repository = GetRepository())
            {
                var tokens = repository.GetMany<Token>(StoredProcNameConstants.SP_GetTokensBySubject, new { subjectCode, tokenType });
                return tokens;
            }
        }

        public Token GetIdentityToken(string tokenKey, int tokenType)
        {
            using (var repository = GetRepository())
            {
                var token = repository.GetOne<Token>(StoredProcNameConstants.SP_GetToken, new { tokenKey, tokenType });
                return token;
            }
        }

        public void DeleteIdentityTokenByKey(string tokenKey, int tokenType)
        {
            using (var repository = GetRepository())
            {
                repository.ExecuteNonQuery(StoredProcNameConstants.SP_DeleteTokenByKey, new { tokenKey, tokenType });
            }
        }

        public void DeleteIdentityTokenBySubjectAndClient(string subjectCode, string clientCode, int tokenType)
        {
            using (var repository = GetRepository())
            {
                repository.ExecuteNonQuery(StoredProcNameConstants.SP_DeleteTokenBySubjectAndClient, new { subjectCode, clientCode, tokenType });
            }
        }

        public void InsertIdentityToken(Token token)
        {
            dynamic param = new
            {
                token.TokenKey,
                token.TokenType,
                token.ClientCode,
                token.SubjectCode,
                token.Expiry,
                token.JsonCode,
                token.AuthCodeChallenge,
                token.AuthCodeChallengeMethod,
                token.IsOpenId,
                token.Nonce,
                token.RedirectUri,
                token.SessionId,
                token.WasConsentShown
            };

            using (var repository = GetRepository())
            {
                repository.ExecuteNonQuery(StoredProcNameConstants.SP_InsertToken, param);
            }
        }

        public Consent GetConsentBySubjectAndClient(string subjectCode, string clientCode)
        {
            using (var repository = GetRepository())
            {
                var token = repository.GetOne<Consent>(StoredProcNameConstants.SP_GetConsentBySubjectAndClient, new { subjectCode, clientCode });
                return token;
            }
        }

        public void DeleteConsentBySubjectAndClient(string subjectCode, string clientCode)
        {
            using (var repository = GetRepository())
            {
                repository.ExecuteNonQuery(StoredProcNameConstants.SP_DeleteConsentBySubjectAndClient, new { subjectCode, clientCode });
            }
        }

        public void InsertConsent(Consent consent)
        {
            using (var repository = GetRepository())
            {
                repository.ExecuteNonQuery(StoredProcNameConstants.SP_InsertConsent, new { consent.SubjectCode, consent.ClientCode, consent.Scopes });
            }
        }

        public void UpdateConsent(Consent consent)
        {
            using (var repository = GetRepository())
            {
                repository.ExecuteNonQuery(StoredProcNameConstants.SP_UpdateConsent, new { consent.SubjectCode, consent.ClientCode, consent.Scopes });
            }
        }

        public IEnumerable<Consent> GetConsentsBySubject(string subjectCode)
        {
            using (var repository = GetRepository())
            {
                var consents = repository.GetMany<Consent>(StoredProcNameConstants.SP_GetConsentsBySubject, new { subjectCode });
                return consents;
            }
        }

        public IEnumerable<Scope> GetAllScopes()
        {
            using (var repository = GetRepository())
            {
                var scopes = repository.GetMany<Scope>(StoredProcNameConstants.SP_GetAllScopes, null);
                return scopes;
            }
        }

        public IEnumerable<ScopeClaim> GetAllScopeClaims()
        {
            using (var repository = GetRepository())
            {
                var scopeClaims = repository.GetMany<ScopeClaim>(StoredProcNameConstants.SP_GetAllScopeClaims, null);
                return scopeClaims;
            }
        }

        public IEnumerable<ScopeSecret> GetAllScopeSecrets()
        {
            using (var repository = GetRepository())
            {
                var scopeSecret = repository.GetMany<ScopeSecret>(StoredProcNameConstants.SP_GetAllScopeSecrets, null);
                return scopeSecret;
            }
        }

        public void UpdateIdentityTokenExpiry(string tokenKey, DateTimeOffset expiry)
        {
            using (var repository = GetRepository())
            {
                repository.ExecuteNonQuery(StoredProcNameConstants.SP_UpdateTokenExpiry, new { tokenKey, expiry });
            }
        }

        //public IEnumerable<OrganizationClientUserClaim> GetAllOrganizationClientUserClaims(int organizationId, string clientCode, long userId)
        //{
        //    //TODO: THIS SP IS NOT IMPLEMENTED -- CHECK IF THIS REQUIRE
        //    using (var repository = GetRepository())
        //    {
        //        var allOrganizationClientUserClaims = repository.GetMany<OrganizationClientUserClaim>(StoredProcName.SP_GetAllOrganizationClientUserClaims, new { organizationId, clientCode, userId });
        //        return allOrganizationClientUserClaims;
        //    }
        //}

        public IEnumerable<PortfolioProductUserClaims> GetAllPortfolioProductUserClaims(int portfolioId, string clientCode, long userId)
        {
            using (var repository = GetRepository())
            {
                var allPortfolioProductUserClaims = repository.GetMany<PortfolioProductUserClaims>(StoredProcNameConstants.SP_GetAllPortfolioProductUserClaims, new { portfolioId, clientCode, userId });
                return allPortfolioProductUserClaims;
            }
        }

        public IEnumerable<GlobalSetting> GetGlobalSettings()
        {
            using (var repository = GetRepository())
            {
                var allGlobalSettings = repository.GetMany<GlobalSetting>(StoredProcNameConstants.SP_GetGlobalSettings, null);
                return allGlobalSettings;
            }

        }

        public List<ClientUserClaim> GetUserClaimTypesForClient(string clientId)
        {
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"getUserClaimTypesForClient{clientId}";
            List<ClientUserClaim> clientUserClaims = rpcache.GetFromCache<List<ClientUserClaim>>(cacheKey, 120, () =>
            {
                dynamic param = new
                {
                    ClientName = clientId
                };

                using (var repository = GetRepository())
                {
                    var result = repository.GetMany<ClientUserClaim>(StoredProcNameConstants.SP_GetUserClaimTypesRequiredForClient, param);
                    return result;
                }
            });
            return clientUserClaims;
        }
        
        public IEnumerable<SameSiteExclusion> GetSameSiteExclusionList()
        {
            using (var repository = GetRepository())
            {
                return repository.GetMany<SameSiteExclusion>(StoredProcNameConstants.SP_GetAllSameSiteValues, null);
            }
        }

        #endregion
    }
}
