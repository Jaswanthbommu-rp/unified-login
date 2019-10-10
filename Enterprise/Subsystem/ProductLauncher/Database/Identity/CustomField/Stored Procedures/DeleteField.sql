
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the Field table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteField (
	 @FieldId BIGINT = NULL 
	,@OrganizationId BIGINT = NULL 
	,@Enabled BIT = NULL 
	,@Name VARCHAR(100) = NULL 
	,@Description VARCHAR(500) = NULL 
	,@FieldTypeId TINYINT = NULL 
	,@Required BIT = NULL 
	,@ReadOnly BIT = NULL 
	,@DefaultValue NVARCHAR(MAX) = NULL 
	,@SyncField VARCHAR(150) = NULL 
	,@Sequence SMALLINT = NULL 
	,@HelpText VARCHAR(MAX) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[Field]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@OrganizationId IS NULL  OR  [OrganizationId] = @OrganizationId)
	AND
		(@Enabled IS NULL  OR  [Enabled] = @Enabled)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@Description IS NULL  OR  [Description] = @Description)
	AND
		(@FieldTypeId IS NULL  OR  [FieldTypeId] = @FieldTypeId)
	AND
		(@Required IS NULL  OR  [Required] = @Required)
	AND
		(@ReadOnly IS NULL  OR  [ReadOnly] = @ReadOnly)
	AND
		(@DefaultValue IS NULL  OR  [DefaultValue] = @DefaultValue)
	AND
		(@SyncField IS NULL  OR  [SyncField] = @SyncField)
	AND
		(@Sequence IS NULL  OR  [Sequence] = @Sequence)
	AND
		(@HelpText IS NULL  OR  [HelpText] = @HelpText)

	DELETE 
		[CustomField].[Field]

	WHERE
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@OrganizationId IS NULL  OR  [OrganizationId] = @OrganizationId)
	AND
		(@Enabled IS NULL  OR  [Enabled] = @Enabled)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@Description IS NULL  OR  [Description] = @Description)
	AND
		(@FieldTypeId IS NULL  OR  [FieldTypeId] = @FieldTypeId)
	AND
		(@Required IS NULL  OR  [Required] = @Required)
	AND
		(@ReadOnly IS NULL  OR  [ReadOnly] = @ReadOnly)
	AND
		(@DefaultValue IS NULL  OR  [DefaultValue] = @DefaultValue)
	AND
		(@SyncField IS NULL  OR  [SyncField] = @SyncField)
	AND
		(@Sequence IS NULL  OR  [Sequence] = @Sequence)
	AND
		(@HelpText IS NULL  OR  [HelpText] = @HelpText)

	RETURN 1 --for success

END

