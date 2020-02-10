
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabTypeControl table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchExactTabTypeControl (
	 @TabTypeControlId INT = NULL 
	,@TabTypeId INT = NULL 
	,@ProductPageId INT = NULL 
	,@ControlId INT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [TabTypeControlId]
		,[TabTypeId]
		,[ProductPageId]
		,[ControlId]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[TabTypeControl]
	WHERE 
		(@TabTypeControlId IS NULL  OR  [TabTypeControlId] = @TabTypeControlId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@ProductPageId IS NULL  OR  [ProductPageId] = @ProductPageId)
	AND
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END