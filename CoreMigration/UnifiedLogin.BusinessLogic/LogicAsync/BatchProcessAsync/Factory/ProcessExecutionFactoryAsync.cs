using System.Collections.Frozen;
using UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Process;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;

/// <summary>
/// Async-first replacement for the sync <c>ProcessExecutionFactory</c>.
/// <para>
/// Eliminates <c>Activator.CreateInstance</c> and the <c>Dictionary&lt;BatchProcessType, Type&gt;</c>
/// approach in favour of a <see cref="FrozenDictionary{TKey,TValue}"/> populated at construction
/// time from DI-injected, fully-initialised <see cref="IProcessExecutionAsync"/> singletons.
/// </para>
/// <para>
/// <b>DI registration (example):</b>
/// <code>
/// services.AddScoped&lt;CreateUpdateProductUserAsync&gt;();
/// services.AddScoped&lt;UpdateProductUserProfileAsync&gt;();
/// services.AddScoped&lt;DeactivateProductUserAsync&gt;();
/// services.AddScoped&lt;ChangeProductUserTypeAsync&gt;();
/// services.AddScoped&lt;EnterpriseCreateUpdateProductUserAsync&gt;();
/// services.AddScoped&lt;ProcessExecutionFactoryAsync&gt;();
/// </code>
/// </para>
/// </summary>
public sealed class ProcessExecutionFactoryAsync
{
    private readonly FrozenDictionary<BatchProcessType, IProcessExecutionAsync> _registry;

    public ProcessExecutionFactoryAsync(
        CreateUpdateProductUserAsync        createUpdateProductUser,
        UpdateProductUserProfileAsync       updateProductUserProfile,
        DeactivateProductUserAsync          deactivateProductUser,
        ChangeProductUserTypeAsync          changeProductUserType,
        EnterpriseCreateUpdateProductUserAsync enterpriseCreateUpdateProductUser)
    {
        ArgumentNullException.ThrowIfNull(createUpdateProductUser);
        ArgumentNullException.ThrowIfNull(updateProductUserProfile);
        ArgumentNullException.ThrowIfNull(deactivateProductUser);
        ArgumentNullException.ThrowIfNull(changeProductUserType);
        ArgumentNullException.ThrowIfNull(enterpriseCreateUpdateProductUser);

        // Shared instances for process types that use the same handler.
        // FrozenDictionary is immutable after construction — zero allocation on read.
        _registry = new Dictionary<BatchProcessType, IProcessExecutionAsync>
        {
            [BatchProcessType.CreateUpdateProductUser]               = createUpdateProductUser,
            [BatchProcessType.ProfileUpdate]                         = updateProductUserProfile,
            [BatchProcessType.DeactivateProductUser]                 = deactivateProductUser,
            [BatchProcessType.UserTypeAdminToRegular]                = changeProductUserType,
            [BatchProcessType.UserTypeRegularToAdmin]                = changeProductUserType,
            [BatchProcessType.UserTypeAdminToExternal]               = changeProductUserType,
            [BatchProcessType.UserTypeExternalToAdmin]               = changeProductUserType,
            [BatchProcessType.EnterpriseRoleCreateUpdateProductUser] = enterpriseCreateUpdateProductUser,
            [BatchProcessType.PrimaryPropertiesUpdateProductUser]    = createUpdateProductUser,
        }.ToFrozenDictionary();
    }

    /// <summary>
    /// Returns the handler registered for <paramref name="processType"/>.
    /// Throws <see cref="InvalidOperationException"/> if no handler is registered.
    /// </summary>
    public IProcessExecutionAsync GetProductLogic(BatchProcessType processType)
        => _registry.TryGetValue(processType, out var handler)
            ? handler
            : throw new InvalidOperationException(
                $"No async handler registered for batch process type '{processType}'.");
}
