CREATE PROCEDURE [Enterprise].[GetUserDetails_Ver01](
	@PersonaId BIGINT = NULL,
	@UserRealPageId UNIQUEIDENTIFIER = NULL
	)
AS
BEGIN
	DECLARE @NOW datetime= GETUTCDATE();

	if @PersonaId is not null
	BEGIN
		;WITH Email
		(
			PartyId,
			ElectronicAddressString
		)
		AS
		(
			SELECT	PartyId,
							ElectronicAddressString
			FROM		Enterprise.ContactMechanismUsage CMU
							INNER JOIN Enterprise.PartyContactMechanism PCM ON PCM.PartyContactMechanismId = CMU.PartyContactMechanismID
							INNER JOIN Enterprise.ContactMechanism CM ON CM.ContactMechanismID = PCM.ContactMechanismId
							INNER JOIN Enterprise.ElectronicAddress EA ON EA.ContactMechanismID = CM.ContactMechanismID
			WHERE	((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
		),
		TeleComm
		(
			PartyId,
			CountryCode,
			AreaCOde,
			PhoneNumber
		)
		AS
		(
			SELECT	PartyId,
							CountryCode,
							AreaCOde,
							PhoneNumber
			FROM	Enterprise.ContactMechanismUsage cmu
						INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
						INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
						INNER JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
			WHERE	((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
		)

		SELECT	PER.PartyId AS PersonPartyId,
						P.RealPageId AS UserRealPageId,
						PER.FirstName,
						PER.MiddleName,
						PER.LastName,
						PEA.PersonaId,
						UL.UserId,
						UL.LoginName,
						ULP.FromDate,
						ULP.ThruDate,
						EM.ElectronicAddressString AS Email,
						TC.AreaCode + TC.PhoneNumber AS PhoneNumber,
						DIM.PartyId AS OrganizationPartyId,
						COALESCE(DIM.MasterId, 0) AS BooksMasterId,
						COALESCE(DIM.CompanyMasterId, 0) AS BooksCustomerMasterId,
						OD.Name AS OrganizationDomain,
						RT.PartyRoleTypeId AS UserRoleTypeId,
						RT.Name UserRoleType,
						CASE WHEN ULP.StatusTypeId not in ( 1,2 ) THEN 'false' ELSE 'true' END AS IsActive
		FROM	Enterprise.Party P
					INNER JOIN Person.Person PER ON PER.PartyId = P.PartyId
					INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId
					INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
					INNER JOIN Person.Persona PEA ON PEA.UserLoginPersonaId = ULP.UserLoginPersonaId
					INNER JOIN Enterprise.PartyRelationShip PR ON (PR.PartyIdFrom = UL.PersonPartyId AND pr.PartyIdTo = ULP.OrganizationPartyId)
					INNER JOIN Enterprise.RoleType RT ON (RT.PartyROleTypeId = PR.RoleTypeIdFrom AND RT.ParentPartyRoleTypeId = 400)
					INNER JOIN Enterprise.Organization O ON ULP.OrganizationPartyId = O.PartyId
					INNER JOIN Enterprise.OrganizationDomain OD on O.OrganizationDomainId = OD.OrganizationDomainId
					INNER JOIN [Enterprise].[VW_DataImportMapping] DIM ON DIM.PartyId = ULP.OrganizationPartyId
					LEFT OUTER JOIN Email EM ON EM.PartyId = P.PartyId
					LEFT OUTER JOIN TeleComm TC ON TC.PartyId = P.PartyId
		WHERE	PR.ThruDate IS NULL
		AND		PEA.PersonaId = @PersonaId
	END
	ELSE
	BEGIN
		;WITH Email
		(
			PartyId,
			ElectronicAddressString
		)
		AS
		(
			SELECT	PartyId,
							ElectronicAddressString
			FROM		Enterprise.ContactMechanismUsage CMU
							INNER JOIN Enterprise.PartyContactMechanism PCM ON PCM.PartyContactMechanismId = CMU.PartyContactMechanismID
							INNER JOIN Enterprise.ContactMechanism CM ON CM.ContactMechanismID = PCM.ContactMechanismId
							INNER JOIN Enterprise.ElectronicAddress EA ON EA.ContactMechanismID = CM.ContactMechanismID
			WHERE	((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
		),
		TeleComm
		(
			PartyId,
			CountryCode,
			AreaCOde,
			PhoneNumber
		)
		AS
		(
			SELECT	PartyId,
							CountryCode,
							AreaCOde,
							PhoneNumber
			FROM	Enterprise.ContactMechanismUsage cmu
						INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
						INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
						INNER JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
			WHERE	((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
		)

		SELECT	PER.PartyId AS PersonPartyId,
						P.RealPageId AS UserRealPageId,
						PER.FirstName,
						PER.MiddleName,
						PER.LastName,
						PEA.PersonaId,
						UL.UserId,
						UL.LoginName,
						ULP.FromDate,
						ULP.ThruDate,
						EM.ElectronicAddressString AS Email,
						TC.AreaCode + TC.PhoneNumber AS PhoneNumber,
						DIM.PartyId AS OrganizationPartyId,
						COALESCE(DIM.MasterId, 0) AS BooksMasterId,
						COALESCE(DIM.CompanyMasterId, 0) AS BooksCustomerMasterId,
						OD.Name AS OrganizationDomain,
						RT.PartyRoleTypeId AS UserRoleTypeId,
						RT.Name UserRoleType,
						CASE WHEN ULP.StatusTypeId not in ( 1,2 ) THEN 'false' ELSE 'true' END AS IsActive
		FROM	Enterprise.Party P
					INNER JOIN Person.Person PER ON PER.PartyId = P.PartyId
					INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId
					INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
					INNER JOIN Person.Persona PEA ON PEA.UserLoginPersonaId = ULP.UserLoginPersonaId
					INNER JOIN Enterprise.PartyRelationShip PR ON (PR.PartyIdFrom = UL.PersonPartyId AND pr.PartyIdTo = ULP.OrganizationPartyId)
					INNER JOIN Enterprise.RoleType RT ON (RT.PartyROleTypeId = PR.RoleTypeIdFrom AND RT.ParentPartyRoleTypeId = 400)
					INNER JOIN Enterprise.Organization O ON ULP.OrganizationPartyId = O.PartyId
					INNER JOIN Enterprise.OrganizationDomain OD on O.OrganizationDomainId = OD.OrganizationDomainId
					INNER JOIN [Enterprise].[VW_DataImportMapping] DIM ON DIM.PartyId = ULP.OrganizationPartyId
					LEFT OUTER JOIN Email EM ON EM.PartyId = P.PartyId
					LEFT OUTER JOIN TeleComm TC ON TC.PartyId = P.PartyId
		WHERE	PR.ThruDate IS NULL
		AND		P.RealPageId = @UserRealPageId
	END
END;
