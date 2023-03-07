CREATE PROCEDURE [Enterprise].[ListProductsByPersonaId]          
(@PersonaId          BIGINT,          
 @ProductStatusValue NVARCHAR(2000) = NULL          
)          
AS          
BEGIN          
     
    
   DECLARE @NOW DATETIME= GETUTCDATE();          
   DECLARE @CompanyOrganizationProduct TABLE ( ProductId INT )         
   DECLARE @ProductCenterProducts Table (ProductId INT, PersonaId INT)
   DECLARE @AdminPortalProductID INT = 89
             
   INSERT INTO @CompanyOrganizationProduct ( ProductId )          
   SELECT ProductId FROM Enterprise.OrganizationProduct OP           
    INNER JOIN Ident.UserLoginPersona ULP ON ULP.OrganizationPartyId = OP.PartyId          
    INNER JOIN Person.Persona per ON (ULP.UserLoginPersonaId = per.UserLoginPersonaId and per.PersonaId = @PersonaId)          
     AND ((@NOW BETWEEN op.FromDate AND op.ThruDate)          
     OR (@NOW >= op.FromDate          
     AND op.ThruDate IS NULL))          
        
  ;with cte as (
	 select productId from Enterprise.PersonaConfiguration where personaId = @PersonaId 
	 AND  ThruDate IS NULL       
	  AND (StatusTypeId = @ProductStatusValue OR @ProductStatusValue IS NULL)
  )
  
  INSERT INTO @ProductCenterProducts ( ProductId , PersonaId)       
  SELECT DISTINCT ps.ProductId,ppc.PersonaId           
  FROM Enterprise.PersonaProductCenter ppc         
  inner join Enterprise.ProductProductCenter p on ppc.ProductCenterId = p.ProductCenterId        
  inner join Enterprise.GlobalProductConfiguration gpc on gpc.ProductId = p.productId        
  inner join Enterprise.ProductConfiguration config on config.ConfigurationID = gpc.ConfigurationID       
  inner join Enterprise.ProductSetting ps on ps.ProductSettingId = config.ProductSettingId        
  inner join Enterprise.ProductSettingType pst on (ps.ProductSettingTypeId = pst.ProductSettingTypeId and pst.[Name] ='GetUserProductCenterEnabled')       
  inner join Enterprise.ProductUserDependency pud on p.ProductId = pud.ProductId      
  inner join cte  c on ( c.ProductId = pud.DependentProductId)         
  inner join @CompanyOrganizationProduct cop on (cop.ProductId = ps.ProductId)
  WHERE ps.[Value] = '1'        
  and ppc.PersonaId = @PersonaId        
  and gpc.ThruDate is null        
  and config.ThruDate is null        
  and ps.ThruDate is null         

  IF EXISTS(SELECT TOP 1 1 FROM Ident.UserLoginPersona ULP 
	INNER JOIN Person.Persona P on ULP.UserLoginPersonaID = P.UserLoginPersonaId
	INNER JOIN Enterprise.OrganizationProduct Org on Org.PartyId = ULP.OrganizationPartyId
	WHERE P.PersonaId = @PersonaId 
	and ProductId = @AdminPortalProductID 
	and Org.Thrudate is NULL)
  BEGIN
     IF NOT EXISTS( SELECT TOP 1 1 FROM Ident.SamlUserAttribute where PersonaId = @PersonaId and ProductId = @AdminPortalProductID)     
	 BEGIN    
		INSERT INTO @ProductCenterProducts ( ProductId , PersonaId)                     
		SELECT @AdminPortalProductID,@PersonaId              
	 END
  END

  IF EXISTS ( SELECT TOP 1 1 FROM @CompanyOrganizationProduct Where ProductID = 4 )          
  BEGIN          
   INSERT INTO @CompanyOrganizationProduct ( ProductId )          
    SELECT ProductId FROM Enterprise.Product where ProductTypeId IN ( SELECT ProductTypeId FROM Enterprise.ProductType where ParentProductTypeId = 400 )          
  END            
   select distinct p.ProductGUID,        
   p.ProductId,        
   p.Name AS ProductName,        
   p.ProductTypeId,        
   p.Description AS ProductDescription,        
   pcp.PersonaId,        
   ul.PersonPartyId,          
   par.RealPageId,          
   o.PartyId AS OrganizationPartyId,          
   o.Name AS OrganizationName,          
   PS.value AS ProductStatus          
   from @ProductCenterProducts pcp         
   inner join Enterprise.Product p on pcp.ProductId = p.ProductId        
   inner join Enterprise.ProductSetting ps on p.ProductId = ps.ProductId        
   inner join Enterprise.ProductSettingType pst on (ps.ProductSettingTypeId = pst.ProductSettingTypeId and pst.Name = 'ProductStatus')        
   INNER JOIN Person.Persona per ON(pcp.PersonaId = per.PersonaId)          
   INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = per.UserLoginPersonaId          
   INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId          
   INNER JOIN Enterprise.Party par ON(UL.PersonPartyId = par.PartyId)          
   INNER JOIN Enterprise.Organization o ON(ULP.OrganizationPartyId = o.PartyId)        
        
   WHERE PS.value = @ProductStatusValue OR @ProductStatusValue IS NULL      
        
   UNION        
        
   SELECT DISTINCT          
                prod.ProductGUID,          
                p.ProductId,          
                prod.[Name] AS ProductName,          
                prod.ProductTypeId,          
                prod.Description AS ProductDescription,          
                per.PersonaId,          
         ul.PersonPartyId,          
                par.RealPageId,          
                o.PartyId AS OrganizationPartyId,          
                o.Name AS OrganizationName,          
                PS.value AS ProductStatus          
   FROM Enterprise.PersonaConfiguration p          
   JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId          
   INNER JOIN Enterprise.productSetting PS ON PC.productsettingid = ps.productsettingid          
   INNER JOIN Enterprise.ProductSettingType PST ON PS.productsettingtypeid = PST.ProductSettingTypeId AND PST.Name = 'ProductStatus'         
   JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId          
   INNER JOIN Person.Persona per ON(p.PersonaId = per.PersonaId)          
   INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = per.UserLoginPersonaId          
   INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId          
   INNER JOIN Enterprise.Party par ON(UL.PersonPartyId = par.PartyId)          
   INNER JOIN Enterprise.Organization o ON(ULP.OrganizationPartyId = o.PartyId)          
   INNER JOIN @CompanyOrganizationProduct OP ON OP.ProductId = prod.ProductId          
         WHERE p.PersonaId = @PersonaId          
               AND ((@NOW BETWEEN p.FromDate AND p.ThruDate)          
                    OR (@NOW >= p.FromDate          
                        AND p.ThruDate IS NULL))          
               AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate)          
                    OR (@NOW >= pc.FromDate          
                        AND pc.ThruDate IS NULL))          
               AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate)          
                    OR (@NOW >= ps.FromDate          
                        AND ps.ThruDate IS NULL))          
               AND (ps.Value = @ProductStatusValue          
                    OR @ProductStatusValue IS NULL);          
END;