CREATE PROCEDURE [Enterprise].[ListProductsByPersonaId]          
(@PersonaId          BIGINT,          
 @ProductStatusValue NVARCHAR(2000) = NULL          
)          
AS          
BEGIN        

           
   DECLARE @NOW DATETIME= GETUTCDATE();                
   CREATE TABLE #CompanyOrganizationProduct ( ProductId INT )               
   DECLARE @ProductCenterProducts Table (ProductId INT, PersonaId INT)      
   DECLARE @AdminPortalProductID INT = 89              
   DECLARE @ProductStatusId INT = 0

   IF @ProductStatusValue IS NOT NULL SET @ProductStatusId = @ProductStatusValue

   INSERT INTO #CompanyOrganizationProduct ( ProductId )                
   SELECT ProductId FROM Enterprise.OrganizationProduct OP                 
   INNER JOIN Ident.UserLoginPersona ULP ON ULP.OrganizationPartyId = OP.PartyId                
   INNER JOIN Person.Persona per ON ULP.UserLoginPersonaId = per.UserLoginPersonaId and per.PersonaId = @PersonaId
   WHERE @NOW >= op.FromDate AND op.ThruDate IS NULL

   drop table if exists #TempSharedProducts 
   create table #TempSharedProducts(ProductConfigurationId int,ConfigurationId int,[Name] nvarchar(200),[value] nvarchar(25),SensitiveData tinyint,
   ProductId int ,BooksProductCode nvarchar(20) ,ProductName nvarchar(200) ,Active bit)
   insert into #TempSharedProducts(ProductConfigurationId,ConfigurationId,[Name],[value],SensitiveData,ProductId,BooksProductCode,ProductName,Active)
   exec [Enterprise].[ListProductGlobalSettingsBySettingType] 'SharedProductId'



   DROP TABLE IF EXISTS #DependentProducts    
   CREATE TABLE #DependentProducts (ProductId int,BaseProductId int)      
   INSERT INTO #DependentProducts (ProductId ,BaseProductId)
   SELECT DISTINCT PS.ProductId,PS.[Value] FROM #TempSharedProducts PS 
   INNER JOIN #CompanyOrganizationProduct COP on COP.ProductId <> PS.[Value] and PS.ProductId = COP.ProductId
   
    
  INSERT INTO #CompanyOrganizationProduct    
  SELECT BaseProductId FROM #DependentProducts   
              
  ;with cte as (      
  select productId from Enterprise.PersonaConfiguration where personaId = @PersonaId       
  AND  ThruDate IS NULL             
   AND (StatusTypeId = @ProductStatusId OR @ProductStatusId = 0)      
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
  inner join #CompanyOrganizationProduct cop on (cop.ProductId = ps.ProductId)      
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
                
  IF EXISTS ( SELECT TOP 1 1 FROM #CompanyOrganizationProduct Where ProductID = 4 )                
  BEGIN                
   INSERT INTO #CompanyOrganizationProduct ( ProductId )                
    SELECT ProductId FROM Enterprise.Product where ProductTypeId IN ( SELECT ProductTypeId FROM Enterprise.ProductType where ParentProductTypeId = 400 )                
  END          
  
  DROP TABLE IF EXISTS #TempFinalResult 
  CREATE TABLE #TempFinalResult([ProductGUID] [uniqueidentifier] NOT NULL,[ProductId] [int] NOT NULL,ProductName [nvarchar](50) NOT NULL,[ProductTypeId] [int] NULL,ProductDescription [nvarchar](1000) NULL,PersonaId [INT] NOT NULL,
  PersonPartyId [INT] NOT NULL,RealPageId [uniqueidentifier] NOT NULL, OrganizationPartyId [BIGINT] NOT NULL,[OrganizationName] [nvarchar](150) NULL,[ProductStatus] [nvarchar](100) NOT NULL)

  INSERT INTO #TempFinalResult ([ProductGUID],[ProductId],ProductName,[ProductTypeId],ProductDescription,PersonaId,PersonPartyId,RealPageId,OrganizationPartyId,[OrganizationName],[ProductStatus])

   SELECT DISTINCT p.ProductGUID,              
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
                p.StatusTypeId AS ProductStatus                
   FROM Enterprise.PersonaConfiguration p                
   JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId                
   INNER JOIN Person.Persona per ON(p.PersonaId = per.PersonaId)                
   INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = per.UserLoginPersonaId                
   INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId                
   INNER JOIN Enterprise.Party par ON(UL.PersonPartyId = par.PartyId)                
   INNER JOIN Enterprise.Organization o ON(ULP.OrganizationPartyId = o.PartyId)                
   INNER JOIN #CompanyOrganizationProduct OP ON OP.ProductId = prod.ProductId                
         WHERE p.PersonaId = @PersonaId                
               AND (@NOW >= p.FromDate AND p.ThruDate IS NULL)
               AND (p.StatusTypeId = @ProductStatusId OR @ProductStatusId = 0)    
               
    UPDATE TF set TF.ProductId = ChildPROD.ProductId , TF.ProductGUID = ChildPROD.ProductGUID,TF.ProductName = ChildPROD.[Name] ,
    TF.ProductTypeId = ChildPROD.ProductTypeId , TF.[ProductDescription] = ChildPROD.[Description] FROM #TempFinalResult TF 
    INNER JOIN #DependentProducts DP on DP.BaseProductId = TF.ProductId
    INNER JOIN Enterprise.[Product] ParentPROD on ParentPROD.ProductId = DP.BaseProductId
    INNER JOIN Enterprise.[Product] ChildPROD on ChildPROD.ProductId = DP.ProductId

    SELECT distinct [ProductGUID],[ProductId],ProductName,[ProductTypeId],ProductDescription,PersonaId,PersonPartyId,RealPageId,OrganizationPartyId,[OrganizationName],[ProductStatus] from #TempFinalResult



END;