CREATE PROCEDURE [CustomField].[AddUpdateDeleteCustomField] (
	 @CFJsonData nvarchar(MAX)
	,@PartyId bigint
	,@Operation varchar(10)
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

		DECLARE @CustomField TABLE (
			CustomFieldId int identity,
			OrganizationId bigint  NULL,
			[Enabled] bit  NULL,
			[Name] nvarchar(100)  NULL,
			Description nvarchar(500) NULL,
			FieldTypeId tinyint  NULL,
			[Required] bit NULL,
			[ReadOnly] bit NULL,
			DefaultValue nvarchar(max) NULL,
			SyncField nvarchar(150) NULL,
			[Sequence] smallint ,
			HelpText varchar(max) NULL,
			MinCharLength int NULL,
			MaxCharLength int NULL)

		INSERT INTO @CustomField ([OrganizationId],[Enabled],[Name],[Description],[FieldTypeId],
					[Required],[ReadOnly],[DefaultValue],[SyncField],[Sequence],[HelpText],
					[MinCharLength],[MaxCharLength])
		SELECT		[OrganizationId],
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
					[MaxCharLength]
			FROM OPENJSON(@CFJsonData)
			WITH (
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
				[MaxCharLength] int '$.MaxCharLength')
			WHERE ISJSON(@CFJsonData) > 0

		IF (@Operation = 'delete')
        BEGIN
            Delete From CustomField.Field
            Where OrganizationId = @PartyId
            AND [Name] IN (SELECT [Name] FROM @CustomField)

			SELECT	0 AS Id,
						'' AS ErrorMessage;
        END
        ELSE IF (@Operation = 'add')
		BEGIN
			Declare @maxSeqId int, @maxFieldId int
			Select @maxSeqId = ISNULL(MAX([Sequence]) ,0)
			FROM CustomField.Field
			Where OrganizationId = @PartyId

			INSERT INTO CustomField.Field ([OrganizationId],[Enabled],[Name],[Description],[FieldTypeId],
					[Required],[ReadOnly],[DefaultValue],[SyncField],[Sequence],[HelpText],
					[MinCharLength],[MaxCharLength],[CreatedDate],[CreatedBy])
			SELECT	[OrganizationId],
					ISNULL([Enabled],1),
					[Name],
					[Description],
					[FieldTypeId],
					[Required],
					[ReadOnly],
					[DefaultValue],
					[SyncField],
					@maxSeqId + CustomFieldId,
					[HelpText],
					[MinCharLength],
					[MaxCharLength],
					@UTCDATE,
					@CreatedBy
				FROM @CustomField

			SELECT	@@ROWCOUNT AS Id,
						'' AS ErrorMessage;
		END
		ELSE IF (@Operation = 'update')
		BEGIN
			UPDATE CustomField.Field 
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
			FROM @CustomField source
			Where source.OrganizationId = @PartyId

			SELECT	@@ROWCOUNT AS Id,
						'' AS ErrorMessage;
        END		
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