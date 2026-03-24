using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Models;

/// <summary>
/// Immutable per-call value object resolved by <see cref="IProductContextServiceAsync"/>.
/// <para>
/// Replaces the mutable instance fields <c>_editorPersona</c>, <c>_userPersona</c>,
/// <c>_editorProductUserId</c>, <c>_productUserId</c>, <c>_productLearnerId</c>,
/// <c>_productManagerId</c> from <c>ManageProductBase</c>.
/// Created fresh on every call — no shared state between concurrent requests.
/// </para>
/// </summary>
public sealed record ProductCallContext(
    Persona  EditorPersona,
    Persona? UserPersona,
    string   EditorProductUsername = "",
    string   EditorProductUserId   = "",
    string   ProductUsername        = "",
    string   ProductUserId          = "",
    string   ProductLearnerId       = "",
    string   ProductManagerId       = ""
);