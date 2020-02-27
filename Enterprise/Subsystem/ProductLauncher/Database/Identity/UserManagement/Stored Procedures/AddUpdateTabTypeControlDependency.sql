
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the TabTypeControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateTabTypeControlDependency (
	 @TabtypeControlDependencyId INT
	,@TabTypeControlId INT
	,@TabTypeId INT
	,@ControlTypeValue NVARCHAR(510)
	,@ComparatorID INT
	,@CreatedBy INT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[TabTypeControlDependency] WHERE [TabtypeControlDependencyId] = @TabtypeControlDependencyId) 
	BEGIN
		UPDATE [UserManagement].[TabTypeControlDependency] SET 
			 [TabTypeControlId] = @TabTypeControlId
			,[TabTypeId] = @TabTypeId
			,[ControlTypeValue] = @ControlTypeValue
			,[ComparatorID] = @ComparatorID
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[TabtypeControlDependencyId] = @TabtypeControlDependencyId

		SELECT			
			 [TabtypeControlDependencyId]
			,[TabTypeControlId]
			,[TabTypeId]
			,[ControlTypeValue]
			,[ComparatorID]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[TabTypeControlDependency]
		WHERE
			[TabtypeControlDependencyId] = @TabtypeControlDependencyId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[TabTypeControlDependency] (
			 [TabTypeControlId]
			,[TabTypeId]
			,[ControlTypeValue]
			,[ComparatorID]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[TabtypeControlDependencyId]
			,inserted.[TabTypeControlId]
			,inserted.[TabTypeId]
			,inserted.[ControlTypeValue]
			,inserted.[ComparatorID]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @TabTypeControlId
			,@TabTypeId
			,@ControlTypeValue
			,@ComparatorID
			,@CreatedBy
			,@CreatedDate);

	

	END;