 -- =============================================
-- Author:  Monte Jennings
-- Create date: 3/19/2019 4:04:38 PM
-- Description: Adds new/Updates the FieldValue table.
-- =============================================
CREATE PROCEDURE CustomField.[AddUpdateFieldFromJSON] (
	 @SourceId bigint
	 ,@DataImportApplicationId int
	 ,@JSON nvarchar(MAX)
	,@CreatedBy bigint
) 
AS 
SET NOCOUNT ON; 
BEGIN
	BEGIN TRY
		DECLARE @UTCDATE datetime = GETUTCDATE()
		DECLARE @Inserted TABLE (
			Id int
		)

		MERGE CustomField.Field AS target
		USING (
			SELECT	[FieldId],
							[OrganizationId],
							[Enabled],
							[Name],
							[Description],
							[FieldTypeId],
							[Required],
							[ReadOnly],
							[DefaultValue],
							[SyncField],
							[Sequence],
							[HelpText],
							[MinCharLength],
							[MaxCharLength],
							[CreatedDate],
							[CreatedBy]
			FROM OPENJSON(@JSON, N'$.customField')
			WITH (
				[FieldId] bigint '$.FieldId',
				[OrganizationId] bigint '$.OrganizationId',
				[Enabled] bit '$.Enabled',
				[Name] nvarchar(100) '$.Name',
				[Description] nvarchar(500) '$.Description',
				[FieldTypeId] tinyint '$.FieldTypeId',
				[Required] bit '$.Required',
				[ReadOnly] bit '$.ReadOnly',
				[DefaultValue] nvarchar(max) '$.DefaultValue',
				[SyncField] nvarchar(150) '$.SyncField',
				[Sequence] smallint '$.Sequence',
				[HelpText] nvarchar(max) '$.HelpText',
				[MinCharLength] int '$.MinCharLength',
				[MaxCharLength] int '$.MaxCharLength',
				[CreatedDate] datetime '$.CreatedDate',
				[CreatedBy] bigint '$.CreatedBy'
			)) AS source
		ON (target.[FieldId] = source.[FieldId])
		WHEN MATCHED THEN 
			UPDATE
			SET [Enabled] = source.[Enabled],
					[Name] = source.[Name],
					[Description] = source.[Description],
					[FieldTypeId] = source.[FieldTypeId],
					[Required] = source.[Required],
					[ReadOnly] = source.[ReadOnly],
					[DefaultValue] = source.[DefaultValue],
					[SyncField] = source.[SyncField],
					[Sequence] = source.[Sequence],
					[HelpText] = source.[Helptext],
					[MinCharLength] = source.[MinCharLength],
					[MaxCharLength] = source.[MaxCharLength],
					[CreatedBy] = @CreatedBy, 
					[CreatedDate] = @UTCDATE
		WHEN NOT MATCHED THEN
			INSERT (
				[OrganizationId],
				[Enabled],
				[Name],
				[Description],
				[FieldTypeId],
				[Required],
				[ReadOnly],
				[DefaultValue],
				[SyncField],
				[Sequence],
				[HelpText],
				[MinCharLength],
				[MaxCharLength],
				[CreatedDate],
				[CreatedBy]
			)
			VALUES (
				source.[OrganizationId],
				source.[Enabled],
				source.[Name],
				source.[Description],
				source.[FieldTypeId],
				source.[Required],
				source.[ReadOnly],
				source.[DefaultValue],
				source.[SyncField],
				source.[Sequence],
				source.[HelpText],
				source.[MinCharLength],
				source.[MaxCharLength],
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
END;  