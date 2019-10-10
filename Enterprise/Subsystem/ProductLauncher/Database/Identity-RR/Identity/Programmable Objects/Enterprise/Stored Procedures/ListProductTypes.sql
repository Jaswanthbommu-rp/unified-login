IF OBJECT_ID('[Enterprise].[ListProductTypes]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductTypes];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductTypes] (
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
GO
