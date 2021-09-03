CREATE PROCEDURE [person].[GetPersonaProductError](@PersonaId BIGINT)	
AS
Begin
	  Declare @personaHasProductError tinyint = 0;
      Select @personaHasProductError = 1 From Enterprise.PersonaProductError Where PersonaId = @PersonaId
	  Select @personaHasProductError
End
	
