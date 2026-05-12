---
date: 2026-05-11T00:00:00-05:00
researcher: Jaswanth Bommu
git_commit: 8b2ea3b4c49008617a163cbee35e23375a08a2ce
branch: master
repository: unified-login-main
topic: "How user creation works and the areas from which a user can be created and updated"
tags: [research, codebase, user-lifecycle, manage-user, create-user, update-user, hots-clone, migration-tool, external-user, multi-company, batch-processor, kafka, audit]
status: complete
last_updated: 2026-05-11
last_updated_by: Jaswanth Bommu
---

# Research: User Creation & Update — How It Works and All Entry Points

**Date**: 2026-05-11
**Researcher**: Jaswanth Bommu
**Git Commit**: 8b2ea3b4c49008617a163cbee35e23375a08a2ce
**Branch**: master
**Repository**: unified-login-main

## Research Question
Explain how user creation works and the areas from where a user can be created and updated.

## Summary

A "user" in unified-login is a **composite entity spanning ~6 SQL tables in three schemas**: `Enterprise.Party` (root), `Person.Person` (1:1 with Party), `Ident.UserLogin` (credentials), `Ident.UserLoginPersona` (per-organization membership row, one per company), `Person.Persona` (per-company profile context), and `Enterprise.PartyRelationship` (the person↔organization link carrying the user type). Identity is keyed by a stable `RealPageId` GUID on the Party.

Creation is orchestrated by **`ManageUser.CreateUser`** (logic) → **`UserRepository.CreateUser`** (repository, runs the full DB transaction calling 10–15 SPs in sequence) → audit logs, registration email, and downstream product-batch enqueue all happen *after* the transaction commits. Two parallel SQL paths exist for the actual create:

1. **LandingAPI path** — `UserRepository.CreateUser` manages a fine-grained sequence of ~15 SPs inside its own `BeginTransaction()/Rollback()`. This is what the UI uses.
2. **Enterprise API path** — `Ident.CreateUser_Ver01` is a single monolithic SP that internally calls the same primitives. This is what `LandingAPIEnterprise/UserController` invokes via `migrationapi`/`enterpriseapi` scopes.

There are **at least 60 distinct API entry points** where user state can be created or mutated across `LandingAPI` and `LandingAPIEnterprise`. They fall into 8 categories: full create (8), profile/contact update (10), status (5), credential (10), persona/role/product assignment (8), invitation/notification (4), bulk/async (5), and webhook-triggered (1). Additionally there are non-API creation paths: **HOTS clone** (writes users directly bypassing `ManageUser`), **Migration tool** (forces `CreateUserSourceType=MigrationTool` and runs through the same SPs), and an **Azure AD lookup helper** (read-only, no current call sites). There is no open self-service signup — `POST /api/user` requires `createuser` right + `migrationapi`/`companyfunctions`/`rplandingapi` scope.

After the synchronous create commits, downstream propagation happens through **two async fan-out mechanisms**: (1) per-product `Batch.ProductBatch` rows picked up by the `BatchProcessor` Windows service's `RunPendingProcess` loop and dispatched via `ProcessExecutionFactory` to `ManageProductUser.CreateProductUser`, and (2) `UserStatusKafkaProducer.PublishUserStatusChangeEventAsync` fired from 5 specific call sites in `ManageUserLogin` and `UserRepository` when status flips active/inactive.

## Detailed Findings

### A. The User Data Model

**Six tables, three schemas, one identity** (`Database/Identity/`):

| Table | Path | What it stores | PK / link |
|---|---|---|---|
| `Enterprise.Party` | `Enterprise/Tables/Party.sql` | Root entity for any party (person or org). `RealPageId` UNIQUEIDENTIFIER is the stable cross-system ID. | `PartyId` BIGINT IDENTITY |
| `Person.Person` | `Person/Tables/Person.sql` | Person-specific fields (FirstName, MiddleName, LastName, Title, Suffix, PreferredContactMethodId). | Shared PK with `Party.PartyId` ON DELETE CASCADE |
| `Ident.UserLogin` | `Ident/Tables/UserLogin.sql` | Credentials and identity. `LoginName`, `PasswordHash`, `PasswordSalt`, `IdentityProviderTypeId`, `TwoFactorEnabled`, `CreateUserSourceId`. | `UserId` BIGINT IDENTITY; FK `PersonPartyId` → `Party.PartyId` |
| `Ident.UserLoginPersona` | `Ident/Tables/UserLoginPersona.sql` | **One row per (user × organization).** Carries the per-company status (`StatusTypeId`), `FromDate`/`ThruDate`/`StatusThruDate`/`LastLoginDate`/`UserDeactivationDate`, `PrimaryOrganization` BIT, `IsRPEmployee`, `IsRealPartner`, `IsDelegateAdmin`. | `UserLoginPersonaId` BIGINT IDENTITY; FKs to `UserLogin` + `Organization.PartyId` + `StatusType`; UNIQUE on `(UserLoginId, OrganizationPartyId)` |
| `Person.Persona` | `Person/Tables/Persona.sql` | Per-company persona attributes: `PersonaName`, `PersonaTypeId` (SuperUser=1, LeasingConsultant=2, Primary=3), `PersonaEnvironmentTypeId`, `IsDefault`, `FromDate`/`ThruDate`. | `PersonaId` BIGINT IDENTITY; logically linked to `UserLoginPersonaId` (no FK constraint) |
| `Enterprise.PartyRelationship` | `Enterprise/Tables/PartyRelationship.sql` | The person↔org link. `RoleTypeIdFrom` encodes the user type ("User", "SuperUser", "External User"). `ThruDate = NULL` means active. | `PartyRelationshipId` BIGINT IDENTITY |

Contact info lives in three subtype tables hanging off `Enterprise.ContactMechanism` via `Enterprise.PartyContactMechanism` (junction with effectivity): `Enterprise.ElectronicAddress` (email), telecommunication numbers, postal/street addresses.

Multi-company external-user metadata is in `Enterprise.ExternalUserRelationship` (PK = `UserLoginPersonaId`, FK to `Enterprise.ThirdPartyRelationship` + optional `Organization`, plus an `OperatorValue` pipe-delimited blob).

**Two well-known company GUIDs** declared as `static readonly` in `Service/SharedObjects/Landing/DefaultUserClaim.cs:14-15`:
- `EmployeeCompanyRealPageId = 0D018E46-C20E-477D-ADED-4E5A35FB8F99` — the RealPage employee virtual org. `UserLoginPersona.IsRPEmployee = 1` mirrors this at row level.
- `ExternalCompanyRealPageId = EEFACE50-9F75-4DCE-B133-A97EE0E0D723` — the "External Users" virtual container company. External users get a *second* `UserLoginPersona` linking them to this virtual org. Both GUIDs are seeded by `Scripts/PostDeployment/PostDeploymentScripts/August2020Release.sql:97-99` from `DataImportMapping.sourceid IN ('-1', '-2')`.

