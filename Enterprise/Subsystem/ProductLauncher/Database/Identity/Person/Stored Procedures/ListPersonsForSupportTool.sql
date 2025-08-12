CREATE PROCEDURE [Person].[ListPersonsForSupportTool]
    @Name VARCHAR(50) = NULL,
    @OrganizationTypeIds VARCHAR(MAX) = NULL
AS
BEGIN

    DECLARE @OrgTypeIdsTable TABLE (OrgTypeId INT);
    IF @OrganizationTypeIds IS NOT NULL AND @OrganizationTypeIds <> ''
    BEGIN
        INSERT INTO @OrgTypeIdsTable (OrgTypeId)
        SELECT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@OrganizationTypeIds, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;
    END

    DECLARE @partytable TABLE (partyid BIGINT NOT NULL, NotificationEmail VARCHAR(200) NOT NULL);
    INSERT INTO @partytable (partyid, NotificationEmail)
    SELECT
        pcm.PartyId,
        ea.ElectronicAddressString
    FROM Enterprise.PartyContactMechanism pcm
    INNER JOIN Enterprise.ContactMechanismUsage cmu ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
    INNER JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = pcm.ContactMechanismID
    WHERE (pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())
      AND cmu.ContactMechanismUsageTypeID = 301;

    ;WITH resultList (CompanyName, CompanyStatus, Username, NotificationEmail, UserType, FirstName, LastName, PartyId, UserId, ThirdPartyIDPDesc, PersonaId, PersonaRealPageId, UserStatus)
    AS (
        -- Main search: by name or login
        SELECT
            O.Name AS CompanyName,
            O.IsActive AS CompanyStatus,
            UL.LoginName AS Username,
            CASE WHEN ert.PartyRoleTypeId = 404 THEN NE.NotificationEmail ELSE ISNULL(NE.NotificationEmail, UL.LoginName) END AS NotificationEmail,
            CASE
                WHEN ert.PartyRoleTypeId = 401 THEN 'Regular User'
                WHEN ert.PartyRoleTypeId = 402 THEN 'RealPage System Administrator'
                WHEN ert.PartyRoleTypeId = 403 THEN 'RealPage Employee'
                WHEN ert.PartyRoleTypeId = 404 THEN 'Regular User (No Email)'
                WHEN ert.PartyRoleTypeId = 405 THEN 'External User'
            END AS UserType,
            PE.FirstName,
            PE.LastName,
            ULP.OrganizationPartyId AS PartyId,
            UL.UserId,
            IPT.Description AS ThirdPartyIDPDesc,
            P.PersonaId,
            prt.realpageId,
            CASE
                WHEN ULP.StatusTypeId = 12 AND UL.LastLoginDate IS NULL THEN 'Pending'
                WHEN ULP.StatusTypeId = 12 AND UL.LastLoginDate IS NOT NULL THEN 'Active'
                ELSE st.Name
            END AS UserStatus
        FROM Ident.UserLogin UL
        INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
        INNER JOIN Enterprise.StatusType st ON ULP.StatusTypeId = st.StatusTypeId
        INNER JOIN Person.Persona P ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
        INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId
        INNER JOIN Person.Person PE ON PE.PartyId = UL.PersonPartyId
        INNER JOIN Enterprise.Party prt ON prt.PartyId = PE.PartyId
        INNER JOIN Enterprise.PartyRelationship PR ON PR.PartyIdFrom = PE.PartyId AND PR.PartyIdTo = O.PartyId
        INNER JOIN Enterprise.RoleType ert ON PR.RoleTypeIdFrom = ert.PartyRoleTypeId AND ert.ParentPartyRoleTypeId = 400
        INNER JOIN Ident.IdentityProviderType IPT ON IPT.IdentityProviderTypeId = UL.IdentityProviderTypeId
        LEFT OUTER JOIN @partytable NE ON NE.PartyId = PE.PartyId
        WHERE (
                (@Name IS NULL)
                OR (CHARINDEX(@Name, PE.FirstName + ' ' + PE.LastName, 1) > 0)
                OR (CHARINDEX(@Name, UL.LoginName, 1) > 0)
                --OR (CHARINDEX(@Name, NE.NotificationEmail, 1) > 0)
            )
        AND PR.Thrudate IS NULL
        AND (
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
            )

        UNION ALL
        SELECT
            O.Name AS CompanyName,
            O.IsActive AS CompanyStatus,
            UL.LoginName AS Username,
            NE.NotificationEmail AS NotificationEmail,
            CASE
                WHEN ert.PartyRoleTypeId = 401 THEN 'Regular User'
                WHEN ert.PartyRoleTypeId = 402 THEN 'RealPage System Administrator'
                WHEN ert.PartyRoleTypeId = 403 THEN 'RealPage Employee'
                WHEN ert.PartyRoleTypeId = 404 THEN 'Regular User (No Email)'
                WHEN ert.PartyRoleTypeId = 405 THEN 'External User'
            END AS UserType,
            PE.FirstName,
            PE.LastName,
            ULP.OrganizationPartyId AS PartyId,
            UL.UserId,
            IPT.Description AS ThirdPartyIDPDesc,
            P.PersonaId,
            prt.realpageId,
            CASE
                WHEN ULP.StatusTypeId = 12 AND UL.LastLoginDate IS NULL THEN 'Pending'
                WHEN ULP.StatusTypeId = 12 AND UL.LastLoginDate IS NOT NULL THEN 'Active'
                ELSE st.Name
            END AS UserStatus
        FROM Ident.UserLogin UL
        INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
        INNER JOIN Enterprise.StatusType st ON ULP.StatusTypeId = st.StatusTypeId
        INNER JOIN Person.Persona P ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
        INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId
        INNER JOIN Person.Person PE ON PE.PartyId = UL.PersonPartyId
        INNER JOIN Enterprise.Party prt ON prt.PartyId = PE.PartyId
        INNER JOIN Enterprise.PartyRelationship PR ON PR.PartyIdFrom = PE.PartyId AND PR.PartyIdTo = O.PartyId
        INNER JOIN Enterprise.RoleType ert ON PR.RoleTypeIdFrom = ert.PartyRoleTypeId AND ert.ParentPartyRoleTypeId = 400
        INNER JOIN Ident.IdentityProviderType IPT ON IPT.IdentityProviderTypeId = UL.IdentityProviderTypeId
        INNER JOIN @partytable NE ON NE.PartyId = PE.PartyId
        WHERE (
                CHARINDEX(@Name, NE.NotificationEmail, 1) > 0
            )
        AND PR.Thrudate IS NULL
        AND (
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
            )
    )
    SELECT DISTINCT CompanyName, CompanyStatus, Username, NotificationEmail, UserType, FirstName, LastName, PartyId, UserId, ThirdPartyIDPDesc, PersonaId, PersonaRealPageId, UserStatus
    FROM resultList;
END
