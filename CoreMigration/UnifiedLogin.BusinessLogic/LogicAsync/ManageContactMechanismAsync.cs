using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for contact mechanism operations.
/// Delegates to the existing sync <see cref="IManageContactMechanism"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageContactMechanismAsync : IManageContactMechanismAsync
{
    private readonly IContactMechanismRepositoryAsync _contactMechanismRepository;

    public ManageContactMechanismAsync(IContactMechanismRepositoryAsync contactMechanismRepository)
    {
        _contactMechanismRepository = contactMechanismRepository ?? throw new ArgumentNullException(nameof(contactMechanismRepository));
    }

    public Task<RepositoryResponse> CreateContactMechanismAsync(CancellationToken cancellationToken = default)
        => _contactMechanismRepository.CreateContactMechanismAsync(cancellationToken);

    public Task<IList<CommonAddress>> ListContactMechanismForPersonAsync(Guid realPageId, string ContactMechanismUsageTypeName, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
        {
            throw new Exception("Invalid parameter realPageId.");
        }

        return _contactMechanismRepository.ListContactMechanismForPersonAsync(realPageId, ContactMechanismUsageTypeName, cancellationToken);
    }

    public Task<RepositoryResponse> LinkContactMechanismToPartyAsync(Guid realPageId, IPartyContactMechanism partyContactMechanism, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
        {
            throw new Exception("Invalid parameter realPageId.");
        }
        if (partyContactMechanism == null)
        {
            throw new ArgumentNullException(nameof(partyContactMechanism), "Null PartyContactMechanism.");
        }
        return _contactMechanismRepository.LinkContactMechanismToPartyAsync(realPageId, partyContactMechanism, cancellationToken);
    }
        

    public Task<RepositoryResponse> LinkUsageTypeToPartyContactMechanismAsync(long partyContactMechanismId, int? contactMechanismUsageTypeId, CancellationToken cancellationToken = default)
    {
        if (contactMechanismUsageTypeId == null)
        {
            throw new ArgumentNullException(nameof(contactMechanismUsageTypeId), "Null contactMechanismUsageTypeId.");
        }
        return _contactMechanismRepository.LinkUsageTypeToPartyContactMechanismAsync(partyContactMechanismId, contactMechanismUsageTypeId, cancellationToken);
    }

    public Task<RepositoryResponse> LinkGeographicBoundaryToContactMechanismAsync(IContactMechanismBoundary contactMechanismBoundary, CancellationToken cancellationToken = default)
    {
        if (contactMechanismBoundary == null)
        {
            throw new ArgumentNullException(nameof(contactMechanismBoundary), "Null contactMechanismBoundary.");
        }
        return _contactMechanismRepository.LinkGeographicBoundaryToContactMechanismAsync(contactMechanismBoundary, cancellationToken);
    }

    public Task<RepositoryResponse> UpdateContactMechanismUsageForPartyAsync(long PartyContactMechanismID, int? ContactMechanismUsageTypeId, CancellationToken cancellationToken = default)
    {
        if (PartyContactMechanismID <= 0)
        {
            throw new Exception("Missing Party Contact Mechanism Id.");
        }

        return _contactMechanismRepository.UpdateContactMechanismUsageForPartyAsync(PartyContactMechanismID, ContactMechanismUsageTypeId, cancellationToken);
    }
}