**Status taxonomy** (`Service/SharedObjects/Enum/UserStatusType.cs`, type `UserUiStatusType`):
`Active=1, Pending=2, Locked=3, Tainted=4, Unlocked=6, Error=7, AccountCreationSuccessful=8, AccountHidden=10, ForceResetPassword=12, Deactivated=19, Expired=23, Disabled=24`. The older `UserLoginStatusType` enum in the same folder is fully commented out — `UserUiStatusType` is the live one.

**No SQL temporal versioning** is used on any user-related table. Effectivity is entirely application-managed via `FromDate`/`ThruDate`. The only `*History` table for user data is `Ident.PasswordHistory` (written by `Ident.UpdateUserLogin` before each password change).

### B. The CreateUser Orchestration

#### Entry: `ManageUser.CreateUser` (`Component/Landing/Logic/ManageUser.cs:259-366`)

Signature: `CreateUserResponse<IErrorData> CreateUser(ProfileDetail profile, IList<Persona> persona, bool createdByEnterpriseAPI = false)`.

The orchestration order:

1. **Clone pre-fetch** (`:261-267`) — if `profile.ClonedUser`, captures `cloneUserPersonaId` + `cloneUserRealpageId` because the repo will overwrite them.
2. **Debug log** (`:268`) — `WriteToLog` records the login name.
3. **Repository call** (`:269`) — `_userRepository.CreateUser(profile, persona)`. The DB transaction lives here. Returns `CreateUserResponse<IErrorData>` with `Status.Success`.
4. **Success gate** (`:272`) — everything below only fires if `response.Status.Success == true`.
5. **Audit-message construction** (`:275-309`) — source-type branching (see §C).
6. **Audit log** (`:311`) — `LogAuditActivity(LogActivityTypeConstants.CREATE_USER /* "Create user" */, LogActivityCategoryType.User, ...)`.
7. **Email gate** (`:315`) — only if `profile.userLogin.FromDate.Subtract(UtcNow).TotalMinutes <= 15` AND `!profile.userLogin.doNotForceChangePassword`. Future-dated users do **not** get the welcome email at creation time.
8. **Re-fetch user** (`:317`) — `_userLoginRepository.GetUserLoginOnly(LoginName)` to feed the email service.
9. **Email send** (`:318`) — `_manageUserRegistrationEmail.SendNewUserRegistrationEmail(userLoginOnly, orgName, userTypeId, orgPartyId)`.
10. **Email audit** (`:323-337`) — conditional on `UserTypeId != RegularUserNoEmail && !Is3rdPartyIDP`. Fires `EMAIL_SENT` ("Email sent") or `EMAIL_RESENT` ("Email resent").
11. **Delegate admin path** (`:341-362`) — if `IsDelegateAdmin`, fires a second `UPDATE_USER` audit with "User admin has added X as Delegate admin" message; if `DelegateRoleTemplate.RoleTemplateId.Any()`, fires a third audit naming the templates.
12. **Return** (`:365`) — the original `CreateUserResponse` unchanged.

#### `UserRepository.CreateUser` (`Component/Landing/Repository/UserRepository.cs:220+`)

This is where the **full DB transaction** runs. It does NOT use `Ident.CreateUser_Ver01`. Instead it manages the sequence directly using `repository.UnitOfWork.BeginTransaction()` / `Rollback()` (no `TransactionScope`). Every SP call that returns `Id == 0` triggers an explicit `Rollback()` and early return — there is no compensating-rollback logic beyond the connection-scope transaction.

Happy-path sequence:

1. `Person.CreatePerson` — inserts `Enterprise.Party` (gets `PartyId` + `RealPageId` via OUTPUT) then `Person.Person`.
2. `Ident.CreateUserLogin` — creates `UserLogin` with the resolved `CreateUserSourceId` from `Enterprise.CreateUserSourceType`.
3. `Ident.UpdateUserLogin` — sets password hash/salt + `FromDate`/`ThruDate` on the persona row if password supplied. (Also writes old hash to `PasswordHistory` if present.)
4. `UpdateProfile` (internal) — if phone numbers present, `Person.CreateContactMechanism` → `LinkContactMechanismToParty` → `CreateTelecommunicationsNumber`.
5. `Person.CreateContactMechanism` — for the email contact mechanism.
6. `Person.LinkContactMechanismToParty` — links it with effectivity dates.
7. `Person.LinkUsageTypeToPartyContactMechanism` — tags as "Email".
8. `Person.CreateElectronicAddress` — upserts the email string. **Called twice** if `LoginName != NotificationEmail` (separate values for login + notification address).

Then in a loop over `organizationList` (one for normal users, two for external users — the second is the External-Users virtual org):

9. `Ident.CreateUserLoginPersona` — sets per-company status (Pending=2 / Active=1 / Disabled=24 based on IDP type + from-date window) and `StatusThruDate = FromDate + ActivityConfiguration NewUserRegistration window` (default 72 h).
10. `Person.CreatePersona` — also creates a `MasterConfiguration` + `MasterConfigurationSetting` for ThemeColor and TimeZone.
11. `Enterprise.UpdateExternalUserRelationship` — upserts into `ExternalUserRelationship` (gated by `FeatureFlag.GetUserCompanyAssociationFeatureFlag()`).
12. `Security.InsertUpdateRoleTemplateUserMapping` — if enterprise role specified.
13. `Security.LinkPersonaToRole` — links persona to a UnifiedPlatform role (Platform Admin / org default / Basic End User).
14. `Enterprise.AddUpdatePropertyMapping` or `Enterprise.AddUpdatePropertyInstanceMapping` — top-level property assignments.
15. `Enterprise.CreateUserEmployeeId` — if employee ID provided.
16. `Enterprise.InsertUpdateSuperVisor` — if supervisor specified.
17. `Person.LinkPersonToOrganization` — called **twice**: once with the user-type role + "User Type" relationship, writing to `Enterprise.PartyRelationship`.
18. `Enterprise.CreateCommunicationEvent` — pending email row (only for local-IDP users).
19. `Settings.AddUpdateFieldValue` — custom fields.

If any SP returns `Id == 0`, the entire connection-scoped transaction is rolled back. The activity log, registration email, and delegate-role audit all fire **outside** this transaction in `ManageUser.cs` after the repo returns success.

#### Alternate: `Ident.CreateUser_Ver01` (Enterprise API monolith)

Single SP at `Database/Identity/Ident/Stored Procedures/CreateUser_Ver01.sql`. Used by `LandingAPIEnterprise/UserController.CreateUser`. Internally it performs steps 1–17 above with its own nested transactions per inner EXEC. The C# wrapper at `EnterpriseStoredProcNameConstants.cs:429` is `Ident.CreateUser_Ver01`.

### C. `CreateUserSourceType` taxonomy

Enum at `Service/SharedObjects/Enum/CreateUserSourceType.cs`:

