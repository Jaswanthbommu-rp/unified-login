using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IManageClientsSetup
    {
        IEnumerable<Client> GetClientsWithDetails();
        IEnumerable<Client> GetClientsNoDetails();
        Client GetClientDetails(int clientId);

        /// <summary>
        /// Used to add a new client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        Client InsertClient(Client client);

        /// <summary>
        /// Used to update a client
        /// </summary>
        /// <param name="orgClient"></param>
        /// <param name="newClient"></param>
        /// <returns></returns>
        Client UpdateClient(Client orgClient, Client newClient);

        /// <summary>
        /// Used to delete a client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        int DeleteClient(Client client);

        /// <summary>
        /// used to get the list of client claims
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientClaim> GetClientClaim();

        /// <summary>
        /// Used to add a new client claim
        /// </summary>
        /// <param name="clientClaim"></param>
        /// <returns></returns>
        ClientClaim InsertClientClaim(ClientClaim clientClaim);

        /// <summary>
        /// used to update an existing client claim
        /// </summary>
        /// <param name="orgClientClaim"></param>
        /// <param name="clientClaim"></param>
        /// <returns></returns>
        ClientClaim UpdateClientClaim(ClientClaim orgClientClaim, ClientClaim newClientClaim);

        /// <summary>
        /// Used to delete a client claim
        /// </summary>
        /// <param name="clientClaim"></param>
        /// <returns></returns>
        int DeleteClientClaim(ClientClaim clientClaim);

        IEnumerable<ClientRedirectUri> GetClientRedirectUri();
        ClientRedirectUri InsertClientRedirectUri(ClientRedirectUri clientRedirectUri);
        ClientRedirectUri UpdateClientRedirectUri(ClientRedirectUri orgClientRedirectUri, ClientRedirectUri newClientRedirectUri);
        int DeleteClientRedirectUri(ClientRedirectUri clientRedirectUri);
        IEnumerable<ClientPostLogoutRedirectUri> GetClientPostLogoutRedirectUri();
        ClientPostLogoutRedirectUri InsertClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri);
        ClientPostLogoutRedirectUri UpdateClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri orgClientPostLogoutRedirectUri, ClientPostLogoutRedirectUri newClientPostLogoutRedirectUri);
        int DeleteClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri);
        IEnumerable<Scope> GetScopes();
        Scope GetScopeById(int scopeId);
        ClientScope InsertClientScope(ClientScope clientScope);
        ClientScope UpdateClientScope(ClientScope orgClientScope, ClientScope newClientScope);
        IEnumerable<ClientScope> GetClientScope();
        Scope InsertScope(Scope scope);
        Scope UpdateScope(Scope orgScope, Scope newScope);
        int DeleteClientScope(ClientScope clientScope);

        /// <summary>
        /// Used to get a list of client secrets
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientSecret> GetClientSecret();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        ClientSecret InsertClientSecret(ClientSecret clientSecret);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgClientSecret"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        ClientSecret UpdateClientSecret(ClientSecret orgClientSecret, ClientSecret newClientSecret);

        /// <summary>
        /// Used to delete a client secret
        /// </summary>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        int DeleteClientSecret(ClientSecret clientSecret);

        /// <summary>
        /// Used to get the list of scope secrets
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScopeSecret> GetScopeSecrets();

        /// <summary>
        /// Used to insert a new scope secret
        /// </summary>
        /// <param name="scopeSecret"></param>
        /// <returns></returns>
        ScopeSecret InsertScopeSecret(ScopeSecret scopeSecret);

        ScopeSecret UpdateScopeSecret(ScopeSecret orgScopeSecret, ScopeSecret newScopeSecret);

        /// <summary>
        /// Used to delete a scope secret
        /// </summary>
        /// <param name="scopeSecret"></param>
        /// <returns></returns>
        int DeleteScopeSecret(ScopeSecret scopeSecret);

        /// <summary>
        /// Used to get a list of scope claims
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScopeClaim> GetScopeClaims();

        /// <summary>
        /// Used to add a new scope claim
        /// </summary>
        /// <param name="scopeClaim"></param>
        /// <returns></returns>
        ScopeClaim InsertScopeClaim(ScopeClaim scopeClaim);

        /// <summary>
        /// Used to update an existing scope claim
        /// </summary>
        /// <param name="orgScopeClaim"></param>
        /// <param name="newScopeClaim"></param>
        /// <returns></returns>
        ScopeClaim UpdateScopeClaim(ScopeClaim orgScopeClaim, ScopeClaim newScopeClaim);

        /// <summary>
        /// Used to delete a scope claim
        /// </summary>
        /// <param name="scopeClaim"></param>
        /// <returns></returns>
        int DeleteScopeClaim(ScopeClaim scopeClaim);

        void WriteToLog(int clientId, string logMessage);
        IEnumerable<ULClaim> GetClaims();
        ULClaim GetClaimById(int claimId);
        ULClaim InsertClaim(ULClaim claim);
        ULClaim UpdateClaim(ULClaim orgClaim, ULClaim newClaim);
        int DeleteClaim(ULClaim claim);
        IEnumerable<ClientClaimMapping> GetClientClaimMapping();
        IEnumerable<ClientClaimMapping> GetClientClaimMappingByClientId(int clientId);
        ClientClaimMapping InsertClientClaimMapping(ClientClaimMapping claimMapping);
        int DeleteClientClaimMapping(ClientClaimMapping claimMapping);
    }
}