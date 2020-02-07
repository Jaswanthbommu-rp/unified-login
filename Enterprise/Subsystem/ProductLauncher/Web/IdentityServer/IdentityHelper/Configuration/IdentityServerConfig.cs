using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logging;
using Log = RP.Enterprise.Foundation.Audit.Core.Component.Log;


namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Configuration
{
    public static class IdentityServerConfig
    {
        #region Public Methods

        public static IdentityServerOptions GetIdentityServerOptions()
        {
            var requireSsl = false; // internally all calls are non-ssl but F5 redirects them to ssl
            //#if DEBUG
            //            requireSsl = false;
            //#endif

            var options = new IdentityServerOptions
            {
                IssuerUri = ConfigReader.GetIssuerUri,
                RequireSsl = requireSsl,
                SiteName = "RealPage Identity Server",
                EnableWelcomePage = false,
                PublicOrigin = ConfigReader.GetPublicOriginUri,
                SigningCertificate = GetSigningCertificate(),
                Factory = new IdentityServerServiceFactory(),
                AuthenticationOptions = new IdentityServer3.Core.Configuration.AuthenticationOptions
                {
                    IdentityProviders = ConfigureIdentityProviders,
                    CookieOptions = new IdentityServer3.Core.Configuration.CookieOptions
                    {
                        AllowRememberMe = false,
                        IsPersistent = false,
                        RememberMeDuration = TimeSpan.FromMinutes(1),
                        SecureMode = CookieSecureMode.Always,
                        SuppressSameSiteNoneCookiesCallback = env =>
                        {
                            var context = new OwinContext(env);
                            return SuppressSameSiteNoneCookies(context);
                        }
                    },
                    EnableSignOutPrompt = false,
                    EnablePostSignOutAutoRedirect = true,
                    EnableAutoCallbackForFederatedSignout = true,
                    InvalidSignInRedirectUrl = ConfigReader.GetReturnUri,
                    PostSignOutAutoRedirectDelay = 5,
                }
            };

            //factory.ViewService = new Registration<IViewService>(typeof(NwpViewService));
            //factory.CorsPolicyService = new Registration<ICorsPolicyService>(new DefaultCorsPolicyService { AllowAll = true });

            return options;
        }

        #endregion

        #region Private Methods

        public static void SetupCustomImplementationHooks(IdentityServerOptions options)
        {
            options.Factory.Register(new Registration<IdentityServerRepository>());
            options.Factory.Register(new Registration<AuthenticateService>());
            options.Factory.Register(new Registration<ManageUserLoginIdentity>());
            options.Factory.Register(new Registration<ManageUserLogin>());

            options.Factory.UserService = new Registration<IUserService, UserService>();
            options.Factory.ClientStore = new Registration<IClientStore, ClientService>();
            options.Factory.AuthorizationCodeStore = new Registration<IAuthorizationCodeStore, AuthorizationService>();
            options.Factory.ConsentStore = new Registration<IConsentStore, ConsentService>();
            options.Factory.ScopeStore = new Registration<IScopeStore, ScopeService>();
            options.Factory.TokenHandleStore = new Registration<ITokenHandleStore, TokenHandleService>();
            options.Factory.RefreshTokenStore = new Registration<IRefreshTokenStore, RefreshTokenService>();
            options.Factory.ClientPermissionsService = new Registration<IClientPermissionsService, ClientPermissionsService>();
            options.Factory.CorsPolicyService = new Registration<ICorsPolicyService, CorsPolicyService>();
            options.Factory.EventService = new Registration<IEventService, AuditEventService>();

            options.EventsOptions.RaiseSuccessEvents = true;
        }

        private static X509Certificate2 GetSigningCertificate()
        {
            //#if DEBUG
            //            return new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\SomeCert.pfx", "idsrv3test");
            //#endif
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            // IF THE CERT CAN'T BE FOUND MAKE SURE IT DOESN'T HAVE THE HIDDEN UNICODE CHARACTER IN THE BEGINNING IN THE WEB.CONFIG!
            var certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint, ConfigReader.GetIdentityServerSigningCertThumbprint, false);
            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                var cert = certCollection[0];
                certStore.Close();
                // Use certificate
                return cert;
            }

            certStore.Close();
            throw new SecurityException("No certificate specified or found for IdentityServer-" + certCollection.Count.ToString() + "-" + ConfigReader.GetIdentityServerSigningCertThumbprint);
        }

        private static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            string correlationId = Guid.NewGuid().ToString();

            // Azure -- uses OpenIdConnect
            var azureConfiguration = GetProviderDetails(ProviderEnum.AzureActiveDirectory).FirstOrDefault();
            if (azureConfiguration != null)
            {
                Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = "ConfigureIdentityProviders.Azure - Start"});
                var azureOptions = GetOIDCOptions(azureConfiguration, signInAsType, correlationId);
                if (azureOptions != null)
                {
                    app.UseOpenIdConnectAuthentication(azureOptions);
                    Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = "ConfigureIdentityProviders.Azure - Loaded"});
                }
            }

            // Google
            var googleOptions = GetGoogleOptions(signInAsType, correlationId);
            if (googleOptions != null)
            {
                Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = "ConfigureIdentityProviders.Google - Start"});
                app.UseGoogleAuthentication(googleOptions);
                Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = "ConfigureIdentityProviders.Google - Loaded"});
            }

            // SAML
            var samlProviderList = GetProviderDetails(ProviderEnum.SAML);
            if (samlProviderList != null)
            {
                foreach (var provider in samlProviderList)
                {
                    Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = $"ConfigureIdentityProviders.SAML - Start {provider.AuthenticationType}"});
                    var samlpvd = GetSAMLOptions(provider, signInAsType, correlationId);
                    if (samlpvd != null)
                    {
                        app.UseSaml2Authentication(samlpvd);
                        Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = $"ConfigureIdentityProviders.SAML - Loaded {provider.AuthenticationType}"});
                    }
                }
            }

            var oidcList = GetProviderDetails(ProviderEnum.OIDC);
            if (oidcList != null)
            {
                foreach (var provider in oidcList)
                {
                    Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = $"ConfigureIdentityProviders.OIDC - Start {provider.AuthenticationType}"});
                    var oidcpvd = GetOIDCOptions(provider, signInAsType, correlationId);
                    if (oidcpvd != null)
                    {
                        app.UseOpenIdConnectAuthentication(oidcpvd);
                        Log.Write(LogType.Diagnostic, new LogDetails() {CorrelationId = correlationId, Message = $"ConfigureIdentityProviders.OIDC - Loaded {provider.AuthenticationType}"});
                    }
                }
            }
        }

        /// <summary>
        /// Used to get the settings for the SAML identity provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="signInAsType"></param>
        /// <returns></returns>
        private static Saml2AuthenticationOptions GetSAMLOptions(ProviderConfiguration provider, string signInAsType, string correlationId)
        {
            try
            {
                var authServicesOptions = new Saml2AuthenticationOptions(false)
                {
                    SignInAsAuthenticationType = signInAsType,
                    AuthenticationType = provider.AuthenticationType,
                    Caption = provider.Description,
                    Notifications = new Saml2Notifications()
                    {
                        AuthenticationRequestCreated = (request, identity, response) =>
                        {
                            request.ForceAuthentication = true;
                        }
                    },
                    SPOptions = new SPOptions
                    {
                        AuthenticateRequestSigningBehavior = SigningBehavior.Never, // or add a signing certificate
                        EntityId = new EntityId(provider.AuthorityUri),
                        ReturnUrl = new Uri(provider.RedirectUri),
                        ModulePath = $"/{provider.AuthenticationType}",
                        PublicOrigin = new Uri(ConfigReader.GetIssuerUri),
                    },
                };

                // may need for samesite fix
                //authServicesOptions.Notifications.AcsCommandResultCreated = (cr, r) =>
                //{
                //    string test = "";
                //    
                //};

                 // keep for debugging purposes
                //authServicesOptions.Notifications.GetPublicOrigin = (request) =>
                //{
                //    Dictionary<string, object> logData = new Dictionary<string, object>();
                //    logData.Add("request", request);
                //    Log.Write(LogType.Diagnostic, new LogDetails() { Message = $"IdentityServerConfig.GetSAMLOptions", AdditionalInfo = logData });
                //    return new Uri("https://asdsad");
                //};
                
                if (!ConfigReader.Environment.Equals("prod", StringComparison.OrdinalIgnoreCase))
                {
                    authServicesOptions.SPOptions.Logger = new TestLogger();// enable to log Saml2AuthenticationOptions issues
                }
                
                IdentityProvider idp = new IdentityProvider(new EntityId(provider.EntityId), authServicesOptions.SPOptions)
                {
                    MetadataLocation = provider.MetadataLocation,
                    LoadMetadata = true,
                    AllowUnsolicitedAuthnResponse = true,
                };

                authServicesOptions.IdentityProviders.Add(idp);
                return authServicesOptions;
            }
            catch (Exception ex)
            {
                Log.Write(LogType.Error, new LogDetails() {CorrelationId = correlationId, Exception = ex, Message = "IdentityServerConfig.GetSAMLOptions - " + ex.Message});
            }

            return null;
        }

        /// <summary>
        /// Used to set up an OIDC connector
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="signInAsType"></param>
        /// <returns></returns>
        private static OpenIdConnectAuthenticationOptions GetOIDCOptions(ProviderConfiguration provider, string signInAsType, string correlationId)
        {
            try
            {
                OpenIdConnectAuthenticationOptions options = new OpenIdConnectAuthenticationOptions
                {
                    AuthenticationType = provider.AuthenticationType,
                    Caption = provider.Description,
                    Scope = provider.Scope,
                    ClientId = provider.ProviderClientId,
                    Authority = provider.AuthorityUri,
                    PostLogoutRedirectUri = provider.PostLogoutRedirectUri,
                    RedirectUri = provider.RedirectUri,
                    AuthenticationMode = (AuthenticationMode) provider.AuthenticationMode,

                    TokenValidationParameters = new TokenValidationParameters
                    {
                        AuthenticationType = provider.TokenValidationAuthenticationType,
                        ValidateIssuer = provider.ValidateIssuer,
                        ValidAudience = provider.ValidAudience,
                    },
                    SignInAsAuthenticationType = signInAsType,
                    MetadataAddress = (!string.IsNullOrEmpty(provider.MetadataLocation) ? provider.MetadataLocation : null),
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        
                        SecurityTokenValidated = n =>
                        {
                            //Making available new claim id_token_serverUID in the AuthenticateExternalAsync to 
                            var id_token = n.ProtocolMessage.IdToken;
                            n.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim("external_idtoken", id_token));
                            if (n.AuthenticationTicket.Properties.Dictionary.ContainsKey("signinid"))
                            {
                                var signinid = n.AuthenticationTicket.Properties.Dictionary["signinid"];
                                if (n.OwinContext.Request.Cookies["userinfo."+ signinid] != null || n.OwinContext.Request.Cookies["ss-userinfo."+ signinid] != null)
                                {
                                    if (n.OwinContext.Request.Cookies["userinfo." + signinid] != null)
                                    {
                                        n.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim("login_username", Encoding.UTF8.GetString(Convert.FromBase64String(n.OwinContext.Request.Cookies["userinfo." + signinid]))));
                                    }
                                    else
                                    {
                                        if (n.OwinContext.Request.Cookies["ss-userinfo." + signinid] != null)
                                        {
                                            n.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim("login_username", Encoding.UTF8.GetString(Convert.FromBase64String(n.OwinContext.Request.Cookies["ss-userinfo." + signinid]))));
                                        }
                                    }
                                    n.OwinContext.Response.Cookies.Delete("userinfo."+ signinid, new Microsoft.Owin.CookieOptions() { Path = "/", HttpOnly = true, Secure = true });
                                    n.OwinContext.Response.Cookies.Delete("ss-userinfo."+ signinid, new Microsoft.Owin.CookieOptions() { Path = "/", HttpOnly = true, Secure = true });
                                }
                            }

                            return Task.FromResult(0);
                        },
                        RedirectToIdentityProvider = async n =>
                        {
                            //n.ProtocolMessage.MaxAge = "10"; // using an age of 5 didn't work with Azure so upping to 10
                            if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                            {
                                if (!SuppressSameSiteNoneCookies((OwinContext)n.OwinContext))
                                {
                                    var hold = n.OwinContext.Response.Headers["Set-Cookie"];
                                    n.OwinContext.Response.Headers["Set-Cookie"] = hold + "; secure; SameSite=None";
                                }
                                if (n.OwinContext.Request.Query.Get("info") != null)
                                {
                                    n.ProtocolMessage.Parameters.Add("login_hint", Encoding.UTF8.GetString(Convert.FromBase64String(n.OwinContext.Request.Query.Get("info"))));
                                    var signinId = n.OwinContext.Request.Query["signin"];
                                    n.OwinContext.Response.Cookies.Append("ss-userinfo." + signinId, n.OwinContext.Request.Query.Get("info"), new Microsoft.Owin.CookieOptions(){ Path = "/; secure; SameSite=None", Secure = true, HttpOnly = true});
                                    n.OwinContext.Response.Cookies.Append("userinfo." + signinId, n.OwinContext.Request.Query.Get("info"), new Microsoft.Owin.CookieOptions(){ Path = "/", Secure = true, HttpOnly = true});
                                }

                                if (n.OwinContext.Get<string>("prompt") != "" && !string.IsNullOrEmpty(n.OwinContext.Get<string>("prompt")))
                                {
                                    n.ProtocolMessage.Prompt = n.OwinContext.Get<string>("prompt").Split(':')[1];
                                }
                            }

                            if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                            {
                                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("external_idtoken");
                                if (idTokenHint != null)
                                {
                                    n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                                    var signOutMessageId = n.OwinContext.Environment.GetSignOutMessageId();
                                    if (signOutMessageId != null)
                                    {
                                        n.OwinContext.Response.Cookies.Append("state", signOutMessageId);
                                    }
                                }

                                // shortcut out of the IDP log out and just go back to our system
                                n.ProtocolMessage.IssuerAddress = n.ProtocolMessage.PostLogoutRedirectUri;
                                n.ProtocolMessage.PostLogoutRedirectUri = null;
                            }

                            await Task.FromResult(n);
                        }
                    }
                };

                return options;
            }
            catch (Exception ex)
            {
                Log.Write(LogType.Error, new LogDetails() {CorrelationId = correlationId, Exception = ex, Message = "IdentityServerConfig.GetOIDCOptions - " + ex.Message});
            }

            return null;
        }

        private static bool CompareAgent(string userAgent, string comparator, string compareValue)
        {
            switch (comparator)
            {
                case "Contains":
                    if (userAgent.Contains(compareValue))
                    {
                        return true;
                    }

                    break;
                case "StartsWith":
                    if (userAgent.StartsWith(compareValue))
                    {
                        return true;
                    }

                    break;
                case "Equals":
                    if (userAgent.Equals(compareValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    break;
                case "EndsWith":
                    if (userAgent.EndsWith(compareValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        private static bool SuppressSameSiteNoneCookies(OwinContext context)
        {
            List<SameSiteExclusion> excludeBrowserDetails = mockExclusions();

            var userAgent = context.Request.Headers["User-Agent"].ToString();
            SameSiteExclusion current = null;
            SameSiteExclusion next = null;
            bool result = false;
            //Dictionary<bool, string> resultList = new Dictionary<bool, string>();
            for (var i = 0; i < excludeBrowserDetails.Count; i++)
            {
                current = excludeBrowserDetails[i];
                if (i < excludeBrowserDetails.Count)
                {
                    while (excludeBrowserDetails[i + 1].LogicalOperator != null)
                    {
                        next = excludeBrowserDetails[i + 1];
                        //resultList.Add(CompareAgent(userAgent, next.ComparatorLeft, next.SameSiteValueLeft));
                        
                        i++;
                    }
                    next = excludeBrowserDetails[i + 1];
                }

                if (current.LogicalOperator == null)
                {
                    result = CompareAgent(userAgent, current.ComparatorLeft, current.SameSiteValueLeft);
                }
                else
                {

                }

                if (result)
                {
                    return true;
                }
            }
                
            return result;                
            
            //return true;
            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS 
            // networking stack.
            if (userAgent.Contains("CPU iPhone OS 12") ||
                userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
            // This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }

        private static List<SameSiteExclusion> mockExclusions()
        {
            List<SameSiteExclusion> excludeBrowserDetails = new List<SameSiteExclusion>();
            excludeBrowserDetails.Add(new SameSiteExclusion()
            {
                ComparatorLeft = "Contains",
                SameSiteValueLeft = "CPU iPhone OS 12",
                LogicalOperator = null,
                ComperatorRight = null,
                SameSiteValueRight = null
            });
            excludeBrowserDetails.Add(new SameSiteExclusion()
            {
                ComparatorLeft = "Contains",
                SameSiteValueLeft = "iPad; CPU OS 12",
                LogicalOperator = null,
                ComperatorRight = null,
                SameSiteValueRight = null
            });

            excludeBrowserDetails.Add(new SameSiteExclusion()
            {
                ComparatorLeft = "Contains",
                SameSiteValueLeft = "Macintosh; Intel Mac OS X 10_14",
                LogicalOperator = "And",
                ComperatorRight = "Contains",
                SameSiteValueRight = "Version/"
            });
            excludeBrowserDetails.Add(new SameSiteExclusion()
            {
                ComparatorLeft = "Contains",
                SameSiteValueLeft = "Version/",
                LogicalOperator = "And",
                ComperatorRight = "Contains",
                SameSiteValueRight = "Safari"
            });
            excludeBrowserDetails.Add(new SameSiteExclusion()
            {
                ComparatorLeft = "Contains",
                SameSiteValueLeft = "Safari",
                LogicalOperator = null,
                ComperatorRight = null,
                SameSiteValueRight = null
            });

            excludeBrowserDetails.Add(new SameSiteExclusion()
            {
                ComparatorLeft = "Contains",
                SameSiteValueLeft = "Chrome/5",
                LogicalOperator = null,
                ComperatorRight = null,
                SameSiteValueRight = null,
            });
            excludeBrowserDetails.Add(new SameSiteExclusion()
            {
                ComparatorLeft = "Contains",
                SameSiteValueLeft = "Chrome/6",
                LogicalOperator = null,
                ComperatorRight = null,
                SameSiteValueRight = null
            });

            return excludeBrowserDetails;
        }

        /// <summary>
        /// Used to set up a Google IDP
        /// </summary>
        /// <param name="signInAsType"></param>
        /// <returns></returns>
        private static GoogleOAuth2AuthenticationOptions GetGoogleOptions(string signInAsType, string correlationId)
        {
            try
            {
                ProviderConfiguration googleConfiguration = GetProviderDetails(ProviderEnum.Google).FirstOrDefault();
                if (googleConfiguration == null)
                {
                    throw new Exception("Unable to load Google configuration!");
                }

                GoogleOAuth2AuthenticationOptions options = new GoogleOAuth2AuthenticationOptions
                {
                    AuthenticationType = googleConfiguration.AuthenticationType, //"Google",
                    Caption = googleConfiguration.Caption, //"Sign-in with Google",
                    //Scope = googleConfiguration.Scope,
                    ClientId = googleConfiguration.ProviderClientId, //"179102204650-0d0rl3u61b9bsd9vn5j2mmtve8it7ong.apps.googleusercontent.com",
                    ClientSecret = googleConfiguration.ClientSecret, // "n61OGT18YRIP8AcvrsclVJMq",
                    AuthenticationMode = (AuthenticationMode) googleConfiguration.AuthenticationMode, // AuthenticationMode.Active,
                    SignInAsAuthenticationType = signInAsType,

                };

                return options;
            }
            catch (Exception ex)
            {
                Log.Write(LogType.Error, new LogDetails() {CorrelationId = correlationId, Exception = ex, Message = "IdentityServerConfig.GetGoogleOptions - " + ex.Message});
            }

            return null;
        }


        /// <summary>
        /// Used to get the settings for the given provider. Retry up to 10 times in case the server may still be starting up.
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        private static IList<ProviderConfiguration> GetProviderDetails(ProviderEnum providerType)
        {
            IList<ProviderConfiguration> providerConfiguration = null;
            int retryCount = 0;
            while (retryCount <= 10)
            {
                retryCount++;
                try
                {
                    var manageProvider = new ManageProvider();
                    var providerName = providerType.ToString().ToLower();
                    providerConfiguration = manageProvider.GetProviderConfigurationByName(providerName);
                    if (providerConfiguration.Count > 0)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }

            return providerConfiguration;
        }

        #endregion
    }
}
