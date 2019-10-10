using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.JsonConverters;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public abstract class BaseTokenService<T> where T : class
    {
        protected readonly TokenTypeEnum TokenType;
        private readonly IScopeStore _scopeStore;
        private readonly IClientStore _clientStore;
        private readonly IdentityServerRepository _identityServerRepository;

		public BaseTokenService(TokenTypeEnum tokenType, IScopeStore scopeStore, IClientStore clientStore, IdentityServerRepository identityServerRepository)
        {
            TokenType = tokenType;
            _scopeStore = scopeStore;
            _clientStore = clientStore;
            _identityServerRepository = identityServerRepository;
        }

        JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //};
            settings.Converters.Add(new ClaimConverter());
            settings.Converters.Add(new ClaimsPrincipalConverter());
            settings.Converters.Add(new ClientConverter(_clientStore));
            settings.Converters.Add(new ScopeConverter(_scopeStore));
            return settings;
        }

        protected string ConvertToJson(T value)
        {
            return JsonConvert.SerializeObject(value, GetJsonSerializerSettings());
        }

        protected T ConvertFromJson(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetJsonSerializerSettings());
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
			var tokens = _identityServerRepository.GetTokensBySubject(subject, (int)TokenType);

            var results = tokens.Select(x => ConvertFromJson(x.JsonCode)).ToArray();
            return Task.FromResult<IEnumerable<ITokenMetadata>>(results.Cast<ITokenMetadata>());
        }

        public Task<T> GetAsync(string key)
        {
			var record = _identityServerRepository.GetIdentityToken(key, (int)TokenType);
            if (record != null)
            {
                return Task.FromResult<T>(ConvertFromJson(record.JsonCode));
            }
            return Task.FromResult<T>(null);
        }

        public Task RemoveAsync(string key)
        {
	        _identityServerRepository.DeleteIdentityTokenByKey(key, (int)TokenType);
            return Task.FromResult(0);
        }

        public Task RevokeAsync(string subject, string client)
        {
	        _identityServerRepository.DeleteIdentityTokenBySubjectAndClient(subject, client, (int)TokenType);
            return Task.FromResult(0);
        }
        public abstract Task StoreAsync(string key, T value);
    }
}
