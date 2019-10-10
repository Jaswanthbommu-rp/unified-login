
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the MasterSlaveField table.
-- =============================================
CREATE PROCEDURE [CustomField].GetMasterSlaveField (
	 @MasterSlaveFieldId BIGINT) 

 AS 

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
		[MasterSlaveFieldId] = @MasterSlaveFieldId