
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the LogicalOperator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].SearchLogicalOperator (
	 @LogicalOperatorId TINYINT = NULL 
	,@Name NVARCHAR(10) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS

BEGIN

	SELECT
		 [LogicalOperatorId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[LogicalOperator]
	WHERE 
		(@LogicalOperatorId IS NULL  OR  [LogicalOperatorId] = @LogicalOperatorId)
	AND
		(@Name IS NULL OR [Name] = @Name OR CHARINDEX(@Name,[Name]) > 0)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL OR [CreatedBy] = @CreatedBy OR CHARINDEX(@CreatedBy,[CreatedBy]) > 0)

END