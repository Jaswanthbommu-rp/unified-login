CREATE PROCEDURE [Person].[ListPersonsForSupportTool](@Name VARCHAR(50) = NULL,
@OrganizationTypeIds VARCHAR(MAX) = NULL)
AS
BEGIN


    -- Convert @OrganizationIds to a table variable
    DECLARE @OrgTypeIdsTable TABLE (OrgTypeId INT)
    IF @OrganizationTypeIds IS NOT NULL AND @OrganizationTypeIds <> ''
    BEGIN
        INSERT INTO @OrgTypeIdsTable (OrgTypeId)
        SELECT value FROM STRING_SPLIT(@OrganizationTypeIds, ',')
    END

	DECLARE @partytable TABLE ( partyid bigint NOT NULL, NotificationEmail varchar(200) NOT NULL )
	INSERT INTO @partytable
	(
		partyid,
		NotificationEmail
	)
	SELECT	p.PartyId,
					ea.ElectronicAddressString AS NotificationEmail
	FROM		Enterprise.ContactMechanismUsage cmu
					INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
					INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
					INNER JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
					INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
					INNER JOIN Person.Person PE ON PE.PartyId = p.PartyId
					INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = p.PartyId
	WHERE	(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())
	AND (	(@Name IS NULL)
			OR (CHARINDEX(@Name, FirstName + ' ' + LastName, 1) > 0)
			OR (CHARINDEX(@Name, ul.LoginName, 1) > 0)
			OR (ea.ElectronicAddressString <> '' AND CHARINDEX(@Name, ea.ElectronicAddressString, 1) > 0)
		)
	AND			cmu.ContactMechanismUsageTypeID = 301

	;WITH resultList ( CompanyName, CompanyStatus, Username, NotificationEmail, UserType, FirstName, LastName, PartyId, UserId, ThirdPartyIDPDesc, PersonaId, PersonaRealPageId, UserStatus, PrimaryOrganization )
	AS (
		SELECT
			O.Name [CompanyName],
			O.IsActive [CompanyStatus],
			UL.LoginName [Username],
			CASE WHEN ert.PartyRoleTypeId = 404 THEN NE.NotificationEmail ELSE ISNULL(NE.NotificationEmail,UL.LoginName) END AS [NotificationEmail],
			CASE
				WHEN ert.PartyRoleTypeId = 401 THEN 'Regular User'
				WHEN ert.PartyRoleTypeId = 402 THEN 'RealPage System Administrator'
				WHEN ert.PartyRoleTypeId = 403 THEN 'RealPage Employee'
				WHEN ert.PartyRoleTypeId = 404 THEN 'Regular User (No Email)'
				WHEN ert.PartyRoleTypeId = 405 THEN 'External User'
			END AS [UserType],
			PE.FirstName [FirstName],
			PE.LastName [LastName],
			ULP.OrganizationPartyId [PartyId],
			UL.UserId [UserId],
			IPT.Description [ThirdPartyIDPDesc],
			P.PersonaId,
			prt.realpageId,
			   (CASE          
        WHEN ((ULP.StatusTypeId = 12) AND (UL.LastLoginDate IS NULL)) THEN 'Pending'          
        WHEN ((ULP.StatusTypeId = 12) AND (UL.LastLoginDate IS NOT NULL)) THEN 'Active'          
        ELSE st.Name          
        END ) AS  UserStatus,
		ULP.PrimaryOrganization AS [PrimaryOrganization]
		FROM	Ident.UserLogin UL
					INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
					INNER JOIN Enterprise.StatusType st on ulp.StatusTypeId = st.StatusTypeId
					INNER JOIN Person.Persona P ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
					INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId
					INNER JOIN Person.Person PE ON PE.PartyId = UL.PersonPartyId
					INNER JOIN Enterprise.Party prt ON prt.PartyId = PE.PartyId
					INNER JOIN Enterprise.PartyRelationship PR ON (PR.PartyIdFrom = PE.PartyId AND pr.PartyIdTo = o.PartyId)
					INNER JOIN Enterprise.RoleType ert ON (pr.RoleTypeIdFrom = ert.PartyRoleTypeId AND ert.ParentPartyRoleTypeId = 400)
					INNER JOIN Ident.IdentityProviderType IPT ON IPT.IdentityProviderTypeId = UL.IdentityProviderTypeId
					LEFT OUTER JOIN @partytable NE ON NE.PartyId = PE.PartyId
		WHERE	(
							(@Name IS NULL)
							OR (CHARINDEX(@Name, FirstName + ' ' + LastName, 1) > 0)
							OR (CHARINDEX(@Name, ul.LoginName, 1) > 0)
							--OR (CHARINDEX(@Name, NE.NotificationEmail, 1) > 0)
						)
		AND		PR.Thrudate IS NULL
		AND (
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
            )

		UNION ALL
		SELECT
			O.Name [CompanyName],
			O.IsActive [CompanyStatus],
			UL.LoginName [Username],
			NE.NotificationEmail [NotificationEmail],
			CASE
				WHEN ert.PartyRoleTypeId = 401 THEN 'Regular User'
				WHEN ert.PartyRoleTypeId = 402 THEN 'RealPage System Administrator'
				WHEN ert.PartyRoleTypeId = 403 THEN 'RealPage Employee'
				WHEN ert.PartyRoleTypeId = 404 THEN 'Regular User (No Email)'
				WHEN ert.PartyRoleTypeId = 405 THEN 'External User'
			END AS [UserType],
			PE.FirstName [FirstName],
			PE.LastName [LastName],
			ULP.OrganizationPartyId [PartyId],
			UL.UserId [UserId],
			IPT.Description [ThirdPartyIDPDesc],
			P.PersonaId,
			prt.realpageId,
	    (CASE          
        WHEN ((ULP.StatusTypeId = 12) AND (UL.LastLoginDate IS NULL)) THEN 'Pending'          
        WHEN ((ULP.StatusTypeId = 12) AND (UL.LastLoginDate IS NOT NULL)) THEN 'Active'          
        ELSE st.Name          
        END ) AS  UserStatus,
		ULP.PrimaryOrganization AS [PrimaryOrganization] 

		FROM	Ident.UserLogin UL
					INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
					INNER JOIN Enterprise.StatusType st on ulp.StatusTypeId = st.StatusTypeId
					INNER JOIN Person.Persona P ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
					INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId
					INNER JOIN Person.Person PE ON PE.PartyId = UL.PersonPartyId
					INNER JOIN Enterprise.Party prt ON prt.PartyId = PE.PartyId
					INNER JOIN Enterprise.PartyRelationship PR ON (PR.PartyIdFrom = PE.PartyId AND pr.PartyIdTo = o.PartyId)
					INNER JOIN Enterprise.RoleType ert ON (pr.RoleTypeIdFrom = ert.PartyRoleTypeId AND ert.ParentPartyRoleTypeId = 400)
					INNER JOIN Ident.IdentityProviderType IPT ON IPT.IdentityProviderTypeId = UL.IdentityProviderTypeId
					INNER JOIN @partytable NE ON NE.PartyId = PE.PartyId
		WHERE	(
							 (CHARINDEX(@Name, NE.NotificationEmail, 1) > 0)
						)
		AND		PR.Thrudate IS NULL
		AND (
                @OrganizationTypeIds IS NULL OR @OrganizationTypeIds = '' 
                OR O.OrganizationTypeId IN (SELECT OrgTypeId FROM @OrgTypeIdsTable)
            )
	) 
	SELECT 
		DISTINCT CompanyName, CompanyStatus, Username, NotificationEmail, UserType, FirstName, LastName, PartyId, UserId, ThirdPartyIDPDesc, PersonaId , PersonaRealPageId, UserStatus, PrimaryOrganization
	FROM 
		resultList
END;
