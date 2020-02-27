
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ControlDependency table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchControlDependency (
	 @ControlDependencyId INT = NULL 
	,@MasterControlId INT = NULL 
	,@SlaveControlID INT = NULL 
	,@MasterControlValue NVARCHAR(MAX) = NULL 
	,@ComparatorID INT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

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
		(@ControlDependencyId IS NULL  OR  [ControlDependencyId] = @ControlDependencyId)
	AND
		(@MasterControlId IS NULL  OR  [MasterControlId] = @MasterControlId)
	AND
		(@SlaveControlID IS NULL  OR  [SlaveControlID] = @SlaveControlID)
	AND
		(@MasterControlValue IS NULL OR [MasterControlValue] = @MasterControlValue OR CHARINDEX(@MasterControlValue,[MasterControlValue]) > 0)
	AND
		(@ComparatorID IS NULL  OR  [ComparatorID] = @ComparatorID)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END