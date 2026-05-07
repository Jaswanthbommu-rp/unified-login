CREATE PROCEDURE [Security].[InsertBatchPersonaIdToEnterpriseRoleBatchProcess]( 
	@EditorPersonaId BIGINT, 
	@PersonaIdList [Enterprise].[IntListType] READONLY,
	@UseAPIV2 BIT = 0
)
AS
BEGIN
	BEGIN TRY  
		INSERT INTO Batch.[EnterpriseRoleBatchProcess] (EditorUserPersonaId,SubjectUserPersonaId,EnterpriseRoleTemplateId,StatusTypeId,  
			CreatedDateTime, BatchProcessTypeId, UseAPIV2)
		SELECT @EditorPersonaId, RTUM.PersonaId, RTUM.RoleTemplateId, 5, GETUTCDATE(), 15, @UseAPIV2
		FROM Security.RoleTemplateUserMapping RTUM
			INNER JOIN @PersonaIdList PIL ON PIL.ID = RTUM.PersonaId
			INNER JOIN Person.Persona p ON p.PersonaId = PIL.ID
			INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
            WHERE ulp.StatusTypeId NOT IN (19,24)
		
		SELECT	@EditorPersonaId AS Id, '' AS ErrorMessage
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END
