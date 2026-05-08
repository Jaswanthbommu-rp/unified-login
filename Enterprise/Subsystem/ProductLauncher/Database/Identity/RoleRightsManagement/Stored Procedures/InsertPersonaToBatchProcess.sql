CREATE PROCEDURE [Security].[InsertPersonaToBatchProcess]( 
	@EditorPersonaId BIGINT, 
	@EnterpriseRoleId BIGINT,
	@UseAPIV2 BIT = 0
	)
AS
	BEGIN
		INSERT INTO Batch.[EnterpriseRoleBatchProcess] (EditorUserPersonaId,SubjectUserPersonaId,EnterpriseRoleTemplateId,StatusTypeId,  
			CreatedDateTime, BatchProcessTypeId, UseAPIV2)
		SELECT @EditorPersonaId, RTUM.PersonaId, @EnterpriseRoleId, 5, GETUTCDATE(), 11, @UseAPIV2
		FROM Security.RoleTemplateUserMapping RTUM
		INNER JOIN Person.Persona p ON p.PersonaId = RTUM.PersonaId
		INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
        WHERE RTUM.RoleTemplateId = @EnterpriseRoleId
            AND ulp.StatusTypeId NOT IN (19,24)
	END
