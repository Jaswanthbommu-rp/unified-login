
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPageControl table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchExactProductPageControl (
	 @ProductPageControlId INT = NULL 
	,@ProductPageId INT = NULL 
	,@ControlId INT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [ProductPageControlId]
		,[ProductPageId]
		,[ControlId]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ProductPageControl]
	WHERE 
		(@ProductPageControlId IS NULL  OR  [ProductPageControlId] = @ProductPageControlId)
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