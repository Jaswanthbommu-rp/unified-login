
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the TabDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateTabDependency (
	 @TabDependencyId INT
	,@ControlId INT
	,@TabTypeId INT
	,@ControlValue NVARCHAR(510)
	,@ComparatorID INT
	,@CreatedBy INT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[TabDependency] WHERE [TabDependencyId] = @TabDependencyId) 
	BEGIN
		UPDATE [UserManagement].[TabDependency] SET 
			 [ControlId] = @ControlId
			,[TabTypeId] = @TabTypeId
			,[ControlValue] = @ControlValue
			,[ComparatorID] = @ComparatorID
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[TabDependencyId] = @TabDependencyId

		SELECT			
			 [TabDependencyId]
			,[ControlId]
			,[TabTypeId]
			,[ControlValue]
			,[ComparatorID]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[TabDependency]
		WHERE
			[TabDependencyId] = @TabDependencyId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[TabDependency] (
			 [ControlId]
			,[TabTypeId]
			,[ControlValue]
			,[ComparatorID]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[TabDependencyId]
			,inserted.[ControlId]
			,inserted.[TabTypeId]
			,inserted.[ControlValue]
			,inserted.[ComparatorID]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @ControlId
			,@TabTypeId
			,@ControlValue
			,@ComparatorID
			,@CreatedBy
			,@CreatedDate);

	

	END;