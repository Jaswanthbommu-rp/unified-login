
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the MinMaxType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].SearchMinMaxType (
	 @MinMaxTypeId TINYINT = NULL 
	,@MinMaxTypeName NVARCHAR(100) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS

BEGIN

	SELECT
		 [MinMaxTypeId]
		,[MinMaxTypeName]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[MinMaxType]
	WHERE 
		(@MinMaxTypeId IS NULL  OR  [MinMaxTypeId] = @MinMaxTypeId)
	AND
		(@MinMaxTypeName IS NULL OR [MinMaxTypeName] = @MinMaxTypeName OR CHARINDEX(@MinMaxTypeName,[MinMaxTypeName]) > 0)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL OR [CreatedBy] = @CreatedBy OR CHARINDEX(@CreatedBy,[CreatedBy]) > 0)

END