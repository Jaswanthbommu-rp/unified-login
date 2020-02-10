
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the TabTypeControl table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteTabTypeControl (
	 @TabTypeControlId INT = NULL 
	,@TabTypeId INT = NULL 
	,@ProductPageId INT = NULL 
	,@ControlId INT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[TabTypeControl]

	WHERE
		(@TabTypeControlId IS NULL  OR  [TabTypeControlId] = @TabTypeControlId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@ProductPageId IS NULL  OR  [ProductPageId] = @ProductPageId)
	AND
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)

	RETURN 1 --for success

END