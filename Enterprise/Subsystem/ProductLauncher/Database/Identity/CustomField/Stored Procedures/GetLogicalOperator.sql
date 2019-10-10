
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the LogicalOperator table.
-- =============================================
CREATE PROCEDURE [CustomField].GetLogicalOperator (
	 @LogicalOperatorId TINYINT) 

 AS 

	SELECT
		 [LogicalOperatorId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[LogicalOperator]
	WHERE
		[LogicalOperatorId] = @LogicalOperatorId