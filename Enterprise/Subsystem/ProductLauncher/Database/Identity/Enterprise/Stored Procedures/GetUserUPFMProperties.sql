CREATE PROCEDURE [Enterprise].[GetUserUPFMProperties]
    @OrganizationPartyId BIGINT,
    @UserStatus          NVARCHAR(MAX) = NULL,   -- comma-separated StatusTypeIds; NULL returns all statuses
    @UserType            NVARCHAR(MAX) = NULL,   -- comma-separated PartyRoleTypeIds; NULL returns all user types
    @ProductIds          [Enterprise].[ProductIdType] READONLY,
    @PageNumber          INT = 1,                -- ignored; kept for signature back-compat
    @PageSize            INT = 0                 -- ignored; kept for signature back-compat
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME = GETUTCDATE();

    -- Parse comma-separated status / user-type filters into table variables.
    -- (Could be promoted to TVPs in a future change so the optimizer gets real cardinality.)
    DECLARE @StatusList TABLE (StatusTypeId INT PRIMARY KEY);
    IF @UserStatus IS NOT NULL
        INSERT INTO @StatusList (StatusTypeId)
        SELECT DISTINCT CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@UserStatus, ',')
        WHERE LTRIM(RTRIM(value)) <> '';

    DECLARE @UserTypeList TABLE (PartyRoleTypeId INT PRIMARY KEY);
    IF @UserType IS NOT NULL
        INSERT INTO @UserTypeList (PartyRoleTypeId)
        SELECT DISTINCT CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@UserType, ',')
        WHERE LTRIM(RTRIM(value)) <> '';

    DECLARE @HasStatusFilter   BIT = CASE WHEN EXISTS (SELECT 1 FROM @StatusList)   THEN 1 ELSE 0 END;
    DECLARE @HasUserTypeFilter BIT = CASE WHEN EXISTS (SELECT 1 FROM @UserTypeList) THEN 1 ELSE 0 END;
    DECLARE @HasProductFilter  BIT = CASE WHEN EXISTS (SELECT 1 FROM @ProductIds)   THEN 1 ELSE 0 END;
    DECLARE @HasProduct3       BIT = CASE WHEN EXISTS (SELECT 1 FROM @ProductIds WHERE ProductId = 3) THEN 1 ELSE 0 END;
    DECLARE @HasActiveStatus   BIT = CASE WHEN EXISTS (SELECT 1 FROM @StatusList WHERE StatusTypeId = 1) THEN 1 ELSE 0 END;
    DECLARE @HasPendingStatus  BIT = CASE WHEN EXISTS (SELECT 1 FROM @StatusList WHERE StatusTypeId = 2) THEN 1 ELSE 0 END;

    -- When both Active (1) and Pending (2) are selected, also include ForceResetPassword (12).
    IF @HasActiveStatus = 1 AND @HasPendingStatus = 1
       AND NOT EXISTS (SELECT 1 FROM @StatusList WHERE StatusTypeId = 12)
        INSERT INTO @StatusList (StatusTypeId) VALUES (12);

    -- Single set-based query:
    --   * No #temptable / #ResultSet round-trips (previous shape re-materialized everything for every page).
    --   * No SELECT DISTINCT — CROSS APPLY picks exactly one open Person -> Org relationship.
    --   * Parameter-conditioned LEFT JOIN replaced with an EXISTS subquery (plan-friendly).
    --   * OrganizationAdminUser handled via NOT EXISTS instead of LEFT JOIN + IS NULL.
    --   * Dead joins (Party op, Party p2, ExternalUserRelationship, ThirdPartyRelationship,
    --     Organization o) removed — their columns were never projected to the final result.
    -- Column shape matches ExportPropertyResponse on the C# side; Dapper binds by name.
    SELECT
        ul.LoginName                                                 AS LoginName,
        p1.FirstName                                                 AS FirstName,
        p1.LastName                                                  AS LastName,
        CASE
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate IS NULL THEN
                CASE WHEN ul.LastLoginDate IS NULL THEN 'Pending' ELSE 'Active' END
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= @Now AND ulp.LastLoginDate IS NOT NULL THEN 'Active'
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= @Now AND ulp.LastLoginDate IS NULL     THEN 'Pending'
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate <  @Now                                   THEN 'Expired'
            ELSE est.[Name]
        END                                                          AS [status],
        prs.RoleTypeName                                             AS UserType,
        ulp.LastLoginDate                                            AS LastLoginDate,
        'Unified Platform'                                           AS productName,
        pi.[Name]                                                    AS PropertyName
    FROM Ident.UserLogin ul
        INNER JOIN Ident.UserLoginPersona ulp
            ON ulp.UserLoginId          = ul.UserId
            AND ulp.OrganizationPartyId = @OrganizationPartyId
            AND ulp.IsRPEmployee        = 0
        INNER JOIN Enterprise.StatusType est
            ON est.StatusTypeId = ulp.StatusTypeId
        INNER JOIN Person.Person p1
            ON p1.PartyId = ul.PersonPartyId
        INNER JOIN Person.Persona p
            ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
        -- Exactly one open (Person -> Org) PartyRelationShip per user.
        -- Eliminates the duplicate-row fan-out that SELECT DISTINCT was previously hiding.
        CROSS APPLY (
            SELECT TOP 1
                rt01.Name AS RoleTypeName,
                prs01.RoleTypeIdFrom
            FROM Enterprise.PartyRelationShip prs01
                INNER JOIN Enterprise.RoleType rt01
                    ON rt01.PartyRoleTypeId       = prs01.RoleTypeIdFrom
                    AND rt01.ParentPartyRoleTypeId = 400
            WHERE prs01.PartyIdFrom = ul.PersonPartyId
                AND prs01.PartyIdTo = ulp.OrganizationPartyId
                AND prs01.ThruDate IS NULL
        ) prs
        INNER JOIN Enterprise.PropertyInstanceMapping pm
            ON pm.PersonaId = p.PersonaId
            AND pm.ProductId = 3
            AND pm.Active = 1
            AND pm.ThruDate IS NULL
        INNER JOIN Enterprise.PropertyInstance pi
            ON pi.PropertyInstanceId = pm.PropertyInstanceId
    WHERE
        -- Exclude organization admins.
        NOT EXISTS (
            SELECT 1
            FROM Enterprise.OrganizationAdminUser oau
            WHERE oau.OrganizationPartyId = ulp.OrganizationPartyId
                AND oau.UserLoginPersonaId = ulp.UserLoginPersonaId
        )
        -- User type filter.
        AND (
            @HasUserTypeFilter = 0
            OR prs.RoleTypeIdFrom IN (SELECT PartyRoleTypeId FROM @UserTypeList)
        )
        -- Status filter (boolean predicates, SARGable on StatusTypeId / StatusThruDate).
        AND (
            @HasStatusFilter = 0
            OR ulp.StatusTypeId IN (SELECT StatusTypeId FROM @StatusList)
            OR (
                ulp.StatusTypeId = 12
                AND (
                    -- Active selected, Pending not selected: include status-12 with a prior login.
                    (@HasActiveStatus = 1 AND @HasPendingStatus = 0
                        AND (
                            (ulp.StatusThruDate IS NULL AND ul.LastLoginDate IS NOT NULL)
                            OR (ulp.StatusThruDate >= @Now AND ulp.LastLoginDate IS NOT NULL)
                        ))
                    OR
                    -- Pending selected, Active not selected: include status-12 with no prior login.
                    (@HasPendingStatus = 1 AND @HasActiveStatus = 0
                        AND (
                            (ulp.StatusThruDate IS NULL AND ul.LastLoginDate IS NULL)
                            OR (ulp.StatusThruDate >= @Now AND ulp.LastLoginDate IS NULL)
                        ))
                )
            )
        )
        -- Product filter. Product 3 (UPFM) is implicit through PropertyInstanceMapping above,
        -- so no PersonaConfiguration check is needed when product 3 is selected (or nothing is selected).
        AND (
            @HasProductFilter = 0
            OR @HasProduct3 = 1
            OR EXISTS (
                SELECT 1
                FROM Enterprise.PersonaConfiguration epc
                WHERE epc.PersonaId  = p.PersonaId
                    AND epc.ProductId IN (SELECT ProductId FROM @ProductIds)
                    AND (epc.ThruDate IS NULL OR epc.ThruDate >= @Now)
            )
        )
    ORDER BY ulp.LastLoginDate DESC
    OPTION (RECOMPILE);
END
