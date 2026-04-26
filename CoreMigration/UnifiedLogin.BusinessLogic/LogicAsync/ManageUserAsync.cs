using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of core user management operations.
/// <para>
/// All lookup I/O (profile snapshot, persona list, employee ID) is performed via native async
/// repositories. The two final write operations — <c>UserRepository.UpdateUser</c> and
/// <c>UserRepository.DisableUserProduct</c> / <c>ActivateUserProducts</c> — have no async
/// counterparts in <see cref="IUserRepositoryAsync"/> yet and are dispatched via
/// <c>Task.Run</c> as a deliberate stepping stone.
/// </para>
/// <para>
/// <b>TODO:</b> When <c>IUserRepositoryAsync</c> gains <c>UpdateUserAsync</c>,
/// <c>DisableUserProductAsync</c>, and <c>ActivateUserProductsAsync</c>, replace the
/// <c>Task.Run</c> blocks and remove the <c>IUserClaimsAccessor.GetUserClaim()</c> calls.
/// </para>
/// </summary>
public sealed class ManageUserAsync : IManageUserAsync
{
    #region Fields

    private readonly IManageProfileAsync               _manageProfile;
    private readonly IUserLoginPersonaRepositoryAsync  _userLoginPersonaRepository;
    private readonly IUserRepositoryAsync              _userRepository;
    private readonly IManageUserRegistrationEmailAsync _manageUserRegistrationEmail;
    private readonly IUserClaimsAccessor               _userClaims;
    private readonly ILogger<ManageUserAsync>          _logger;

    #endregion

    #region Constructor

    public ManageUserAsync(
        IManageProfileAsync               manageProfile,
        IUserLoginPersonaRepositoryAsync  userLoginPersonaRepository,
        IUserRepositoryAsync              userRepository,
        IManageUserRegistrationEmailAsync manageUserRegistrationEmail,
        IUserClaimsAccessor               userClaims,
        ILogger<ManageUserAsync>          logger)
    {
        _manageProfile               = manageProfile               ?? throw new ArgumentNullException(nameof(manageProfile));
        _userLoginPersonaRepository  = userLoginPersonaRepository  ?? throw new ArgumentNullException(nameof(userLoginPersonaRepository));
        _userRepository              = userRepository              ?? throw new ArgumentNullException(nameof(userRepository));
        _manageUserRegistrationEmail = manageUserRegistrationEmail ?? throw new ArgumentNullException(nameof(manageUserRegistrationEmail));
        _userClaims                  = userClaims                  ?? throw new ArgumentNullException(nameof(userClaims));
        _logger                      = logger                      ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserAsync(
        Guid loggedInUserRealPageId,
        IProfileDetail profile,
        CancellationToken cancellationToken = default)
    {
        if (loggedInUserRealPageId == Guid.Empty)
            return new RepositoryResponse { ErrorMessage = "Edit User: Invalid parameter realPageId." };

        // Mirror sync behaviour: when re-activating a disabled user, reset the from-date
        bool sendNotification = profile.userLogin.Status == UserUiStatusType.Disabled
            && (profile.userLogin.IsActive ?? false);

        if (sendNotification)
            profile.userLogin.FromDate = DateTime.UtcNow;

        long orgPartyId = profile.Persona.First().OrganizationPartyId;

        // Phase 1 — fetch old profile snapshot and persona list in parallel
        var oldProfileTask = _manageProfile.GetProfileDetailAsync(
            profile.RealPageId, orgPartyId, cancellationToken: cancellationToken);
        var personaListTask = _userLoginPersonaRepository.ListUserLoginPersonaAsync(
            null, profile.Persona[0].UserId, orgPartyId, cancellationToken);
        await Task.WhenAll(oldProfileTask, personaListTask);

        var oldProfile  = oldProfileTask.Result;
        var personaList = personaListTask.Result;

        // Phase 2 — hydrate employee ID onto the old-profile snapshot (needed by the SP)
        if (personaList.Count > 0)
        {
            var employeeId = await _userRepository.GetUserEmployeeIdAsync(
                personaList[0].UserLoginPersonaId, orgPartyId, cancellationToken);

            oldProfile.EmployeeId      = (employeeId is not null && !string.IsNullOrEmpty(employeeId.EmployeeId))
                ? employeeId.EmployeeId : string.Empty;
            oldProfile.UserEmployeeId  = (employeeId is not null && employeeId.UserEmployeeId > 0)
                ? employeeId.UserEmployeeId : 0;
        }

        // SYNC: UserRepository.UpdateUser has no async port yet — dispatched on thread pool
        var userClaim    = _userClaims.GetUserClaim();
        var syncUserRepo = new UserRepository(userClaim);
        var response     = await Task.Run(
            () => syncUserRepo.UpdateUser(loggedInUserRealPageId, profile, oldProfile),
            cancellationToken);

        _logger.LogDebug("UpdateUserAsync complete for {RealPageId}. RepositoryId={Id}",
            profile.RealPageId, response.Id);

        // Send re-activation email when user transitions from Disabled → Active
        if (response.Id > 0 && sendNotification)
        {
            await _manageUserRegistrationEmail.SendNewUserRegistrationEmailAsync(
                profile, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserStatusAsync(
        Guid editorRealPageId,
        long editorPersonaId,
        IList<UserLoginOnly> userLogins,
        UserUiStatusType? userLoginStatusType,
        CancellationToken cancellationToken = default)
    {
        // SYNC: DisableUserProduct / ActivateUserProducts have no async ports yet
        var userClaim    = _userClaims.GetUserClaim();
        var syncUserRepo = new UserRepository(userClaim);

        await Task.Run(() =>
        {
            if (userLoginStatusType == UserUiStatusType.Disabled)
                syncUserRepo.DisableUserProduct(editorRealPageId, editorPersonaId, userLogins);
            else if (userLoginStatusType == UserUiStatusType.Active)
                syncUserRepo.ActivateUserProducts(editorRealPageId, editorPersonaId, userLogins);
        }, cancellationToken);

        return new RepositoryResponse();
    }

    #endregion
}
