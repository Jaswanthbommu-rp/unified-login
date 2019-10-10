
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Field table.
-- =============================================
CREATE PROCEDURE [CustomField].GetField (
	 @FieldId BIGINT) 

 AS 

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