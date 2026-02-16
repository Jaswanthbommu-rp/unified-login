using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UnifiedLogin.BusinessLogic.Attributes
{
	/// <summary>
	/// Scope-based authorization attribute for ASP.NET Core.
	/// Validates that the authenticated user possesses at least one of the required scope values.
	/// Supports multiple scope claims ("scope" or "Scope") and space-delimited claim values per standard OAuth2 conventions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class AuthorizeScopeAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string[] _scopesToCheck;

		/// <summary>
		/// Specify one or more required scopes. If none supplied, only authentication is enforced.
		/// </summary>
		public AuthorizeScopeAttribute(params string[] scopes)
		{
            // Parse comma-separated scopes from each parameter
            _scopesToCheck = (scopes ?? Array.Empty<string>())
                .SelectMany(s => s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }

		/// <summary>
		/// Authorization logic executed early in the MVC pipeline.
		/// </summary>
		public void OnAuthorization(AuthorizationFilterContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			// Skip if [AllowAnonymous] is present.
			if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
				return;

			var user = context.HttpContext.User;
			if (user?.Identity?.IsAuthenticated != true)
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			// Collect scope values from claims (supports space-delimited list in a single claim value).
			var scopeValues = user.FindAll(c => string.Equals(c.Type, "scope", StringComparison.OrdinalIgnoreCase) || string.Equals(c.Type, "Scope", StringComparison.OrdinalIgnoreCase))
				.SelectMany(c => c.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			bool allowed = _scopesToCheck.Length == 0 || _scopesToCheck.Any(s => scopeValues.Contains(s));
			if (!allowed)
			{
				context.Result = new ForbidResult();
			}
		}
	}
}