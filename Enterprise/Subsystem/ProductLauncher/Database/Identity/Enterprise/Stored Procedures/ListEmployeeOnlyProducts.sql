CREATE PROCEDURE [Enterprise].[ListEmployeeOnlyProducts] 
AS 
	DECLARE @NOW DATETIME = GETUTCDATE();
    SELECT DISTINCT        
		p.ProductGUID,        
		p.ProductId,        
		p.[Name] AS ProductName,        
		p.ProductTypeId,        
		p.Description AS ProductDescription,        
		0 As PersonaId,        
		0 As PersonPartyId,        
		Convert(uniqueidentifier,'00000000-0000-0000-0000-000000000000') As RealPageId,        
		0 AS OrganizationPartyId,        
		'' AS OrganizationName,        
		'8' AS ProductStatus     
	FROM Enterprise.Product p
	INNER JOIN Enterprise.GlobalProductConfiguration gpc ON p.ProductId = gpc.ProductId
	JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
	INNER JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
	INNER JOIN Enterprise.ProductSettingType pst ON ps.ProductSettingTypeId = pst.ProductSettingTypeId
	Where PST.Name = 'IsAvailableForRealPageEmployeeOnly' 
	And ps.Value = '1'
	AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
	AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
	AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))