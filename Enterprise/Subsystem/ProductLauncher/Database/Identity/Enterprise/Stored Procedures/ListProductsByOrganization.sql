CREATE PROCEDURE [Enterprise].[ListProductsByOrganization]  
(  
 @OrganizationRealPageId UNIQUEIDENTIFIER = NULL,
 @PartyId BIGINT = NULL
)  
  
AS  
BEGIN  
	
	DECLARE @NOW  DATETIME = GETUTCDATE();
	
	if @PartyId IS NOT NULL
	BEGIN
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
		WHERE 
			Par.PartyId = @PartyId
			AND @NOW >= op.FromDate 
			AND op.ThruDate IS NULL
	END
	ELSE
	BEGIN
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
		WHERE 
			par.RealPageId = @OrganizationRealPageId
			AND @NOW >= op.FromDate 
			AND op.ThruDate IS NULL
	END
END;
