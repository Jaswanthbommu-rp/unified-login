CREATE PROCEDURE [Enterprise].[GetUserDetails_Ver01](
	@PersonaId BIGINT = NULL,
	@UserRealPageId UNIQUEIDENTIFIER = NULL
	)
AS
BEGIN
	DECLARE @NOW DATETIME= GETUTCDATE();
	DECLARE @PartyId BIGINT

	IF @PersonaId IS NOT NULL
	BEGIN
		SELECT @PartyId = ul.PersonPartyId
			FROM Ident.UserLoginPersona ulp 
			INNER JOIN Person.Persona P ON P.UserLoginPersonaId = ulp.UserLoginPersonaId
			INNER JOIN Ident.UserLogin ul ON ul.UserId = ulp.UserLoginId
		WHERE
			P.PersonaId = @PersonaId

		;WITH Email
		(
			PartyId,
			ElectronicAddressString
		)
		AS
		(
			SELECT	pcm.PartyId,
							ea.ElectronicAddressString
			FROM		Enterprise.ContactMechanismUsage CMU
							INNER JOIN Enterprise.PartyContactMechanism PCM ON PCM.PartyContactMechanismId = CMU.PartyContactMechanismID
							INNER JOIN Enterprise.ElectronicAddress EA ON EA.ContactMechanismID = PCM.ContactMechanismID
			WHERE	((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
				AND pcm.PartyId = @PartyId
		),
		TeleComm
		(
			PartyId,
			CountryCode,
			AreaCOde,
			PhoneNumber,
			RowNumber
		)
		AS
		(
			SELECT	pcm.PartyId,
							tm.CountryCode,
							tm.AreaCOde,
							tm.PhoneNumber,
							ROW_NUMBER() OVER(PARTITION BY pcm.partyid ORDER BY tm.[Default] DESC, tm.ContactMechanismID ASC ) AS rn
			FROM	Enterprise.ContactMechanismUsage cmu
						INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
						INNER JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = pcm.ContactMechanismID
			WHERE ((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
				AND pcm.PartyId = @PartyId
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
						CASE 
						WHEN ULP.StatusTypeId NOT IN (1, 2, 12) THEN 'false'
						WHEN ULP.StatusTypeId = 12 AND ULP.StatusThruDate IS NOT NULL AND ULP.StatusThruDate < @NOW THEN 'false'
						ELSE 'true'
						END AS IsActive,
						ULP.IsRPEmployee
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
					LEFT OUTER JOIN TeleComm TC ON TC.PartyId = P.PartyId AND TC.RowNumber = 1
		WHERE	PR.ThruDate IS NULL
		AND		PEA.PersonaId = @PersonaId
	END
	ELSE
	BEGIN
		SELECT @PartyId = PartyId
			FROM Enterprise.Party
		WHERE
			RealPageId = @UserRealPageId

		;WITH Email
		(
			PartyId,
			ElectronicAddressString
		)
		AS
		(
			SELECT	PCM.PartyId,
							EA.ElectronicAddressString
			FROM		Enterprise.ContactMechanismUsage CMU
							INNER JOIN Enterprise.PartyContactMechanism PCM ON PCM.PartyContactMechanismId = CMU.PartyContactMechanismID
							INNER JOIN Enterprise.ElectronicAddress EA ON EA.ContactMechanismID = PCM.ContactMechanismID
			WHERE	((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
				AND pcm.PartyId = @PartyId
		),
		TeleComm
		(
			PartyId,
			CountryCode,
			AreaCOde,
			PhoneNumber,
			RowNumber
		)
		AS
		(
			SELECT	pcm.PartyId,
							tm.CountryCode,
							tm.AreaCOde,
							tm.PhoneNumber,
							ROW_NUMBER() OVER(PARTITION BY pcm.partyid ORDER BY tm.[Default] DESC, tm.ContactMechanismID ASC ) AS rn
			FROM	Enterprise.ContactMechanismUsage cmu
						INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
						INNER JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = pcm.ContactMechanismID
			WHERE	((pcm.ThruDate IS NULL) OR (pcm.ThruDate > @NOW))
				AND pcm.PartyId = @PartyId
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
						RT.Name UserRoleType,CASE 
						WHEN ULP.StatusTypeId NOT IN (1, 2, 12) THEN 'false'
						WHEN ULP.StatusTypeId = 12 AND ULP.StatusThruDate IS NOT NULL AND ULP.StatusThruDate < @NOW THEN 'false'
						ELSE 'true'
						END AS IsActive,
						ULP.IsRPEmployee
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
					LEFT OUTER JOIN TeleComm TC ON TC.PartyId = P.PartyId AND TC.RowNumber = 1
		WHERE	PR.ThruDate IS NULL
		AND		P.RealPageId = @UserRealPageId
	END
END;
