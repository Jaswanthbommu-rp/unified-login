CREATE PROCEDURE [Security].[GetADGroupsByPersona]
(
@PersonaId bigint
)
AS
BEGIN
	SELECT  
		ADGroupId
		,CreatedDate 
		,GETUTCDATE() as SystemDBDateTime
	FROM Security.ADGroupUser  
	WHERE PersonaId=@PersonaId
END