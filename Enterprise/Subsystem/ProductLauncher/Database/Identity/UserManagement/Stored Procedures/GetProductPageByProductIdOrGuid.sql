-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetProductPageByProductIdOrGuid] (
	@ProductId INT = NULL
	,@ProductGUID uniqueidentifier
	,@PageType varchar(100) = 'ProductAccess'
)
AS

BEGIN
	SELECT
		 umpp.[ProductPageId]
		,umpp.[ProductId]
		,umpp.[DisplayName]
		,umpp.IsActive
		,umpp.[CreatedBy]
		,umpp.[CreatedDate]
		,umpp.ProductPageTypeId  
	FROM
		[UserManagement].[ProductPage] umpp
	INNER JOIN 	[Enterprise].[Product] ep	On
		ep.[ProductId] = umpp.[ProductId]
	INNER JOIN [UserManagement].[ProductPageType] PGT ON
		PGT.ProductPageTypeId = umpp.ProductPageTypeId
	WHERE PGT.Value = @PageType
	AND	(ep.[ProductId] IS NULL OR ep.[ProductId] = @ProductId)
		OR
		(ep.[ProductGUID] IS NULL OR ep.[ProductGUID] = @ProductGUID)
END