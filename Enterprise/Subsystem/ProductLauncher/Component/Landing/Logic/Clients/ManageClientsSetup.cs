using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Clients;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Clients
{
    public class ManageClientsSetup
	{
		#region Private Variables
		private DefaultUserClaim _userClaims;
		private readonly ClientsSetupRepository _clientsSetupRepository;

		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaim"></param>
		/// <param name="clientsSetupRepository"></param>
		public ManageClientsSetup(DefaultUserClaim userClaim, ClientsSetupRepository clientsSetupRepository)
		{
			_userClaims = userClaim;
			_clientsSetupRepository = clientsSetupRepository;
		}

		#region Client
		public IEnumerable<Client> GetClientsWithDetails()
		{
			return _clientsSetupRepository.GetClientsWithDetails();
		}
		public IEnumerable<Client> GetClientsNoDetails()
        {
            return _clientsSetupRepository.GetClientsNoDetails();
        }

		public Client GetClientDetails(int clientId)
		{
			return _clientsSetupRepository.GetClientDetailsById(clientId);
		}

		/// <summary>
		/// Used to add a new client
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public Client InsertClient(Client client)
		{
			return _clientsSetupRepository.InsertClient(client);
		}

		/// <summary>
		/// Used to update a client
		/// </summary>
		/// <param name="orgClient"></param>
		/// <param name="newClient"></param>
		/// <returns></returns>
		public Client UpdateClient(Client orgClient, Client newClient)
		{
			return _clientsSetupRepository.UpdateClient(orgClient, newClient);
		}

		/// <summary>
		/// Used to delete a client
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public int DeleteClient(Client client)
		{
			return _clientsSetupRepository.DeleteClient(client);
		}

		#endregion

		#region ClientClaims
		/// <summary>
		/// used to get the list of client claims
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ClientClaim> GetClientClaim()
		{
			return _clientsSetupRepository.GetClientClaim();
		}

		/// <summary>
		/// Used to add a new client claim
		/// </summary>
		/// <param name="clientClaim"></param>
		/// <returns></returns>
		public ClientClaim InsertClientClaim(ClientClaim clientClaim)
		{
			return _clientsSetupRepository.InsertClientClaim(clientClaim);
		}

		/// <summary>
		/// used to update an existing client claim
		/// </summary>
		/// <param name="orgClientClaim"></param>
		/// <param name="clientClaim"></param>
		/// <returns></returns>
		public ClientClaim UpdateClientClaim(ClientClaim orgClientClaim, ClientClaim newClientClaim)
		{
			return _clientsSetupRepository.UpdateClientClaim(orgClientClaim, newClientClaim);
		}

		/// <summary>
		/// Used to delete a client claim
		/// </summary>
		/// <param name="clientClaim"></param>
		/// <returns></returns>
		public int DeleteClientClaim(ClientClaim clientClaim)
		{
			return _clientsSetupRepository.DeleteClientClaim(clientClaim);
		}

		#endregion

		#region ClientRedirectUri
		public IEnumerable<ClientRedirectUri> GetClientRedirectUri()
		{
			return _clientsSetupRepository.GetClientRedirectUri();
		}

		public ClientRedirectUri InsertClientRedirectUri(ClientRedirectUri clientRedirectUri)
		{
			return _clientsSetupRepository.InsertClientRedirectUri(clientRedirectUri);
		}

		public ClientRedirectUri UpdateClientRedirectUri(ClientRedirectUri orgClientRedirectUri, ClientRedirectUri newClientRedirectUri)
		{
			return _clientsSetupRepository.UpdateClientRedirectUri(orgClientRedirectUri, newClientRedirectUri);
		}

		public int DeleteClientRedirectUri(ClientRedirectUri clientRedirectUri)
		{
			return _clientsSetupRepository.DeleteClientRedirectUri(clientRedirectUri);
		}

		#endregion

		#region ClientPostLogoutRedirectUri
		public IEnumerable<ClientPostLogoutRedirectUri> GetClientPostLogoutRedirectUri()
		{
			return _clientsSetupRepository.GetClientPostLogoutRedirectUri();
		}

		public ClientPostLogoutRedirectUri InsertClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri)
		{
			return _clientsSetupRepository.InsertClientPostLogoutRedirectUri(clientPostLogoutRedirectUri);
		}

		public ClientPostLogoutRedirectUri UpdateClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri orgClientPostLogoutRedirectUri, ClientPostLogoutRedirectUri newClientPostLogoutRedirectUri)
		{
			return _clientsSetupRepository.UpdateClientPostLogoutRedirectUri(orgClientPostLogoutRedirectUri, newClientPostLogoutRedirectUri);
		}

		public int DeleteClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri)
		{
			return _clientsSetupRepository.DeleteClientPostLogoutRedirectUri(clientPostLogoutRedirectUri);
		}

		#endregion

		#region Scope
		public IEnumerable<Scope> GetScopes()
		{
			return _clientsSetupRepository.GetScopes();
		}

		public Scope GetScopeById(int scopeId)
		{
			return _clientsSetupRepository.GetScopeById(scopeId);
		}

		public ClientScope InsertClientScope(ClientScope clientScope)
		{
			return _clientsSetupRepository.InsertClientScope(clientScope);
		}

		public ClientScope UpdateClientScope(ClientScope orgClientScope, ClientScope newClientScope)
		{
			return _clientsSetupRepository.UpdateClientScope(orgClientScope, newClientScope);
		}

		#endregion

		#region Client Scopes
		public IEnumerable<ClientScope> GetClientScope()
		{
			return _clientsSetupRepository.GetClientScope();
		}

		public Scope InsertScope(Scope scope)
		{
			return _clientsSetupRepository.InsertScope(scope);
		}

		public Scope UpdateScope(Scope orgScope, Scope newScope)
		{
			return _clientsSetupRepository.UpdateScope(orgScope, newScope);
		}

		public int DeleteClientScope(ClientScope clientScope)
		{
			return _clientsSetupRepository.DeleteClientScope(clientScope);
		}

		#endregion

		#region Client Secret
		/// <summary>
		/// Used to get a list of client secrets
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ClientSecret> GetClientSecret()
		{
			return _clientsSetupRepository.GetClientSecret();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientSecret"></param>
		/// <returns></returns>
		public ClientSecret InsertClientSecret(ClientSecret clientSecret)
		{
			return _clientsSetupRepository.InsertClientSecret(clientSecret);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="orgClientSecret"></param>
		/// <param name="clientSecret"></param>
		/// <returns></returns>
		public ClientSecret UpdateClientSecret(ClientSecret orgClientSecret, ClientSecret newClientSecret)
		{
			return _clientsSetupRepository.UpdateClientSecret(orgClientSecret, newClientSecret);
		}

		/// <summary>
		/// Used to delete a client secret
		/// </summary>
		/// <param name="clientSecret"></param>
		/// <returns></returns>
		public int DeleteClientSecret(ClientSecret clientSecret)
		{
			return _clientsSetupRepository.DeleteClientSecret(clientSecret);
		}
		#endregion Client Secret

		#region Scope Secrets
		/// <summary>
		/// Used to get the list of scope secrets
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ScopeSecret> GetScopeSecrets()
		{
			return _clientsSetupRepository.GetScopeSecrets();
		}

		/// <summary>
		/// Used to insert a new scope secret
		/// </summary>
		/// <param name="scopeSecret"></param>
		/// <returns></returns>
		public ScopeSecret InsertScopeSecret(ScopeSecret scopeSecret)
		{
			return _clientsSetupRepository.InsertScopeSecret(scopeSecret);
		}
		
		public ScopeSecret UpdateScopeSecret(ScopeSecret orgScopeSecret, ScopeSecret newScopeSecret)
		{
			return _clientsSetupRepository.UpdateScopeSecret(orgScopeSecret, newScopeSecret);
		}

		/// <summary>
		/// Used to delete a scope secret
		/// </summary>
		/// <param name="scopeSecret"></param>
		/// <returns></returns>
		public int DeleteScopeSecret(ScopeSecret scopeSecret)
		{
			return _clientsSetupRepository.DeleteScopeSecret(scopeSecret);
		}
		#endregion

		#region Scope Claim
		/// <summary>
		/// Used to get a list of scope claims
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ScopeClaim> GetScopeClaims()
		{
			return _clientsSetupRepository.GetScopeClaims();
		}

		/// <summary>
		/// Used to add a new scope claim
		/// </summary>
		/// <param name="scopeClaim"></param>
		/// <returns></returns>
		public ScopeClaim InsertScopeClaim(ScopeClaim scopeClaim)
		{
			return _clientsSetupRepository.InsertScopeClaim(scopeClaim);
		}

		/// <summary>
		/// Used to update an existing scope claim
		/// </summary>
		/// <param name="orgScopeClaim"></param>
		/// <param name="newScopeClaim"></param>
		/// <returns></returns>
		public ScopeClaim UpdateScopeClaim(ScopeClaim orgScopeClaim, ScopeClaim newScopeClaim)
		{
			return _clientsSetupRepository.UpdateScopeClaim(orgScopeClaim, newScopeClaim);
		}

		/// <summary>
		/// Used to delete a scope claim
		/// </summary>
		/// <param name="scopeClaim"></param>
		/// <returns></returns>
		public int DeleteScopeClaim(ScopeClaim scopeClaim)
		{
			return _clientsSetupRepository.DeleteScopeClaim(scopeClaim);
		}
		#endregion

		public void WriteToLog(int clientId, string logMessage)
		{
			var clientName = GetClientsNoDetails().FirstOrDefault(c => c.ClientId == clientId)?.ClientName;
			
			LogActivity.WriteActivity(new ActivityDetails
			{
				LogActivityTypeName = LogActivityTypeConstants.CLIENT_SETTINGS_UPDATE,
				LogCategoryName = LogActivityCategoryType.InternalProductSettings.ToString(),
				CorrelationId = _userClaims.CorrelationId.ToString(),
				BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
				OrganizationPartyId = _userClaims.OrganizationPartyId,
				Message = string.Format(logMessage, _userClaims.FirstName, _userClaims.LastName, clientName),
				FromUserLoginName = _userClaims.LoginName,
				FromUserLoginId = _userClaims.UserId,
				FromUserFirstName = _userClaims.FirstName,
				FromUserLastName = _userClaims.LastName,
				FromUserRealpageId = _userClaims.UserRealPageGuid.ToString()
			});
		}

		#region Claim
        public IEnumerable<ULClaim> GetClaims()
        {
            return _clientsSetupRepository.GetClaims();
        }

        public ULClaim GetClaimById(int claimId)
        {
            return _clientsSetupRepository.GetClaimById(claimId);
        }

        public ULClaim InsertClaim(ULClaim claim)
        {
            return _clientsSetupRepository.InsertClaim(claim);
        }

        public ULClaim UpdateClaim(ULClaim orgClaim, ULClaim newClaim)
        {
            return _clientsSetupRepository.UpdateClaim(orgClaim, newClaim);
        }

        public int DeleteClaim(ULClaim claim)
        {
            return _clientsSetupRepository.DeleteClaim(claim);
        }
		#endregion

        #region ClientClaimMapping
        public IEnumerable<ClientClaimMapping> GetClientClaimMapping()
        {
            return _clientsSetupRepository.GetClaimClientMapping();
        }

        public IEnumerable<ClientClaimMapping> GetClientClaimMappingByClientId(int clientId)
        {
            return _clientsSetupRepository.GetClientClaimMappingByClientId(clientId);
        }

        public ClientClaimMapping InsertClientClaimMapping(ClientClaimMapping claimMapping)
        {
            return _clientsSetupRepository.InsertClientClaimMapping(claimMapping);
        }

        public int DeleteClientClaimMapping(ClientClaimMapping claimMapping)
        {
            return _clientsSetupRepository.DeleteClientClaimMapping(claimMapping);
        }
        #endregion
	}
}
