using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace UnifiedLogin.BusinessLogic.Attributes
{
	/// <summary>
	/// Used to secure a controller using scopes
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class AuthorizeScopeAttribute : AuthorizeAttribute
	{
		#region Private Variables Ctor
		/// <summary>
		/// The list of allowable scopes
		/// </summary>
		private readonly string[] _scopeToCheck;

		/// <summary>
		/// AuthorizeScope attribute
		/// </summary>
		/// <param name="scope"></param>
		public AuthorizeScopeAttribute(params string[] scope)
		{
			this._scopeToCheck = scope;
		}

		#endregion

		#region Public / Protected methods

		/// <summary>
		/// Returns if request is authorized or not
		/// </summary>
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			if (SkipAuthorization(actionContext))
			{
				return true;
			}
			bool isAuthorized = base.IsAuthorized(actionContext);
			return isAuthorized;
		}

		/// <summary>
		/// Triggers in authorization
		/// </summary>
		/// <param name="actionContext"></param>
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			if (SkipAuthorization(actionContext))
			{
				return;
			}
			if (base.IsAuthorized(actionContext))
			{
				List<Claim> claimList = ClaimsPrincipal.Current.Claims.ToList();
				try
				{
					//if (claimDetails != null &&)// claimDetails.OrganizationPartyId > 0 && !string.IsNullOrEmpty(claimDetails.Roles))}
					{
						bool allowedScope = false;
						{
							foreach (var scope in _scopeToCheck)
							{
								// Check user has access to right

								if (claimList.Any(p => p.Type.Equals("Scope", StringComparison.OrdinalIgnoreCase) && p.Value.Equals(scope, StringComparison.OrdinalIgnoreCase)))
								{
									allowedScope = true;
									break;
								}
							}
						}
						if (!allowedScope)
						{
							this.HandleUnauthorizedRequest(actionContext);
						}
					}
				}
				catch (Exception ex)
				{
					try
					{
						this.HandleUnauthorizedRequest(actionContext);
					}
					finally
					{
					}
				}
			}
			else
			{
				base.OnAuthorization(actionContext);
			}
		}

		/// <summary>
		/// Handle responsed that are not authenticated successfully
		/// </summary>
		/// <param name="actionContext"></param>
		protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
		{
			if (base.IsAuthorized(actionContext))
			{
				actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
				actionContext.Response.ReasonPhrase = "The server understood the request but refuses to authorize it.(No Right)";
			}
			else
			{
				base.HandleUnauthorizedRequest(actionContext);
			}
		}

		#endregion

		#region Private methods
		/// <summary>
		/// Skip authorization because the method is set to AllowAnonymous
		/// </summary>
		/// <param name="actionContext"></param>
		/// <returns></returns>
		private static bool SkipAuthorization(HttpActionContext actionContext)
		{
			Contract.Assert(actionContext != null);

			return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
				   || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
		}

		#endregion
	}
}