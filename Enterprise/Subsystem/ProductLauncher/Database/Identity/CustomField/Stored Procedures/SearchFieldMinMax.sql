
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the FieldMinMax table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].SearchFieldMinMax (
	 @FieldId BIGINT = NULL 
	,@MinMaxTypeId TINYINT = NULL 
	,@Minimum INT = NULL 
	,@Maximum INT = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [FieldId]
		,[MinMaxTypeId]
		,[Minimum]
		,[Maximum]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[CustomField].[FieldMinMax]
	WHERE 
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@MinMaxTypeId IS NULL  OR  [MinMaxTypeId] = @MinMaxTypeId)
	AND
		(@Minimum IS NULL  OR  [Minimum] = @Minimum)
	AND
		(@Maximum IS NULL  OR  [Maximum] = @Maximum)
	AND
		(@CreatedBy IS NULL OR [CreatedBy] = @CreatedBy OR CHARINDEX(@CreatedBy,[CreatedBy]) > 0)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)

END