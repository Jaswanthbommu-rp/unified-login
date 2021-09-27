CREATE PROCEDURE [Security].[GetProductsByPersonaId]
	@PersonaId int = 0,
	@StatusTypeId int = 8
AS
BEGIN    
 SET NOCOUNT ON    
     
 DECLARE @NOW DATETIME= GETUTCDATE();    
 DECLARE @CompanyOrganizationProduct TABLE ( ProductId INT )     
 DECLARE @UserProducts TABLE ( ProductId INT, isFavorite TINYINT, StatusTypeId INT )    
 DECLARE @LearningProductID INT = 19    
    
 INSERT INTO @CompanyOrganizationProduct ( ProductId )    
 SELECT     
  DISTINCT OP.ProductId     
 FROM     
  Person.Persona P    
  INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId    
  INNER JOIN Enterprise.OrganizationProduct OP ON ULP.OrganizationPartyId = OP.PartyId    
 WHERE     
  P.PersonaId = @PersonaId    
  AND ((@NOW BETWEEN OP.FromDate AND OP.ThruDate) OR (@NOW >= OP.FromDate AND OP.ThruDate IS NULL))    
 UNION    
 SELECT ProductId FROM Enterprise.Product Where AssignToAllUsers = 1    
    
 IF 2 = ( select count(1) from @CompanyOrganizationProduct WHERE ProductId in ( 19, 36 ) )    
 BEGIN    
  SET @LearningProductID = 36    
  DELETE FROM @CompanyOrganizationProduct where ProductId = 19    
 END    
    
 IF EXISTS ( SELECT TOP 1 1 FROM @CompanyOrganizationProduct Where ProductID = 4 )    
 BEGIN    
  INSERT INTO @CompanyOrganizationProduct ( ProductId )    
   Select ProductId from Enterprise.Product where ProductTypeId IN ( SELECT ProductTypeId FROM Enterprise.ProductType where ParentProductTypeId = 400 )    
  DELETE FROM @CompanyOrganizationProduct WHERE ProductId = 4    
 END     
    
 -- ADD EASYLMS OR FIX ITS STATUS    
 IF EXISTS (SELECT TOP 1 1 FROM @UserProducts WHERE ProductId = @LearningProductID)    
 BEGIN    
  UPDATE @UserProducts SET StatusTypeId = 8 WHERE ProductId = @LearningProductID    
 END    
 ELSE    
 BEGIN    
  INSERT INTO @UserProducts ( ProductId, isFavorite, StatusTypeId )    
  VALUES    
   ( @LearningProductID, 0, 8 )    
 END    
     
  INSERT INTO @UserProducts ( ProductId , isFavorite, StatusTypeId)   
  SELECT DISTINCT ps.ProductId, 0, 8       
  FROM Enterprise.PersonaProductCenter ppc     
  inner join Enterprise.ProductProductCenter p on ppc.ProductCenterId = p.ProductCenterId    
  inner join Enterprise.GlobalProductConfiguration gpc on gpc.ProductId = p.productId    
  inner join Enterprise.ProductConfiguration config on config.ConfigurationID = gpc.ConfigurationID   
  inner join Enterprise.ProductSetting ps on ps.ProductSettingId = config.ProductSettingId    
  inner join Enterprise.ProductSettingType pst on (ps.ProductSettingTypeId = pst.ProductSettingTypeId and pst.Name ='GetUserProductCenterEnabled')   
  inner join Enterprise.ProductUserDependency pud on p.ProductId = pud.ProductId  
  inner join Enterprise.PersonaConfiguration PC on pc.ProductId = pud.DependentProductId  
  WHERE ps.Value = 1    
  and ppc.PersonaId = @PersonaId    
  and gpc.ThruDate is null    
  and config.ThruDate is null    
  and ps.ThruDate is null  
     AND PC.ThruDate IS NULL   
  AND (PC.StatusTypeId = @StatusTypeId OR @StatusTypeId IS NULL)  
  
 INSERT INTO @UserProducts ( ProductId, isFavorite, StatusTypeId )    
  SELECT    
   ProductId    
   ,IsFavorite    
   ,StatusTypeId    
  FROM Enterprise.PersonaConfiguration PC    
  WHERE    
   PC.PersonaId = @PersonaId    
   AND PC.ThruDate IS NULL    
    
 ;with ProductSettings AS (    
  SELECT ps.productid, pst.name, ps.value from enterprise.GlobalProductConfiguration GPC     
   INNER JOIN Enterprise.ProductConfiguration PC on GPC.ConfigurationId = PC.ConfigurationId    
   INNER JOIN enterprise.ProductSetting ps ON PC.ProductSettingId = PS.ProductSettingId    
   INNER JOIN enterprise.ProductSettingType pst on ps.ProductSettingTypeId = pst.ProductSettingTypeId     
  WHERE    
   pst.name in ( 'isresource', 'isnewtab', 'ProductUrl', 'ShowInAppSwitcher' )    
   AND gpc.ThruDate is null    
   AND pc.ThruDate is null    
   AND ps.ThruDate is null    
 )    
 SELECT     
  PC.ProductId    
  ,P.Name    
  ,P.BooksProductCode    
  ,CONVERT(TINYINT, PC.isFavorite) as isFavorite    
  ,PC.StatusTypeId    
  ,P.Description    
  ,PT.ParentProductTypeId as FamilyId    
  ,PT2.Name as FamilyName    
  ,CONVERT(TINYINT,ISNULL(ps1.value,0)) as IsNewTab    
  ,CONVERT(TINYINT,ISNULL(ps2.value,0)) as IsResource    
  ,PS3.Value as Url    
  ,CONVERT(TINYINT,ISNULL(ps4.value,0)) as ShowInAppSwitcher    
 FROM    
  @UserProducts PC    
  INNER JOIN Enterprise.Product P  ON PC.ProductId = P.ProductId    
  LEFT OUTER JOIN enterprise.producttype pt on p.ProductTypeId = pt.ProductTypeId    
  LEFT OUTER JOIN Enterprise.ProductType PT2 on PT.ParentProductTypeId = PT2.ProductTypeId    
  INNER JOIN @CompanyOrganizationProduct OP on P.ProductId = OP.ProductId    
  LEFT OUTER JOIN ProductSettings ps1 on ps1.ProductId = p.ProductId and ps1.name = 'IsNewTab'    
  LEFT OUTER JOIN ProductSettings ps2 on ps2.ProductId = p.ProductId and ps2.name = 'IsResource'    
  INNER JOIN ProductSettings ps3 on ps3.ProductId = p.ProductId and ps3.name = 'ProductUrl'    
  LEFT OUTER JOIN ProductSettings ps4 on ps4.ProductId = p.ProductId and ps4.name = 'ShowInAppSwitcher'    
 WHERE    
  PC.StatusTypeId = @StatusTypeId    
 UNION    
 SELECT    
  pr.ProductId    
  ,P.Name    
  ,P.BooksProductCode    
  ,CONVERT(TINYINT, ISNULL(UP.isFavorite,0)) as isFavorite    
  ,8 as StatusTypeId    
  ,P.Description    
  ,PT.ParentProductTypeId as FamilyId    
  ,PT2.Name as FamilyName    
  ,CONVERT(TINYINT,ISNULL(ps1.value,0)) as IsNewTab    
  ,CONVERT(TINYINT,ISNULL(ps2.value,0)) as IsResource    
  ,PS3.Value as Url    
  ,CONVERT(TINYINT,ISNULL(ps4.value,0)) as ShowInAppSwitcher    
 FROM     
  Security.PersonaRole ppv    
        INNER JOIN Security.Role r ON R.RoleID = ppv.RoleID    
        INNER JOIN Security.[RoleRight] r2 ON r.RoleID = r2.RoleID    
        INNER JOIN Security.[Right] rvt ON r2.RightId = rvt.RightId    
  INNER JOIN Enterprise.ProductRight PR on PR.RightShortName = rvt.RightName AND ( PR.DependantProductId is null OR PR.DependantProductId in ( SELECT ProductId FROM Enterprise.PersonaConfiguration WHERE PersonaId = PPV.PersonaId AND StatusTypeId = 8 ))   
 
  INNER JOIN Enterprise.Product P  ON PR.ProductId = P.ProductId    
  LEFT OUTER JOIN enterprise.producttype pt on p.ProductTypeId = pt.ProductTypeId    
  LEFT OUTER JOIN Enterprise.ProductType PT2 on PT.ParentProductTypeId = PT2.ProductTypeId    
  INNER JOIN @CompanyOrganizationProduct OP on P.ProductId = OP.ProductId    
  LEFT OUTER JOIN @UserProducts UP ON P.ProductId = UP.ProductId    
  LEFT OUTER JOIN ProductSettings ps1 on ps1.ProductId = p.ProductId and ps1.name = 'IsNewTab'    
  LEFT OUTER JOIN ProductSettings ps2 on ps2.ProductId = p.ProductId and ps2.name = 'IsResource'    
  LEFT OUTER JOIN ProductSettings ps3 on ps3.ProductId = p.ProductId and ps3.name = 'ProductUrl'    
  LEFT OUTER JOIN ProductSettings ps4 on ps4.ProductId = p.ProductId and ps4.name = 'ShowInAppSwitcher'    
 WHERE     
  ppv.PersonaId = @personaid    
    
 ORDER BY isFavorite, IsResource, P.Name    
     
END