
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the LogicalOperator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].GetLogicalOperatorJoinMasterSlaveFieldByComperatorId (
@ComperatorId TINYINT
)
AS

BEGIN
	SELECT
		 [CustomField].[LogicalOperator].[LogicalOperatorId]
		,[CustomField].[LogicalOperator].[Name]
		,[CustomField].[LogicalOperator].[CreatedDate]
		,[CustomField].[LogicalOperator].[CreatedBy]
	FROM
		[CustomField].[LogicalOperator]
	INNER Join
		[CustomField].[MasterSlaveField] A
	On
		[A].[LogicalOperatorId] = [CustomField].[LogicalOperator].[LogicalOperatorId]
	INNER Join
		[CustomField].[Comperator] B
	On
		[A].[ComperatorId] = [B].[ComperatorId]
	WHERE
		[B].[ComperatorId] = @ComperatorId

END