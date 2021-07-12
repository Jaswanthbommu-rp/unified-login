CREATE PROCEDURE [Enterprise].[ListProductsByOrganization]  
(  
 @OrganizationRealPageId UNIQUEIDENTIFIER = NULL,
 @PartyId BIGINT = NULL
)  
  
AS  
  
BEGIN  
	
	DECLARE @NOW  DATETIME = GETUTCDATE();
	
    SELECT DISTINCT pr.ProductGUID ,  
            pr.ProductId ,  
            pr.[Name] AS ProductName ,  
            pts.ProductTypeId AS SolutionId ,  
            pts.[Name] AS Solution ,  
            ptf.ProductTypeId AS FamilyId ,  
            ptf.[Name] AS Family ,  
            pr.Description AS ProductDescription,
            pr.[Active]
    FROM [Enterprise].Organization o  
            JOIN [Enterprise].OrganizationProduct op ON op.PartyId = o.PartyId  
            JOIN [Enterprise].[Product] pr ON pr.ProductId = op.ProductId  
            LEFT JOIN [Enterprise].[ProductType] pts ON pts.ProductTypeId = pr.ProductTypeId  
            LEFT JOIN [Enterprise].[ProductType] ptf ON ptf.ProductTypeId = pts.ParentProductTypeId  
            JOIN Enterprise.Party par ON par.PartyId = o.PartyId  
    WHERE (par.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)
	AND (Par.PartyId = @PartyId OR @PartyId IS NULL)
	AND ((@NOW BETWEEN op.FromDate AND op.ThruDate) OR (@NOW >= op.FromDate AND op.ThruDate IS NULL)) 
END;
