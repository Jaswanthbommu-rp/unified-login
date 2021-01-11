CREATE PROCEDURE [Person].[ListPersonsForSupportTool](@Name NVARCHAR(50) = NULL)
AS
BEGIN

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
	WHERE	(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())
	AND (ea.ElectronicAddressString <> '' AND CHARINDEX(@Name, ea.ElectronicAddressString, 1) > 0)
	AND			cmu.ContactMechanismUsageTypeID = 301

	;WITH resultList ( CompanyName, Username, NotificationEmail, UserType, FirstName, LastName, PartyId, UserId, ThirdPartyIDPDesc )
	AS (
		SELECT
			O.Name [CompanyName],
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
			IPT.Description [ThirdPartyIDPDesc]
		FROM	Ident.UserLogin UL
					INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
					INNER JOIN Person.Persona P ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
					INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId
					INNER JOIN Person.Person PE ON PE.PartyId = UL.PersonPartyId
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
		UNION ALL
		SELECT
			O.Name [CompanyName],
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
			IPT.Description [ThirdPartyIDPDesc]
		FROM	Ident.UserLogin UL
					INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
					INNER JOIN Person.Persona P ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
					INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId
					INNER JOIN Person.Person PE ON PE.PartyId = UL.PersonPartyId
					INNER JOIN Enterprise.PartyRelationship PR ON (PR.PartyIdFrom = PE.PartyId AND pr.PartyIdTo = o.PartyId)
					INNER JOIN Enterprise.RoleType ert ON (pr.RoleTypeIdFrom = ert.PartyRoleTypeId AND ert.ParentPartyRoleTypeId = 400)
					INNER JOIN Ident.IdentityProviderType IPT ON IPT.IdentityProviderTypeId = UL.IdentityProviderTypeId
					INNER JOIN @partytable NE ON NE.PartyId = PE.PartyId
		WHERE	(
							 (CHARINDEX(@Name, NE.NotificationEmail, 1) > 0)
						)
		AND		PR.Thrudate IS NULL
	) 
	SELECT 
		DISTINCT CompanyName, Username, NotificationEmail, UserType, FirstName, LastName, PartyId, UserId, ThirdPartyIDPDesc  
	FROM 
		resultList
END;
