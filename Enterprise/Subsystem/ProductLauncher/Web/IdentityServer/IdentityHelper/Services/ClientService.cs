using System.Collections.Generic;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using RPModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
	public class ClientService : IClientStore
	{
		private readonly IIdentityServerRepository _identityServerRepository;

		public ClientService(IIdentityServerRepository identityServerRepository)
		{
			_identityServerRepository = identityServerRepository;
		}

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
			RPObjectCache rpcache = new RPObjectCache();
			var cacheKey = "clients_" + clientId;
			RPModel.Client client = await rpcache.GetFromCacheAsync<RPModel.Client>(cacheKey, 300, () => {
				// load from api
				return _identityServerRepository.GetClient(clientId);
			});

			if (client != null)
            {
                var idClient = new Client
				{
                    AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
                    AccessTokenLifetime = client.AccessTokenLifetime,
                    AccessTokenType = (AccessTokenType)client.AccessTokenType,
                    AllowAccessToAllCustomGrantTypes = client.AllowAccessToAllGrantTypes,
                    AllowAccessToAllScopes = client.AllowAccessToAllScopes,
                    AllowClientCredentialsOnly = client.AllowClientCredentialsOnly,
                    AllowRememberConsent = client.AllowRememberConsent,
                    AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                    AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                    ClientId = client.ClientCode,
                    ClientName = client.ClientName,
                    ClientUri = client.ClientUri,
                    Enabled = client.Enabled,
                    EnableLocalLogin = client.EnableLocalLogin,
                    Flow = (Flows)client.Flow,
                    IdentityTokenLifetime = client.IdentityTokenLifetime,
                    IncludeJwtId = client.IncludeJwtId,
                    LogoUri = client.LogoUri,
                    LogoutSessionRequired = client.LogoutSessionRequired,
                    LogoutUri = client.LogoutUri,
                    PrefixClientClaims = client.PrefixClientClaims,
                    RefreshTokenExpiration = (TokenExpiration)client.RefreshTokenExpiration,
                    RefreshTokenUsage = (TokenUsage)client.RefreshTokenUsage,
                    RequireConsent = client.RequireConsent,
                    RequireSignOutPrompt = client.RequireSignOutPrompt,
                    SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                    UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenOnRefresh,
                    PostLogoutRedirectUris = client.ClientPostLogoutRedirectUris.Select(r => r.Uri).ToList(),
                    AllowedScopes = client.ClientScopes.Select(s => s.Scope).ToList(),
                    ClientSecrets = client.ClientSecrets.Select(s =>
                    {
                        return new Secret(s.Value, s.Description, s.Expiration);
                    }).ToList(),
                    RedirectUris = client.ClientRedirectUris.Select(r => r.Uri).ToList()
					,Claims = client.ClientClaims.Select(s =>
					{
						return new Claim(s.Type, s.Value); 
					}).ToList()
				};

                return idClient;
            }
            return null;
        }
    }
}
