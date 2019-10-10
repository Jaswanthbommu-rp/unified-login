CREATE PROCEDURE [Enterprise].[ListProductsByRealPageId]
	@RealPageId UNIQUEIDENTIFIER
AS

SET NOCOUNT ON;

DECLARE @products TABLE
(
    ProductGUID             UNIQUEIDENTIFIER,
	ProductId			    int,
	[Name]                   nvarchar(100), 
	SolutionId              int, 
	Solution                nvarchar(100), 
	FamilyId                int,  
	Family                  nvarchar(100), 
	[Description] 			nvarchar(100),	                    
	ProductSettingId		int,
	ProductSettingTypeId	int,
	SettingName				nvarchar(50),
	SettingValue			nvarchar(1000),
	SettingDescription		nvarchar(100),
    HasAccess				bit
)	

INSERT INTO @products
	SELECT  ep.ProductGUID,
		ep.ProductId, 
		ep.Name, 
		eptSln.ProductTypeId as SolutionId, 
		eptSln.Name as Solution, 
		eptFam.ProductTypeId AS FamilyId,  
		eptFam.Name as Family,
		ep.Description, 		                    
		eps.ProductSettingId,
		epst.ProductSettingTypeId,
		epst.Name AS SettingName,
		eps.Value AS SettingValue,
		epst.Description AS SettingDescription,
		1 AS HasAcccess	 
		FROM Enterprise.Product ep 
		JOIN [Enterprise].[ProductSetting] eps ON eps.ProductId = ep.ProductId
		JOIN [Enterprise].[ProductSettingType] epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId 
		JOIN (
			SELECT DISTINCT(isua.ProductId) FROM [Ident].[SamlUserAttribute] isua
				JOIN [Enterprise].[Party] epar ON epar.PartyId = isua.[PersonaId]
				JOIN  Enterprise.Product ep ON isua.ProductId = ep.ProductId AND epar.RealPageId = @RealPageId
		) saml ON saml.ProductId = ep.ProductId		                                        
		LEFT JOIN [Enterprise].[ProductType] eptSln ON eptSln.ProductTypeId = ep.ProductTypeId	                    
		JOIN [Enterprise].[ProductType] eptFam ON eptFam.ProductTypeId = eptSln.ParentProductTypeId                            
	WHERE   ep.Name NOT IN ('Landing', 'ClientPortal', 'Product Learning Portal') ;

--Return the products
SELECT	DISTINCT(ProductId), 
        ProductGUID,		                     
		[Name],                             
		[Description]
FROM	@products;

--Return the product settings/solution/family for each product
SELECT	ProductId, 	
		ProductSettingId,
		ProductSettingTypeId,
		SettingName AS Name,
		SettingValue AS Value,
		SettingDescription AS Description,
        Solution,
        Family
FROM	@products;        
