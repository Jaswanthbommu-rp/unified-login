IF OBJECT_ID('[Enterprise].[ListProductFamilies]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductFamilies];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductFamilies]
	
AS
	SET NOCOUNT ON;

	SELECT  ept.ProductTypeId,  
	        ept.Name, 
	        ept.Description 	                        								
	FROM    [Enterprise].[ProductType] ept
		    WHERE ept.ParentProductTypeId IS NULL
	;
GO
