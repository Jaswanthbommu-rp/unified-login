Create Procedure [Enterprise].[GetOrganizationHasPersonaProductError] (@PartyId bigint)
AS
Begin
	
  SELECT CASE WHEN COUNT(PPE.PersonaId) > 0 THEN 1 ELSE 0 End
  FROM Enterprise.PersonaProductError PPE  
  INNER JOIN Person.Persona PE ON PE.PersonaId = PPE.PersonaId  
  INNER JOIN Ident.UserLoginPersona ULP ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId
  WHERE  ULP.OrganizationPartyId = @PartyId 
End
