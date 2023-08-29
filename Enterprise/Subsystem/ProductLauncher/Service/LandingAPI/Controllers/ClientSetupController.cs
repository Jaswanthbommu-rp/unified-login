using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Clients;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class ClientSetupController : BaseApiController
    {
        #region Private variables

        private IRepository _repository;

        //private IClientsSetupRepository _clientsSetupRepository;
        private IManageClientsSetup _manageClientsSetup;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientSetupController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }


        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="userClaims"></param>
        /// <param name="repository"></param>
        /// <param name="manageClientsSetup"></param>
        public ClientSetupController(DefaultUserClaim userClaims, IRepository repository, IManageClientsSetup manageClientsSetup)
        {
            _repository = repository;
            _manageClientsSetup = new ManageClientsSetup(userClaims, repository);
            _userClaims = userClaims;
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageClientsSetup = new ManageClientsSetup(_userClaims);
        }

        #endregion

        #region Public Methods

        [AllowAnonymous]
        [Route("clientsetup/test")]
        [HttpGet]
        public HttpResponseMessage GetSuccessResult()
        {
            return Request.CreateResponse(HttpStatusCode.OK, Guid.NewGuid());
        }

        #region Client

        /// <summary>
        /// Used to get a list of clients
        /// </summary>
        /// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "A list of client scopes", Type = typeof(Client))]
        //[Route("clientsetup/client")]
        //[HttpGet]
        //public IEnumerable<Client> GetClients()
        //{
        //    return _manageClientsSetup.GetClientsWithDetails();
        //}

        /// <summary>
        /// Used to get a client by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "A client and its details", Type = typeof(Client))]
        //[Route("clientsetup/client/{id}")]
        //[HttpGet]
        //public Client GetClients(int id)
        //{
        //    return _manageClientsSetup.GetClientDetails(id);
        //}

        /// <summary>
        /// Used to create a client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client created", Type = typeof(Client))]
        [Route("clientsetup/client")]
        [HttpPost]
        public HttpResponseMessage InsertClient(Client client)
        {
            ObjectOutput<Client, IErrorData> output = new ObjectOutput<Client, IErrorData>();

            // validate inputs
            if (!string.IsNullOrEmpty(validClient(client)))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = $"Insert failed, {validClient(client)}", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            Client clientResult = _manageClientsSetup.InsertClient(client);
            output.obj = clientResult;

            if (clientResult == null)
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to update a client
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientUpdate"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client updated", Type = typeof(Client))]
        [Route("clientsetup/client/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateClient(int id, ClientUpdate clientUpdate)
        {
            ObjectOutput<Client, IErrorData> output = new ObjectOutput<Client, IErrorData>();

            if (!string.IsNullOrEmpty(validClient(clientUpdate.client)))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = $"Update failed. {validClient(clientUpdate.client)}", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            clientUpdate.client.ClientId = id;
            clientUpdate.originalClient.ClientId = id;
            var clientResult = _manageClientsSetup.UpdateClient(clientUpdate.originalClient, clientUpdate.client);
            output.obj = clientResult;

            if (clientResult == null
                || clientUpdate.client.ClientCode != clientResult.ClientCode
                || clientUpdate.client.ClientName != clientResult.ClientName
                || clientUpdate.client.ClientUri != clientResult.ClientUri
                || clientUpdate.client.LogoUri != clientResult.LogoUri
                || clientUpdate.client.Flow != clientResult.Flow
                || clientUpdate.client.LogoutUri != clientResult.LogoutUri
                || clientUpdate.client.IdentityTokenLifetime != clientResult.IdentityTokenLifetime
                || clientUpdate.client.AccessTokenLifetime != clientResult.AccessTokenLifetime
                || clientUpdate.client.AuthorizationCodeLifetime != clientResult.AuthorizationCodeLifetime
                || clientUpdate.client.AbsoluteRefreshTokenLifetime != clientResult.AbsoluteRefreshTokenLifetime
                || clientUpdate.client.SlidingRefreshTokenLifetime != clientResult.SlidingRefreshTokenLifetime
                || clientUpdate.client.RefreshTokenUsage != clientResult.RefreshTokenUsage
                || clientUpdate.client.RefreshTokenExpiration != clientResult.RefreshTokenExpiration
                || clientUpdate.client.AccessTokenType != clientResult.AccessTokenType
                || clientUpdate.client.UpdateAccessTokenOnRefresh != clientResult.UpdateAccessTokenOnRefresh
                || clientUpdate.client.Enabled != clientResult.Enabled
                || clientUpdate.client.LogoutSessionRequired != clientResult.LogoutSessionRequired
                || clientUpdate.client.RequireSignOutPrompt != clientResult.RequireSignOutPrompt
                || clientUpdate.client.AllowAccessToAllScopes != clientResult.AllowAccessToAllScopes
                || clientUpdate.client.AllowClientCredentialsOnly != clientResult.AllowClientCredentialsOnly
                || clientUpdate.client.RequireConsent != clientResult.RequireConsent
                || clientUpdate.client.AllowRememberConsent != clientResult.AllowRememberConsent
                || clientUpdate.client.EnableLocalLogin != clientResult.EnableLocalLogin
                || clientUpdate.client.IncludeJwtId != clientResult.IncludeJwtId
                || clientUpdate.client.AlwaysSendClientClaims != clientResult.AlwaysSendClientClaims
                || clientUpdate.client.PrefixClientClaims != clientResult.PrefixClientClaims
                || clientUpdate.client.AllowAccessToAllGrantTypes != clientResult.AllowAccessToAllGrantTypes
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to delete a client
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client deleted")]
        [Route("clientsetup/client/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClient(int id, Client client)
        {
            client.ClientId = id;
            int result = _manageClientsSetup.DeleteClient(client);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        //#region ClientClaims

        ///// <summary>
        ///// Used to get a list of client claims
        ///// </summary>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "A list of client claims", Type = typeof(ClientClaim))]
        //[Route("clientsetup/clientclaim")]
        //[HttpGet]
        //public IEnumerable<ClientClaim> GetClientClaim()
        //{
        //    return _manageClientsSetup.GetClientClaim();
        //}
        //
        ///// <summary>
        ///// Used to create a client claim
        ///// </summary>
        ///// <param name="clientClaim"></param>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "The client claim created", Type = typeof(ClientClaim))]
        //[Route("clientsetup/clientclaim")]
        //[HttpPost]
        //public HttpResponseMessage InsertClientClaim(ClientClaim clientClaim)
        //{
        //    ObjectOutput<ClientClaim, IErrorData> output = new ObjectOutput<ClientClaim, IErrorData>();
        //
        //    // verify the client id exists
        //    if (!ClientIdExists(clientClaim.ClientId))
        //    {
        //        output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid client id", Success = false };
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, output);
        //    }
        //
        //    var clientClaimResult = _manageClientsSetup.InsertClientClaim(clientClaim);
        //    output.obj = clientClaimResult;
        //
        //    if (clientClaimResult == null)
        //    {
        //        output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed", Success = false };
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, output);
        //    }
        //
        //    return Request.CreateResponse(HttpStatusCode.OK, output);
        //}
        //
        ///// <summary>
        ///// Used to update a client claim
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="clientClaim"></param>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "The client claim updated")]
        //[Route("clientsetup/clientclaim/{id}")]
        //[HttpPut]
        //public HttpResponseMessage UpdateClientClaim(int id, ClientClaimUpdate clientClaim)
        //{
        //    ObjectOutput<ClientClaim, IErrorData> output = new ObjectOutput<ClientClaim, IErrorData>();
        //
        //    // verify the client id exists
        //    if (!ClientIdExists(clientClaim.clientClaim.ClientId))
        //    {
        //        output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid client id", Success = false };
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, output);
        //    }
        //
        //    clientClaim.clientClaim.Id = id;
        //    clientClaim.originalClientClaim.Id = id;
        //    ClientClaim clientClaimResult = _manageClientsSetup.UpdateClientClaim(clientClaim.originalClientClaim, clientClaim.clientClaim);
        //    output.obj = clientClaimResult;
        //
        //    if (clientClaimResult == null
        //        || clientClaim.clientClaim.ClientId != clientClaimResult.ClientId
        //        || clientClaim.clientClaim.Type != clientClaimResult.Type
        //        || clientClaim.clientClaim.Value != clientClaimResult.Value
        //       )
        //    {
        //        output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, output);
        //    }
        //
        //    return Request.CreateResponse(HttpStatusCode.OK, output);
        //}
        //
        ///// <summary>
        ///// Used to delete a client claim
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="clientClaim"></param>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "The client claim deleted")]
        //[Route("clientsetup/clientclaim/{id}")]
        //[HttpDelete]
        //public HttpResponseMessage DeleteClientClaim(int id, ClientClaim clientClaim)
        //{
        //    ObjectOutput<ClientClaim, IErrorData> output = new ObjectOutput<ClientClaim, IErrorData>();
        //    clientClaim.Id = id;
        //    int result = _manageClientsSetup.DeleteClientClaim(clientClaim);
        //    if (result == 0)
        //    {
        //        output.Status = new Status<IErrorData>() { ErrorMsg = "Delete failed, no records deleted", Success = false };
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, output);
        //    }
        //
        //    output.Status = new Status<IErrorData>() { Success = true };
        //    return Request.CreateResponse(HttpStatusCode.OK, output);
        //}

        //#endregion

        #region Scope

        /// <summary>
        /// Used to get a list of scopes
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of client scopes", Type = typeof(Scope))]
        [Route("clientsetup/scope")]
        [HttpGet]
        public IEnumerable<Scope> GetScopes()
        {
            return _manageClientsSetup.GetScopes();
        }

        /// <summary>
        /// Used to get a scope by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A client scopes", Type = typeof(Scope))]
        [Route("clientsetup/scope/{id}")]
        [HttpGet]
        public Scope GetScopeById(int id)
        {
            return _manageClientsSetup.GetScopeById(id);
        }

        /// <summary>
        /// Used to create a scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope created", Type = typeof(Scope))]
        [Route("clientsetup/scope")]
        [HttpPost]
        public Scope InsertScope(Scope scope)
        {



            var insert = _manageClientsSetup.InsertScope(scope);

            if (insert.ScopeId > 0)
                _manageClientsSetup.WriteToLog(0, $"{{0}} {{1}} added scope {scope.Name}.");

            return insert;
        }

        /// <summary>
        /// Used to update a scope
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope updated")]
        [Route("clientsetup/scope/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateScope(int id, ScopeUpdate scope)
        {




            scope.scope.ScopeId = id;
            scope.originalScope.ScopeId = id;
            Scope scopeResult = _manageClientsSetup.UpdateScope(scope.originalScope, scope.scope);
            ObjectOutput<Scope, IErrorData> output = new ObjectOutput<Scope, IErrorData>() { obj = scopeResult };

            if (scopeResult == null
                || scope.scope.Type != scopeResult.Type
                || scope.scope.AllowUnrestrictedIntrospection != scopeResult.AllowUnrestrictedIntrospection
                || scope.scope.Emphasize != scopeResult.Emphasize
                || scope.scope.ClaimsRule != scopeResult.ClaimsRule
                || scope.scope.Name != scopeResult.Name
                || scope.scope.DisplayName != scopeResult.DisplayName
                || scope.scope.Description != scopeResult.Description
                || scope.scope.Enabled != scopeResult.Enabled
                || scope.scope.Required != scopeResult.Required
                || scope.scope.IncludeAllClaimsForUser != scopeResult.IncludeAllClaimsForUser
                || scope.scope.ShowInDiscoveryDocument != scopeResult.ShowInDiscoveryDocument
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }
            else
            {
                var o = scope.originalScope;
                var n = scope.scope;

                StringBuilder logMessage = new StringBuilder($"{{0}} {{1}} updated scope {o.Name}. ");

                if (o.Enabled != n.Enabled)
                    logMessage.Append($"From Enabled: \"{o.Enabled}\" to \"{n.Enabled}\". ");

                if (o.Required != n.Required)
                    logMessage.Append($"From Required: \"{o.Required}\" to \"{n.Required}\". ");

                if (o.Name != n.Name)
                    logMessage.Append($"From Name: \"{o.Name}\" to \"{n.Name}\". ");

                if (o.DisplayName != n.DisplayName)
                    logMessage.Append($"From Display Name: \"{o.DisplayName}\" to \"{n.DisplayName}\". ");

                if (o.Description != n.Description)
                    logMessage.Append($"From Description: \"{o.Description}\" to \"{n.Description}\". ");

                if (o.Emphasize != n.Emphasize)
                    logMessage.Append($"From Emphasize: \"{o.Emphasize}\" to \"{n.Emphasize}\". ");

                if (o.IncludeAllClaimsForUser != n.IncludeAllClaimsForUser)
                    logMessage.Append($"From Include All Claims For User: \"{o.IncludeAllClaimsForUser}\" to \"{n.IncludeAllClaimsForUser}\". ");

                if (o.ShowInDiscoveryDocument != n.ShowInDiscoveryDocument)
                    logMessage.Append($"From Show In Discovery Document: \"{o.ShowInDiscoveryDocument}\" to \"{n.ShowInDiscoveryDocument}\". ");

                if (o.AllowUnrestrictedIntrospection != n.AllowUnrestrictedIntrospection)
                    logMessage.Append($"From Allow Unrestricted Introspection: \"{o.AllowUnrestrictedIntrospection}\" to \"{n.AllowUnrestrictedIntrospection}\". ");


                if (o.Type != n.Type)
                {
                    // Either Identity (OpenID Connect related) or Resource (OAuth2 resources)
                    var oldtype = o.Type == 0 ? "Identity" : "Resource";
                    var newtype = n.Type == 0 ? "Identity" : "Resource";

                    logMessage.Append($"From Type: \"{oldtype}\" to \"{newtype}\".");
                }

                _manageClientsSetup.WriteToLog(0, logMessage.ToString());

                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        #endregion

        #region Client Scopes

        /// <summary>
        /// Used to get a list of client scopes
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of client scopes", Type = typeof(ClientScope))]
        [Route("clientsetup/clientscope")]
        [HttpGet]
        public IEnumerable<ClientScope> GetClientScopes()
        {



            return _manageClientsSetup.GetClientScope();
        }

        /// <summary>
        /// Used to create a client scope
        /// </summary>
        /// <param name="clientScope"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client scope created", Type = typeof(ClientScope))]
        [Route("clientsetup/clientscope")]
        [HttpPost]
        public ClientScope InsertClientScope(ClientScope clientScope)
        {



            var insert = _manageClientsSetup.InsertClientScope(clientScope);

            if (insert.Id > 0)
                _manageClientsSetup.WriteToLog(clientScope.ClientId, $"{{0}} {{1}} added {clientScope.Scope} to {{2}} scopes.");

            return insert;
        }

        /// <summary>
        /// Used to update a client scope
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientScope"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client scope updated")]
        [Route("clientsetup/clientscope/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateClientScope(int id, ClientScopeUpdate clientScope)
        {




            // verify the client id exists
            if (!ClientIdExists(clientScope.clientScope.ClientId))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid client id");
            }

            clientScope.clientScope.Id = id;
            clientScope.originalClientScope.Id = id;
            ClientScope clientScopeResult = _manageClientsSetup.UpdateClientScope(clientScope.originalClientScope, clientScope.clientScope);
            ObjectOutput<ClientScope, IErrorData> output = new ObjectOutput<ClientScope, IErrorData>() { obj = clientScopeResult };

            if (clientScopeResult == null
                || clientScope.clientScope.ClientId != clientScopeResult.ClientId
                || clientScope.clientScope.Scope != clientScopeResult.Scope
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to delete a client scope
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientScope"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client scope deleted")]
        [Route("clientsetup/clientscope/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClientScope(int id, ClientScope clientScope)
        {



            clientScope.Id = id;
            int result = _manageClientsSetup.DeleteClientScope(clientScope);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }

            _manageClientsSetup.WriteToLog(clientScope.ClientId, $"{{0}} {{1}} removed {clientScope.Scope} from {{2}} scopes.");

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        //#region ClientRedirectUri
        ///// <summary>
        ///// Used to get a list of client redirect uris
        ///// </summary>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "A list of client redirect uris", Type = typeof(ClientRedirectUri))]
        //[Route("clientsetup/clientredirecturi")]
        //[HttpGet]
        //public IEnumerable<ClientRedirectUri> GetClientRedirectUri()
        //{
        //
        //
        //
        //    return _manageClientsSetup.GetClientRedirectUri();
        //}
        //
        ///// <summary>
        ///// Used to create a client redirect uri
        ///// </summary>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "The client redirect uri created", Type = typeof(ClientRedirectUri))]
        //[Route("clientsetup/clientredirecturi")]
        //[HttpPost]
        //public ClientRedirectUri InsertClientRedirectUri(ClientRedirectUri clientRedirectUri)
        //{
        //
        //
        //
        //    var result = _manageClientsSetup.InsertClientRedirectUri(clientRedirectUri);
        //    if (result.Id > 0)
        //    {
        //        _manageClientsSetup.WriteToLog(clientRedirectUri.ClientId, $"{{0}} {{1}} added URI {clientRedirectUri.Uri} to {{2}}.");
        //    }
        //
        //    return result;
        //}
        //
        ///// <summary>
        ///// Used to update a client redirect uri
        ///// </summary>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "The client redirect uri updated", Type = typeof(ClientRedirectUri))]
        //[Route("clientsetup/clientredirecturi/{id}")]
        //[HttpPut]
        //public HttpResponseMessage UpdateClientRedirectUri(int id, ClientRedirectUriUpdate clientRedirectUri)
        //{
        //
        //
        //
        //    clientRedirectUri.clientRedirectUri.Id = id;
        //    clientRedirectUri.originalClientRedirectUri.Id = id;
        //    ClientRedirectUri clientRedirectUriResult = _manageClientsSetup.UpdateClientRedirectUri(clientRedirectUri.originalClientRedirectUri, clientRedirectUri.clientRedirectUri);
        //    ObjectOutput<ClientRedirectUri, IErrorData> output = new ObjectOutput<ClientRedirectUri, IErrorData>() { obj = clientRedirectUriResult };
        //
        //    if (clientRedirectUriResult == null
        //        || clientRedirectUri.clientRedirectUri.ClientId != clientRedirectUriResult.ClientId
        //        || clientRedirectUri.clientRedirectUri.Uri != clientRedirectUriResult.Uri
        //       )
        //    {
        //        output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
        //
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, output);
        //    }
        //
        //    _manageClientsSetup.WriteToLog(clientRedirectUri.clientRedirectUri.ClientId, $"{{0}} {{1}} edited URI ID {id} for {{2}}. From {clientRedirectUri.originalClientRedirectUri.Uri} to {clientRedirectUri.clientRedirectUri.Uri}.");
        //    return Request.CreateResponse(HttpStatusCode.OK, output);
        //}
        //
        ///// <summary>
        ///// Used to delete a client redirect uri
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="clientRedirectUri"></param>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "The client redirect uri deleted")]
        //[Route("clientsetup/clientredirecturi/{id}")]
        //[HttpDelete]
        //public HttpResponseMessage DeleteClientRedirectUri(int id, ClientRedirectUri clientRedirectUri)
        //{
        //
        //
        //
        //    clientRedirectUri.Id = id;
        //    int result = _manageClientsSetup.DeleteClientRedirectUri(clientRedirectUri);
        //    if (result == 0)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
        //    }
        //
        //    _manageClientsSetup.WriteToLog(clientRedirectUri.ClientId, $"{{0}} {{1}} deleted URI {clientRedirectUri.Uri} from {{2}}.");
        //    return Request.CreateResponse(HttpStatusCode.OK);
        //}
        //
        //#endregion

        #region ClientPostLogoutRedirectUri

        /// <summary>
        /// Used to get a list of client post logout redirect uris
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of client post logout redirect uris", Type = typeof(ClientPostLogoutRedirectUri))]
        [Route("clientsetup/clientpostlogoutredirecturi")]
        [HttpGet]
        public IEnumerable<ClientPostLogoutRedirectUri> GetClientPostLogoutRedirectUri()
        {



            return _manageClientsSetup.GetClientPostLogoutRedirectUri();
        }

        /// <summary>
        /// Used to create a client post logout redirect uri
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client post logout redirect uri created", Type = typeof(ClientPostLogoutRedirectUri))]
        [Route("clientsetup/clientpostlogoutredirecturi")]
        [HttpPost]
        public HttpResponseMessage InsertClientPostLogoutRedirectUri(ClientPostLogoutRedirectUri clientPostLogoutRedirectUri)
        {
            ObjectOutput<ClientPostLogoutRedirectUri, IErrorData> output = new ObjectOutput<ClientPostLogoutRedirectUri, IErrorData>();
            // verify the client id exists
            if (!ClientIdExists(clientPostLogoutRedirectUri.ClientId))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid client id", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }




            ClientPostLogoutRedirectUri clientPostLogoutRedirectUriResult = _manageClientsSetup.InsertClientPostLogoutRedirectUri(clientPostLogoutRedirectUri);
            output.obj = clientPostLogoutRedirectUriResult;

            if (clientPostLogoutRedirectUriResult == null)
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }
            else
            {
                //[Employee first name and last name] added post-logout uri [post-lougout redirect uri] to [client name].
                var client = _manageClientsSetup.GetClientDetails(clientPostLogoutRedirectUri.ClientId);
                _manageClientsSetup.WriteToLog(clientPostLogoutRedirectUri.ClientId, $"{{0}} {{1}} added post-logout uri \"{clientPostLogoutRedirectUri.Uri}\" to {client.ClientName}.");

                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        /// <summary>
        /// Used to update a client post logout redirect uri
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client redirect uri updated", Type = typeof(ClientPostLogoutRedirectUri))]
        [Route("clientsetup/clientpostlogoutredirecturi/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateClientPostLogoutRedirectUri(int id, ClientPostLogoutRedirectUriUpdate clientPostLogoutRedirectUri)
        {
            ObjectOutput<ClientPostLogoutRedirectUri, IErrorData> output = new ObjectOutput<ClientPostLogoutRedirectUri, IErrorData>();
            // verify the client id exists
            if (!ClientIdExists(clientPostLogoutRedirectUri.clientPostLogoutRedirectUri.ClientId))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid client id", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }




            clientPostLogoutRedirectUri.clientPostLogoutRedirectUri.Id = id;
            clientPostLogoutRedirectUri.originalClientPostLogoutRedirectUri.Id = id;
            ClientPostLogoutRedirectUri clientPostLogoutRedirectUriResult = _manageClientsSetup.UpdateClientPostLogoutRedirectUri(clientPostLogoutRedirectUri.originalClientPostLogoutRedirectUri, clientPostLogoutRedirectUri.clientPostLogoutRedirectUri);
            output.obj = clientPostLogoutRedirectUriResult;

            if (clientPostLogoutRedirectUriResult == null
                || clientPostLogoutRedirectUri.clientPostLogoutRedirectUri.ClientId != clientPostLogoutRedirectUriResult.ClientId
                || clientPostLogoutRedirectUri.clientPostLogoutRedirectUri.Uri != clientPostLogoutRedirectUriResult.Uri
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };

                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }
            else
            {
                var o = clientPostLogoutRedirectUri.originalClientPostLogoutRedirectUri;
                var n = clientPostLogoutRedirectUri.clientPostLogoutRedirectUri;

                //[Employee first name and last name] updated post-logout redirect uri for [client name]. From [field 1]: "[previous value]" to "[new value]".
                if (o.Uri != n.Uri)
                {
                    var client = _manageClientsSetup.GetClientDetails(clientPostLogoutRedirectUri.clientPostLogoutRedirectUri.ClientId);
                    _manageClientsSetup.WriteToLog(o.ClientId, $"{{0}} {{1}} updated post-logout uri for {client.ClientName}. From URI: \"{o.Uri}\" to \"{n.Uri}\".");
                }

                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        /// <summary>
        /// Used to delete a client post logout redirect uri
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientPostLogoutRedirectUri"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client post logout redirect uri deleted")]
        [Route("clientsetup/clientpostlogoutredirecturi/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClientPostLogoutRedirectUri(int id, ClientPostLogoutRedirectUri clientPostLogoutRedirectUri)
        {
            ObjectOutput<ClientPostLogoutRedirectUri, IErrorData> output = new ObjectOutput<ClientPostLogoutRedirectUri, IErrorData>();



            clientPostLogoutRedirectUri.Id = id;
            int result = _manageClientsSetup.DeleteClientPostLogoutRedirectUri(clientPostLogoutRedirectUri);
            if (result == 0)
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Delete failed, no records deleted", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }
            else
            {
                //[Employee first name and last name] deleted post-logout uri [post-lougout redirect uri] from [client name].
                var client = _manageClientsSetup.GetClientDetails(clientPostLogoutRedirectUri.ClientId);
                _manageClientsSetup.WriteToLog(clientPostLogoutRedirectUri.ClientId, $"{{0}} {{1}} deleted post-logout uri \"{clientPostLogoutRedirectUri.Uri}\" from {client.ClientName}.");
                output.Status = new Status<IErrorData>() { Success = true };
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        #endregion

        #region Client Secrets

        /// <summary>
        /// Used to get a list of client secrets
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of client secrets", Type = typeof(ClientSecret))]
        [Route("clientsetup/clientsecret")]
        [HttpGet]
        public IEnumerable<ClientSecret> GetClientSecret()
        {



            IEnumerable<ClientSecret> clientSecrets = _manageClientsSetup.GetClientSecret();
            return clientSecrets;
        }

        /// <summary>
        /// Used to create a client secret
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client secret created", Type = typeof(ClientSecret))]
        [Route("clientsetup/clientsecret")]
        [HttpPost]
        public ClientSecret InsertClientSecret(ClientSecret clientSecret)
        {
            var response = _manageClientsSetup.InsertClientSecret(clientSecret);

            if (response.ClientId > 0)
            {
                var client = _manageClientsSetup.GetClientDetails(clientSecret.ClientId);
                _manageClientsSetup.WriteToLog(clientSecret.ClientId, $"{{0}} {{1}} added secret {clientSecret.Description} to {client.ClientName} client.");
            }

            return response;
        }

        /// <summary>
        /// Used to update a client secret
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client secret updated")]
        [Route("clientsetup/clientsecret/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateClientSecret(int id, ClientSecretUpdate clientSecret)
        {
            clientSecret.clientSecret.Id = id;
            clientSecret.originalClientSecret.Id = id;
            ClientSecret clientSecretResult = _manageClientsSetup.UpdateClientSecret(clientSecret.originalClientSecret, clientSecret.clientSecret);
            ObjectOutput<ClientSecret, IErrorData> output = new ObjectOutput<ClientSecret, IErrorData>() { obj = clientSecretResult };

            if (clientSecretResult == null
                || clientSecret.clientSecret.ClientId != clientSecretResult.ClientId
                || clientSecret.clientSecret.Description != clientSecretResult.Description
                || clientSecret.clientSecret.Value != clientSecretResult.Value
                || clientSecret.clientSecret.Type != clientSecretResult.Type
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };

                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            var o = clientSecret.originalClientSecret;
            var n = clientSecret.clientSecret;

            var client = _manageClientsSetup.GetClientDetails(clientSecret.clientSecret.ClientId);

            StringBuilder logMessage = new StringBuilder($"{{0}} {{1}} updated secret {o.Description} for {client.ClientName} client. ");

            if (o.Description != n.Description)
                logMessage.Append($"From Description: \"{o.Description}\" to \"{n.Description}\". ");

            if (o.Value != n.Value)
                logMessage.Append($"From Value: \"{o.Value}\" to \"{n.Value}\". ");

            if (o.Expiration != n.Expiration)
            {
                var oldShortExpDate = o.Expiration.UtcDateTime.ToShortDateString();
                var newShortExpDate = n.Expiration.UtcDateTime.ToShortDateString();

                logMessage.Append($"From Expiration: \"{oldShortExpDate}\" to \"{newShortExpDate}\". ");
            }

            _manageClientsSetup.WriteToLog(0, logMessage.ToString());

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to delete a client secret
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The client secret deleted")]
        [Route("clientsetup/clientsecret/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClientSecret(int id, ClientSecret clientSecret)
        {
            clientSecret.Id = id;
            int result = _manageClientsSetup.DeleteClientSecret(clientSecret);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }
            else
            {
                var client = _manageClientsSetup.GetClientDetails(clientSecret.ClientId);
                _manageClientsSetup.WriteToLog(0, $"{{0}} {{1}} deleted secret {clientSecret.Description} from {client.ClientName} client.");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion

        #region Scope Secrets

        /// <summary>
        /// Used to get a list of scope secrets
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of scope secrets", Type = typeof(ScopeSecret))]
        [Route("clientsetup/scopesecret")]
        [HttpGet]
        public IEnumerable<ScopeSecret> GetScopeSecrets()
        {
            return _manageClientsSetup.GetScopeSecrets();
        }

        /// <summary>
        /// Used to create a scope secret
        /// </summary>
        /// <param name="scopeSecret"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope secret created", Type = typeof(ScopeSecret))]
        [Route("clientsetup/scopesecret")]
        [HttpPost]
        public HttpResponseMessage InsertScopeSecret(ScopeSecret scopeSecret)
        {
            ObjectOutput<ScopeSecret, IErrorData> output = new ObjectOutput<ScopeSecret, IErrorData>();

            // verify the scope id exists
            if (!ScopeIdExists(scopeSecret.ScopeId))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid scope id", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            ScopeSecret scopeSecretResult = _manageClientsSetup.InsertScopeSecret(scopeSecret);
            output.obj = scopeSecretResult;

            if (scopeSecretResult == null)
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to update a scope secret
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scopeSecret"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope secret updated", Type = typeof(ScopeSecret))]
        [Route("clientsetup/scopesecret/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateScopeSecret(int id, ScopeSecretUpdate scopeSecret)
        {
            ObjectOutput<ScopeSecret, IErrorData> output = new ObjectOutput<ScopeSecret, IErrorData>();

            // verify the scope id exists
            if (!ScopeIdExists(scopeSecret.scopeSecret.ScopeId))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid scope id", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            scopeSecret.scopeSecret.Id = id;
            scopeSecret.originalScopeSecret.Id = id;
            ScopeSecret scopeSecretResult = _manageClientsSetup.UpdateScopeSecret(scopeSecret.originalScopeSecret, scopeSecret.scopeSecret);
            output.obj = scopeSecretResult;

            if (scopeSecretResult == null
                || scopeSecret.scopeSecret.ScopeId != scopeSecretResult.ScopeId
                || scopeSecret.scopeSecret.Value != scopeSecretResult.Value
                || scopeSecret.scopeSecret.Description != scopeSecretResult.Description
                || scopeSecret.scopeSecret.Expiration != scopeSecretResult.Expiration
                || scopeSecret.scopeSecret.Type != scopeSecretResult.Type
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to delete a scope secret
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scopeSecret"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope secret deleted")]
        [Route("clientsetup/scopesecret/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteScopeSecret(int id, ScopeSecret scopeSecret)
        {
            scopeSecret.Id = id;
            int result = _manageClientsSetup.DeleteScopeSecret(scopeSecret);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region Scope Claims

        /// <summary>
        /// Used to get a list of scope claims
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of scope claims", Type = typeof(ScopeClaim))]
        [Route("clientsetup/scopeclaim")]
        [HttpGet]
        public IEnumerable<ScopeClaim> GetScopeClaims()
        {
            return _manageClientsSetup.GetScopeClaims();
        }

        /// <summary>
        /// Used to create a scope claim
        /// </summary>
        /// <param name="scopeClaim"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope claim created", Type = typeof(ScopeClaim))]
        [Route("clientsetup/scopeclaim")]
        [HttpPost]
        public HttpResponseMessage InsertScopeClaim(ScopeClaim scopeClaim)
        {
            ObjectOutput<ScopeClaim, IErrorData> output = new ObjectOutput<ScopeClaim, IErrorData>();

            // verify the scope id exists
            if (!ScopeIdExists(scopeClaim.ScopeId))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid scope id", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            ScopeClaim scopeClaimResult = _manageClientsSetup.InsertScopeClaim(scopeClaim);
            output.obj = scopeClaimResult;

            if (scopeClaimResult == null)
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// used to update a scope claim
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scopeClaim"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope claim updated", Type = typeof(ScopeClaim))]
        [Route("clientsetup/scopeclaim/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateScopeClaim(int id, ScopeClaimUpdate scopeClaim)
        {
            ObjectOutput<ScopeClaim, IErrorData> output = new ObjectOutput<ScopeClaim, IErrorData>();

            // verify the scope id exists
            if (!ScopeIdExists(scopeClaim.scopeClaim.ScopeId))
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Insert failed, invalid scope id", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            scopeClaim.scopeClaim.Id = id;
            scopeClaim.originalScopeClaim.Id = id;
            ScopeClaim scopeClaimResult = _manageClientsSetup.UpdateScopeClaim(scopeClaim.originalScopeClaim, scopeClaim.scopeClaim);
            output.obj = scopeClaimResult;

            if (scopeClaimResult == null
                || scopeClaim.scopeClaim.ScopeId != scopeClaimResult.ScopeId
                || scopeClaim.scopeClaim.Name != scopeClaimResult.Name
                || scopeClaim.scopeClaim.Description != scopeClaimResult.Description
                || scopeClaim.scopeClaim.AlwaysIncludeInIdToken != scopeClaimResult.AlwaysIncludeInIdToken
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to delete a scope claim
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scopeClaim"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The scope claim deleted")]
        [Route("clientsetup/scopeclaim/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteScopeClaim(int id, ScopeClaim scopeClaim)
        {
            scopeClaim.Id = id;
            int result = _manageClientsSetup.DeleteScopeClaim(scopeClaim);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region Claim

        /// <summary>
        /// Used to get a list of claims
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of claims", Type = typeof(ULClaim))]
        [Route("clientsetup/claim")]
        [HttpGet]
        public IEnumerable<ULClaim> GetClaims()
        {
            return _manageClientsSetup.GetClaims();
        }

        /// <summary>
        /// Used to get a claim by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A client claim", Type = typeof(ULClaim))]
        [Route("clientsetup/claim/{id}")]
        [HttpGet]
        public ULClaim GetClaimById(int id)
        {
            return _manageClientsSetup.GetClaimById(id);
        }

        /// <summary>
        /// Used to create a claim
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The claim created", Type = typeof(ULClaim))]
        [Route("clientsetup/claim")]
        [HttpPost]
        public ULClaim InsertClaim(ULClaim claim)
        {
            var insert = _manageClientsSetup.InsertClaim(claim);
            if (insert.ClaimId > 0)
                _manageClientsSetup.WriteToLog(0, $"{{0}} {{1}} added claim {claim.ClaimName}.");

            return insert;
        }

        /// <summary>
        /// Used to update a claim
        /// </summary>
        /// <param name="id"></param>
        /// <param name="claim"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The claim updated")]
        [Route("clientsetup/claim/{id}")]
        [HttpPut]
        public HttpResponseMessage UpdateClaim(int id, ClaimUpdate claim)
        {
            ProductRepository productRepository = new ProductRepository();

            claim.claim.ClaimId = id;
            claim.originalClaim.ClaimId = id;
            ULClaim claimResult = _manageClientsSetup.UpdateClaim(claim.originalClaim, claim.claim);
            ObjectOutput<ULClaim, IErrorData> output = new ObjectOutput<ULClaim, IErrorData>() { obj = claimResult };

            if (claimResult == null
                || claim.claim.ClaimName != claimResult.ClaimName
                || claim.claim.SAMLAttributeName != claimResult.SAMLAttributeName
                || claim.claim.ProductId != claimResult.ProductId
               )
            {
                output.Status = new Status<IErrorData>() { ErrorMsg = "Update failed", Success = false };
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }
            else
            {
                var o = claim.originalClaim;
                var n = claim.claim;
                var product = productRepository.GetAllProducts().Where(x => x.ProductId == n.ProductId).FirstOrDefault();
                if (product != null)
                    n.ProductName = product.Name;

                StringBuilder logMessage = new StringBuilder($"{{0}} {{1}} updated claim {o.ClaimName}. ");

                if (o.ClaimName != n.ClaimName)
                    logMessage.Append($"From Claim Name: \"{o.ClaimName}\" to \"{n.ClaimName}\". ");

                if (o.SAMLAttributeName != n.SAMLAttributeName)
                    logMessage.Append($"From SAMLAttributeName: \"{o.SAMLAttributeName}\" to \"{n.SAMLAttributeName}\". ");

                if (o.ProductName != n.ProductName)
                    logMessage.Append($"From Product: \"{o.ProductName}\" to \"{n.ProductName}\". ");

                _manageClientsSetup.WriteToLog(0, logMessage.ToString());

                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        /// <summary>
        /// Used to delete a claim
        /// </summary>
        /// <param name="id"></param>
        /// <param name="claim"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The claim deleted")]
        [Route("clientsetup/claim/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClaim(int id, ULClaim claim)
        {
            // verify it is not in use by any clients
            IEnumerable<ClientClaimMapping> mappingList = _manageClientsSetup.GetClientClaimMapping();
            if (mappingList.Any(p => p.ClaimId == id))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Claim still in use");
            }

            claim.ClaimId = id;
            int result = _manageClientsSetup.DeleteClaim(claim);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }
            else
            {
                _manageClientsSetup.WriteToLog(0, $"{{0}} {{1}} deleted claim {claim.ClaimName}.");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion

        #region ClientClaimMapping

        /// <summary>
        /// Used to get a list of claim to client mappings
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of claim to client mappings", Type = typeof(ClientClaimMapping))]
        [Route("clientsetup/clientclaimmapping")]
        [HttpGet]
        public IEnumerable<ClientClaimMapping> GetClientClaimMapping()
        {
            return _manageClientsSetup.GetClientClaimMapping();
        }

        /// <summary>
        /// Used to get a list of claims by client id
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A client claim", Type = typeof(ClientClaimMapping))]
        [Route("clientsetup/clientclaimmapping/{clientid}")]
        [HttpGet]
        public IEnumerable<ClientClaimMapping> GetClientClaimMappingByClientId(int clientId)
        {
            return _manageClientsSetup.GetClientClaimMappingByClientId(clientId);
        }

        /// <summary>
        /// Used to create a client to claim mapping
        /// </summary>
        /// <param name="claimMapping"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The mapping created", Type = typeof(ClientClaimMapping))]
        [Route("clientsetup/clientclaimmapping")]
        [HttpPost]
        public HttpResponseMessage InsertClientClaimMapping(ClientClaimMapping claimMapping)
        {
            // verify the client and claim exist
            IEnumerable<Client> clientList = _manageClientsSetup.GetClientsNoDetails();

            if (clientList.All(p => p.ClientId != claimMapping.ClientId))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid client id given");
            }

            if (_manageClientsSetup.GetClaims().All(p => p.ClaimId != claimMapping.ClaimId))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid claim id given");
            }

            ClientClaimMapping result = _manageClientsSetup.InsertClientClaimMapping(claimMapping);
            if (result.ClientUserClaimId > 0)
            {
                var claim = _manageClientsSetup.GetClaims().Where(x => x.ClaimId == claimMapping.ClaimId).First();
                _manageClientsSetup.WriteToLog(claimMapping.ClientId, $"{{0}} {{1}} added User Claim {claim.ProductName} : {claim.ClaimName} to {{2}}.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Used to delete a claim to client mapping
        /// </summary>
        /// <param name="id"></param>
        /// <param name="claimMapping"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The claim mapping deleted")]
        [Route("clientsetup/clientclaimmapping/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClientClaimMapping(int id, ClientClaimMapping claimMapping)
        {
            claimMapping.ClientUserClaimId = id;
            int result = _manageClientsSetup.DeleteClientClaimMapping(claimMapping);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }

            var claim = _manageClientsSetup.GetClaims().Where(x => x.ClaimId == claimMapping.ClaimId).First();
            _manageClientsSetup.WriteToLog(claimMapping.ClientId, $"{{0}} {{1}}  removed User Claim {claim.ProductName} : {claim.ClaimName} from {{2}}.");

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        /// <summary>
        /// Used to get a list of cors urls
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of global settings", Type = typeof(GlobalSetting))]
        [Route("clientsetup/globalsettings")]
        [HttpGet]
        public IEnumerable<GlobalSetting> GetGlobalSettings()
        {
            GlobalSettingRepository gsr = new GlobalSettingRepository();

            return gsr.GetGlobalSettings();
        }

        #region private

        /// <summary>
        /// Used to verify if a given client id exists
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private bool ClientIdExists(int clientId)
        {
            IEnumerable<Client> clientList = _manageClientsSetup.GetClientsWithDetails();
            // verify the client id exists
            if (!clientList.Any(p => p.ClientId == clientId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Used to verify if a given scope id exists
        /// </summary>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        private bool ScopeIdExists(int scopeId)
        {
            IEnumerable<Scope> scopeList = _manageClientsSetup.GetScopes();
            // verify the scope id exists
            if (!scopeList.Any(p => p.ScopeId == scopeId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Used to validate client settings
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private string validClient(Client client)
        {

            if (client.Flow < 0 || client.Flow > 7)
            {
                return "invalid flow type";
            }

            if (!(client.AccessTokenType == 0 || client.AccessTokenType == 1))
            {
                //output.Status = new Status<IErrorData>() {ErrorMsg = "Insert failed, invalid access token type", Success = false};
                return "invalid access token type";
            }

            if (!(client.RefreshTokenExpiration == 0 || client.RefreshTokenExpiration == 1))
            {
                return "invalid refresh token expiration";
            }

            if (!(client.RefreshTokenUsage == 0 || client.RefreshTokenUsage == 1))
            {
                return "invalid refresh token usage";
            }

            return "";
        }

        #endregion

        #endregion

        #region Output classes

        public class ClientSecretUpdate
        {
            public ClientSecret originalClientSecret { get; set; }
            public ClientSecret clientSecret { get; set; }
        }

        public class ClientScopeUpdate
        {
            public ClientScope originalClientScope { get; set; }
            public ClientScope clientScope { get; set; }
        }

        public class ClientRedirectUriUpdate
        {
            public ClientRedirectUri originalClientRedirectUri { get; set; }
            public ClientRedirectUri clientRedirectUri { get; set; }
        }

        public class ClientUpdate
        {
            public Client originalClient { get; set; }
            public Client client { get; set; }
        }

        public class ClientClaimUpdate
        {
            public ClientClaim originalClientClaim { get; set; }
            public ClientClaim clientClaim { get; set; }

        }

        public class ClientPostLogoutRedirectUriUpdate
        {
            public ClientPostLogoutRedirectUri originalClientPostLogoutRedirectUri { get; set; }
            public ClientPostLogoutRedirectUri clientPostLogoutRedirectUri { get; set; }
        }

        public class ScopeUpdate
        {
            public Scope originalScope { get; set; }
            public Scope scope { get; set; }
        }

        public class ScopeSecretUpdate
        {
            public ScopeSecret originalScopeSecret { get; set; }
            public ScopeSecret scopeSecret { get; set; }
        }

        public class ScopeClaimUpdate
        {
            public ScopeClaim originalScopeClaim { get; set; }
            public ScopeClaim scopeClaim { get; set; }
        }

        public class ClaimUpdate
        {
            public ULClaim originalClaim { get; set; }
            public ULClaim claim { get; set; }
        }

        #endregion
    }
}
