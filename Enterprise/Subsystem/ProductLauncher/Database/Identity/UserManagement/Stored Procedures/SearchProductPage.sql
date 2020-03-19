
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[SearchProductPage] (
	 @ProductPageId INT = NULL 
	,@ProductId INT = NULL 
	,@DisplayName NVARCHAR(510) = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [ProductPageId]
		,[ProductId]
		,[DisplayName]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ProductPage]
	WHERE 
		(@ProductPageId IS NULL  OR  [ProductPageId] = @ProductPageId)
	AND
		(@ProductId IS NULL  OR  [ProductId] = @ProductId)
	AND
		(@DisplayName IS NULL OR [DisplayName] = @DisplayName OR CHARINDEX(@DisplayName,[DisplayName]) > 0)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END