| Value | Int | Audit message branch |
|---|---|---|
| `UnifiedPlatform` | 24 | Default normal-UI path; context-sensitive message (ClonedUser, RP employee, EnterpriseAPI use, regular) |
| `ExcelImport` | 25 | Generic message: `"New User {0} {1} successfully created by {3}."` with `{3}="ExcelImport"` |
| `MigrationTool` | 26 | Same generic message; forced via `if (profile.MigratedUser) profile.CreateUserSourceType = CreateUserSourceType.MigrationTool` (`ManageUser.cs:278`) and re-forced inside `UserRepository.CreateUser:479-482` |
| `RPX` | 27 | Same generic message |

The audit message construction at `ManageUser.cs:275-309` branches on these values, producing strings like:
- ClonedUser → includes source person's name
- RP employee company → `"by RealPage user {2}"`
- `createdByEnterpriseAPI == true` → `"using enterprise API"` suffix
- Else → `"by user {2}"`

### D. API Entry Points — Inventory

Below is the canonical list. Auth scope is omitted when `default [Authorize]`.

#### 1. Full user create

| Route | File:line | Manage class | Auth |
|---|---|---|---|
| `POST /api/user` | `LandingAPI/UserController.cs:697` | `ManageUser.CreateUser` | `[AuthorizeRight("createuser","cloneuser")]` + `[AuthorizeScope("companyfunctions", "rplandingapi", "migrationapi")]` |
| `POST /apienterprise/user` | `LandingAPIEnterprise/UserController.cs:165` | `ManageUser.CreateUser` (via `UserManagement`) | `[AuthorizeScope("enterpriseapi")]` (with `upfmId` client-cred path) |
| `POST /api/persons` | `LandingAPI/PersonController.cs:56` | `ManagePerson.CreatePerson` | default |
| `POST /api/userlogins/{realPageId}` | `LandingAPI/UserLoginController.cs:78` | `ManageUserLogin.CreateUserLogin` | default |
| `POST /api/persona` | `LandingAPI/PersonaController.cs:76` | `ManagePersona.CreatePersona` | default |
| `POST /api/newuser/profile` | `LandingAPI/UserController.cs:977` | `ManageUser.UpdateNewUser` | `[AllowAnonymous]` — token-gated, completes invitation |
| `POST /api/user/setstarterprofile` | `LandingAPI/UserController.cs:669` | `ManageUser.SetStarterProfile` | `[AllowAnonymous]` |
| `POST /apienterprise/userclone` | `LandingAPIEnterprise/HotsUserCloneController.cs:99` | `ManageHotsCloneUsers` | `usermanagement` scope inline |

#### 2. Profile / contact-info updates

| Route | File:line | Manage class |
|---|---|---|
| `PUT /api/user` | `LandingAPI/UserController.cs:397` | `ManageUser.UpdateUser` (+ `ManageUserLogin`, `ManageRoleType`, `ManagePersona`, `ManageOrganization`, `ManageProduct`) — `[AuthorizeRight("editusers","editotherprofile","editownprofile")]` |
| `PUT /apienterprise/user` | `LandingAPIEnterprise/UserController.cs:374` | `ManageUser.UpdateUser` (via `UserManagement`) |
| `PUT /api/persons/{realPageId}` | `LandingAPI/PersonController.cs:138` | `ManagePerson.UpdatePerson` |
| `PUT /api/profiles/{realPageId}` | `LandingAPI/ProfileController.cs:279` | `ManageProfile.UpdateProfile` — `[AuthorizeRight("editotherprofile","editownprofile")]` |
| `PUT /api/userlogins/{realPageId}` | `LandingAPI/UserLoginController.cs:187` | `ManageUserLogin.UpdateUserLogin` |
| `POST,PUT /api/persons/{realPageId}/electronicaddress` | `LandingAPI/ElectronicAddressController.cs:60,134` | `ManageContactMechanism`, `ManageElectronicAddress` |
| `POST,PUT /api/persons/{realPageId}/telecommunicationnumber` | `LandingAPI/TelecommunicationNumberController.cs:60,134` | `ManageContactMechanism`, `ManageTelecommunicationNumber` |
| `POST,PUT /api/persons/{realPageId}/postaladdress` | `LandingAPI/PostalAddressController.cs:61,149` | `ManageContactMechanism`, `ManageStreetAddress`, `ManageGeographicBoundary` |

#### 3. Status changes

| Route | File:line | Behavior |
|---|---|---|
| `POST /api/userlogin/status` | `LandingAPI/UserLoginController.cs:573` | Single-user toggle: Active/Disabled/Locked/Unlocked. `[AuthorizeRight("lockunlockusers","activatedeactivateusers")]`. Delegates to `ManageUserLogin.CreateUpdateUserStatus` then `ManageUser.UpdateUserStatus`. |
| `PATCH /api/userlogins` | `LandingAPI/UserLoginController.cs:236` | Bulk status update across a batch or all users in org; filters out the calling user. |
| `POST /apienterprise/user/status/{unityRealPageUserId}` | `LandingAPIEnterprise/UserController.cs:553` | Enterprise activate/deactivate via `UserManagement.ActivateDeactivateUser`. |
| `POST /api/disableexpiredusers` | `LandingAPI/UserLoginController.cs:615` | **`[AllowAnonymous]`** — called by `UserNotification` WinService. `ManageUser.DisableUsersFromProducts`. |
| `POST /api/userlogins/processfutureuserlogins` | `LandingAPI/UserLoginController.cs:517` | **`[AllowAnonymous]`** — called by `UserNotification` WinService. Activates users whose `FromDate` has arrived. |

`ManageUserLogin.CreateUpdateUserStatus` (`:297`) is the single entry for all status transitions. Side effects:
- `Disabled (24)` → `Kafka: is_active=false` (`:465-471`), audit `LOGIN_DISABLED`, downstream product-batch enqueue via `UserRepository.UpdateUserStatusByCompany:6076` → `SP_ListPersonaToDisableUserProduct` → per-product `SP_CreateProductBatch`.
- `Active (1 or Pending 2)` → `Kafka: is_active=true`, audit `LOGIN_ENABLED`, welcome email re-sent if user never logged in and not 3rd-party IDP.
- `Locked (3)` → audit `USER_LOCKED`. No Kafka, no email.
- `Unlocked` → `_credentialRepository.UpdateUserActivityAttempts` resets attempt counter, audit `USER_UNLOCKED`.
- `Expired (23)` → audit `USER_EXPIRED`.

#### 4. Credential operations

