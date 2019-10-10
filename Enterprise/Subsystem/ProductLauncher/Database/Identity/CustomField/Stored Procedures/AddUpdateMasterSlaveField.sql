
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the MasterSlaveField table.
-- =============================================
CREATE PROCEDURE [CustomField].AddUpdateMasterSlaveField (
	 @MasterSlaveFieldId BIGINT
	,@MasterFieldId BIGINT
	,@SlaveFieldId BIGINT
	,@LogicalOperatorId TINYINT
	,@ComperatorId TINYINT
	,@MasterFieldValue NTEXT
	,@CreatedDate DATETIME
	,@CreatedBy NVARCHAR(650)
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[MasterSlaveField] WHERE [MasterSlaveFieldId] = @MasterSlaveFieldId) 
	BEGIN
		UPDATE [CustomField].[MasterSlaveField] SET 
			 [MasterFieldId] = @MasterFieldId
			,[SlaveFieldId] = @SlaveFieldId
			,[LogicalOperatorId] = @LogicalOperatorId
			,[ComperatorId] = @ComperatorId
			,[MasterFieldValue] = @MasterFieldValue
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[MasterSlaveFieldId] = @MasterSlaveFieldId

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
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[MasterSlaveField] (
			 [MasterFieldId]
			,[SlaveFieldId]
			,[LogicalOperatorId]
			,[ComperatorId]
			,[MasterFieldValue]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[MasterSlaveFieldId]
			,inserted.[MasterFieldId]
			,inserted.[SlaveFieldId]
			,inserted.[LogicalOperatorId]
			,inserted.[ComperatorId]
			,inserted.[MasterFieldValue]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @MasterFieldId
			,@SlaveFieldId
			,@LogicalOperatorId
			,@ComperatorId
			,@MasterFieldValue
			,@CreatedDate
			,@CreatedBy);

	

	END;