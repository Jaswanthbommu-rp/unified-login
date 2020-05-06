using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Services;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using RPModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public class ConsentService : IConsentStore
    {
	    private readonly IIdentityServerRepository _identityServerRepository;

		public ConsentService(IIdentityServerRepository identityServerRepository)
		{
			_identityServerRepository = identityServerRepository;
		}

        public Task<IdentityServer3.Core.Models.Consent> LoadAsync(string subject, string client)
        {
			var found = _identityServerRepository.GetConsentBySubjectAndClient(subject, client);
            if (found == null)
            {
                return Task.FromResult<IdentityServer3.Core.Models.Consent>(null);
            }
            var result = new IdentityServer3.Core.Models.Consent
            {
                Subject = found.SubjectCode,
                ClientId = found.ClientCode,
                Scopes = ParseScopes(found.Scopes)
            };

            return Task.FromResult<IdentityServer3.Core.Models.Consent>(result);
        }

        public Task UpdateAsync(IdentityServer3.Core.Models.Consent consent)
        {
			var item = _identityServerRepository.GetConsentBySubjectAndClient(consent.Subject, consent.ClientId);
            bool needToInsert = false;

            if (item == null)
            {
                item = new RPModel.Consent
                {
                    SubjectCode = consent.Subject,
                    ClientCode = consent.ClientId
                };
                needToInsert = true;
            }

            if (consent.Scopes == null || !consent.Scopes.Any())
            {
                if (!needToInsert)
                {
                    _identityServerRepository.DeleteConsentBySubjectAndClient(consent.Subject, consent.ClientId);
                }
            }

            item.Scopes = StringifyScopes(consent.Scopes);

            if (needToInsert)
            {
                _identityServerRepository.InsertConsent(item);
            }
            else
            {
                _identityServerRepository.UpdateConsent(item);
            }

            return Task.FromResult(0);
        }

        public Task<IEnumerable<IdentityServer3.Core.Models.Consent>> LoadAllAsync(string subject)
        {
			var found = _identityServerRepository.GetConsentsBySubject(subject);

            var results = found.Select(x => new IdentityServer3.Core.Models.Consent
            {
                Subject = x.SubjectCode,
                ClientId = x.ClientCode,
                Scopes = ParseScopes(x.Scopes)
            });

            return Task.FromResult<IEnumerable<IdentityServer3.Core.Models.Consent>>(results.ToArray().AsEnumerable());
        }

        private IEnumerable<string> ParseScopes(string scopes)
        {
            if (scopes == null || String.IsNullOrWhiteSpace(scopes))
            {
                return Enumerable.Empty<string>();
            }

            return scopes.Split(',');
        }

        private string StringifyScopes(IEnumerable<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                return null;
            }

            return scopes.Aggregate((s1, s2) => s1 + "," + s2);
        }

        public Task RevokeAsync(string subject, string client)
        {
			_identityServerRepository.DeleteConsentBySubjectAndClient(subject, client);
            return Task.FromResult(0);
        }
    }
}
