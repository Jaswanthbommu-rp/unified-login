using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using RPModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public class RefreshTokenService : BaseTokenService<IdentityServer3.Core.Models.RefreshToken>, IRefreshTokenStore
    {
	    private static IIdentityServerRepository _identityServerRepository;

	    public RefreshTokenService(IIdentityServerRepository identityServerRepository, IScopeStore scopeStore, IClientStore clientStore)
		    : base(TokenTypeEnum.RefreshToken, scopeStore, clientStore, identityServerRepository)
	    {
		    _identityServerRepository = identityServerRepository;
	    }

	    public override Task StoreAsync(string key, RefreshToken value)
        {
	        var identityServerRepository = new IdentityServerRepository();
			var token = identityServerRepository.GetIdentityToken(key, (int)TokenType);
            if (token == null)
            {
                token = new RPModel.Token
                {
                    TokenKey = key,
                    SubjectCode = value.SubjectId,
                    ClientCode = value.ClientId,
                    JsonCode = ConvertToJson(value),
                    TokenType = (int)TokenType,
                    Expiry = value.CreationTime.AddSeconds(value.LifeTime)
                };
                identityServerRepository.InsertIdentityToken(token);
                return Task.FromResult(0);
            }

            token.Expiry = value.CreationTime.AddSeconds(value.LifeTime);
            identityServerRepository.UpdateIdentityTokenExpiry(key, token.Expiry);
            return Task.FromResult(0);
        }
    }
}