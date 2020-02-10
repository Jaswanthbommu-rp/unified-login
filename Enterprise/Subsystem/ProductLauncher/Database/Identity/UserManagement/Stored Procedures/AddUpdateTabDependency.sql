
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the TabDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateTabDependency (
	 @TabDependencyId INT
	,@ControlIdentifier NVARCHAR(510)
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
			 [ControlIdentifier] = @ControlIdentifier
			,[TabTypeId] = @TabTypeId
			,[ControlValue] = @ControlValue
			,[ComparatorID] = @ComparatorID
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[TabDependencyId] = @TabDependencyId

		SELECT			
			 [TabDependencyId]
			,[ControlIdentifier]
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
			 [ControlIdentifier]
			,[TabTypeId]
			,[ControlValue]
			,[ComparatorID]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[TabDependencyId]
			,inserted.[ControlIdentifier]
			,inserted.[TabTypeId]
			,inserted.[ControlValue]
			,inserted.[ComparatorID]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @ControlIdentifier
			,@TabTypeId
			,@ControlValue
			,@ComparatorID
			,@CreatedBy
			,@CreatedDate);

	

	END;