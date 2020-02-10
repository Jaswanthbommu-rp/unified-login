
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateControlDependency (
	 @ControlDependencyId INT
	,@MasterTabTypeControlId INT
	,@SlaveTabTypeControlID INT
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
			 [MasterTabTypeControlId] = @MasterTabTypeControlId
			,[SlaveTabTypeControlID] = @SlaveTabTypeControlID
			,[MasterControlValue] = @MasterControlValue
			,[ComparatorID] = @ComparatorID
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ControlDependencyId] = @ControlDependencyId

		SELECT			
			 [ControlDependencyId]
			,[MasterTabTypeControlId]
			,[SlaveTabTypeControlID]
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
			 [MasterTabTypeControlId]
			,[SlaveTabTypeControlID]
			,[MasterControlValue]
			,[ComparatorID]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ControlDependencyId]
			,inserted.[MasterTabTypeControlId]
			,inserted.[SlaveTabTypeControlID]
			,inserted.[MasterControlValue]
			,inserted.[ComparatorID]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @MasterTabTypeControlId
			,@SlaveTabTypeControlID
			,@MasterControlValue
			,@ComparatorID
			,@CreatedBy
			,@CreatedDate);

	

	END;