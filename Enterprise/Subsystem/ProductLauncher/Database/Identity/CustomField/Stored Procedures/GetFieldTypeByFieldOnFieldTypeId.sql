
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the FieldType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].GetFieldTypeByFieldOnFieldTypeId (
@FieldTypeId TINYINT
)
AS

BEGIN
	SELECT
		 [CustomField].[FieldType].[FieldTypeId]
		,[CustomField].[FieldType].[Name]
		,[CustomField].[FieldType].[Description]
		,[CustomField].[FieldType].[CreatedDate]
		,[CustomField].[FieldType].[CreatedBy]
	FROM
		[CustomField].[FieldType]
	INNER Join
		[CustomField].[Field]
	On
		[CustomField].[Field].[FieldTypeId] = [CustomField].[FieldType].[FieldTypeId]
	WHERE
		[CustomField].[Field].[FieldTypeId] = @FieldTypeId

END