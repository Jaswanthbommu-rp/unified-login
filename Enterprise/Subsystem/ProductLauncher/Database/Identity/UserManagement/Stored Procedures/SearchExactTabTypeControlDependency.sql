
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabTypeControlDependency table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchExactTabTypeControlDependency (
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
		(@TabtypeControlDependencyId IS NULL  OR  [TabtypeControlDependencyId] = @TabtypeControlDependencyId)
	AND
		(@TabTypeControlId IS NULL  OR  [TabTypeControlId] = @TabTypeControlId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@ControlTypeValue IS NULL  OR  [ControlTypeValue] = @ControlTypeValue)
	AND
		(@ComparatorID IS NULL  OR  [ComparatorID] = @ComparatorID)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END