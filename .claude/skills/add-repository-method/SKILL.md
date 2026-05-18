---
name: add-repository-method
description: Add a Dapper stored-procedure call on an existing {Entity}Repository in unified-login-main. Use when the user needs to surface a new SP. Captures the StoredProcNameConstants registration, the BaseRepository.GetRepository()/GetOne/GetMany pattern, dynamic param object, the test-mocking shape (Mock<IRepository> + StoredProcNameConstants.X + It.IsAny<object>()), and the unit-test constructor on the repository.
---

# Add a Dapper stored-proc call to a repository

This repo uses Dapper through `RP.Enterprise.Foundation.DataAccess.Component.IRepository`. Repositories inherit `BaseRepository`, pin a `DbConnectionEnum` (e.g., `IdpConfigurationDb`), and dispatch SPs by name through `StoredProcNameConstants`.

## Inputs to confirm

1. **Repository file** — pick the existing `{Entity}Repository.cs` under `Enterprise/Subsystem/ProductLauncher/Component/Landing/Repository/`. Only create a new repository if no `{Entity}Repository` exists for the table.
2. **Stored procedure** — confirm it exists in the `Identity` (or `Identity-RR`) SQL DB project under `Enterprise/Subsystem/ProductLauncher/Database/Identity/`. If it does not, stop and tell the user to add it to the DB project first — `IdentityDatabase.sln` deploys those.
3. **Connection enum** — match the existing constructor (`DbConnectionEnum.IdpConfigurationDb` is the default in this repo).
4. **Return shape** — `GetOne<T>` for a scalar/row, `GetMany<T>` for a list, `Execute` for void/affected-rows.
5. **TVPs** — if the SP takes a table-valued parameter (bulk ops), use the existing TVP helper pattern (search `DataTable` usage in `UserRepository`); do not pass `IEnumerable<T>` directly.

## Files to touch

- `Service/SharedObjects/Constants/StoredProcNameConstants.cs` — add the SP name constant in the right `//Section` grouping.
- `Component/Landing/Repository/{Entity}Repository.cs` — add the method.
- `Component/Landing/Repository/Interfaces/I{Entity}Repository.cs` — add the signature.
- `Service/LandingAPI/LandingAPI.Test/Logic/{Manage{Entity}}Tests.cs` (or a dedicated repository test file) — add the mock-backed test.

## Constant template

Edit `Service/SharedObjects/Constants/StoredProcNameConstants.cs` and add under the matching section (the file is grouped by entity with `//{Entity}` comment headers). Constant name = `SP_` + SP method name in PascalCase. Example:

```csharp
//PasswordPolicy
public const string SP_CreatePasswordPolicy = "Ident.CreatePasswordPolicy";
public const string SP_GetPasswordPolicy    = "Ident.GetPasswordPolicy";
public const string SP_UpdatePasswordPolicy = "Ident.UpdatePasswordPolicy";
public const string SP_DisablePasswordPolicy = "Ident.DisablePasswordPolicy";   // new
```

## Repository method template

Mirror [Component/Landing/Repository/PasswordPolicyRepository.cs:100](../../Enterprise/Subsystem/ProductLauncher/Component/Landing/Repository/PasswordPolicyRepository.cs):

```csharp
/// <summary>
/// Disable a Password Policy
/// </summary>
public RepositoryResponse DisablePasswordPolicy(long passwordPolicyId, int userId)
{
    dynamic param = new
    {
        passwordPolicyId,
        userId
    };

    using (var repository = GetRepository())
    {
        return repository.GetOne<RepositoryResponse>(
            StoredProcNameConstants.SP_DisablePasswordPolicy,
            param);
    }
}
```

For a list result:

```csharp
using (var repository = GetRepository())
{
    return repository.GetMany<MyDto>(StoredProcNameConstants.SP_ListThings, param);
}
```

For a TVP-backed bulk write, use the TVP marshalling helper used by existing bulk methods — do not hand-roll a `DataTable`.

### Repository wiring

If you are creating a new repository, **two constructors are required**:

```csharp
public {Entity}Repository() : base(DbConnectionEnum.IdpConfigurationDb) { }

/// <summary>Unit test constructor</summary>
public {Entity}Repository(IRepository repository) : base(repository) { }
```

The second constructor is what tests use to inject a mocked `IRepository`.

## Test template

Mirror [LandingAPI.Test/Logic/ManagePasswordPolicyTests.cs:34](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/Logic/ManagePasswordPolicyTests.cs):

```csharp
[Fact]
public void DisablePasswordPolicy_HappyPath_ReturnsId()
{
    // Arrange
    Mock<IRepository> mockRepository = new Mock<IRepository>();
    mockRepository
        .Setup(m => m.GetOne<RepositoryResponse>(
            StoredProcNameConstants.SP_DisablePasswordPolicy,
            It.IsAny<object>()))
        .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

    var repo = new PasswordPolicyRepository(mockRepository.Object);

    // Act
    var result = repo.DisablePasswordPolicy(passwordPolicyId: 42, userId: 7);

    // Assert
    Assert.Equal(1, result.Id);
}
```

### Mocking parameter shape

When asserting the SP received the *right* parameters, prefer `It.Is<object>(predicate)` with reflection. Example (matches CLAUDE.md guidance):

```csharp
mockRepository
    .Setup(m => m.GetOne<RepositoryResponse>(
        StoredProcNameConstants.SP_DisablePasswordPolicy,
        It.Is<object>(o => GetProp<long>(o, "passwordPolicyId") == 42L
                        && GetProp<int>(o, "userId") == 7)))
    .Returns(new RepositoryResponse { Id = 1 });

// helper
private static T GetProp<T>(object o, string name)
    => (T)o.GetType().GetProperty(name).GetValue(o);
```

If the SUT caches results, call `new RPObjectCache().BustCache();` in Arrange before each call.

## After adding — checklist

1. Add or update the interface (`I{Entity}Repository`) signature.
2. Confirm the SP exists in the DB project and is deployed in dev (check `IdentityDatabase.sln`).
3. Build `MasterProjectSolution.sln` and run the new test.
4. If the new SP changes existing semantics, search call sites (`Grep StoredProcNameConstants.SP_Old`) before adjusting.

## Important

- **Never** hard-code the SP name as a literal string — always go through `StoredProcNameConstants`. The tests assert on the constant; a literal silently breaks the assertion.
- **Never** open a connection manually. Always `using (var repository = GetRepository()) { ... }`.
- **Do not** add caching inside the repository. Caching lives in `Manage{Entity}` via `RPObjectCache`/`RedisCacheService` — the repository must remain a thin SP wrapper.
- **Do not** mix entities — keep `User`-related SPs in `UserRepository`, `Person`-related in `PersonRepository`, etc.
