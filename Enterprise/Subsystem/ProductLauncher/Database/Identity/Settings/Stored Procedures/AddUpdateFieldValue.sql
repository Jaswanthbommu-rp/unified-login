Create PROCEDURE [Settings].[AddUpdateFieldValue] (
	 @JSON nvarchar(MAX),
	 @CreatedBy bigint
) 
AS 
SET NOCOUNT ON 
BEGIN
	BEGIN TRY
		DECLARE @UTCDATE datetime = GETUTCDATE()
		DECLARE @Inserted TABLE (
			Id int
		)

		MERGE [Settings].[SettingTableRowValue] AS target
		USING (
			SELECT	[FieldValueId],
					[UserLoginPersonaId],
					[FieldId],
					[Value]
			FROM OPENJSON(@JSON)
			WITH (
				[FieldValueId] bigint '$.FieldValueId',
				[UserLoginPersonaId] bigint '$.UserLoginPersonaId',
				[FieldId] bigint '$.FieldId',
				[Value] nvarchar(100) '$.Value'
			)) AS source
		ON (target.SettingTableRowValueId = source.FieldValueId)
		WHEN MATCHED AND LEN(source.[Value]) = 0 THEN
			DELETE
		WHEN MATCHED THEN
			UPDATE
			SET	[Value] = source.[Value],
					[UpdatedDate] = @UTCDATE,
					[ModifiedBy] = @CreatedBy
		WHEN NOT MATCHED AND LEN([Value]) > 0 THEN
			INSERT (
				UserLoginPersonaId,
				SettingTableRowId,
				Value,
				CreatedDate,
				ModifiedBy
			)
			VALUES (
				source.UserLoginPersonaId,
				source.FieldId,
				source.Value,
				@UTCDATE,
				@CreatedBy
			);

		SELECT	@@ROWCOUNT AS Id,
						'' AS ErrorMessage
	END TRY  
	BEGIN CATCH
        DECLARE @ErrorLogID int;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
					ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
	END CATCH
END
