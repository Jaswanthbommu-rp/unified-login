using System.Collections.Generic;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
    public interface IClientsSetupRepository
    {
        /// <summary>
        /// Get a list of clients and their details
        /// </summary>
        /// <returns></returns>
        IEnumerable<Client> GetClientsWithDetails();

        /// <summary>
        /// Get a list of clients without details
        /// </summary>
        /// <returns></returns>
        IEnumerable<Client> GetClientsNoDetails();

        /// <summary>
        /// Used to get a client and its details
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Client GetClientDetailsById(int clientId);

        /// <summary>
        /// Used to insert a new client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        Client InsertClient(Client client);

        /// <summary>
        /// Used to update an existing client
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
        /// Get a list of client redirect uris
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientClaim> GetClientClaim();

        /// <summary>
        /// Used to insert a new client claim
        /// </summary>
        /// <param name="clientClaim"></param>
        /// <returns></returns>
        ClientClaim InsertClientClaim(ClientClaim clientClaim);

        /// <summary>
        /// Used to update client redirect uris
        /// </summary>
        /// <param name="orgClientClaim"></param>
        /// <param name="newClientClaim"></param>
        /// <returns></returns>
        ClientClaim UpdateClientClaim(ClientClaim orgClientClaim, ClientClaim newClientClaim);

        /// <summary>
        /// Used to delete a client claim
        /// </summary>
        /// <param name="clientClaim"></param>
        /// <returns></returns>
        int DeleteClientClaim(ClientClaim clientClaim);

        /// <summary>
        /// Get a list of client redirect uris
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientRedirectUri> GetClientRedirectUri();

        /// <summary>
        /// Used to insert a new client redirect uri
        /// </summary>
        /// <param name="clientRedirectUri"></param>
        /// <returns></returns>
        ClientRedirectUri InsertClientRedirectUri(ClientRedirectUri clientRedirectUri);

        /// <summary>
        /// Used to update client redirect uris
        /// </summary>
        /// <param name="orgClientRedirectUri"></param>
        /// <param name="newClientRedirectUri"></param>
        /// <returns></returns>
        ClientRedirectUri UpdateClientRedirectUri(ClientRedirectUri orgClientRedirectUri, ClientRedirectUri newClientRedirectUri);

        /// <summary>
        /// Used to delete a client redirect uri
        /// </summary>
        /// <param name="clientRedirectUri"></param>
        /// <returns></returns>
        int DeleteClientRedirectUri(ClientRedirectUri clientRedirectUri);

        /// <summary>
        /// Get a list of client post logout redirect uris
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientPostLogoutRedirectUri> GetClientPostLogoutRedirectUri();

        /// <summary>
        /// Used to insert a new client post logout redirect uri
        /// </summary>
        /// <param name="clientPostLogoutRedirectUri"></param>
        /// <returns></returns>
        ClientPostLogoutRedirectUri InsertClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri);

        /// <summary>
        /// Used to update client post logout redirect uris
        /// </summary>
        /// <param name="orgClientPostLogoutRedirectUri"></param>
        /// <param name="newClientPostLogoutRedirectUri"></param>
        /// <returns></returns>
        ClientPostLogoutRedirectUri UpdateClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri orgClientPostLogoutRedirectUri, ClientPostLogoutRedirectUri newClientPostLogoutRedirectUri);

        /// <summary>
        /// Used to delete a client post logout redirect uri
        /// </summary>
        /// <param name="clientPostLogoutRedirectUri"></param>
        /// <returns></returns>
        int DeleteClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri);

        /// <summary>
        /// Used to get a list of client scopes
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientScope> GetClientScope();

        /// <summary>
        /// Used to insert a new client scope
        /// </summary>
        /// <param name="clientScope"></param>
        /// <returns></returns>
        ClientScope InsertClientScope(ClientScope clientScope);

        /// <summary>
        /// Used to update a client scope
        /// </summary>
        /// <param name="orgClientScope"></param>
        /// <param name="newClientScope"></param>
        /// <returns></returns>
        ClientScope UpdateClientScope(ClientScope orgClientScope, ClientScope newClientScope);

        /// <summary>
        /// Used to delete a client scope
        /// </summary>
        /// <param name="clientScope"></param>
        /// <returns></returns>
        int DeleteClientScope(ClientScope clientScope);

        /// <summary>
        /// Used to get a list of client secrets
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientSecret> GetClientSecret();

        /// <summary>
        /// Used to add a new client secret
        /// </summary>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        ClientSecret InsertClientSecret(ClientSecret clientSecret);

        /// <summary>
        /// Used to update an existing client secret
        /// </summary>
        /// <param name="orgClientSecret"></param>
        /// <param name="newClientSecret"></param>
        /// <returns></returns>
        ClientSecret UpdateClientSecret(ClientSecret orgClientSecret, ClientSecret newClientSecret);

        /// <summary>
        /// Used to delete a client secret
        /// </summary>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        int DeleteClientSecret(ClientSecret clientSecret);

        /// <summary>
        /// Used to get a list of scopes
        /// </summary>
        /// <returns></returns>
        IEnumerable<Scope> GetScopes();

        /// <summary>
        /// Get a scope by id
        /// </summary>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        Scope GetScopeById(int scopeId);

        /// <summary>
        /// Used to insert a new scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        Scope InsertScope(Scope scope);

        /// <summary>
        /// Used to update a scope
        /// </summary>
        /// <param name="orgScope"></param>
        /// <param name="newScope"></param>
        /// <returns></returns>
        Scope UpdateScope(Scope orgScope, Scope newScope);

        /// <summary>
        /// Used to get a list of scope secrets
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScopeSecret> GetScopeSecrets();

        /// <summary>
        /// Used to add a new scope secret
        /// </summary>
        /// <param name="scopeSecret"></param>
        /// <returns></returns>
        ScopeSecret InsertScopeSecret(ScopeSecret scopeSecret);

        /// <summary>
        /// Used to update a scope secret
        /// </summary>
        /// <param name="orgScopeSecret"></param>
        /// <param name="newScopeSecret"></param>
        /// <returns></returns>
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
        /// Used to insert a new scope claim
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

        /// <summary>
        /// Used to get a list of claims
        /// </summary>
        /// <returns></returns>
        IEnumerable<ULClaim> GetClaims();

        /// <summary>
        /// Get a claim by id
        /// </summary>
        /// <param name="claimId"></param>
        /// <returns></returns>
        ULClaim GetClaimById(int claimId);

        /// <summary>
        /// Used to insert a new claim
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        ULClaim InsertClaim(ULClaim claim);

        /// <summary>
        /// Used to update a claim
        /// </summary>
        /// <param name="orgClaim"></param>
        /// <param name="newClaim"></param>
        /// <returns></returns>
        ULClaim UpdateClaim(ULClaim orgClaim, ULClaim newClaim);

        /// <summary>
        /// Used to delete a claim
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        int DeleteClaim(ULClaim claim);

        /// <summary>
        /// Used to get a list of claim to client mappings
        /// </summary>
        /// <returns></returns>
        IEnumerable<ClientClaimMapping> GetClaimClientMapping();

        /// <summary>
        /// Get claims by client id
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        IEnumerable<ClientClaimMapping> GetClientClaimMappingByClientId(int clientId);

        /// <summary>
        /// Used to insert a new claim for the given client
        /// </summary>
        /// <param name="claimMapping"></param>
        /// <returns></returns>
        ClientClaimMapping InsertClientClaimMapping(ClientClaimMapping claimMapping);

        /// <summary>
        /// Used to delete a claim to client mapping
        /// </summary>
        /// <param name="claimMapping"></param>
        /// <returns></returns>
        int DeleteClientClaimMapping(ClientClaimMapping claimMapping);

        IRepository GetRepository();
        string GetConnectionString(DbConnectionEnum dbConnectionType);
    }
}