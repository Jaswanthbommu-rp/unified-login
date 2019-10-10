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