CREATE PROCEDURE [Person].[GetCompanyUserList]
    @OrgPartyId BIGINT,
    @UserStatus NVARCHAR(MAX) = NULL,   -- comma-separated StatusTypeIds, e.g. '1,2'
    @UserType   NVARCHAR(MAX) = NULL,   -- comma-separated RoleTypeIdFrom values
    @ProductIds NVARCHAR(MAX) = NULL    -- comma-separated ProductIds
AS
BEGIN
    SET NOCOUNT ON;

    -- Parse filter values into temp tables for safe IN() use.
    CREATE TABLE #StatusIds    (Val INT);
    CREATE TABLE #UserTypeIds  (Val INT);
    CREATE TABLE #ProductIdList(Val INT);

    IF @UserStatus IS NOT NULL AND LEN(LTRIM(RTRIM(@UserStatus))) > 0
        INSERT INTO #StatusIds    SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@UserStatus,  ',') WHERE TRY_CAST(value AS INT) IS NOT NULL;

    IF @UserType IS NOT NULL AND LEN(LTRIM(RTRIM(@UserType))) > 0
        INSERT INTO #UserTypeIds  SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@UserType,    ',') WHERE TRY_CAST(value AS INT) IS NOT NULL;

    IF @ProductIds IS NOT NULL AND LEN(LTRIM(RTRIM(@ProductIds))) > 0
        INSERT INTO #ProductIdList SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@ProductIds, ',') WHERE TRY_CAST(value AS INT) IS NOT NULL;

    -- Determine whether special ForceToReset OR clause is needed.
    DECLARE @HasStatus1  BIT = CASE WHEN EXISTS(SELECT 1 FROM #StatusIds    WHERE Val = 1) THEN 1 ELSE 0 END;
    DECLARE @HasStatus2  BIT = CASE WHEN EXISTS(SELECT 1 FROM #StatusIds    WHERE Val = 2) THEN 1 ELSE 0 END;
    DECLARE @HasProduct3 BIT = CASE WHEN EXISTS(SELECT 1 FROM #ProductIdList WHERE Val = 3) THEN 1 ELSE 0 END;

    -- If both Active (1) and Pending (2) are present, also include ForceToReset (12).
    IF @HasStatus1 = 1 AND @HasStatus2 = 1
        INSERT INTO #StatusIds (Val) SELECT 12 WHERE NOT EXISTS(SELECT 1 FROM #StatusIds WHERE Val = 12);

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT DISTINCT
            o.PartyId          [CompanyPartyId],
            op.RealPageId      [OrganizationRealPageId],
            p.personaId        [PersonaId],
            p1.FirstName,
            p1.LastName,
            ul.LoginName,
            o.[Name]           [CompanyName],
            rt01.[Name]        [UserType],
            ISNULL(TPR.ThirdPartyRelationship, '''') AS [UserRelationship],
            ulp.LastLoginDate  [LastLoginDate],
            CASE
                WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate IS NULL
                     THEN (CASE WHEN ul.LastLoginDate IS NULL THEN ''Pending'' WHEN ul.LastLoginDate IS NOT NULL THEN ''Active'' ELSE est.[Name] END)
                WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NOT NULL THEN ''Active''
                WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NULL    THEN ''Pending''
                WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate  < GETUTCDATE()  THEN ''Expired''
                ELSE est.[Name]
            END AS [Status]
        FROM ident.userlogin ul
        INNER JOIN ident.UserLoginPersona ulp
            ON ulp.UserLoginId = ul.UserId
        INNER JOIN Enterprise.StatusType est
            ON ulp.StatusTypeId = est.StatusTypeId
        INNER JOIN Person.Person p1
            ON p1.PartyId = ul.PersonPartyId
        INNER JOIN person.persona p
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
        LEFT  JOIN Enterprise.ExternalUserRelationship eur
            ON eur.UserLoginPersonaId = ulp.UserLoginPersonaId
        LEFT  JOIN Enterprise.ThirdPartyRelationship tpr
            ON tpr.ThirdPartyRelationshipId = eur.ThirdPartyRelationshipId
        INNER JOIN Enterprise.RoleType AS rt01
            ON rt01.PartyRoleTypeId = prs01.RoleTypeIdFrom
            AND rt01.ParentPartyRoleTypeId = 400';

    -- Conditional JOIN for product filter (omitted when product 3 is in the list or no product filter).
    IF (SELECT COUNT(*) FROM #ProductIdList) > 0 AND @HasProduct3 = 0
        SET @sql += N'
        INNER JOIN Enterprise.PersonaConfiguration epc
            ON epc.PersonaId = p.PersonaId';

    SET @sql += N'
        LEFT JOIN enterprise.OrganizationAdminUser oau
            ON oau.OrganizationPartyId  = o.PartyId
            AND oau.UserLoginPersonaId  = ulp.UserLoginPersonaId
        WHERE o.PartyId = @OrgPartyId
          AND oau.OrganizationAdminUserId IS NULL
          AND ulp.IsRPEmployee = 0';

    -- Optional status filter.
    IF (SELECT COUNT(*) FROM #StatusIds) > 0
    BEGIN
        SET @sql += N'
          AND (ulp.StatusTypeId IN (SELECT Val FROM #StatusIds)';

        -- Active-only: include ForceToReset rows that display as Active.
        IF @HasStatus1 = 1 AND @HasStatus2 = 0
            SET @sql += N'
               OR (1 = (CASE
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate IS NULL
                         THEN (CASE WHEN ul.LastLoginDate IS NULL THEN 0 WHEN ul.LastLoginDate IS NOT NULL THEN 1 ELSE 0 END)
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NOT NULL THEN 1
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NULL    THEN 0
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate  < GETUTCDATE() THEN 0
                    ELSE 0 END))';

        -- Pending-only: include ForceToReset rows that display as Pending.
        IF @HasStatus1 = 0 AND @HasStatus2 = 1
            SET @sql += N'
               OR (1 = (CASE
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate IS NULL
                         THEN (CASE WHEN ul.LastLoginDate IS NULL THEN 1 WHEN ul.LastLoginDate IS NOT NULL THEN 0 ELSE 0 END)
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NOT NULL THEN 0
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate >= GETUTCDATE() AND ulp.LastLoginDate IS NULL    THEN 1
                    WHEN ulp.StatusTypeId = 12 AND ulp.StatusThruDate  < GETUTCDATE() THEN 0
                    ELSE 0 END))';

        SET @sql += N')';
    END

    -- Optional user type filter.
    IF (SELECT COUNT(*) FROM #UserTypeIds) > 0
        SET @sql += N'
          AND prs01.RoleTypeIdFrom IN (SELECT Val FROM #UserTypeIds)';

    -- Optional product filter (only when product 3 is not in the list).
    IF (SELECT COUNT(*) FROM #ProductIdList) > 0 AND @HasProduct3 = 0
        SET @sql += N'
          AND epc.ProductId IN (SELECT Val FROM #ProductIdList)
          AND (epc.ThruDate IS NULL OR epc.ThruDate >= GETUTCDATE())';

    SET @sql += N'
        ORDER BY ulp.LastLoginDate DESC
        FOR JSON PATH;';

    EXEC sp_executesql @sql, N'@OrgPartyId BIGINT', @OrgPartyId = @OrgPartyId;

    DROP TABLE IF EXISTS #StatusIds;
    DROP TABLE IF EXISTS #UserTypeIds;
    DROP TABLE IF EXISTS #ProductIdList;
END
