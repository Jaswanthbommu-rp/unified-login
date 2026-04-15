CREATE PROCEDURE [Enterprise].[GetUserUPFMProperties]
    @OrgPartyId         BIGINT = 350,
    @UserStatus         NVARCHAR(MAX) = '1,24,23,3,2',   -- comma-separated StatusTypeIds
    @UserType           NVARCHAR(MAX) = '401,402,404,405',   -- comma-separated PartyRoleTypeIds
    @ProductIds         NVARCHAR(MAX) = '3',   -- comma-separated ProductIds
    @PageNumber         INT = 1,
    @PageSize           INT = 100000
AS
BEGIN
    SET NOCOUNT ON;

    -- Parse comma-separated inputs into table variables
    DECLARE @StatusList TABLE (StatusTypeId INT);
    IF @UserStatus IS NOT NULL
        INSERT INTO @StatusList (StatusTypeId)
        SELECT CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@UserStatus, ',')
        WHERE LTRIM(RTRIM(value)) <> '';

    DECLARE @UserTypeList TABLE (PartyRoleTypeId INT);
    IF @UserType IS NOT NULL
        INSERT INTO @UserTypeList (PartyRoleTypeId)
        SELECT CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@UserType, ',')
        WHERE LTRIM(RTRIM(value)) <> '';

    DECLARE @ProductList TABLE (ProductId INT);
    IF @ProductIds IS NOT NULL
        INSERT INTO @ProductList (ProductId)
        SELECT CAST(LTRIM(RTRIM(value)) AS INT)
        FROM STRING_SPLIT(@ProductIds, ',')
        WHERE LTRIM(RTRIM(value)) <> '';

    -- Determine flags used in conditional logic
    DECLARE @HasStatusFilter   BIT = CASE WHEN EXISTS (SELECT 1 FROM @StatusList) THEN 1 ELSE 0 END;
    DECLARE @HasUserTypeFilter BIT = CASE WHEN EXISTS (SELECT 1 FROM @UserTypeList) THEN 1 ELSE 0 END;
    DECLARE @HasProductFilter  BIT = CASE WHEN EXISTS (SELECT 1 FROM @ProductList) THEN 1 ELSE 0 END;
    DECLARE @HasProduct3       BIT = CASE WHEN EXISTS (SELECT 1 FROM @ProductList WHERE ProductId = 3) THEN 1 ELSE 0 END;
    DECLARE @HasActiveStatus   BIT = CASE WHEN EXISTS (SELECT 1 FROM @StatusList WHERE StatusTypeId = 1) THEN 1 ELSE 0 END;
    DECLARE @HasPendingStatus  BIT = CASE WHEN EXISTS (SELECT 1 FROM @StatusList WHERE StatusTypeId = 2) THEN 1 ELSE 0 END;

    -- When both Active (1) and Pending (2) are selected, also include ForceResetPassword (12)
    IF @HasActiveStatus = 1 AND @HasPendingStatus = 1
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM @StatusList WHERE StatusTypeId = 12)
            INSERT INTO @StatusList (StatusTypeId) VALUES (12);
    END
    Create table #temptable
    (
        PersonaId BIGINT PRIMARY KEY,
        CompanyPartyId BIGINT,
        OrganizationRealPageId NVARCHAR(255),
        FirstName NVARCHAR(255),
        LastName NVARCHAR(255),
        LoginName NVARCHAR(255),
        CompanyName NVARCHAR(255),
        UserType NVARCHAR(255),
        UserRelationship NVARCHAR(255),
        LastLoginDate DATETIME,
        Status NVARCHAR(50)
    )
    
    INSERT INTO #temptable
    (
        PersonaId,
        CompanyPartyId,
        OrganizationRealPageId,
        FirstName,
        LastName,
        LoginName,
        CompanyName,
        UserType,
        UserRelationship,
        LastLoginDate,
        Status
    )
    SELECT DISTINCT
        p.PersonaId                                     AS [PersonaId],
        o.PartyId                                       AS [CompanyPartyId],
        op.RealPageId                                   AS [OrganizationRealPageId],
        p1.FirstName,
        p1.LastName,
        ul.LoginName,
        o.Name                                          AS [CompanyName],
        rt01.Name                                       AS [UserType],
        ISNULL(TPR.ThirdPartyRelationship, '')          AS [UserRelationship],
        ulp.LastLoginDate                               AS [LastLoginDate],
        CASE
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate IS NULL THEN
                CASE
                    WHEN ul.LastLoginDate IS NULL     THEN 'Pending'
                    WHEN ul.LastLoginDate IS NOT NULL THEN 'Active'
                    ELSE est.[Name]
                END
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NOT NULL THEN 'Active'
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NULL     THEN 'Pending'
            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate <  GETUTCDATE()                                   THEN 'Expired'
            ELSE est.[Name]
        END                                             AS [Status]
    FROM Ident.UserLogin ul
        INNER JOIN Ident.UserLoginPersona ulp
            ON ulp.UserLoginId = ul.UserId
        INNER JOIN Enterprise.StatusType est
            ON ulp.StatusTypeId = est.StatusTypeId
        INNER JOIN Person.Person p1
            ON p1.PartyId = ul.PersonPartyId
        INNER JOIN Person.Persona p
            ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
        INNER JOIN Enterprise.Organization o
            ON o.PartyId = ulp.OrganizationPartyId
        INNER JOIN Enterprise.Party op
            ON op.PartyId = o.PartyId
        INNER JOIN Enterprise.Party p2
            ON p2.PartyId = ul.PersonPartyId
        INNER JOIN Enterprise.PartyRelationShip AS prs01
            ON prs01.PartyIdFrom = ul.PersonPartyId
            AND prs01.PartyIdTo  = ulp.OrganizationPartyId
            AND prs01.ThruDate IS NULL
        LEFT JOIN Enterprise.ExternalUserRelationship eur
            ON eur.UserLoginPersonaId = ulp.UserLoginPersonaId
        LEFT JOIN Enterprise.ThirdPartyRelationship tpr
            ON tpr.ThirdPartyRelationshipId = eur.ThirdPartyRelationshipId
        INNER JOIN Enterprise.RoleType AS rt01
            ON rt01.PartyRoleTypeId = prs01.RoleTypeIdFrom
            AND rt01.ParentPartyRoleTypeId = 400
        -- Conditionally join PersonaConfiguration when product 3 is NOT in the selected list
        LEFT JOIN Enterprise.PersonaConfiguration epc
            ON epc.PersonaId = p.PersonaId
            AND @HasProductFilter = 1
            AND @HasProduct3 = 0
        LEFT JOIN Enterprise.OrganizationAdminUser oau
            ON oau.OrganizationPartyId = o.PartyId
            AND oau.UserLoginPersonaId = ulp.UserLoginPersonaId
    WHERE
        o.PartyId = @OrgPartyId
        --AND p.personaid <> 148264 -- Exclude specific PersonaId
        AND ulp.OrganizationPartyId = @OrgPartyId
        AND oau.OrganizationAdminUserId IS NULL
        AND ulp.IsRPEmployee = 0
        -- Status filter with ForceResetPassword (StatusTypeId = 12) logic
        AND (
            @HasStatusFilter = 0
            OR (
                ulp.StatusTypeId IN (SELECT StatusTypeId FROM @StatusList)
                -- Active selected but NOT Pending: include status 12 users who logged in before
                OR (
                    @HasActiveStatus = 1 AND @HasPendingStatus = 0
                    AND 1 = (
                        CASE
                            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate IS NULL THEN
                                CASE WHEN ul.LastLoginDate IS NOT NULL THEN 1 ELSE 0 END
                            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NOT NULL THEN 1
                            ELSE 0
                        END
                    )
                )
                -- Pending selected but NOT Active: include status 12 users who never logged in
                OR (
                    @HasPendingStatus = 1 AND @HasActiveStatus = 0
                    AND 1 = (
                        CASE
                            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate IS NULL THEN
                                CASE WHEN ul.LastLoginDate IS NULL THEN 1 ELSE 0 END
                            WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NULL THEN 1
                            ELSE 0
                        END
                    )
                )
            )
        )
        -- User type filter
        AND (
            @HasUserTypeFilter = 0
            OR prs01.RoleTypeIdFrom IN (SELECT PartyRoleTypeId FROM @UserTypeList)
        )
        -- Product filter (only when product 3 is NOT selected)
        AND (
            @HasProductFilter = 0
            OR @HasProduct3 = 1
            OR (
                epc.ProductId IN (SELECT ProductId FROM @ProductList)
                AND (epc.ThruDate IS NULL OR epc.ThruDate >= GETUTCDATE())
            )
        )
    ORDER BY ulp.LastLoginDate DESC
    
    -- Join with property mappings into a result temp table
    DROP TABLE IF EXISTS #ResultSet

    SELECT tt.LoginName, tt.FirstName, tt.LastName, tt.[status], tt.UserType, tt.LastLoginDate,
        'Unified Platform' as productName, pi.[Name] as PropertyName 
    INTO #ResultSet
    FROM #temptable tt 
    INNER JOIN Enterprise.PropertyInstanceMapping pm
        ON pm.PersonaId = tt.PersonaId
        AND pm.ProductId = 3
        AND pm.Active = 1
        AND pm.ThruDate IS NULL
    INNER JOIN Enterprise.PropertyInstance pi
        ON pi.PropertyInstanceId = pm.PropertyInstanceId

    DROP TABLE #temptable

    -- Pre-compute total count to avoid COUNT(1) OVER() full scan
    DECLARE @TotalRecords INT
    SELECT @TotalRecords = COUNT(1) FROM #ResultSet

    -- Return paginated results
    SELECT LoginName, FirstName, LastName, [status], UserType,
           LastLoginDate, productName, PropertyName,
           @TotalRecords AS TotalRecords
    FROM #ResultSet
    ORDER BY LastLoginDate DESC
    OFFSET ((@PageNumber - 1) * @PageSize) ROWS
    FETCH NEXT @PageSize ROWS ONLY

    DROP TABLE #ResultSet

END
GO