| Route | File:line | Auth | Manage class |
|---|---|---|---|
| `POST /api/credential/forgotpassword` | `CredentialController.cs:163` | `[AllowAnonymous]` | `ManageCredential.ForgotPassword` |
| `POST /api/credential/setpassword` | `CredentialController.cs:381` | `[AllowAnonymous]` | `ManageCredential.SetPassword` |
| `POST /api/credential/resetpassword` | `CredentialController.cs:459` | `[Authorize]` | `ManageCredential.ResetPassword` |
| `POST /api/credential/settemporarypassword` | `CredentialController.cs:531` | `[Authorize]` | `ManageCredential.SetTemporaryPassword` (admin sets temp pw for target user) |
| `POST /api/credential/verifysecurityanswers` | `CredentialController.cs:125` | `[AllowAnonymous]` | `ManageCredential.VerifySecurityAnswers` |
| `POST /api/credential/setusersecurityquestions` | `CredentialController.cs:422` | `[AllowAnonymous]` | `ManageCredential.SetUserSecurityQuestions` |
| `POST /api/credential/userselectedsecurityquestions` | `CredentialController.cs:656` | `[Authorize]` | `ManageCredential.SaveUserSelectedSecurityQuestions` |
| `PUT /api/userlogins/clearpasswordandquestions` | `UserLoginController.cs:487` | `[AuthorizeRight("resendinvitation")]` | `ManageUserLogin.ClearPasswordAndQuestions` |
| `PUT /api/multifactorauth/appauth/user/{realPageId}` | `MultiFactorAuthController.cs:53` | `[AuthorizeScope("rplandingapi")]` | `TwoFactorLogic.UpdateUserTwoFactorStatus` |
| `DELETE /api/multifactorauth/appauth/user/{realPageId}/token` | `MultiFactorAuthController.cs:77` | `[AuthorizeScope("rplandingapi")]` | `TwoFactorLogic.DeleteUserAppAuthToken` + reset to pending |

