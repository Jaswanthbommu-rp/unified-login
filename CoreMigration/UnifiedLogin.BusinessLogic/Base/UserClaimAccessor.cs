using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Base;

/// <summary>
/// Resolves <see cref="DefaultUserClaim"/> from the active HTTP request.
///
/// Uses <see cref="IHttpContextAccessor.HttpContext.Items"/> as a per-request
/// cache — the claim is built once from the <see cref="ClaimsPrincipal"/>
/// and reused for the lifetime of the request.
/// </summary>
public sealed class HttpContextUserClaimAccessor : IUserClaimAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserClaimAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor
           ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    /// <inheritdoc/>
    public DefaultUserClaim Current
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;

            // No HTTP context — Worker Service or background thread calling
            // through an HTTP-registered scope. Return a safe fallback.
            if (ctx is null)
                return BuildSystemClaim();

            // Per-request cache: build once, reuse on every .Current access
            // within the same request pipeline.
            const string itemKey = nameof(DefaultUserClaim);

            if (ctx.Items.TryGetValue(itemKey, out var cached)
                && cached is DefaultUserClaim existing)
            {
                return existing;
            }

            var claim = ctx.User?.Identity?.IsAuthenticated == true
                ? new DefaultUserClaim(ctx.User)
                : BuildSystemClaim();

            ctx.Items[itemKey] = claim;
            return claim;
        }
    }

    /// <summary>
    /// Returns a minimal non-null claim for unauthenticated or
    /// background-invoked scopes.
    /// </summary>
    private static DefaultUserClaim BuildSystemClaim() =>
        new() { CorrelationId = Guid.NewGuid() };
}