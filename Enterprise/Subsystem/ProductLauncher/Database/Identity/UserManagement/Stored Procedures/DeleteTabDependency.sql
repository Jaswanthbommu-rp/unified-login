
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the TabDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteTabDependency (
	 @TabDependencyId INT = NULL 
	,@ControlId INT = NULL 
	,@TabTypeId INT = NULL 
	,@ControlValue NVARCHAR(510) = NULL 
	,@ComparatorID INT = NULL 
	,@CreatedBy INT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[TabDependency]

	WHERE
		(@TabDependencyId IS NULL  OR  [TabDependencyId] = @TabDependencyId)
	AND
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@ControlValue IS NULL  OR  [ControlValue] = @ControlValue)
	AND
		(@ComparatorID IS NULL  OR  [ComparatorID] = @ComparatorID)

	RETURN 1 --for success

END