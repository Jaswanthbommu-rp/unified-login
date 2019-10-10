
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the Field table.
-- =============================================
CREATE PROCEDURE [CustomField].[AddUpdateField] (
	 @FieldId BIGINT
	,@OrganizationId BIGINT
	,@Enabled BIT
	,@Name NVARCHAR(100)
	,@Description NVARCHAR(500)
	,@FieldTypeId TINYINT
	,@Required BIT
	,@ReadOnly BIT
	,@DefaultValue NTEXT
	,@SyncField NVARCHAR(150)
	,@Sequence SMALLINT
	,@HelpText TEXT
	,@CreatedDate DATETIME
	,@CreatedBy BIGINT
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[Field] WHERE [FieldId] = @FieldId) 
	BEGIN
		UPDATE [CustomField].[Field] SET 
			 [OrganizationId] = @OrganizationId
			,[Enabled] = @Enabled
			,[Name] = @Name
			,[Description] = @Description
			,[FieldTypeId] = @FieldTypeId
			,[Required] = @Required
			,[ReadOnly] = @ReadOnly
			,[DefaultValue] = @DefaultValue
			,[SyncField] = @SyncField
			,[Sequence] = @Sequence
			,[HelpText] = @HelpText
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[FieldId] = @FieldId

		SELECT			
			 [FieldId]
			,[OrganizationId]
			,[Enabled]
			,[Name]
			,[Description]
			,[FieldTypeId]
			,[Required]
			,[ReadOnly]
			,[DefaultValue]
			,[SyncField]
			,[Sequence]
			,[HelpText]
			,[CreatedDate]
			,[CreatedBy]
		FROM
			[CustomField].[Field]
		WHERE
			[FieldId] = @FieldId
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[Field] (
			 [OrganizationId]
			,[Enabled]
			,[Name]
			,[Description]
			,[FieldTypeId]
			,[Required]
			,[ReadOnly]
			,[DefaultValue]
			,[SyncField]
			,[Sequence]
			,[HelpText]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[FieldId]
			,inserted.[OrganizationId]
			,inserted.[Enabled]
			,inserted.[Name]
			,inserted.[Description]
			,inserted.[FieldTypeId]
			,inserted.[Required]
			,inserted.[ReadOnly]
			,inserted.[DefaultValue]
			,inserted.[SyncField]
			,inserted.[Sequence]
			,inserted.[HelpText]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @OrganizationId
			,@Enabled
			,@Name
			,@Description
			,@FieldTypeId
			,@Required
			,@ReadOnly
			,@DefaultValue
			,@SyncField
			,@Sequence
			,@HelpText
			,@CreatedDate
			,@CreatedBy);

	

	END;