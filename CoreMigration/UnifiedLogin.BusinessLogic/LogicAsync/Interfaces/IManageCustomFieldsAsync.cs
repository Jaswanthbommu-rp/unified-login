using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for custom field operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageCustomFields"/>.
/// </summary>
public interface IManageCustomFieldsAsync
{
    Task<IList<CustomField>> GetCustomFieldAsync(IDictionary<object, object> globals, long partyId, CancellationToken cancellationToken = default);
}
