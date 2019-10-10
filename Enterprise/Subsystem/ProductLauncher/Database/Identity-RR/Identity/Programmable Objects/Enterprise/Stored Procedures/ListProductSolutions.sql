IF OBJECT_ID('[Enterprise].[ListProductSolutions]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductSolutions];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductSolutions]
	@ParentProductTypeId int
AS
	SET NOCOUNT ON;

	IF (@ParentProductTypeId IS NOT NULL)
    BEGIN
        SELECT  ept.ProductTypeId,
                ept.ParentProductTypeId,
	            ept.Name, 
	            ept.Description 	                        								
	    FROM    [Enterprise].[ProductType] ept
		        WHERE ept.ParentProductTypeId = @ParentProductTypeId
    END
    ELSE
    BEGIN
        SELECT  ept.ProductTypeId,  
                ept.ParentProductTypeId,
	            ept.Name, 
	            ept.Description 	                        								
	    FROM    [Enterprise].[ProductType] ept
		        WHERE ept.ParentProductTypeId IS NOT NULL
    END
GO
