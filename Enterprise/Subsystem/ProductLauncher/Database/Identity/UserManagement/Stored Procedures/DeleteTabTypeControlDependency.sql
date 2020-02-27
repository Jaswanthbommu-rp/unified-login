
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the TabTypeControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteTabTypeControlDependency (
	 @TabtypeControlDependencyId INT = NULL 
	,@TabTypeControlId INT = NULL 
	,@TabTypeId INT = NULL 
	,@ControlTypeValue NVARCHAR(510) = NULL 
	,@ComparatorID INT = NULL 
	,@CreatedBy INT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[TabTypeControlDependency]

	WHERE
		(@TabtypeControlDependencyId IS NULL  OR  [TabtypeControlDependencyId] = @TabtypeControlDependencyId)
	AND
		(@TabTypeControlId IS NULL  OR  [TabTypeControlId] = @TabTypeControlId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@ControlTypeValue IS NULL  OR  [ControlTypeValue] = @ControlTypeValue)
	AND
		(@ComparatorID IS NULL  OR  [ComparatorID] = @ComparatorID)

	RETURN 1 --for success

END