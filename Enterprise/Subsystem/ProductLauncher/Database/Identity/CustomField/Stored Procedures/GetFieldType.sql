-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the FieldType table.
-- =============================================
CREATE PROCEDURE [CustomField].GetFieldType (
	@FieldTypeId tinyint = NULL
) 
AS
BEGIN
	SELECT	[FieldTypeId]
					,[Name]
					,[Description]
					,[CreatedDate]
					,[CreatedBy]
	FROM	[CustomField].[FieldType]
	WHERE	((@FieldTypeId IS NULL) OR ([FieldTypeId] = @FieldTypeId))
END