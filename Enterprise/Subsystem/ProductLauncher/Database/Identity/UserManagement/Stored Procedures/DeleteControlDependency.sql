
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].[DeleteControlDependency] (
	 @ControlDependencyId INT = NULL 
	,@MasterControlId INT = NULL 
	,@SlaveControlID INT = NULL 
	,@MasterControlValue NVARCHAR(MAX) = NULL 
	,@ComparatorId TINYINT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[ControlDependency]

	WHERE
		(@ControlDependencyId IS NULL  OR  [ControlDependencyId] = @ControlDependencyId)
	AND
		(@MasterControlId IS NULL  OR  [MasterControlId] = @MasterControlId)
	AND
		(@SlaveControlID IS NULL  OR  [SlaveControlID] = @SlaveControlID)
	AND
		(@MasterControlValue IS NULL  OR  [MasterControlValue] = @MasterControlValue)
	AND
		(@ComparatorId IS NULL  OR  [ComparatorId] = @ComparatorId)

	RETURN 1 --for success

END