`Ident.UpdateUserLogin` writes the old hash to `Ident.PasswordHistory` automatically before each password change (no separate C# call).

#### 5. Persona / role / product assignment

| Route | File:line | Manage class |
|---|---|---|
| `POST /api/user/assignproductstoadministrators` | `UserController.cs:53` | `ManageUser.AssignProductsToAdministrators` |
| `PUT /api/personas/products/{productId}/productSettings` | `PersonaController.cs:290` | `ManageProduct.UpdateProductSetting` |
| `PUT /api/productuser/details` | `ProductUserController.cs:71` | `ManageProductUser.UpdateProductUserAccountDetails` |
| `POST /api/productuser/user` | `ProductUserController.cs:44` | `ManageProductUser.CreateProductUser` — `[AllowAnonymous]` (WinService internal) |
| `DELETE /api/productuser/details` | `ProductUserController.cs:98` | `ManageProductUser.DeleteSamlUserProductInfoAndStatus` |
| `PUT,DELETE /apienterprise/user/productuser/details` | `LandingAPIEnterprise/UserController.cs:1235,1264` | `ManageProductUser.UpdateProductUserAccountDetails` / `DeleteSamlUserProductInfoAndStatus` — `[AuthorizeScope("internalapi")]` |
| `GET /api/employeeaccess/company/{companyRealPageId}/persona` | `EmployeeAccessController.cs:70` | `ManageEmployeeAccess.GetOrCreateEmployeePersonaId` — **side-effecting GET** that creates a persona for an RP employee |
| `POST /api/employeeaccess/product/{productId}/persona/{personaId}` | `EmployeeAccessController.cs:88` | `ManageEmployeeAccess.CreateEmployeeProductUser` |

#### 6. Invitation / notification

| Route | File:line | Behavior |
|---|---|---|
| `POST /api/userlogins/resendinvitation` | `UserLoginController.cs:396` | `[AuthorizeRight("resendinvitation")]` → `ManageUserLogin.ResendInvitation(sendAsWelcome:false)` |
| `POST /api/userlogins/resendinvitationexternal/{realpageId}` | `UserLoginController.cs:428` | `[AuthorizeScope("enterpriseapi")]` — synthesizes claims for callers without `sub` |
| `POST /api/usernotification/sendwelcomeemail` | `UserNotificationController.cs:37` | `[AllowAnonymous]` (Kafka-driven via `UserNotification` WinService) → `ManageUserLogin.ResendInvitation(sendAsWelcome:true)` |
| `POST /api/userlogins/bulk-update-idp/{isEnabled}` | `UserLoginController.cs:708` | `[AuthorizeRight("editusers")]` → `ManageUser.ThirdPartyIdpBulkUpdate` (SP `Ident.UpdateUsersIDP`) |

#### 7. Bulk / async / batch

| Route | File:line | Auth | Behavior |
|---|---|---|---|
| `POST /api/batchprocessor` | `BatchProcessController.cs:31` | `[AllowAnonymous]` | `BatchProcessorLogic.ProcessBatch` — receives a single `ProductBatch` from the BatchProcessor WinService, dispatches via `ProcessExecutionFactory` to one of: `CreateUpdateProductUser`, `UpdateProductUserProfile`, `ChangeProductUserType`, `EnterpriseCreateUpdateProductUser`. Comment in the controller says "TODO: Make it authorize by having client id for Windows Service in ID server." |
| `POST /api/erpbatchprocessor` | `BatchProcessController.cs:58` | `[AllowAnonymous]` | `ManageEnterpriseRoleProductBatch.GenerateEnterpriseRoleUserProductBatch` |
| `POST /api/ppbatchprocessor` | `BatchProcessController.cs:83` | `[AllowAnonymous]` | `ManagePrimaryPropertiesBatch.GeneratePrimaryPropertiesUserProductBatch` |
| `POST /api/bulkuserbatchprocessor` | `BatchProcessController.cs:108` | `[AllowAnonymous]` | `ManageBulkUserBatch.GenerateProductUnAssignProductBatch` (`Component/Landing/Logic/ManageBulkUserBatch.cs:57`) |
| `POST /api/userpropertiessync` | `UserPropertiesSyncController.cs:64-66` | `internalapi` scope inline | `ManageUserPropertiesSync.TranslateAndSaveUserProductProperties` — reads user's product properties, translates UPFM → product property instance IDs via `ManageBlueBook.GetTranslatePropertiesFromUPFMToProductv3`, calls `PropertyRepository.StageUserProductPrimaryProperties` |

#### 8. Webhook

| Route | File:line | Behavior |
|---|---|---|
| `POST /api/webhook/books` | `WebHookController.cs:101` | `[AllowAnonymous]` with HMAC-SHA256 signature against `TibcoWebHookSigningSecret`. Handles `books.customerproperty.*`, `books.customercompany.*`, and `provisioning.upfmorder/upfmclone.create`. The clone-create event triggers HOTS user cloning. **Does not create or update users directly** — only org/property topology and HOTS trigger. |

#### Password policy (affects user auth behavior)

| Route | File:line | Behavior |
|---|---|---|
| `POST /api/passwordpolicies` | `PasswordPolicyController.cs:43` | `ManagePasswordPolicy.CreatePasswordPolicy` (org-scoped, governs all users in it) |
| `PUT /api/passwordpolicies` | `PasswordPolicyController.cs:127` | `ManagePasswordPolicy.UpdatePasswordPolicy` |

### E. Special / Non-Standard Creation Paths

**1. HOTS user clone** (`Component/Landing/Logic/ManageHotsCloneUsers.cs`).

Entry: `POST /apienterprise/userclone` → `HotsUserCloneController.HOTCloneUsers` (`:99`), gated by inline `usermanagement` scope claim. Resolves the clone-company admin user via `ManageOrganization.GetOrganizationAdminUserRealPageId` and calls `RecreateClaimsForClient` (`:199`) to reconstruct a `DefaultUserClaim` from scratch.

`ManageHotsCloneUsers.CloneUsersFromBaseLineCompany` (`:75`) is gated by `ProductInternalSetting.IsCloneUsersProcessEnabledForHOTS`. The flow:
1. `Hots.ListHotsBaseOrganizationUsers` → all active non-admin users in baseline org.
2. Per user: `ManageProfile.GetProfileDetail` + `CheckIfUserAlreadyExists` (login name = `{FirstName}{LastName}{clonePartyId}@realpage.com`).
3. Fetch product batch data via `ManageCloneProductBatch.GetUserProductBatchData`.
4. `CompareBaseAndCloneProductProperties` and `CompareBaseAndCloneProductRoles` — match by name, substitute clone-company IDs.
5. Generate random 10-char password via `Membership.GeneratePassword(10,1)` and hash.
6. **`HOTSCloneUserRepository.CreateUser`** — the actual write, **bypassing `ManageUser.CreateUser` entirely**.

After all users are created, `CheckUsersProductStatus` polls (max 5 retries × 2s) until product provisioning reaches status `8` (complete), then `PostToHOTS` fires a bearer-token authenticated POST callback to HOTS.

`WebHookController.PostBooks` provides a parallel HOTS trigger via the `provisioning.upfmclone.create` event (`:234`) — validates the baseline company GUID from the payload and invokes `ManageHotsCloneUsers`.

SQL schema objects: `Hots.ListHotsBaseOrganizationUsers`, `Hots.InsertHotsCompanyRelationship`, `Hots.InsertHotsPropertyRelationship`, `Hots.GetBaseCompanyRealpageId`, `Hots.UpdateHotsCloneUserPassword`. Tables: `Hots.CompanyRelationship`, `Hots.PropertyRelationship`.

**2. Migration tool path** (`Database/MigrationTool/Migration.sqlproj`).

A separate staging database with:
- `Staging.ProductUser` — source product user records from the migrating system
- `Staging.UserMapping` — maps `ProductUserId` → `MergedUserId`
- `Ident.MergedUser` — canonical merged-user record (`MergedUserId`, `LoginName`, `DefaultPassword`, `RealPageId`, `UserType`, `StatusId`, `ActivityTypeId`)
- `Staging.PropertyMapping` — property mappings
- SP `Staging.ListProductUsersByOrganizationId`

Flow: `ProfileDetail.MigratedUser` bool set by caller → in `ManageUser.CreateUser:278` force-overwrites `profile.CreateUserSourceType = CreateUserSourceType.MigrationTool` → in `UserRepository.CreateUser:479-482` re-forces the string `"MigrationTool"` before calling the user creation SP. The `migrationapi` scope on `POST /api/user` is the dedicated token scope for migration callers.

**3. External user / Multi-company (PR 406991, "Dev Only")**

Working-tree change at `ManageUser.cs:751-752` adds two flags to the persona in `GetProfile`:
```csharp
persona.hasMultiCompany = userPersonaOrganizationList.Count > 1;
persona.hasMultiPersona = personaList.Count > 1;
```
Plus `:735` sets `profileDetail.hasExistsInExternalUsersCompany` by checking against `ExternalCompanyRealPageId`. Behind `FeatureFlag.GetUserCompanyAssociationFeatureFlag()`, lines `:784-792` call `GetExternalUserRelationship` (SP `Enterprise.GetExternalUserRelationship`) and attach the result.

The `ExternalUserRelationship` shape (`Service/SharedObjects/Landing/ExternalUserRelationship.cs:5-21`): `UserLoginPersonaId`, `ThirdPartyRelationShipId`, `ThirdPartyRelationShip`, `ThirdPartyCompanyName`, `ThirdPartyCompanyRealPageId`, `OperatorCode`, `OperatorValue`. The "claim my user" feature in the PR title is **read-path enrichment only** in this repo — it surfaces multi-company/multi-persona indicators to the Angular UI which presents the "claim" UX. No new create endpoint was added.

**4. Azure AD user lookup** (`Component/Landing/Logic/ManageMicrosoftAzure.cs`)

A single method `GetADUserInfo(string userName)` (`:59`) that:
1. Obtains a client-credentials bearer token from `AzureTokenAddress + /token` using `AzureUnifiedLoginUserClientId/Secret/Scopes` from `ProductInternalSetting`.
2. Calls Microsoft Graph `/v1.0/users?$filter=userPrincipalName eq '{userName}'`.
3. Returns an `AzureUser` shape after validating UPN match.

**Read-only. No call sites in this repo.** The `AzureAuth` route in route config is in `unified-login-core` (the IdentityServer repo), not here. `HomeController.AzureAuth` does not exist in this repo's MVC project.

**5. Self-service registration**

No open public-signup endpoint exists. `POST /api/user` requires `createuser` right + `companyfunctions`/`rplandingapi`/`migrationapi` scope. The closest are the `[AllowAnonymous]` token-gated endpoints that complete a pre-created account:
- `GET user/validate` (`:598`) — validates registration token; activates pre-created account.
- `GET user/validate-token` (`:625`) — verifies the registration verification token.
- `POST user/setstarterprofile` (`:668`) — sets starter profile.
- `POST newuser/profile` (`:976`) — fills in profile (phone/job title/role) using `activityToken`.

### F. Update Flows by Category

#### Profile updates
- `ManageUser.UpdateUser(loggedInRealPageId, profile)` (`:414`): captures pre-edit snapshot via `GetUserProfile`, calls `UserRepository.UpdateUser:2564 → UpdateUserData:6314` which opens its own `BeginTransaction()` and calls (conditionally): `Person.UpdatePerson`, `Ident.LinkIdentityProviderToUserLogin`, `Ident.UpdateUserLoginPersona`, `Ident.UpdateUserLogin` (which writes to `PasswordHistory` if password changed), `Settings.AddUpdateFieldValue` (custom fields), `Ident.UpdateUserStatusByCompany`/`UpdateBulkUserStatus` for status. Re-activation path (`:424-430`) sets `FromDate = UtcNow` and flags `sendNotification`.
- `ChangeUserTypeExternal` (internal to `UpdateUser`): handles transitions to/from External via `Person.LinkPersonToOrganization` / `UnlinkPersonToOrganization` / `Ident.CreateUserLoginPersona` / `Person.CreatePersona`.

#### Persona updates
- `ManagePersona.UpdateActivePersona` (`:326`) → `Person.UpdateActivePersona` — switches which persona is active.
- Persona content updates run inside `UserRepository.UpdateUser:2161-2216`, three `SP_UpdatePersona` calls under different conditions.
- `ManagePersona.ChangeCompanyNotification` (`:335`) — sends Unified Notifications event, no DB write.

#### User-property + custom-fields updates
- `ManageUserProperty.GetAssignedPropertyForPersona` (`:20`) — read only. Property assignment goes through the product batch system.
- `ManageUserPropertiesSync.TranslateAndSaveUserProductProperties` (`:70`) — async, translates UPFM property IDs to product property instance IDs via Books, calls `PropertyRepository.StageUserProductPrimaryProperties` / `DeleteStagedUserProductPrimaryProperties`. Verbosity gated by `Elk_LogManageUserPropertiesSync` setting.
- `ManageCustomFields.AddUpdateFieldValue` (`:57`) — single SP-call upsert; values keyed on `UserLoginPersonaId`.

#### `UserRepository` Update methods
`UpdateNewUser` (`Ident.UpdateUserLogin` + `Person.UpdatePerson` + `Ident.UpdateUserStatusByCompany`), `UpdateUserListUser:1831` (multi-persona role changes), `UpdateUserLogin`, `UpdateUser:2564` (composite), `ThirdPartyIdpBulkUpdate` (`Ident.UpdateUsersIDP`), `UpdateUserEmployeeId`, `UpdateUserStatusByCompany`, `UpdateUserActivityAttempts` (`Ident.UpdateActivityAttempt`), `AddUpdateEmployeeProductADGroupMapping`.

### G. Audit Activity Types (constants in `Service/SharedObjects/Constants/LogActivityTypeConstants.cs`)

User-lifecycle audit constants (lines 9–41):
- `CREATE_USER = "Create user"`, `UPDATE_USER = "Update user"`, `CLONE_USER = "Clone user"`, `USER_UPDATE_INTERNAL = "User Update - Internal"`
- `LOGIN_ENABLED`, `LOGIN_DISABLED`, `USER_LOCKED`, `USER_UNLOCKED`, `USER_EXPIRED`
- `CHANGE_PASSWORD_SUCCESS/FAILURE`, `CHANGE_SECURITY_QUESTIONS_SUCCESS/FAILURE`
- `EMAIL_SENT`, `EMAIL_RESENT`, `EMAIL_RESETPASSWORDSENT = "Reset Password Email"`
- `PRODUCT_ACCESS`, `ENTERPRISE_ROLES`, `ROLES_RIGHTS`, `PRIMARY_PROPERTIES`
- `USER_REQUESTED_NEW_ACTIVATION_LINK`

`TwoFactorLogic` writes `"Update User"` as a **raw string literal** at `:65, 83`, not via the constant — drift risk.

### H. Email Notifications

Two production templates, three send paths:

1. **Welcome / new-user registration / re-invitation** — `ManageUserRegistrationEmail.SendNewUserRegistrationEmail`. Triggered from `ManageUser.CreateUser:315-318` (when `FromDate ≤ UtcNow + 15min` AND `!doNotForceChangePassword`), and from `ManageUserLogin.cs:392` on reactivation when user never logged in.
2. **Password reset** — `ManageUserRegistrationEmail.SendPasswordResetEmail` (`ManageUserLogin.cs:658`); `CommunicationEventPurposeType.PasswordReset`, embeds `ActivityType.ForgotPassword` token.

Send path picks SendGrid vs Unified Email vs CES SOAP via `ProductInternalSetting` flags (`IsSendGridEnabled`, `IsUnifiedEmailEnabled`); outcome persisted via `ManageCommunicationEvents.CreateCommunicationEvent` with `EmailStatusType.EmailSuccess` / `EmailError`.

**No** "account locked"/"account unlocked"/"profile changed" emails. Lock/unlock produce audit only.

### I. Kafka — `UserStatusKafkaProducer.PublishUserStatusChangeEventAsync`

Five call sites publish `UnifiedLoginUserStatusEvent` to the `UserStatus` Kafka topic (`Web.config:45`, dev value `unified-login-user-status-dev`):
- `ManageUserLogin.cs:465` — status flip to Disabled in `CreateUpdateUserStatus`
- `ManageUserLogin.cs:964` — within bulk status update
- `ManageUserLogin.cs:1350` — within lock/unlock/expire path
- `UserRepository.cs:2887` — inside `UpdateUserStatusByCompany`
- `UserRepository.cs:6604` — inside the main `UpdateUserData` save when status changed

All five sites publish with `is_active: bool` derived from the new status.

### J. Async Propagation to Downstream Products

After the synchronous create/update commits, downstream sync happens through three async fan-out mechanisms:

1. **`Batch.ProductBatch` rows** — written by `SP_CreateProductBatch` inside the create/update transaction. Picked up by `BatchProcessor.RunPendingProcess` (or `RunRetryProcess`) → `ProcessBatch` → `ProcessExecutionFactory` mapping by `BatchProcessType`:

| BatchProcessType | Int | → ProcessExecution | → Manage method |
|---|---|---|---|
| `CreateUpdateProductUser` | 1 | `CreateUpdateProductUser` | `ManageProductUser.CreateProductUser` |
| `ProfileUpdate` | 2 | `UpdateProductUserProfile` | `ManageProductUser.UpdateProductUserProfile` |
| `DeactivateProductUser` | 3 | `DeactivateProductUser` | **throws `NotImplementedException`** |
| `UserTypeRegularToAdmin/AdminToRegular/AdminToExternal/ExternalToAdmin` | 5/6/9/8 | `ChangeProductUserType` | `ManageProductUser.ChangeUserType` |
| `EnterpriseRoleCreateUpdateProductUser` | 10 | `EnterpriseCreateUpdateProductUser` | `ManageProductUser.CreateEnterpriseRoleProductUser` |
| `PrimaryPropertiesUpdateProductUser` | 14 | `CreateUpdateProductUser` | `ManageProductUser.CreateProductUser` |

2. **Dedicated batch tables** for enterprise role + primary property changes (`Batch.EnterpriseRoleBatchProcess`, `Batch.PrimaryPropertiesBatchProcess`) processed by `RunEnterpriseRoleUpdateProcess` / `RunPrimaryPropertiesUpdateProcess` — covered in detail in `thoughts/shared/research/2026-05-11-enterprise-roles-primary-properties.md`.

3. **Bulk user batch table** (`Batch.BulkUserBatch*`) — `RunBulkUserUpdateProcess` polls and POSTs to `/api/bulkuserbatchprocessor`, which calls `ManageBulkUserBatch.GenerateProductUnAssignProductBatch` (`:57`). This path is specifically about bulk **product unassignment** for a list of users.

4. **`UserNotification` WinService** runs three timer-driven jobs (`UserNotificationService.cs`):
   - `SendRegularUserNotification` — every tick where `Minute % 15 == 0`, SP `Ident.ListFutureLogins`. POSTs back to LandingAPI for resend-welcome emails.
   - `ProcessPendingUsers` — same cadence, SP `Ident.ListPendingUsers`.
   - `ProcessDisableUsersinProducts` — daily at `AppSettings["ScheduledTime"]` UTC, SP `Ident.ListExpiringUsers`, POSTs `/api/disableexpiredusers`. All three gated by LD flag `use-core-api-v2-for-service`.

### K. Anonymous-but-mutating Endpoints (security boundary)

The following routes are `[AllowAnonymous]` and mutate user state — they rely on token validation or HMAC inside the handler rather than IdentityServer scope:

| Route | Trust mechanism |
|---|---|
| `POST /api/batchprocessor`, `/erpbatchprocessor`, `/ppbatchprocessor`, `/bulkuserbatchprocessor` | None beyond null-check. Comment: `//TODO: Make it authorize by having client id for Windows Service in ID server` |
| `POST /api/usernotification/sendwelcomeemail` | None beyond `userLogins.Count > 0` |
| `POST /api/disableexpiredusers`, `/userlogins/processfutureuserlogins` | None — called by `UserNotification` WinService |
| `POST /api/webhook/books` | HMAC-SHA256 against `TibcoWebHookSigningSecret` |
| `POST /api/credential/forgotpassword` | Validates two activity tokens (ForgotPassword + VerifyAnswers) |
| `POST /api/credential/setpassword`, `verifysecurityanswers`, `setusersecurityquestions`, `getsecurityquestions` | Internal validation in `ManageCredential` against existing user state / tokens |
| `POST /api/user/newuser/profile`, `setstarterprofile`, `validate`, `validate-token` | Activity token from email |

---

## Code References

### Core orchestration
- `Component/Landing/Logic/ManageUser.cs:259-366` — `CreateUser`
- `Component/Landing/Logic/ManageUser.cs:414-475` — `UpdateUser`
- `Component/Landing/Logic/ManageUser.cs:96-173` — `ValidateUser`
- `Component/Landing/Logic/ManageUser.cs:170` — sole `TODO` (`throw new Exception("Unable to get token"); // TODO; research on this`)
- `Component/Landing/Logic/ManageUser.cs:481-501` — `UpdateUserStatus` + `DisableUsersFromProducts`
- `Component/Landing/Logic/ManageUser.cs:672-818` — `GetUserProfile` (with working-tree change at 735, 751-752, 784-792 for multi-company / external user)
- `Component/Landing/Logic/ManageUser.cs:879-895` — `WriteToLog` helper
- `Component/Landing/Repository/UserRepository.cs:220+` — `CreateUser` transactional sequence
- `Component/Landing/Repository/UserRepository.cs:2564,6314` — `UpdateUser` + `UpdateUserData`
- `Component/Landing/Repository/UserRepository.cs:2887,6604` — Kafka publish call sites
- `Component/Landing/Logic/ManageUserLogin.cs:297` — `CreateUpdateUserStatus` (all status transitions)
- `Component/Landing/Logic/ManageUserLogin.cs:465,964,1350` — Kafka publish call sites
- `Component/Landing/Logic/ManageCredential.cs:163,345,658,772` — credential operations
- `Component/Landing/Logic/TwoFactorLogic.cs:30,47` — 2FA token + status
- `Component/Landing/Logic/ManageHotsCloneUsers.cs:75-310` — clone flow + login-name composition
- `Component/Landing/Logic/ManageMicrosoftAzure.cs:59-75` — Azure AD lookup (unused)
- `Component/Landing/Logic/ManageBulkUserBatch.cs:57` — bulk unassign batch
- `Component/Landing/Logic/Messaging/UserStatusKafkaProducer.cs:45,99-120` — Kafka publish

### Controllers (full list in §D)
- `Service/LandingAPI/Controllers/UserController.cs:53,397,598,625,668,697,977` — primary user surface
- `Service/LandingAPI/Controllers/UserLoginController.cs:78,187,236,396,428,487,517,573,615,708` — login lifecycle
- `Service/LandingAPI/Controllers/CredentialController.cs:125,163,381,422,459,531,656` — credentials
- `Service/LandingAPI/Controllers/MultiFactorAuthController.cs:53,77` — 2FA
- `Service/LandingAPI/Controllers/PersonController.cs:56,138` — person
- `Service/LandingAPI/Controllers/PersonaController.cs:76,290` — persona
- `Service/LandingAPI/Controllers/ProfileController.cs:279` — profile
- `Service/LandingAPI/Controllers/EmployeeAccessController.cs:70,88` — employee access
- `Service/LandingAPI/Controllers/ProductUserController.cs:44,71,98` — product user
- `Service/LandingAPI/Controllers/BatchProcessController.cs:31,58,83,108` — async batch
- `Service/LandingAPI/Controllers/UserPropertiesSyncController.cs:64-66` — property sync
- `Service/LandingAPI/Controllers/UserNotificationController.cs:37` — welcome resend
- `Service/LandingAPI/Controllers/WebHookController.cs:101,234` — Tibco/HOTS trigger
- `Service/LandingAPI/Controllers/ElectronicAddressController.cs:60,134`
- `Service/LandingAPI/Controllers/TelecommunicationNumberController.cs:60,134`
- `Service/LandingAPI/Controllers/PostalAddressController.cs:61,149`
- `Service/LandingAPI/Controllers/PasswordPolicyController.cs:43,127`
- `Service/LandingAPIEnterprise/Controllers/UserController.cs:165,374,553,1235,1264` — enterprise surface
- `Service/LandingAPIEnterprise/Controllers/HotsUserCloneController.cs:99`

### Data model + SPs
- `Database/Identity/Enterprise/Tables/Party.sql`
- `Database/Identity/Person/Tables/Person.sql`
- `Database/Identity/Ident/Tables/UserLogin.sql`
- `Database/Identity/Ident/Tables/UserLoginPersona.sql`
- `Database/Identity/Person/Tables/Persona.sql`
- `Database/Identity/Enterprise/Tables/PartyRelationship.sql`
- `Database/Identity/Enterprise/Tables/ExternalUserRelationship.sql`
- `Database/Identity/Ident/Stored Procedures/CreateUser_Ver01.sql`
- `Database/Identity/Ident/Stored Procedures/CreateUserLogin.sql`
- `Database/Identity/Ident/Stored Procedures/CreateUserLoginPersona.sql`
- `Database/Identity/Person/Stored Procedures/CreatePerson.sql`
- `Database/Identity/Person/Stored Procedures/CreatePersona.sql`
- `Database/Identity/Ident/Stored Procedures/UpdateUserLogin.sql`
- `Database/Identity/Ident/Stored Procedures/UpdateUserStatusByCompany.sql`
- `Database/Identity/Ident/Stored Procedures/ListPendingUsers.sql`
- `Database/Identity/Enterprise/Stored Procedures/UpdateExternalUserRelationship.sql`
- `Database/Identity/Enterprise/Stored Procedures/GetExternalUserRelationship.sql`
- `Database/MigrationTool/Migration/Ident/Tables/MergedUser.sql`
- `Service/SharedObjects/Constants/StoredProcNameConstants.cs:45,48,55,57,64,66,78,79,112,114,190,191,195,211,219,223,332,360,361,429`
- `Service/SharedObjects/Constants/LogActivityTypeConstants.cs:9-41`
- `Service/SharedObjects/Enum/CreateUserSourceType.cs`
- `Service/SharedObjects/Enum/UserStatusType.cs`
- `Service/SharedObjects/Landing/DefaultUserClaim.cs:14-15`
- `Service/SharedObjects/Landing/ExternalUserRelationship.cs:5-21`
- `Scripts/PostDeployment/PostDeploymentScripts/August2020Release.sql:97-99` — well-known company GUID seeding

---

## Architecture Insights

1. **Two parallel create paths exist** — fine-grained C# orchestration (`UserRepository.CreateUser`) for the UI, monolithic SP (`Ident.CreateUser_Ver01`) for the Enterprise API. Both end up with the same row layout, but they have **different transactional semantics** (the C# path uses connection-level transactions with per-SP rollback; the monolithic SP uses nested per-EXEC transactions). Any change to user creation logic must be applied in two places.

2. **No DI container, no factory** — `ManageUser` constructs its repositories with `new`. This forces test paths to use one of two test constructors (full mock injection vs. `IRepository + HttpMessageHandler`). It also means there's no way to add cross-cutting concerns (validation, telemetry) without touching every Manage class.

3. **"User" is a 6-table composite with two virtual orgs** — to truly delete or audit a user, you must walk `Party → Person → UserLogin → UserLoginPersona[]/Persona[] → PartyRelationship[] → ExternalUserRelationship → ContactMechanism[] → PasswordHistory → ProductBatch[]`. The two well-known company GUIDs (RP-employee + External-Users) layer additional rows on top. There is no aggregate root or document-level view of "a user."

4. **Status flips drive two-stage propagation** — synchronous DB write + Kafka event (5 known call sites) + per-product `ProductBatch` row enqueue → BatchProcessor polls + HTTP fanout per product. The `DeactivateProductUser` ProcessExecution is `NotImplementedException`-stubbed, which means deactivation actually flows through `CreateUpdateProductUser` (with status flags) rather than a dedicated path.

5. **Side effects fire OUTSIDE the transaction** — registration email, audit log, delegate role audit, Kafka publish in some paths. If the transaction commits but the email send fails, the audit will still record `EMAIL_RESENT` and the user has no welcome email. Conversely, if the transaction rolls back but a `CommunicationEvent` row was already written (pending mode), there's an orphan row.

6. **Anonymous internal endpoints are extensive** — 9 routes that mutate user state are `[AllowAnonymous]`. Of those, only the Books webhook validates HMAC and only credential routes validate activity tokens. The four `BatchProcessor*` endpoints are entirely unauthenticated (per the inline TODO "client id for Windows Service in ID server"). This is the largest unsecured surface in the system.

7. **PR 406991 ("Multi-Company User / claim my user") is read-path only in this repo** — the working-tree change adds two flags to `GetProfile`'s output. The actual "claim" UI lives in the Angular `unified-login-landing` repo. The write-path counterpart (whatever endpoint claims an external user into a real company) was not added in this PR.

8. **Drift risk: SQL `Ident.CreateUser_Ver01` and C# orchestration must stay in sync** — both call the same primitives but in slightly different orders. Any new step added to one (e.g., a new related table) requires the same addition in the other; nothing enforces this. The PR pattern (`User Story 2229269: Multi-Company User: Update to the External User behavior and claim my user (Dev Only)`) is consistent: features add to *both* paths separately.

---

## Historical Context (from thoughts/)

Two prior research documents in this directory inform this one:

- `thoughts/shared/research/2026-05-11-unified-login-architecture.md` — system-wide architecture overview. Identifies the four-repo topology, layer boundaries, and external integrations referenced here.
- `thoughts/shared/research/2026-05-11-enterprise-roles-primary-properties.md` — drills into the enterprise role + primary-properties async propagation flow. Already documents the gaps in `Batch.EnterpriseRoleBatchProcess` and `Batch.PrimaryPropertiesBatchProcess` dispatch, which the create-user flow indirectly enqueues into.

Root-level docs:
- `BatchProcessor-Bottlenecks.md` — performance issues in the WinService that processes user-creation downstream propagation.
- `Legacy-Errors-Analysis-05.05.2026.md` — top error class (6,475 NREs in `ManageProductResidentPortal`) directly affects what happens AFTER a user is created and the per-product fan-out runs.

## Related Research

- `thoughts/shared/research/2026-05-11-unified-login-architecture.md`
- `thoughts/shared/research/2026-05-11-enterprise-roles-primary-properties.md`

## Open Questions

1. **What is the actual "claim my user" write endpoint?** PR 406991 added the *display* hooks for multi-company / hasExistsInExternalUsersCompany / ExternalUserRelationship, but no new create-or-merge endpoint was added in this repo. Is the claim flow implemented elsewhere (`unified-login-coreapi`) or still pending?

2. **`Ident.CreateUser_Ver01` and `UserRepository.CreateUser` drift** — given they're maintained separately, is there a verification test (manual or automated) that confirms they produce identical row layouts? If not, that's a gap.

3. **`/api/bulkuserbatchprocessor` semantics** — the controller name suggests bulk user create/update but the method `GenerateProductUnAssignProductBatch` is specifically for **product unassignment**. What's the entry point for bulk user *creation* (the Excel import flow that sets `CreateUserSourceType.ExcelImport=25`)? Likely a separate endpoint or a single-create loop driven by the UI — needs confirmation.

4. **`DeactivateProductUser` ProcessExecution is `NotImplementedException`-stubbed** — what does deactivation actually flow through? Presumably `CreateUpdateProductUser` with a status flag, but this is implicit. If a downstream product is expecting a `Deactivate` call and getting `CreateUpdate`, behavior may diverge.

5. **`hasMultiCompany` / `hasMultiPersona` semantics** — `hasMultiCompany` is set from `userPersonaOrganizationList.Count > 1` and `hasMultiPersona` from `personaList.Count > 1`. These two collections seem related — does one ever exceed the other? What's the difference between "multiple companies" and "multiple personas"? Documentation in the UI repo may clarify.

6. **Migration tool freshness** — the `MigrationTool` enum value and SQL project exist. Are they still actively used, or legacy from past data migrations? Given the recent commit cadence focuses on `MigrationTool` exclusively via the `migrationapi` scope, the path is supported but may have no current callers.

7. **`UserStatusKafkaProducer` — five call sites, but is the topic consumed in this repo?** The producer is here but no consumer code is visible in `unified-login-main`. Likely consumed by `unified-login-coreapi` or the User Updates email subscription service (per recent PR 2696571 "Publish user events on deactivation and reactivation to support Product Updates email subscriptions"). Worth confirming the consumer side of the contract.

8. **2FA audit drift** — `TwoFactorLogic.cs:65,83` writes `"Update User"` as a string literal instead of using `LogActivityTypeConstants.UPDATE_USER`. A rename of the constant would silently miss those two sites.
