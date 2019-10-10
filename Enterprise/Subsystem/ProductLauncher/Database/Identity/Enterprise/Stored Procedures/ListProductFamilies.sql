CREATE PROCEDURE [Enterprise].[ListProductFamilies]
	
AS
	SET NOCOUNT ON;

	SELECT  ept.ProductTypeId,  
	        ept.Name, 
	        ept.Description 	                        								
	FROM    [Enterprise].[ProductType] ept
		    WHERE ept.ParentProductTypeId IS NULL
	;
