CREATE PROCEDURE [Security].[InsertPersonasForBatchProcess] ( @EditorPersonaId BIGINT, @EnterpriseRoleId BIGINT)
AS
	BEGIN
		INSERT INTO Batch.[BatchProcessEnterpriseRoleProductUpdate] (EditorUserPersonaId,SubjectUserPersonaId,EnterpriseRoleTemplateId,StatusTypeId,  
			CreatedDateTime, BatchProcessTypeId)
		SELECT @EditorPersonaId, PersonaId, @EnterpriseRoleId, 5, GETUTCDATE(), 11
		FROM Security.RoleTemplateUserMapping
		WHERE RoleTemplateId = @EnterpriseRoleId
	END