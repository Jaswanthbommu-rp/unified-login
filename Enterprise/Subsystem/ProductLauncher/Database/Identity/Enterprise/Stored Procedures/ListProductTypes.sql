CREATE PROCEDURE Enterprise.ListProductTypes (
    @ParentProductTypeName NVARCHAR(50) = NULL
)  
AS  
BEGIN  
	SELECT	c.ProductTypeId,
			c.ParentProductTypeId,
			c.Name,
			c.Description,
			c.ProductTypeGuid,
			p.Name AS ParentProductTypeName
	FROM	Enterprise.ProductType c  
			LEFT JOIN Enterprise.ProductType p ON c.ParentProductTypeId = p.ProductTypeId  
	WHERE	(c.Name = @ParentProductTypeName OR @ParentProductTypeName IS NULL);     
END  