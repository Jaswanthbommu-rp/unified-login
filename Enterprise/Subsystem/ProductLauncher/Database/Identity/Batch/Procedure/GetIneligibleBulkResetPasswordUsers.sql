CREATE PROCEDURE [Batch].[GetIneligibleBulkResetPasswordUsers]
    @RealPageIds         [dbo].[PartyGUID] READONLY,
    @OrganizationPartyId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Eligibility rules (User Story 2758775):
    --   1. User must exist
    --   2. User is NOT enabled with Third-Party IDP flag
    --   3. User is ACTIVE in the admin's company
    --   4. The admin's company is the user's Primary Company designation
    --
    -- Returns one row per INELIGIBLE RealPageId with the failure reason.
    -- Returns zero rows when all input users are eligible.

    ;WITH UserContext AS (
        SELECT
            ids.[RealPageID]                                                    AS [RealPageId],
            ul.UserId,
            CASE WHEN ipt.Name = 'ID3' THEN 0 ELSE 1 END                        AS Is3rdPartyIDP,
            ulp.OrganizationPartyId                                             AS UserOrgPartyId,
            ulp.PrimaryOrganization,
            ulp.StatusTypeId
        FROM @RealPageIds ids
            LEFT JOIN Enterprise.Party           p   ON p.RealPageId           = ids.[RealPageID]
            LEFT JOIN Ident.UserLogin            ul  ON ul.PersonPartyId       = p.PartyId
            LEFT JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ul.IdentityProviderTypeId
            LEFT JOIN Ident.UserLoginPersona     ulp ON ulp.UserLoginId        = ul.UserId
                                                    AND ulp.OrganizationPartyId = @OrganizationPartyId
        WHERE ids.[RealPageID] IS NOT NULL
    )
    SELECT
        [RealPageId],
        CASE
            WHEN UserId              IS NULL                     THEN 'User not found'
            WHEN UserOrgPartyId      IS NULL                     THEN 'User does not belong to your company'
            WHEN PrimaryOrganization IS NULL
              OR PrimaryOrganization = 0                         THEN 'Your company is not the user''s primary company'
            WHEN Is3rdPartyIDP = 1                               THEN 'User is enabled with Third-Party IDP'
            WHEN StatusTypeId <> 1  /* UserDbStatusType.Active */ THEN 'User is not active'
            ELSE NULL
        END AS [Reason]
    FROM UserContext
    WHERE
        UserId IS NULL
        OR UserOrgPartyId IS NULL
        OR PrimaryOrganization IS NULL
        OR PrimaryOrganization = 0
        OR Is3rdPartyIDP = 1
        OR StatusTypeId <> 1;
END
