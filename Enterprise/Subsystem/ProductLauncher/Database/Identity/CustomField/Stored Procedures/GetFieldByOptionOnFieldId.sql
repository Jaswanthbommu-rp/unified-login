
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Field table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].GetFieldByOptionOnFieldId (
@FieldId BIGINT
)
AS

BEGIN
	SELECT
		 [CustomField].[Field].[FieldId]
		,[CustomField].[Field].[OrganizationId]
		,[CustomField].[Field].[Enabled]
		,[CustomField].[Field].[Name]
		,[CustomField].[Field].[Description]
		,[CustomField].[Field].[FieldTypeId]
		,[CustomField].[Field].[Required]
		,[CustomField].[Field].[ReadOnly]
		,[CustomField].[Field].[DefaultValue]
		,[CustomField].[Field].[SyncField]
		,[CustomField].[Field].[Sequence]
		,[CustomField].[Field].[HelpText]
		,[CustomField].[Field].[CreatedDate]
		,[CustomField].[Field].[CreatedBy]
	FROM
		[CustomField].[Field]
	INNER Join
		[CustomField].[Option]
	On
		[CustomField].[Option].[FieldId] = [CustomField].[Field].[FieldId]
	WHERE
		[CustomField].[Option].[FieldId] = @FieldId

END