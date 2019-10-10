CREATE PROCEDURE [Enterprise].[ListUserDetails_Ver01] 
(@PersonaId  BIGINT 
)
AS
     BEGIN
         WITH  Email
              AS (
					  SELECT PartyId,
							 ElectronicAddressString
					  FROM Enterprise.ContactMechanismUsage CMU
						   INNER JOIN Enterprise.PartyContactMechanism PCM ON PCM.PartyContactMechanismId = CMU.PartyContactMechanismID
						   INNER JOIN Enterprise.ContactMechanism CM ON CM.ContactMechanismID = PCM.ContactMechanismId
						   INNER JOIN Enterprise.ElectronicAddress EA ON EA.ContactMechanismID = CM.ContactMechanismID
					  WHERE pcm.ThruDate IS NULL
							OR pcm.ThruDate > GETUTCDATE()),
              TeleComm
              AS (
					  SELECT PartyId,
							 CountryCode,
							 AreaCOde,
							 PhoneNumber
					  FROM Enterprise.ContactMechanismUsage cmu
						   JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
						   JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
						   JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
					  WHERE pcm.ThruDate IS NULL
							OR pcm.ThruDate > GETUTCDATE())
              SELECT PER.PartyId,
					 P.RealPageId,
                     PER.FirstName,
                     PER.LastName,
                     PEA.PersonaId,
                     UL.UserID,
                     UL.LoginName,
                     EM.ElectronicAddressString,
                     CONVERT(VARCHAR, TC.AreaCode) + TC.PhoneNumber AS PhoneNumber,
					 COALESCE(ISNULL(DIM.MasterId, 0),0) AS BooksMasterId,
					 COALESCE(ISNULL(DIM.CompanyMasterId, 0),0)  AS BooksCustomerMasterId,
                     RT.Name UserRoleType
              FROM Enterprise.Party P
                   INNER JOIN Person.Person PER ON PER.PartyId = P.PartyId
                   INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId
				   INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
                   INNER JOIN Person.Persona PEA ON ULP.UserLoginPersonaId = PEA.UserLoginPersonaId
                   INNER JOIN Enterprise.PartyRelationShip PR ON PR.PartyIdFrom = UL.PersonPartyId
                   INNER JOIN Enterprise.RoleType RT ON RT.PartyROleTypeId = PR.RoleTypeIdFrom
				   INNER JOIN [Enterprise].[VW_DataImportMapping] DIM ON DIM.PartyId = ULP.OrganizationPartyId
				   
				   LEFT OUTER JOIN Email EM ON EM.PartyId = P.PartyId
                   LEFT OUTER JOIN TeleComm TC ON TC.PartyId = P.PartyId
              WHERE PR.RoleTypeIdFrom IN(400, 401, 402, 403, 404, 405)
                   AND (PEA.PersonaId = @PersonaId
                       OR @PersonaId IS NULL)
                   
     END;

