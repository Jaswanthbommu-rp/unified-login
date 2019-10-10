using System;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using RPModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public class AuthorizationService : BaseTokenService<AuthorizationCode>, IAuthorizationCodeStore
    {
	    private static IdentityServerRepository _identityServerRepository;

	    public AuthorizationService(IdentityServerRepository identityServerRepository, IScopeStore scopeStore, IClientStore clientStore)
		    : base(TokenTypeEnum.AuthorizationCode, scopeStore, clientStore, _identityServerRepository)
	    {
		    _identityServerRepository = identityServerRepository;
	    }

	    public override Task StoreAsync(string key, AuthorizationCode value)
        {
            var expiry = value.Client?.AuthorizationCodeLifetime ?? 3600;

            var authCodeRecord = new RPModel.Token()
            {
                TokenKey = key,
                TokenType = (int)TokenTypeEnum.AuthorizationCode,
                Expiry = DateTimeOffset.UtcNow.AddSeconds(expiry),
                IsOpenId = value.IsOpenId,
                Nonce = value.Nonce,
                RedirectUri = value.RedirectUri,
                SessionId = value.SessionId,
                SubjectCode = value.SubjectId,
                WasConsentShown = value.WasConsentShown,
                JsonCode = ConvertToJson(value),
                ClientCode = value.ClientId,
                AuthCodeChallenge = value.CodeChallenge,
                AuthCodeChallengeMethod = value.CodeChallengeMethod
            };
			_identityServerRepository.InsertIdentityToken(authCodeRecord);

            return Task.FromResult(0);
        }
    }
}
