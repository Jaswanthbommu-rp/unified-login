CREATE PROCEDURE [Security].[GetADGroupsByPersona]
(
@PersonaId bigint
)
AS
BEGIN
	SELECT  
		ADG.ADGroupId
		,ADU.CreatedDate 
		,GETUTCDATE() as SystemDBDateTime
	FROM Security.ADGroupUser  ADU
	INNER JOIN Security.ADGroup ADG ON ADG.ADGroupId = ADU.ADGroupId
	WHERE PersonaId=@PersonaId AND ADG.IsActive = 1
END