using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Clients
{
	public class ClientsSetupRepository : BaseRepository
	{
		#region Constructor

		/// <summary>
		/// Profile base Constructor
		/// </summary>
		public ClientsSetupRepository() : base(DbConnectionEnum.IdpConfigurationDb) { }
		#endregion

		#region Client
		/// <summary>
		/// Get a list of clients and their details
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Client> GetClientsWithDetails()
		{
			IEnumerable<Client> clientList = null;
			IEnumerable<ClientRedirectUri> clientRedirectUris = GetClientRedirectUri();
			IEnumerable<ClientScope> clientScopes = GetClientScope();
			IEnumerable<ClientSecret> clientSecrets = GetClientSecret();
			IEnumerable<ClientPostLogoutRedirectUri> clientPostLogoutRedirectUris = GetClientPostLogoutRedirectUri();
			IEnumerable<ClientClaim> clientClaims =  GetClientClaim();
			IEnumerable<ClientClaimMapping> clientUserClaims = GetClaimClientMapping();

			using (var repository = GetRepository())
			{
				clientList = repository.GetMany<Client>(StoredProcNameConstants.SP_ClientsSelect, null);
			}

			clientList.ToList().ForEach(c =>
			{
				c.ClientRedirectUris = clientRedirectUris.Where(p => p.ClientId == c.ClientId);
				c.ClientScopes = clientScopes.Where(p => p.ClientId == c.ClientId);
				c.ClientSecrets = clientSecrets.Where(p => p.ClientId == c.ClientId);
				c.ClientPostLogoutRedirectUris = clientPostLogoutRedirectUris.Where(p => p.ClientId == c.ClientId);
				c.ClientClaims = clientClaims.Where(p => p.ClientId == c.ClientId);
				c.ClientUserClaims = clientUserClaims.Where(p => p.ClientId == c.ClientId);
			});
			return clientList;
		}

        /// <summary>
        /// Get a list of clients without details
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Client> GetClientsNoDetails()
        {
            using (var repository = GetRepository())
            {
                return repository.GetMany<Client>(StoredProcNameConstants.SP_ClientsSelect, null);
            }
        }

        /// <summary>
		/// Used to get a client and its details
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public Client GetClientDetailsById(int clientId)
		{
			Client client = GetClientsWithDetails().First(p => p.ClientId == clientId);

			if (client != null)
			{
				client.ClientRedirectUris = GetClientRedirectUri().Where(p => p.ClientId == clientId);
				client.ClientScopes = GetClientScope().Where(p => p.ClientId == clientId);
				client.ClientSecrets = GetClientSecret().Where(p => p.ClientId == clientId);
				client.ClientPostLogoutRedirectUris = GetClientPostLogoutRedirectUri().Where(p => p.ClientId == clientId);
				client.ClientClaims = GetClientClaim().Where(p => p.ClientId == clientId);
			}

			return client;
		}

		/// <summary>
		/// Used to insert a new client
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public Client InsertClient(Client client)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientCode = client.ClientCode,
					ClientName = client.ClientName,
					ClientUri = client.ClientUri,
					LogoUri = client.LogoUri,
					Flow = client.Flow,
					LogoutUri = client.LogoUri,
					IdentityTokenLifetime = client.IdentityTokenLifetime,
					AccessTokenLifetime = client.AccessTokenLifetime,
					AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
					AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
					SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
					RefreshTokenUsage = client.RefreshTokenUsage,
					RefreshTokenExpiration = client.RefreshTokenExpiration,
					AccessTokenType = client.AccessTokenType,
					UpdateAccessTokenOnRefresh = client.UpdateAccessTokenOnRefresh,
					Enabled = client.Enabled,
					LogoutSessionRequired = client.LogoutSessionRequired,
					RequireSignOutPrompt = client.RequireSignOutPrompt,
					AllowAccessToAllScopes = client.AllowAccessToAllScopes,
					AllowClientCredentialsOnly = client.AllowClientCredentialsOnly,
					RequireConsent = client.RequireConsent,
					AllowRememberConsent = client.AllowRememberConsent,
					EnableLocalLogin = client.EnableLocalLogin,
					IncludeJwtId = client.IncludeJwtId,
					AlwaysSendClientClaims = client.AlwaysSendClientClaims,
					PrefixClientClaims = client.PrefixClientClaims,
					AllowAccessToAllGrantTypes = client.AllowAccessToAllGrantTypes
				};
				return repository.GetOne<Client>(StoredProcNameConstants.SP_ClientsInsert, param);
			}
		}

		/// <summary>
		/// Used to update an existing client
		/// </summary>
		/// <param name="orgClient"></param>
		/// <param name="newClient"></param>
		/// <returns></returns>
		public Client UpdateClient(Client orgClient, Client newClient)
		{
			using (var repository = GetRepository())
			{
				int clientUriIsNull = orgClient.ClientUri == null ? 1 : 0;

				dynamic param = new
				{
					ClientCode = newClient.ClientCode,
					ClientName = newClient.ClientName,
					ClientUri = newClient.ClientUri,
					LogoUri = newClient.LogoUri,
					Flow = newClient.Flow,
					LogoutUri = newClient.LogoutUri,
					IdentityTokenLifetime = newClient.IdentityTokenLifetime,
					AccessTokenLifetime = newClient.AccessTokenLifetime,
					AuthorizationCodeLifetime = newClient.AuthorizationCodeLifetime,
					AbsoluteRefreshTokenLifetime = newClient.AbsoluteRefreshTokenLifetime,
					SlidingRefreshTokenLifetime = newClient.SlidingRefreshTokenLifetime,
					RefreshTokenUsage = newClient.RefreshTokenUsage,
					RefreshTokenExpiration = newClient.RefreshTokenExpiration,
					AccessTokenType = newClient.AccessTokenType,
					UpdateAccessTokenOnRefresh = newClient.UpdateAccessTokenOnRefresh,
					Enabled = newClient.Enabled,
					LogoutSessionRequired = newClient.LogoutSessionRequired,
					RequireSignOutPrompt = newClient.RequireSignOutPrompt,
					AllowAccessToAllScopes = newClient.AllowAccessToAllScopes,
					AllowClientCredentialsOnly = newClient.AllowClientCredentialsOnly,
					RequireConsent = newClient.RequireConsent,
					AllowRememberConsent = newClient.AllowRememberConsent,
					EnableLocalLogin = newClient.EnableLocalLogin,
					IncludeJwtId = newClient.IncludeJwtId,
					AlwaysSendClientClaims = newClient.AlwaysSendClientClaims,
					PrefixClientClaims = newClient.PrefixClientClaims,
					AllowAccessToAllGrantTypes = newClient.AllowAccessToAllGrantTypes,
					Original_ClientId = orgClient.ClientId,
					Original_ClientCode  = orgClient.ClientCode,
					Original_ClientName = orgClient.ClientName,
					IsNull_ClientUri = clientUriIsNull,
					Original_ClientUri = orgClient.ClientUri,
					Original_Flow = orgClient.Flow,
					Original_IdentityTokenLifetime = orgClient.IdentityTokenLifetime,
					Original_AccessTokenLifetime = orgClient.AccessTokenLifetime,
					Original_AuthorizationCodeLifetime = orgClient.AuthorizationCodeLifetime,
					Original_AbsoluteRefreshTokenLifetime = orgClient.AbsoluteRefreshTokenLifetime,
					Original_SlidingRefreshTokenLifetime = orgClient.SlidingRefreshTokenLifetime,
					Original_RefreshTokenUsage = orgClient.RefreshTokenUsage,
					Original_RefreshTokenExpiration = orgClient.RefreshTokenExpiration,
					Original_AccessTokenType = orgClient.AccessTokenType,
					Original_UpdateAccessTokenOnRefresh = orgClient.UpdateAccessTokenOnRefresh,
					Original_Enabled = orgClient.Enabled,
					Original_LogoutSessionRequired = orgClient.LogoutSessionRequired,
					Original_RequireSignOutPrompt = orgClient.RequireSignOutPrompt,
					Original_AllowAccessToAllScopes = orgClient.AllowAccessToAllScopes,
					Original_AllowClientCredentialsOnly = orgClient.AllowClientCredentialsOnly,
					Original_RequireConsent = orgClient.RequireConsent,
					Original_AllowRememberConsent = orgClient.AllowRememberConsent,
					Original_EnableLocalLogin = orgClient.EnableLocalLogin,
					Original_IncludeJwtId = orgClient.IncludeJwtId,
					Original_AlwaysSendClientClaims = orgClient.AlwaysSendClientClaims,
					Original_PrefixClientClaims = orgClient.PrefixClientClaims,
					Original_AllowAccessToAllGrantTypes = orgClient.AllowAccessToAllGrantTypes,
					ClientId = newClient.ClientId,
				};

				return repository.GetOne<Client>(StoredProcNameConstants.SP_ClientsUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a client
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public int DeleteClient(Client client)
		{
			using (var repository = GetRepository())
			{
				int clientUriIsNull = client.ClientUri == null ? 1 : 0;

				dynamic param = new
				{
					Original_ClientId = client.ClientId,
					Original_ClientCode = client.ClientCode,
					Original_ClientName = client.ClientName,
					IsNull_ClientUri = clientUriIsNull,
					Original_ClientUri = client.ClientUri,
					Original_Flow = client.Flow,
					Original_IdentityTokenLifetime = client.IdentityTokenLifetime,
					Original_AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
					Original_AccessTokenLifetime = client.AccessTokenLifetime,
					Original_AbsoluteRefreshTokenLifetime= client.AbsoluteRefreshTokenLifetime,
					Original_SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
					Original_RefreshTokenUsage = client.RefreshTokenUsage,
					Original_RefreshTokenExpiration = client.RefreshTokenExpiration,
					Original_AccessTokenType = client.AccessTokenType,
					Original_UpdateAccessTokenOnRefresh = client.UpdateAccessTokenOnRefresh,
					Original_Enabled = client.Enabled,
					Original_LogoutSessionRequired = client.LogoutSessionRequired,
					Original_RequireSignOutPrompt = client.RequireSignOutPrompt,
					Original_AllowAccessToAllScopes = client.AllowAccessToAllScopes,
					Original_AllowClientCredentialsOnly = client.AllowClientCredentialsOnly,
					Original_RequireConsent = client.RequireConsent,
					Original_AllowRememberConsent = client.AllowRememberConsent,
					Original_EnableLocalLogin = client.EnableLocalLogin,
					Original_IncludeJwtId = client.IncludeJwtId,
					Original_AlwaysSendClientClaims = client.AlwaysSendClientClaims,
					Original_PrefixClientClaims = client.PrefixClientClaims,
					Original_AllowAccessToAllGrantTypes = client.AllowAccessToAllGrantTypes
				};

				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClientsDelete, param);
			}
		}

		#endregion

		#region ClientClaims
		/// <summary>
		/// Get a list of client redirect uris
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ClientClaim> GetClientClaim()
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<ClientClaim>(StoredProcNameConstants.SP_ClientClaimsSelect, null);
			}
		}

		/// <summary>
		/// Used to insert a new client claim
		/// </summary>
		/// <param name="clientClaim"></param>
		/// <returns></returns>
		public ClientClaim InsertClientClaim(ClientClaim clientClaim)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = clientClaim.ClientId,
					Type = clientClaim.Type,
					Value = clientClaim.Value
				};

				return repository.GetOne<ClientClaim>(StoredProcNameConstants.SP_ClientClaimsInsert, param);
			}
		}

		/// <summary>
		/// Used to update client redirect uris
		/// </summary>
		/// <param name="orgClientClaim"></param>
		/// <param name="newClientClaim"></param>
		/// <returns></returns>
		public ClientClaim UpdateClientClaim(ClientClaim orgClientClaim, ClientClaim newClientClaim)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = newClientClaim.ClientId,
					Type = newClientClaim.Type,
					Value = newClientClaim.Value,
					Original_ClientClaimsId = orgClientClaim.Id,
					Original_ClientId = orgClientClaim.ClientId,
					Original_Type = orgClientClaim.Type,
					Original_Value = orgClientClaim.Value,
					ClientClaimsId = orgClientClaim.Id
				};
				return repository.GetOne<ClientClaim>(StoredProcNameConstants.SP_ClientClaimsUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a client claim
		/// </summary>
		/// <param name="clientClaim"></param>
		/// <returns></returns>
		public int DeleteClientClaim(ClientClaim clientClaim)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					Original_ClientClaimsId = clientClaim.Id,
					Original_ClientId = clientClaim.ClientId,
					Original_Type  = clientClaim.Type,
					Original_Value = clientClaim.Value
				};

				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClientClaimsDelete, param);
			}
		}

		#endregion

		#region ClientRedirectUri

		/// <summary>
		/// Get a list of client redirect uris
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ClientRedirectUri> GetClientRedirectUri()
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<ClientRedirectUri>(StoredProcNameConstants.SP_ClientRedirectUrisSelect, null);
			}
		}

		/// <summary>
		/// Used to insert a new client redirect uri
		/// </summary>
		/// <param name="clientRedirectUri"></param>
		/// <returns></returns>
		public ClientRedirectUri InsertClientRedirectUri(ClientRedirectUri clientRedirectUri)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = clientRedirectUri.ClientId,
					Uri = clientRedirectUri.Uri
				};

				return repository.GetOne<ClientRedirectUri>(StoredProcNameConstants.SP_ClientRedirectUrisInsert, param);
			}
		}

		/// <summary>
		/// Used to update client redirect uris
		/// </summary>
		/// <param name="orgClientRedirectUri"></param>
		/// <param name="newClientRedirectUri"></param>
		/// <returns></returns>
		public ClientRedirectUri UpdateClientRedirectUri(ClientRedirectUri orgClientRedirectUri, ClientRedirectUri newClientRedirectUri)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = newClientRedirectUri.ClientId,
					Uri = newClientRedirectUri.Uri,
					Original_ClientRedirectUriId = orgClientRedirectUri.Id,
					Original_ClientId = orgClientRedirectUri.ClientId,
					Original_Uri = orgClientRedirectUri.Uri,
					ClientRedirectUriId = orgClientRedirectUri.Id
				};
				return repository.GetOne<ClientRedirectUri>(StoredProcNameConstants.SP_ClientRedirectUrisUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a client redirect uri
		/// </summary>
		/// <param name="clientRedirectUri"></param>
		/// <returns></returns>
		public int DeleteClientRedirectUri(ClientRedirectUri clientRedirectUri)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					Original_ClientRedirectUriId = clientRedirectUri.Id,
					IsNull_Uri = clientRedirectUri.Uri == null ? 1 : 0,
					Original_ClientId = clientRedirectUri.ClientId,
					Original_Uri = clientRedirectUri.Uri
				};

				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClientRedirectUrisDelete, param);
			}
		}

		#endregion

		#region ClientPostLogoutRedirectUris

		/// <summary>
		/// Get a list of client post logout redirect uris
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ClientPostLogoutRedirectUri> GetClientPostLogoutRedirectUri()
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<ClientPostLogoutRedirectUri>(StoredProcNameConstants.SP_ClientPostLogoutRedirectUrisSelect, null);
			}
		}

		/// <summary>
		/// Used to insert a new client post logout redirect uri
		/// </summary>
		/// <param name="clientPostLogoutRedirectUri"></param>
		/// <returns></returns>
		public ClientPostLogoutRedirectUri InsertClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = clientPostLogoutRedirectUri.ClientId,
					Uri = clientPostLogoutRedirectUri.Uri
				};

				return repository.GetOne<ClientPostLogoutRedirectUri>(StoredProcNameConstants.SP_ClientPostLogoutRedirectUrisInsert, param);
			}
		}

		/// <summary>
		/// Used to update client post logout redirect uris
		/// </summary>
		/// <param name="orgClientPostLogoutRedirectUri"></param>
		/// <param name="newClientPostLogoutRedirectUri"></param>
		/// <returns></returns>
		public ClientPostLogoutRedirectUri UpdateClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri orgClientPostLogoutRedirectUri, ClientPostLogoutRedirectUri newClientPostLogoutRedirectUri)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = newClientPostLogoutRedirectUri.ClientId,
					Uri = newClientPostLogoutRedirectUri.Uri,
					Original_ClientPostLogoutRedirectUriId = orgClientPostLogoutRedirectUri.Id,
					Original_ClientId = orgClientPostLogoutRedirectUri.ClientId,
					Original_Uri = orgClientPostLogoutRedirectUri.Uri,
					ClientPostLogoutRedirectUriId = orgClientPostLogoutRedirectUri.Id
				};
				return repository.GetOne<ClientPostLogoutRedirectUri>(StoredProcNameConstants.SP_ClientPostLogoutRedirectUrisUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a client post logout redirect uri
		/// </summary>
		/// <param name="clientPostLogoutRedirectUri"></param>
		/// <returns></returns>
		public int DeleteClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					Original_ClientPostLogoutRedirectUriId = clientPostLogoutRedirectUri.Id,
					Original_ClientId = clientPostLogoutRedirectUri.ClientId,
					Original_Uri = clientPostLogoutRedirectUri.Uri
				};

				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClientPostLogoutRedirectUrisDelete, param);
			}
		}


		#endregion

		#region Client Scopes
		/// <summary>
		/// Used to get a list of client scopes
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ClientScope> GetClientScope()
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<ClientScope>(StoredProcNameConstants.SP_ClientScopesSelect, null);
			}
		}
		
		/// <summary>
		/// Used to insert a new client scope
		/// </summary>
		/// <param name="clientScope"></param>
		/// <returns></returns>
		public ClientScope InsertClientScope(ClientScope clientScope)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = clientScope.ClientId,
					Scope = clientScope.Scope
				};

				return repository.GetOne<ClientScope>(StoredProcNameConstants.SP_ClientScopesInsert, param);
			}
		}

		/// <summary>
		/// Used to update a client scope
		/// </summary>
		/// <param name="orgClientScope"></param>
		/// <param name="newClientScope"></param>
		/// <returns></returns>
		public ClientScope UpdateClientScope(ClientScope orgClientScope, ClientScope newClientScope)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = newClientScope.ClientId,
					Scope = newClientScope.Scope,
					Original_ClientScopeId = orgClientScope.Id,
					Original_ClientId = orgClientScope.ClientId,
					Original_Scope = orgClientScope.Scope,
					ClientScopeId = orgClientScope.Id
				};
				return repository.GetOne<ClientScope>(StoredProcNameConstants.SP_ClientScopesUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a client scope
		/// </summary>
		/// <param name="clientScope"></param>
		/// <returns></returns>
		public int DeleteClientScope(ClientScope clientScope)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					Original_ClientScopeId = clientScope.Id,
					Original_ClientId = clientScope.ClientId,
					Original_Scope = clientScope.Scope
				};

				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClientScopesDelete, param);
			}
		}

		#endregion

		#region Client Secret
		/// <summary>
		/// Used to get a list of client secrets
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ClientSecret> GetClientSecret()
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<ClientSecret>(StoredProcNameConstants.SP_ClientSecretsSelect, null);
			}
		}

		/// <summary>
		/// Used to add a new client secret
		/// </summary>
		/// <param name="clientSecret"></param>
		/// <returns></returns>
		public ClientSecret InsertClientSecret(ClientSecret clientSecret)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ClientId = clientSecret.ClientId,
					Value = clientSecret.Value,
					Type = clientSecret.Type,
					Description = clientSecret.Description,
					Expiration = clientSecret.Expiration
				};
				return repository.GetOne<ClientSecret>(StoredProcNameConstants.SP_ClientSecretsInsert, param);
			}
		}

		/// <summary>
		/// Used to update an existing client secret
		/// </summary>
		/// <param name="orgClientSecret"></param>
		/// <param name="newClientSecret"></param>
		/// <returns></returns>
		public ClientSecret UpdateClientSecret(ClientSecret orgClientSecret, ClientSecret newClientSecret)
		{
			using (var repository = GetRepository())
			{
				int typeIsNull = orgClientSecret.Type == null ? 1 : 0;
				int descriptionIsNull = orgClientSecret.Description == null ? 1 : 0;
				int expirationIsNull = orgClientSecret.Expiration == DateTimeOffset.MinValue ? 1 : 0;

				dynamic param = new
				{
					ClientId = newClientSecret.ClientId,
					Value = newClientSecret.Value,
					Type = newClientSecret.Type,
					Description = newClientSecret.Description,
					Expiration = newClientSecret.Expiration,
					Original_ClientSecretId = orgClientSecret.Id,
					Original_ClientId = orgClientSecret.ClientId,
					Original_Value = orgClientSecret.Value,
					IsNull_Type = typeIsNull,
					Original_Type = orgClientSecret.Type,
					IsNull_Description = descriptionIsNull,
					Original_Description = orgClientSecret.Description,
					IsNull_Expiration = expirationIsNull,
					Original_Expiration = orgClientSecret.Expiration,
					ClientSecretId = newClientSecret.Id
				};

				return repository.GetOne<ClientSecret>(StoredProcNameConstants.SP_ClientSecretsUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a client secret
		/// </summary>
		/// <param name="clientSecret"></param>
		/// <returns></returns>
		public int DeleteClientSecret(ClientSecret clientSecret)
		{
			using (var repository = GetRepository())
			{
				int typeIsNull = clientSecret.Type == null ? 1 : 0;
				int descriptionIsNull = clientSecret.Description == null ? 1 : 0;
				int expirationIsNull = clientSecret.Expiration == DateTimeOffset.MinValue ? 1 : 0;

				dynamic param = new
				{
					Original_ClientSecretId = clientSecret.Id,
					Original_ClientId = clientSecret.ClientId,
					Original_Value = clientSecret.Value,
					IsNull_Type = typeIsNull,
					Original_Type = clientSecret.Type,
					IsNull_Description = descriptionIsNull,
					Original_Description = clientSecret.Description,
					IsNull_Expiration = expirationIsNull,
					Original_Expiration = clientSecret.Expiration
				};

				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClientSecretsDelete, param);
			}
		}

		#endregion

		#region Scope
		/// <summary>
		/// Used to get a list of scopes
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Scope> GetScopes()
		{
			IEnumerable<Scope> scopeList = null;
			using (var repository = GetRepository())
			{
				scopeList = repository.GetMany<Scope>(StoredProcNameConstants.SP_ScopesSelect, null);
			}

			if (scopeList != null)
			{
				IEnumerable<ScopeSecret> scopeSecretList = GetScopeSecrets();
				scopeList.ToList().ForEach(s =>
				{
					s.ScopeClaims = GetScopeClaims().Where(p => p.ScopeId == s.ScopeId).ToList();
					s.ScopeSecrets = scopeSecretList?.Where(p => p.ScopeId == s.ScopeId).ToList();
				});
			}
			return scopeList;
		}

		/// <summary>
		/// Get a scope by id
		/// </summary>
		/// <param name="scopeId"></param>
		/// <returns></returns>
		public Scope GetScopeById(int scopeId)
		{
			Scope scope = GetScopes().First(p => p.ScopeId == scopeId);

			if (scope != null)
			{
				scope.ScopeClaims = GetScopeClaims().Where(p => p.ScopeId == scopeId).ToList();
				scope.ScopeSecrets = GetScopeSecrets().Where(p => p.ScopeId == scopeId).ToList();
			}

			return scope;
		}

		/// <summary>
		/// Used to insert a new scope
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public Scope InsertScope(Scope scope)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					Name = scope.Name,
					DisplayName = scope.DisplayName,
					Description = scope.Description,
					ClaimsRule = scope.ClaimsRule,
					Enabled = scope.Enabled,
					Required = scope.Required,
					Emphasize = scope.Emphasize,
					Type = scope.Type,
					IncludeAllClaimsForUser = scope.IncludeAllClaimsForUser,
					ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
					AllowUnrestrictedIntrospection = scope.AllowUnrestrictedIntrospection
				};
				return repository.GetOne<Scope>(StoredProcNameConstants.SP_ScopesInsert, param);
			}
		}

		/// <summary>
		/// Used to update a scope
		/// </summary>
		/// <param name="orgScope"></param>
		/// <param name="newScope"></param>
		/// <returns></returns>
		public Scope UpdateScope(Scope orgScope, Scope newScope)
		{
			using (var repository = GetRepository())
			{
				int displayNameIsNull = orgScope.DisplayName == null ? 1 : 0;
				int descriptionIsNull = orgScope.Description == null ? 1 : 0;
				int claimsRuleIsNull = orgScope.ClaimsRule == null ? 1 : 0;

				dynamic param = new
				{
					Name  = newScope.Name,
					DisplayName = newScope.DisplayName,
					Description = newScope.Description,
					ClaimsRule = newScope.ClaimsRule,
					Enabled = newScope.Enabled,
					Required = newScope.Required,
					Emphasize = newScope.Emphasize,
					Type = newScope.Type,
					IncludeAllClaimsForUser = newScope.IncludeAllClaimsForUser,
					ShowInDiscoveryDocument = newScope.ShowInDiscoveryDocument,
					AllowUnrestrictedIntrospection = newScope.AllowUnrestrictedIntrospection,
					Original_ScopeId = orgScope.ScopeId,
					Original_Name = orgScope.Name,
					IsNull_DisplayName = displayNameIsNull,
					Original_DisplayName = orgScope.DisplayName,
					IsNull_Description = descriptionIsNull,
					Original_Description = orgScope.Description,
					IsNull_ClaimsRule = claimsRuleIsNull,
					Original_ClaimsRule = orgScope.ClaimsRule,
					Original_Enabled = orgScope.Enabled,
					Original_Required = orgScope.Required,
					Original_Emphasize = orgScope.Emphasize,
					Original_Type = orgScope.Type,
					Original_IncludeAllClaimsForUser = orgScope.IncludeAllClaimsForUser,
					Original_ShowInDiscoveryDocument = orgScope.ShowInDiscoveryDocument,
					Original_AllowUnrestrictedIntrospection = orgScope.AllowUnrestrictedIntrospection,
					ScopeId = newScope.ScopeId
				};
				return repository.GetOne<Scope>(StoredProcNameConstants.SP_ScopesUpdate, param);
			}
		}
		#endregion

		#region ScopeSecrets

		/// <summary>
		/// Used to get a list of scope secrets
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ScopeSecret> GetScopeSecrets()
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<ScopeSecret>(StoredProcNameConstants.SP_ScopeSecretsSelect, null);
			}
		}

		/// <summary>
		/// Used to add a new scope secret
		/// </summary>
		/// <param name="scopeSecret"></param>
		/// <returns></returns>
		public ScopeSecret InsertScopeSecret(ScopeSecret scopeSecret)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ScopeId  = scopeSecret.ScopeId,
					Description = scopeSecret.Description,
					Type = scopeSecret.Type,
					Value = scopeSecret.Value,
					Expiration = scopeSecret.Expiration
				};
				return repository.GetOne<ScopeSecret>(StoredProcNameConstants.SP_ScopeSecretsInsert, param);
			}
		}

		/// <summary>
		/// Used to update a scope secret
		/// </summary>
		/// <param name="orgScopeSecret"></param>
		/// <param name="newScopeSecret"></param>
		/// <returns></returns>
		public ScopeSecret UpdateScopeSecret(ScopeSecret orgScopeSecret, ScopeSecret newScopeSecret)
		{
			using (var repository = GetRepository())
			{
				int typeIsNull = orgScopeSecret.Type == null ? 1 : 0;
				int expirationIsNull = orgScopeSecret.Expiration == null ? 1 : 0;
				int descriptionIsNull = orgScopeSecret.Description == null ? 1 : 0;

				dynamic param = new
				{
					ScopeId = newScopeSecret.ScopeId,
					Description = newScopeSecret.Description,
					Type = newScopeSecret.Type,
					Value = newScopeSecret.Value,
					Expiration = newScopeSecret.Expiration,
					Original_ScopeSecretId = orgScopeSecret.Id,
					Original_ScopeId = orgScopeSecret.ScopeId,
					IsNull_Description = descriptionIsNull,
					Original_Description = orgScopeSecret.Description,
					IsNull_Type = typeIsNull,
					Original_Type = orgScopeSecret.Type,
					Original_Value = orgScopeSecret.Value,
					IsNull_Expiration = expirationIsNull,
					Original_Expiration = orgScopeSecret.Expiration,
					ScopeSecretId = newScopeSecret.Id
				};
				return repository.GetOne<ScopeSecret>(StoredProcNameConstants.SP_ScopeSecretsUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a scope secret
		/// </summary>
		/// <param name="scopeSecret"></param>
		/// <returns></returns>
		public int DeleteScopeSecret(ScopeSecret scopeSecret)
		{
			using (var repository = GetRepository())
			{
				int typeIsNull = scopeSecret.Type == null ? 1 : 0;
				int descriptionIsNull = scopeSecret.Description == null ? 1 : 0;
				int expirationIsNull = (scopeSecret.Expiration == DateTimeOffset.MinValue || scopeSecret.Expiration == null) ? 1 : 0;

				dynamic param = new
				{
					Original_ScopeSecretId = scopeSecret.Id,
					Original_ScopeId = scopeSecret.ScopeId,
					IsNull_Description = descriptionIsNull,
					Original_Description = scopeSecret.Description,
					IsNull_Type = typeIsNull,
					Original_Type = scopeSecret.Type,
					Original_Value = scopeSecret.Value,
					IsNull_Expiration = expirationIsNull,
					Original_Expiration = scopeSecret.Expiration
				};

				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ScopeSecretsDelete, param);
			}
		}
		#endregion

		#region ScopeClaims
		/// <summary>
		/// Used to get a list of scope claims
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ScopeClaim> GetScopeClaims()
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<ScopeClaim>(StoredProcNameConstants.SP_ScopeClaimsSelect, null);
			}
		}

		/// <summary>
		/// Used to insert a new scope claim
		/// </summary>
		/// <param name="scopeClaim"></param>
		/// <returns></returns>
		public ScopeClaim InsertScopeClaim(ScopeClaim scopeClaim)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					ScopeId = scopeClaim.ScopeId,
					Name = scopeClaim.Name,
					Description = scopeClaim.Description,
					AlwaysIncludeInIdToken = scopeClaim.AlwaysIncludeInIdToken
				};
				return repository.GetOne<ScopeClaim>(StoredProcNameConstants.SP_ScopeClaimsInsert, param);
			}
		}

		/// <summary>
		/// Used to update an existing scope claim
		/// </summary>
		/// <param name="orgScopeClaim"></param>
		/// <param name="newScopeClaim"></param>
		/// <returns></returns>
		public ScopeClaim UpdateScopeClaim(ScopeClaim orgScopeClaim, ScopeClaim newScopeClaim)
		{
			using (var repository = GetRepository())
			{
				int descriptionIsNull = orgScopeClaim.Description == null ? 1 : 0;

				dynamic param = new
				{
					ScopeId = newScopeClaim.ScopeId,
					Name = newScopeClaim.Name,
					Description = newScopeClaim.Description,
					AlwaysIncludeInIdToken = newScopeClaim.AlwaysIncludeInIdToken,
					Original_ScopeClaimId = orgScopeClaim.Id,
					Original_ScopeId = orgScopeClaim.ScopeId,
					Original_Name = orgScopeClaim.Name,
					IsNull_Description = descriptionIsNull,
					Original_Description = orgScopeClaim.Description,
					Original_AlwaysIncludeInIdToken = orgScopeClaim.AlwaysIncludeInIdToken,
					ScopeClaimId = orgScopeClaim.Id
				};
				return repository.GetOne<ScopeClaim>(StoredProcNameConstants.SP_ScopeClaimsUpdate, param);
			}
		}

		/// <summary>
		/// Used to delete a scope claim
		/// </summary>
		/// <param name="scopeClaim"></param>
		/// <returns></returns>
		public int DeleteScopeClaim(ScopeClaim scopeClaim)
		{
			using (var repository = GetRepository())
			{
				int descriptionIsNull = scopeClaim.Description == null ? 1 : 0;

				dynamic param = new
				{
					Original_ScopeClaimId = scopeClaim.Id,
					Original_ScopeId = scopeClaim.ScopeId,
					Original_Name = scopeClaim.Name,
					IsNull_Description = descriptionIsNull,
					Original_Description = scopeClaim.Description,
					Original_AlwaysIncludeInIdToken = scopeClaim.AlwaysIncludeInIdToken
				};
				return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ScopeClaimsDelete, param);
			}
		}

		

		#endregion

		#region Claim
        /// <summary>
        /// Used to get a list of claims
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ULClaim> GetClaims()
        {
            IEnumerable<ULClaim> claimList = null;
            using (var repository = GetRepository())
            {
                claimList = repository.GetMany<ULClaim>(StoredProcNameConstants.SP_ClaimSelect, null);
            }

            return claimList;
        }

        /// <summary>
        /// Get a claim by id
        /// </summary>
        /// <param name="claimId"></param>
        /// <returns></returns>
        public ULClaim GetClaimById(int claimId)
        {
            return GetClaims().First(p => p.ClaimId == claimId);
        }

        /// <summary>
        /// Used to insert a new claim
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public ULClaim InsertClaim(ULClaim claim)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    ClaimName = claim.ClaimName,
                    SAMLAttributeName = claim.SAMLAttributeName,
					ProductId = claim.ProductId
                };
                return repository.GetOne<ULClaim>(StoredProcNameConstants.SP_ClaimInsert, param);
            }
        }

        /// <summary>
        /// Used to update a claim
        /// </summary>
        /// <param name="orgClaim"></param>
        /// <param name="newClaim"></param>
        /// <returns></returns>
        public ULClaim UpdateClaim(ULClaim orgClaim, ULClaim newClaim)
        {
            using (var repository = GetRepository())
            {
               int isSAMLAttributeNameIsNull = orgClaim.SAMLAttributeName == null ? 1 : 0;

               dynamic param = new
               {
                   ClaimName = newClaim.ClaimName,
                   SAMLAttributeName = newClaim.SAMLAttributeName,
                   ProductId = newClaim.ProductId,
                   Original_ClaimId = newClaim.ClaimId,
                   Original_ClaimName = orgClaim.ClaimName,
                   IsNull_SAMLAttributeName = isSAMLAttributeNameIsNull,
                   Original_SAMLAttributeName = orgClaim.SAMLAttributeName,
                   Original_ProductId = orgClaim.ProductId,
                   ClaimId = orgClaim.ClaimId
               };

                return repository.GetOne<ULClaim>(StoredProcNameConstants.SP_ClaimUpdate, param);
            }
        }

        /// <summary>
        /// Used to delete a claim
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public int DeleteClaim(ULClaim claim)
        {
            int isSAMLAttributeNameIsNull = claim.SAMLAttributeName == null ? 1 : 0;

            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    Original_ClaimId = claim.ClaimId, 
                    Original_ClaimName = claim.ClaimName,
                    IsNull_SAMLAttributeName = isSAMLAttributeNameIsNull,
                    Original_SAMLAttributeName = claim.SAMLAttributeName,
                    Original_ProductId = claim.ProductId
                };

                return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClaimDelete, param);
            }
        }
		#endregion

		#region ClaimClientMapping
        /// <summary>
        /// Used to get a list of claim to client mappings
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ClientClaimMapping> GetClaimClientMapping()
        {
            IEnumerable<ClientClaimMapping> clientClaimMappingList = null;
            using (var repository = GetRepository())
            {
                clientClaimMappingList = repository.GetMany<ClientClaimMapping>(StoredProcNameConstants.SP_ClientUserClaimSelect, null);
            }

            return clientClaimMappingList;
        }

        /// <summary>
        /// Get claims by client id
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public IEnumerable<ClientClaimMapping> GetClientClaimMappingByClientId(int clientId)
        {
            return GetClaimClientMapping().Where(p => p.ClientId == clientId);
        }

        /// <summary>
        /// Used to insert a new claim for the given client
        /// </summary>
        /// <param name="claimMapping"></param>
        /// <returns></returns>
        public ClientClaimMapping InsertClientClaimMapping(ClientClaimMapping claimMapping)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    ClientId = claimMapping.ClientId,
                    ClaimId = claimMapping.ClaimId
                };
                return repository.GetOne<ClientClaimMapping>(StoredProcNameConstants.SP_ClientUserClaimInsert, param);
            }
        }

        /// <summary>
        /// Used to delete a claim to client mapping
        /// </summary>
        /// <param name="claimMapping"></param>
        /// <returns></returns>
        public int DeleteClientClaimMapping(ClientClaimMapping claimMapping)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    Original_ClientUserClaimId = claimMapping.ClientUserClaimId,
                    Original_ClientId = claimMapping.ClientId,
                    Original_ClaimId = claimMapping.ClaimId
                };

                return repository.ExecuteNonQuery(StoredProcNameConstants.SP_ClientUserClaimDelete, param);
            }
        }
		#endregion
	}
}
