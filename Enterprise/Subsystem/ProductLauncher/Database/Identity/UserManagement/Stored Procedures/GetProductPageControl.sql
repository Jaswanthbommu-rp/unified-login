
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ProductPageControl table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetProductPageControl (
	 @ProductPageControlId INT) 

 AS 

	SELECT
		 [ProductPageControlId]
		,[ProductPageId]
		,[ControlId]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ProductPageControl]
	WHERE
		[ProductPageControlId] = @ProductPageControlId