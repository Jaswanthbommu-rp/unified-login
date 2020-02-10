
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ProductPageTabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetProductPageTabTypeByTabTypeId (
	 @TabTypeId INT) 

 AS 

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
		[TabTypeId] = @TabTypeId