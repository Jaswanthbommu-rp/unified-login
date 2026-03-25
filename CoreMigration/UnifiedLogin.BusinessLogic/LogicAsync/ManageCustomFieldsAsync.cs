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
    private readonly IManageCustomFields _manageCustomFields;

    public ManageCustomFieldsAsync(IManageCustomFields manageCustomFields)
    {
        _manageCustomFields = manageCustomFields ?? throw new ArgumentNullException(nameof(manageCustomFields));
    }

    public Task<IList<CustomField>> GetCustomFieldAsync(IDictionary<object, object> globals, long partyId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCustomFields.GetCustomField(globals, partyId));
}
