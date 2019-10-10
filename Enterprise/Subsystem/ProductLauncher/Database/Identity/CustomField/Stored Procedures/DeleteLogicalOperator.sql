
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the LogicalOperator table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteLogicalOperator (
	 @LogicalOperatorId TINYINT = NULL 
	,@Name NVARCHAR(10) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[LogicalOperator]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@LogicalOperatorId IS NULL  OR  [LogicalOperatorId] = @LogicalOperatorId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)

	DELETE 
		[CustomField].[LogicalOperator]

	WHERE
		(@LogicalOperatorId IS NULL  OR  [LogicalOperatorId] = @LogicalOperatorId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)

	RETURN 1 --for success

END

