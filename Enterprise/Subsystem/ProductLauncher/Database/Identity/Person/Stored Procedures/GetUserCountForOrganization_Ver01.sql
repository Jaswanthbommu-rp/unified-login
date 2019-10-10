CREATE PROCEDURE Person.GetUserCountForOrganization_Ver01(@PartyId BIGINT)
AS
     BEGIN
         SELECT COUNT(1)
         FROM Person.Persona PE
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
			INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId
         WHERE O.PartyId = @PartyId;
     END;