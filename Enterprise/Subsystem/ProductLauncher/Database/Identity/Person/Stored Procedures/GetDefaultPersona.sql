CREATE PROCEDURE [Person].[GetDefaultPersona]
		@RealPageId UNIQUEIDENTIFIER
AS
BEGIN

	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	PE.PersonaId, 
			PE.PersonaTypeId as PersonaType, 
			ppt.Name 
	FROM	Person.Persona PE
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
			INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
			INNER JOIN Enterprise.Party P ON UL.PersonPartyId = P.PartyId
			JOIN Person.PersonaType ppt ON ppt.PersonaTypeId = PE.PersonaTypeId	
	WHERE	P.RealPageId  = @RealPageId and PE.IsDefault = 'True'
	AND ((@NOW BETWEEN PE.FromDate AND PE.ThruDate) OR (@NOW >= PE.FromDate AND PE.ThruDate IS NULL))
END