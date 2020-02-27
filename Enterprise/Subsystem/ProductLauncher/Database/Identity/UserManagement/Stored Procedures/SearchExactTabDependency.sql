
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabDependency table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchExactTabDependency (
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
		(@TabDependencyId IS NULL  OR  [TabDependencyId] = @TabDependencyId)
	AND
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@ControlValue IS NULL  OR  [ControlValue] = @ControlValue)
	AND
		(@ComparatorID IS NULL  OR  [ComparatorID] = @ComparatorID)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END