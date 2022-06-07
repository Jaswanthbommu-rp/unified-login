CREATE PROCEDURE [Enterprise].[AddUpdatePersonaProductPropertyInstanceMapping] (
@Personas	[Enterprise].[SyncPersonaList] READONLY,
@ProductId int)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Now datetime = GETUTCDATE()
	declare @MAX_ID INT
	declare @Current_ID INT = 1
	BEGIN TRY
		DECLARE @personaList TABLE (
			Id int identity,
			PersonaId bigint
		)

		Insert Into @personaList(PersonaId)
		Select PersonaId From @Personas

		SELECT @MAX_ID = max(Id) from @personaList

		WHILE @Current_ID <= @MAX_ID
		BEGIN
			Declare @personaId bigint
			SELECT @personaId = PersonaId
			FROM @personaList WHERE Id = @Current_ID

			INSERT INTO Enterprise.PropertyInstanceMapping (
						PersonaId,
						ProductId,
						PropertyInstanceId
			)
			SELECT @personaId,
				   @ProductId,
				   PropertyInstanceId
			FROM [Enterprise].[UserSyncProductPrimaryPropertiesStaging]
			WHERE PersonaId = @personaId
			AND ProductId = @ProductId
			EXCEPT
			SELECT PersonaId,ProductId,PropertyInstanceId
			FROM	Enterprise.PropertyInstanceMapping
			WHERE PersonaId = @personaId
			AND ProductId = @ProductId
			AND ThruDate IS NULL

			IF NOT EXISTS(Select 1 FROM Enterprise.PersonaProductPropertiesSyncHistory WHERE PersonaId = @personaId AND ProductId = @ProductId)
			BEGIN
				INSERT INTO Enterprise.PersonaProductPropertiesSyncHistory(PersonaId,ProductId,ProductPropertiesSyncDate)
				SELECT @personaId,@ProductId,@Now
			END
			ELSE
			BEGIN
				UPDATE Enterprise.PersonaProductPropertiesSyncHistory SET ProductPropertiesSyncDate = @Now
				WHERE PersonaId = @personaId
				AND ProductId = @ProductId
			END

			Set @Current_ID = @Current_ID + 1
		END

		SELECT	1 AS Id,'' AS ErrorMessage;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID INT;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
	
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END
