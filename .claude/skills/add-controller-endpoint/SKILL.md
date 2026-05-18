---
name: add-controller-endpoint
description: Scaffold a new ASP.NET Web API 2 endpoint on Service/LandingAPI or Service/LandingAPIEnterprise in unified-login-main. Use when the user wants to expose a new HTTP route. Captures BaseApiController inheritance, [Authorize]/[AuthorizeScope] selection, DefaultUserClaim usage, Swagger annotations, AllowCors, and the test-constructor pattern for mocks.
---

# Add a new Web API 2 endpoint

This repo uses ASP.NET Web API 2 (.NET Framework 4.8) under two services:

- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/` — user-facing UL APIs.
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPIEnterprise/` — partner/enterprise APIs (scope-gated).

Controllers inherit `BaseApiController`, which exposes `_userClaims` (`DefaultUserClaim` with `UserId`, `CorrelationId`, `OrganizationRealPageGuid`, `CustomerMasterId`) and `_realpageUserId`.

## Inputs to confirm

1. **Which service** — `LandingAPI` (default) or `LandingAPIEnterprise` (scope-protected, used by partners).
2. **Controller name** — e.g., `ProductContactCenterController` for a new product endpoint, or extend an existing controller.
3. **Route + verb** — `/api` is the global prefix from the routing config; the controller's `[Route(...)]` is the segment after that. Example: `[Route("products/contactcenter/users")]` `[HttpGet]`.
4. **Auth shape** — `[Authorize]` for end-user JWT, `[AuthorizeScope("enterpriseapi")]` / `[AuthorizeScope("userinfoapi")]` for `LandingAPIEnterprise` (see existing scopes in [Service/LandingAPIEnterprise/Controllers/PropertyController.cs](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPIEnterprise/Controllers/PropertyController.cs)). Provisioning / export endpoints **must** use `AuthorizeScope` per `.github/copilot-instructions.md`.
5. **Request/response DTOs** — confirm the DTO lives in `Service/SharedObjects/`. Do not declare DTOs inside the controller file.
6. **Logic class** — which `Manage{Entity}` orchestrates this. Controllers should be thin: validate inputs, call logic, map errors to `HttpStatusCode`.

## Files to touch

- Either add a method to an existing controller, or create `Service/{LandingAPI|LandingAPIEnterprise}/Controllers/{Name}Controller.cs`.
- DTOs (if any) under `Service/SharedObjects/`.
- A controller test under `Service/LandingAPI/LandingAPI.Test/ControllerTest/` for the new method (delegate to the `add-xunit-test` skill).

## Endpoint template

Match the shape in [Controllers/ProductRumController.cs:29](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Controllers/ProductRumController.cs) (LandingAPI) or [LandingAPIEnterprise/Controllers/PropertyController.cs:78](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPIEnterprise/Controllers/PropertyController.cs) (LandingAPIEnterprise).

```csharp
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// {ProductName} product controller
    /// </summary>
    public class ProductContactCenterController : BaseApiController
    {
        /// <summary>
        /// {one-line summary — surfaces in Swagger}
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="datafilter">Data filter</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of ContactCenter users", Type = typeof(ListResponse))]
        [Route("products/contactcenter/users")]
        [HttpGet]
        [Authorize]               // or [AuthorizeScope("userinfoapi")] for LandingAPIEnterprise
        public HttpResponseMessage GetUsers(long editorPersonaId, [FromUri] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var logic = new ManageProductContactCenter(base._userClaims);
            var result = logic.GetUsers(editorPersonaId, datafilter);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
```

### Required attributes

- **All endpoints:** `[SwaggerResponse]` for at minimum `Unauthorized`, `BadRequest`, `InternalServerError`, and the success status with `Type = typeof(...)`. When you have a structured response DTO, also add `[SwaggerResponseExamples(typeof(MyDto), typeof(MyDtoExample))]` and create the example provider class.
- **Routes:** under `/api` (global prefix), use `[Route("...")]` per method. Do not use class-level `[RoutePrefix]` unless the existing controller already does.
- **Auth:** `[Authorize]` is the default; switch to `[AuthorizeScope("...")]` for `LandingAPIEnterprise` endpoints or any provisioning/export route.
- **CORS:** add `[AllowCors]` only where required — it is per-route, never global.

### Input validation

- Guard against default/empty `Guid` (`_realpageUserId == Guid.Empty`).
- Guard against `0` / `null` IDs.
- Return `HttpStatusCode.BadRequest` with a string message — match the existing pattern.

### Logic + claims

- Instantiate the logic class with `base._userClaims` (`DefaultUserClaim`). Do not extract claims manually inside the controller; `BaseApiController` already populates `_userClaims`, `_realpageUserId`, `_EnterpriseUserId`.
- Keep the controller free of business logic — orchestration happens in `Manage{Entity}`.

## Test constructor

Controllers expose a parameterless default constructor (so the ASP.NET pipeline can instantiate them) and a test constructor accepting any logic-interface mocks the test needs. See [Controllers/PasswordPolicyController.cs:27](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Controllers/PasswordPolicyController.cs) for an example with a single mocked dependency. For a controller that constructs its logic inside the method (like `ProductRumController`), the controller test typically stubs the static factories or relies on `RPObjectCache.BustCache()` plus mocked SPs (see `add-xunit-test` skill).

## After scaffolding — checklist

1. Add the new method's test (use the `add-xunit-test` skill).
2. Build `MasterProjectSolution.sln` and confirm Swagger renders the endpoint locally at `https://www-local2.realpage.com/api/swagger/ui/index`.
3. If this endpoint exposes provisioning or export, verify the `AuthorizeScope` matches the existing scope name used by sibling controllers — do not invent a new scope without checking IdentityServer config in `unified-login-core`.

## Important

- **Do not** add controller-level global error handlers — `BaseApiController` and Web.config already handle that.
- **Do not** wire CORS globally; use `[AllowCors]` per route.
- **Do not** leak DTOs from `Component.Landing.Repository.*` — return types should come from `Service/SharedObjects/`.
- **Do not** add a new HTTP route for a partner/enterprise consumer to `LandingAPI` — that surface is end-user; partner traffic lives in `LandingAPIEnterprise`.
