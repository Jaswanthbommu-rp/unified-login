
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the MasterSlaveField table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteMasterSlaveField (
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


	UPDATE
		[CustomField].[MasterSlaveField]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
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
		(@MasterFieldValue IS NULL  OR  [MasterFieldValue] = @MasterFieldValue)

	DELETE 
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
		(@MasterFieldValue IS NULL  OR  [MasterFieldValue] = @MasterFieldValue)

	RETURN 1 --for success

END

