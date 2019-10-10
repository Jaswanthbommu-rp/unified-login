
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the MasterSlaveField table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].SearchMasterSlaveField (
	 @MasterSlaveFieldId BIGINT = NULL 
	,@MasterFieldId BIGINT = NULL 
	,@SlaveFieldId BIGINT = NULL 
	,@LogicalOperatorId TINYINT = NULL 
	,@ComperatorId TINYINT = NULL 
	,@MasterFieldValue NVARCHAR(MAX) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS

BEGIN

	SELECT
		 [MasterSlaveFieldId]
		,[MasterFieldId]
		,[SlaveFieldId]
		,[LogicalOperatorId]
		,[ComperatorId]
		,[MasterFieldValue]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[MasterSlaveField]
	WHERE 
		(@MasterSlaveFieldId IS NULL  OR  [MasterSlaveFieldId] = @MasterSlaveFieldId)
	AND
		(@MasterFieldId IS NULL  OR  [MasterFieldId] = @MasterFieldId)
	AND
		(@SlaveFieldId IS NULL  OR  [SlaveFieldId] = @SlaveFieldId)
	AND
		(@LogicalOperatorId IS NULL  OR  [LogicalOperatorId] = @LogicalOperatorId)
	AND
		(@ComperatorId IS NULL  OR  [ComperatorId] = @ComperatorId)
	AND
		(@MasterFieldValue IS NULL OR [MasterFieldValue] = @MasterFieldValue OR CHARINDEX(@MasterFieldValue,[MasterFieldValue]) > 0)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL OR [CreatedBy] = @CreatedBy OR CHARINDEX(@CreatedBy,[CreatedBy]) > 0)

END