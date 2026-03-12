using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Base;

/// <summary>
/// Abstracts per-request access to <see cref="DefaultUserClaim"/>.
///
/// Replaces the constructor-parameter pattern that caused constructor
/// explosions across <see cref="ProductRepository"/>, <see cref="ManageProfile"/>,
/// <see cref="ManageUserLogin"/>, and <see cref="PersonaRepository"/>.
///
/// Register as <b>Scoped</b> — one resolved instance per HTTP request
/// (or per Worker Service execution scope).
/// </summary>
public interface IUserClaimAccessor
{
    /// <summary>
    /// Returns the <see cref="DefaultUserClaim"/> for the current execution scope.
    /// Never returns null — falls back to an anonymous system claim for background work.
    /// </summary>
    DefaultUserClaim Current { get; }
}
