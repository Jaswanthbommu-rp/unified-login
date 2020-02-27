
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateControlDependency (
	 @ControlDependencyId INT
	,@MasterControlId INT
	,@SlaveControlID INT
	,@MasterControlValue NTEXT
	,@ComparatorID INT
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
			,[ComparatorID] = @ComparatorID
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ControlDependencyId] = @ControlDependencyId

		SELECT			
			 [ControlDependencyId]
			,[MasterControlId]
			,[SlaveControlID]
			,[MasterControlValue]
			,[ComparatorID]
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
			,[ComparatorID]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ControlDependencyId]
			,inserted.[MasterControlId]
			,inserted.[SlaveControlID]
			,inserted.[MasterControlValue]
			,inserted.[ComparatorID]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @MasterControlId
			,@SlaveControlID
			,@MasterControlValue
			,@ComparatorID
			,@CreatedBy
			,@CreatedDate);

	

	END;