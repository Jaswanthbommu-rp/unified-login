
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the MinMaxType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].GetMinMaxTypeJoinFieldMinMaxByFieldId (
@FieldId BIGINT
)
AS

BEGIN
	SELECT
		 [CustomField].[MinMaxType].[MinMaxTypeId]
		,[CustomField].[MinMaxType].[MinMaxTypeName]
		,[CustomField].[MinMaxType].[CreatedDate]
		,[CustomField].[MinMaxType].[CreatedBy]
	FROM
		[CustomField].[MinMaxType]
	INNER Join
		[CustomField].[FieldMinMax] A
	On
		[A].[MinMaxTypeId] = [CustomField].[MinMaxType].[MinMaxTypeId]
	INNER Join
		[CustomField].[Field] B
	On
		[A].[FieldId] = [B].[FieldId]
	WHERE
		[B].[FieldId] = @FieldId

END