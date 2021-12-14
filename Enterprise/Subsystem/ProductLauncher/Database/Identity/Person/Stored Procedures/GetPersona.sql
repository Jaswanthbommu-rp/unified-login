CREATE PROCEDURE [Person].[GetPersona](@PersonaId BIGINT)
AS
     BEGIN
         DECLARE @NOW DATETIME= GETUTCDATE();
         SELECT pe.PersonaId,
                UL.PersonPartyId,
                p.RealPageId,
                ULP.OrganizationPartyId,
                pe.PersonaTypeId,
                pe.PersonaEnvironmentTypeId,
                pe.PersonaName AS 'Name',
                pe.FromDate,
                pe.ThruDate,
                pe.IsDefault,
                UL.UserId,
                PR.RoleTypeIdFrom 'UserTypeId',
				OD.Name as OrganizationDomain
		FROM Person.Persona PE
		INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
		INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
		INNER JOIN Enterprise.Party p ON UL.PersonPartyId = P.PartyId
		INNER JOIN Enterprise.PartyRelationship PR ON(PR.PartyIdFrom = UL.PersonPartyId AND PR.PartyIdTo = ULP.OrganizationPartyId
                                                            AND PR.RoleTypeIdTo = 205
                                                            AND PR.ThruDate IS NULL)
		INNER JOIN Enterprise.Organization O ON ULP.OrganizationPartyId = O.PartyId
		INNER JOIN Enterprise.OrganizationDomain OD on O.OrganizationDomainId = OD.OrganizationDomainId AND OD.ThruDate IS NULL
         WHERE pe.PersonaId = @PersonaId
               AND ((@NOW BETWEEN pe.FromDate AND pe.ThruDate)
                    OR (@NOW >= pe.FromDate
                        AND pe.ThruDate IS NULL));
     END;
