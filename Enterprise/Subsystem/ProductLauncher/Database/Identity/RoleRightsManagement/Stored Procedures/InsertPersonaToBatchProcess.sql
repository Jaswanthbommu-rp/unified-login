CREATE PROCEDURE [Security].[InsertPersonaToBatchProcess]( 
	@EditorPersonaId BIGINT, 
	@EnterpriseRoleId BIGINT,
	@UseAPIV2 BIT = 0
	)
AS
	BEGIN
		INSERT INTO Batch.[EnterpriseRoleBatchProcess] (EditorUserPersonaId,SubjectUserPersonaId,EnterpriseRoleTemplateId,StatusTypeId,  
			CreatedDateTime, BatchProcessTypeId, UseAPIV2)
		SELECT @EditorPersonaId, PersonaId, @EnterpriseRoleId, 5, GETUTCDATE(), 11, @UseAPIV2
		FROM Security.RoleTemplateUserMapping
		WHERE RoleTemplateId = @EnterpriseRoleId
	END
