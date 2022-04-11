Create PROCEDURE [Batch].[InsertBatchPersonaIdToPrimaryPropertiesBatchProcess]( 
	@EditorPersonaId BIGINT, 
	@PersonaIdList [Enterprise].[IntListType] READONLY
)
AS
BEGIN
	BEGIN TRY  
		INSERT INTO Batch.[PrimaryPropertiesBatchProcess] (EditorUserPersonaId,SubjectUserPersonaId,StatusTypeId,  
			CreatedDateTime, BatchProcessTypeId)
		SELECT @EditorPersonaId, PIL.ID, 5, GETUTCDATE(), 13
		FROM   @PersonaIdList PIL 
		
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
