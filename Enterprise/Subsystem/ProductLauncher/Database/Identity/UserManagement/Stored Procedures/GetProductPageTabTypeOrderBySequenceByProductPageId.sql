
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ProductPageTabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetProductPageTabTypeOrderBySequenceByProductPageId] (
	 @ProductPageId INT) 

 AS 

	SELECT
		 [UserManagement].[TabType].[TabTypeId]
		,[UserManagement].[TabType].[UIId]
		,[UserManagement].[TabType].[DisplayName]
		,[UserManagement].[ProductPageTabType].[Sequence]
		,[UserManagement].[TabType].[CreatedBy]
		,[UserManagement].[TabType].[CreatedDate]
	FROM
		[UserManagement].[ProductPageTabType]
	INNER JOIN [UserManagement].[TabType] ON ProductPageTabType.TabTypeId = TabType.TabTypeId
	WHERE
		[ProductPageId] = @ProductPageId
	ORDER BY [UserManagement].[ProductPageTabType].[Sequence]