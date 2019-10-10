using System;
using System.Threading.Tasks;
using IdentityServer3.Core.Services;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using RPModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public class TokenHandleService : BaseTokenService<IdentityServer3.Core.Models.Token>, ITokenHandleStore
    {
	    private static IdentityServerRepository _identityServerRepository;

		public TokenHandleService(IdentityServerRepository identityServerRepository, IScopeStore scopeStore, IClientStore clientStore)
            : base(TokenTypeEnum.TokenHandle, scopeStore, clientStore, _identityServerRepository)
		{
			_identityServerRepository = identityServerRepository;
		}

        public override Task StoreAsync(string key, IdentityServer3.Core.Models.Token value)
        {
            var token = new RPModel.Token
            {
                TokenKey = key,
                SubjectCode = value.SubjectId,
                ClientCode = value.ClientId,
                JsonCode = ConvertToJson(value),
                TokenType = (int)TokenType,
                Expiry = DateTimeOffset.UtcNow.AddSeconds(value.Lifetime),

            };
			_identityServerRepository.InsertIdentityToken(token);

            return Task.FromResult(0);
        }
    }
}