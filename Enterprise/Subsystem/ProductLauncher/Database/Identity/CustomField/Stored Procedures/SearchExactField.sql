
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Field table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].[SearchExactField] (
	 @FieldId BIGINT = NULL 
	,@OrganizationId BIGINT = NULL 
	,@Enabled BIT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@Description NVARCHAR(500) = NULL 
	,@FieldTypeId TINYINT = NULL 
	,@Required BIT = NULL 
	,@ReadOnly BIT = NULL 
	,@DefaultValue NVARCHAR(MAX) = NULL 
	,@SyncField NVARCHAR(150) = NULL 
	,@Sequence SMALLINT = NULL 
	,@HelpText NVARCHAR(MAX) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy BIGINT = NULL 
)

AS

BEGIN

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
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)

END