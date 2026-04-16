CREATE PROCEDURE [Person].[GetUserAuditLastLoginByProduct]
    @OrgPartyId BIGINT,
    @UserStatus NVARCHAR(MAX) = NULL,   -- comma-separated StatusTypeIds
    @UserType   NVARCHAR(MAX) = NULL,   -- comma-separated RoleTypeIdFrom values
    @ProductIds NVARCHAR(MAX) = NULL    -- comma-separated ProductIds
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #StatusIds    (Val INT);
    CREATE TABLE #UserTypeIds  (Val INT);
    CREATE TABLE #ProductIdList(Val INT);

    IF @UserStatus IS NOT NULL AND LEN(LTRIM(RTRIM(@UserStatus))) > 0
        INSERT INTO #StatusIds    SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@UserStatus,  ',') WHERE TRY_CAST(value AS INT) IS NOT NULL;
    IF @UserType IS NOT NULL AND LEN(LTRIM(RTRIM(@UserType))) > 0
        INSERT INTO #UserTypeIds  SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@UserType,    ',') WHERE TRY_CAST(value AS INT) IS NOT NULL;
    IF @ProductIds IS NOT NULL AND LEN(LTRIM(RTRIM(@ProductIds))) > 0
        INSERT INTO #ProductIdList SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@ProductIds, ',') WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE @HasProduct3 BIT = CASE WHEN EXISTS(SELECT 1 FROM #ProductIdList WHERE Val = 3) THEN 1 ELSE 0 END;

    -- Part 1: activity-based last login (all products).
    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT
            p1.FirstName,
            p1.LastName,
            ISNULL(ur.UserRelationshipName, '''') AS UserRelationship,
            st.[Name]          AS UserStatus,
            ul.LoginName       AS UserName,
            CASE WHEN MONTH(plau.LoginDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,plau.LoginDate)
                 ELSE DATEADD(HOUR,-5,plau.LoginDate) END AS ProductLastLoginDate,
            prd.[Name]         AS Product
        FROM Ident.Userlogin ul
        INNER JOIN Ident.UserLoginPersona ulp   ON ulp.UserLoginId       = ul.UserId
        INNER JOIN Enterprise.Organization o    ON o.PartyId             = ulp.OrganizationPartyId
        INNER JOIN Enterprise.StatusType est    ON ulp.StatusTypeId      = est.StatusTypeId
        INNER JOIN Person.Person p1             ON p1.PartyId            = ul.PersonPartyId
        INNER JOIN Person.Persona p             ON p.UserLoginPersonaId  = ulp.UserLoginPersonaId
        INNER JOIN Enterprise.ProductLoginUserActivitySummary AS plau
                                                ON plau.PersonaId        = p.PersonaId
        INNER JOIN Enterprise.Product prd       ON prd.ProductId         = plau.ProductId
        LEFT  JOIN Enterprise.ExternalUserRelationship eup
                                                ON eup.UserLoginPersonaId = ulp.UserLoginPersonaId
        LEFT  JOIN Enterprise.UserRelationShip ur ON ur.Id               = eup.ThirdPartyRelationshipId
        INNER JOIN Enterprise.StatusType st     ON st.StatusTypeId       = ulp.StatusTypeId
        INNER JOIN Enterprise.PartyRelationShip AS prs01
                                                ON prs01.PartyIdFrom     = ul.PersonPartyId
                                                AND prs01.PartyIdTo      = ulp.OrganizationPartyId
                                                AND prs01.ThruDate IS NULL
        INNER JOIN Enterprise.RoleType AS rt01  ON rt01.PartyRoleTypeId  = prs01.RoleTypeIdFrom
                                                AND rt01.ParentPartyRoleTypeId = 400
        LEFT  JOIN Enterprise.PersonaConfiguration pc
                                                ON pc.ProductId          = prd.ProductId
                                                AND pc.PersonaId         = p.PersonaId
        WHERE o.PartyId = @OrgPartyId';

    IF (SELECT COUNT(*) FROM #ProductIdList) > 0
        SET @sql += N' AND plau.ProductId IN (SELECT Val FROM #ProductIdList)';
    IF (SELECT COUNT(*) FROM #StatusIds) > 0
        SET @sql += N' AND ulp.StatusTypeId IN (SELECT Val FROM #StatusIds)';
    IF (SELECT COUNT(*) FROM #UserTypeIds) > 0
        SET @sql += N' AND prs01.RoleTypeIdFrom IN (SELECT Val FROM #UserTypeIds)';

    -- Part 2: product 3 fallback — ULP-based last login, added only when product 3 is requested.
    IF @HasProduct3 = 1
    BEGIN
        SET @sql += N'
        UNION ALL
        SELECT
            p1.FirstName,
            p1.LastName,
            ISNULL(ur.UserRelationshipName, '''') AS UserRelationship,
            st.[Name]         AS UserStatus,
            ul.LoginName      AS UserName,
            CASE WHEN MONTH(ulp.LastLoginDate) IN (1,2,11,12)
                 THEN DATEADD(HOUR,-6,ulp.LastLoginDate)
                 ELSE DATEADD(HOUR,-5,ulp.LastLoginDate) END AS ProductLastLoginDate,
            prd.[Name]        AS Product
        FROM Ident.Userlogin ul
        INNER JOIN Ident.UserLoginPersona ulp   ON ulp.UserLoginId       = ul.UserId
        INNER JOIN Enterprise.Organization o    ON o.PartyId             = ulp.OrganizationPartyId
        INNER JOIN Enterprise.StatusType est    ON ulp.StatusTypeId      = est.StatusTypeId
        INNER JOIN Person.Person p1             ON p1.PartyId            = ul.PersonPartyId
        INNER JOIN Person.Persona p             ON p.UserLoginPersonaId  = ulp.UserLoginPersonaId
        LEFT  JOIN Enterprise.ExternalUserRelationship eup
                                                ON eup.UserLoginPersonaId = ulp.UserLoginPersonaId
        LEFT  JOIN Enterprise.UserRelationShip ur ON ur.Id               = eup.ThirdPartyRelationshipId
        INNER JOIN Enterprise.StatusType st     ON st.StatusTypeId       = ulp.StatusTypeId
        INNER JOIN Enterprise.PartyRelationShip AS prs01
                                                ON prs01.PartyIdFrom     = ul.PersonPartyId
                                                AND prs01.PartyIdTo      = ulp.OrganizationPartyId
                                                AND prs01.ThruDate IS NULL
        INNER JOIN Enterprise.RoleType AS rt01  ON rt01.PartyRoleTypeId  = prs01.RoleTypeIdFrom
                                                AND rt01.ParentPartyRoleTypeId = 400
        INNER JOIN Enterprise.Product prd       ON prd.ProductId         = 3
        WHERE o.PartyId = @OrgPartyId';

        IF (SELECT COUNT(*) FROM #ProductIdList) > 0
            SET @sql += N' AND prd.ProductId IN (SELECT Val FROM #ProductIdList)';
        IF (SELECT COUNT(*) FROM #StatusIds) > 0
            SET @sql += N' AND ulp.StatusTypeId IN (SELECT Val FROM #StatusIds)';
        IF (SELECT COUNT(*) FROM #UserTypeIds) > 0
            SET @sql += N' AND prs01.RoleTypeIdFrom IN (SELECT Val FROM #UserTypeIds)';
    END

    SET @sql += N';';

    EXEC sp_executesql @sql, N'@OrgPartyId BIGINT', @OrgPartyId = @OrgPartyId;

    DROP TABLE IF EXISTS #StatusIds;
    DROP TABLE IF EXISTS #UserTypeIds;
    DROP TABLE IF EXISTS #ProductIdList;
END