CREATE PROCEDURE [Security].[GetPersonasAssignedToRole] (
	@RoleId int)
AS
BEGIN
  SELECT P.PersonaId, P.UserLoginPersonaId, P.PersonaTypeId, P.PersonaEnvironmentTypeId, P.FromDate, P.ThruDate, P.IsDefault
  FROM [Security].[PersonaRole] PR
  INNER JOIN [Person].[Persona] P ON P.PersonaId = PR.PersonaId
  WHERE PR.RoleId = @RoleId
END
