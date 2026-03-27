using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for custom field operations.
/// Delegates to the existing sync <see cref="IManageCustomFields"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageCustomFieldsAsync : IManageCustomFieldsAsync
{
    private readonly IManageCustomFieldsAsync _manageCustomFields;

    public ManageCustomFieldsAsync(IManageCustomFieldsAsync manageCustomFields)
    {
        _manageCustomFields = manageCustomFields ?? throw new ArgumentNullException(nameof(manageCustomFields));
    }

    public async Task<IList<CustomField>> GetCustomFieldAsync(IDictionary<object, object> globals, long partyId, CancellationToken cancellationToken = default)
        => await _manageCustomFields.GetCustomFieldAsync(globals, partyId, cancellationToken);
}
