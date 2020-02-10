
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPageTabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchExactProductPageTabType (
	 @ProductPageTabTypeId INT = NULL 
	,@ProductPageId INT = NULL 
	,@TabTypeId INT = NULL 
	,@Sequence TINYINT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [ProductPageTabTypeId]
		,[ProductPageId]
		,[TabTypeId]
		,[Sequence]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ProductPageTabType]
	WHERE 
		(@ProductPageTabTypeId IS NULL  OR  [ProductPageTabTypeId] = @ProductPageTabTypeId)
	AND
		(@ProductPageId IS NULL  OR  [ProductPageId] = @ProductPageId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@Sequence IS NULL  OR  [Sequence] = @Sequence)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END