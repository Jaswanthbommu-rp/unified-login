
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].[AddUpdateControlDependency] (
	 @ControlDependencyId INT
	,@MasterControlId INT
	,@SlaveControlID INT
	,@MasterControlValue NTEXT
	,@ComparatorId TINYINT
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[ControlDependency] WHERE [ControlDependencyId] = @ControlDependencyId) 
	BEGIN
		UPDATE [UserManagement].[ControlDependency] SET 
			 [MasterControlId] = @MasterControlId
			,[SlaveControlID] = @SlaveControlID
			,[MasterControlValue] = @MasterControlValue
			,[ComparatorId] = @ComparatorId
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ControlDependencyId] = @ControlDependencyId

		SELECT			
			 [ControlDependencyId]
			,[MasterControlId]
			,[SlaveControlID]
			,[MasterControlValue]
			,[ComparatorId]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[ControlDependency]
		WHERE
			[ControlDependencyId] = @ControlDependencyId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[ControlDependency] (
			 [MasterControlId]
			,[SlaveControlID]
			,[MasterControlValue]
			,[ComparatorId]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ControlDependencyId]
			,inserted.[MasterControlId]
			,inserted.[SlaveControlID]
			,inserted.[MasterControlValue]
			,inserted.[ComparatorId]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @MasterControlId
			,@SlaveControlID
			,@MasterControlValue
			,@ComparatorId
			,@CreatedBy
			,@CreatedDate);

	

	END;