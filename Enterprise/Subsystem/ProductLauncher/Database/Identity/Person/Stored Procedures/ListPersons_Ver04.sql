CREATE PROCEDURE [Person].[ListPersons_Ver04] (  
 @RealPageId UNIQUEIDENTIFIER = NULL,          
 @ParentPartyRoleTypeId INT = NULL,          
 @UserListFilterType TINYINT = 0,          
 @AssignedProducts NVARCHAR(MAX),          
 @FilterBy NVARCHAR(MAX),          
 @SortBy NVARCHAR(MAX),          
 @RowsPerPage INT = 0,          
 @PageNumber INT = 1          
)          
AS          
BEGIN          
 DECLARE @PartyId BIGINT,          
  @NOW DATETIME= GETUTCDATE(),          
  @sortOrder NVARCHAR(128),          
  @sortDirection NVARCHAR(4),          
  @sortValue INT = 100,          
  @filterName NVARCHAR(512),          
  @filterProductId INT = NULL,          
  @filterStatusTypeId INT = 0,          
  @filterEnterpriseRoleCount int = 0,  
  @filterUserTypeCount INT = 0,
  @filterPartyRoleTypeId VARCHAR(MAX) = NULL,         
  @filterPersonaProductError tinyint = NULL,        
  @minSequence smallint,          
  @csvAssignedProducts varchar(max),          
  @csvStatus varchar(max),          
  @csvEnterpriseRole varchar(max),  
  @PrimaryProperties varchar(max),  
  @PrimaryPropertiesCount int = 0,   
  @ProductSettingTypeId int,          
  @OffsetMinutes smallint,        
  @csvOperator varchar(max),        
  @filterOperatorCount int = NULL,    
  @EmployeeCompanyPartyId bigint;       
          
 DECLARE @filterStatus TABLE (          
  StatusTypeId int PRIMARY KEY          
 )   
 
  DECLARE @filterUserType TABLE (          
  UserTypeId int PRIMARY KEY          
 ) 
          
 DECLARE @filterEnterpriseRole TABLE (          
  RoleTemplateId int PRIMARY KEY          
 )          
  DECLARE @tblPrimaryProperties TABLE (          
  Properties BIGINT PRIMARY KEY          
 )         
 DECLARE @HoldPersona TABLE (          
  PersonaId bigint          
 )          
          
 DECLARE @AssignedProductIds TABLE (          
  ProductId int PRIMARY KEY          
 )          
        
   DECLARE @filterOperator TABLE (          
  OperatorId VARCHAR(1000) NOT NULL    
  )       
      
 DECLARE @tblFilterBy TABLE (          
  ColumnName varchar(128),          
  SearchValue varchar(max)          
 )          
          
 CREATE TABLE #PersonaProduct(          
  PersonaId bigint,          
  ProductId bigint          
 )          
        
 CREATE TABLE #PersonaProductError(          
  PersonaId bigint,        
  IsProductError tinyint         
 )         
          
 SELECT @RowsPerPage = CASE          
  WHEN @RowsPerPage <= 0 THEN 2147483647          
  ELSE @RowsPerPage          
 END;           
          
 IF (ISJSON(@SortBy) = 0)          
 BEGIN          
  SET @SortBy = NULL          
 END          
          
 IF (ISJSON(@FilterBy) = 0)          
 BEGIN          
  SET @FilterBy = NULL          
 END          
           
 SELECT @ProductSettingTypeId = ProductSettingTypeId          
 FROM  Enterprise.ProductSettingType          
 WHERE Name = 'ProductStatus'        
    
 select @EmployeeCompanyPartyId = PartyId from Enterprise.Party P    
 where P.RealPageId = '0D018E46-C20E-477D-ADED-4E5A35FB8F99'    
          
 SELECT @csvAssignedProducts = ColumnValue          
 FROM OPENJSON (JSON_QUERY(@AssignedProducts, '$.assignedProducts'))          
 WITH (          
     ColumnName nvarchar(max) '$.ColumnName',          
     ColumnValue nvarchar(max) '$.SearchValue'          
    )          
          
 INSERT INTO @AssignedProductIds (          
  ProductId          
 )          
 SELECT CONVERT(int, value)          
 FROM  STRING_SPLIT(@csvAssignedProducts, ',');          
          
 SELECT @SortValue =          
   CASE ColumnName          
    WHEN N'InitialSort' THEN 100          
    WHEN N'Name' THEN 100          
    WHEN N'Products' THEN 101          
    WHEN N'LastLogin' THEN 102          
    WHEN N'LoginName' THEN 103          
    WHEN N'Status' THEN 104          
    WHEN N'EmployeeId' THEN 105          
    WHEN N'EnterpriseRoleName' THEN 106      
    WHEN N'Operator' THEN 107        
    ELSE 100          
   END * CASE SortDirection WHEN N'ASC' THEN 1 ELSE -1 END           
 FROM OPENJSON (JSON_QUERY(@SortBy, '$.sortBy'))          
 WITH (          
     ColumnName nvarchar(max) '$.ColumnName',          
     SortDirection nvarchar(max) '$.SortDirection'          
    )          
 WHERE ISJSON(@SortBy) > 0;          
          
 INSERT INTO @tblFilterBy (          
  ColumnName,          
  SearchValue          
 )          
 SELECT ColumnName,          
     SearchValue          
 FROM OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))          
 WITH (          
     ColumnName nvarchar(max) '$.ColumnName',          
     SearchValue nvarchar(max) '$.SearchValue'          
    )          
 WHERE ISJSON(@FilterBy) > 0          
          
 SELECT @OffsetMinutes = SearchValue          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'OffsetMinutes'          
          
 SET @OffsetMinutes = ISNULL(@OffsetMinutes, 0)          
          
 SELECT @filterName = SearchValue          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'Name'          
 AND   SearchValue NOT IN ( '%', '')          
          
 SELECT @filterProductId = CONVERT(int, SearchValue)          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'ProductId'          
 AND   ISNUMERIC(SearchValue) = 1          
          
  SELECT @filterPartyRoleTypeId = SearchValue          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'UserType'          
 AND   SearchValue NOT IN ( '%', '')  
          
 SELECT @csvStatus = SearchValue          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'Status'          
 AND   SearchValue NOT IN ( '%', '')          
          
 SELECT @csvEnterpriseRole = SearchValue          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'RoleTemplateId'          
 AND   SearchValue NOT IN ( '%', '')   
   
 SELECT @PrimaryProperties = SearchValue          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'PrimaryProperties'          
 AND   SearchValue NOT IN ( '%', '')   
         
 SELECT @filterPersonaProductError = CONVERT(tinyint, SearchValue)          
 FROM  @tblFilterBy          
 WHERE ColumnName = 'PersonaHasProductError'          
 AND   ISNUMERIC(SearchValue) = 1         
        
 IF (@filterPersonaProductError = 0)        
 SET @filterPersonaProductError = NULL        
          
 IF (LEN(@csvStatus) > 0)          
 BEGIN          
  INSERT INTO @filterStatus (          
   StatusTypeId          
  )          
  SELECT CONVERT(int, value)          
  FROM STRING_SPLIT(@csvStatus, ',');          
 END 
 
  IF (LEN(@filterPartyRoleTypeId) > 0)          
 BEGIN          
  INSERT INTO @filterUserType (          
   UserTypeId          
  )          
  SELECT CONVERT(int, value)          
  FROM STRING_SPLIT(@filterPartyRoleTypeId, ',');          
 END 
          
 IF (LEN(@csvEnterpriseRole) > 0)          
 BEGIN          
  INSERT INTO @filterEnterpriseRole (          
   RoleTemplateId          
  )          
  SELECT CONVERT(int, value)          
  FROM STRING_SPLIT(@csvEnterpriseRole, ',');          
 END          
          
 SELECT @filterEnterpriseRoleCount = COUNT(RoleTemplateId)          
 FROM @filterEnterpriseRole  
   
  IF (LEN(@PrimaryProperties) > 0)          
 BEGIN          
  INSERT INTO @tblPrimaryProperties (          
   Properties          
  )          
  SELECT CONVERT(bigint, value)          
  FROM STRING_SPLIT(@PrimaryProperties, ',');          
 END          
          
 SELECT @PrimaryPropertiesCount = COUNT(Properties)          
 FROM @tblPrimaryProperties  
       
 --operator logic         
  SELECT @csvOperator = SearchValue          
  FROM  @tblFilterBy          
  WHERE ColumnName = 'Operator'        
  AND   SearchValue NOT IN ( '%', '')         
        
  IF (LEN(@csvOperator) > 0)          
  BEGIN          
   INSERT INTO @filterOperator (          
    OperatorId    
   )        
   SELECT value    
   FROM STRING_SPLIT(@csvOperator, ',');         
  END        
      
 SELECT @filterOperatorCount = COUNT(OperatorId) FROM  @filterOperator    
 SELECT @filterUserTypeCount = COUNT(UserTypeId) FROM  @filterUserType
 SELECT @filterStatusTypeId = COUNT(StatusTypeId)          
 FROM  @filterStatus          
 WHERE StatusTypeId > 0          
          
 SELECT @PartyId = PartyId          
 FROM  Enterprise.Party          
 WHERE RealPageId = @RealPageId          
          
 IF (@UserListFilterType IN (1, 2))          
 BEGIN          
  INSERT INTO @HoldPersona (          
   PersonaId          
  )          
  SELECT pe.PersonaId          
  FROM Enterprise.OrganizationAdminUser OAU          
   INNER JOIN Ident.UserLoginPersona ULP ON OAU.UserLoginPersonaId = ULP.UserLoginPersonaId AND ulp.PrimaryOrganization = 1          
   INNER JOIN Person.Persona PE ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId          
  WHERE          
   OAU.OrganizationPartyId = @PartyId          
 END          
          
 IF (@UserListFilterType = 2)          
 BEGIN          
  INSERT INTO @HoldPersona (          
   PersonaId          
  )          
  SELECT P.PersonaId          
  FROM Person.Persona p          
     INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId          
     INNER JOIN Ident.UserLogin ul ON ulp.UserLoginId = ul.UserId          
     INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = ul.PersonPartyId and pr.PartyIdTo = ulp.OrganizationPartyId and pr.ThruDate is null          
     INNER JOIN Enterprise.RoleType rt ON rt.PartyRoleTYpeId = pr.RoleTypeIdFrom          
  WHERE rt.Name = 'SuperUser'          
  AND  ulp.OrganizationPartyId = @PartyId          
 END          
          
 IF (@UserListFilterType = 0)           
 BEGIN           
  INSERT INTO @HoldPersona (          
   PersonaId          
  )          
  SELECT  0           
 END;    
 
 DECLARE @CompanyOrganizationProduct TABLE (ProductId INT)       
 INSERT INTO @CompanyOrganizationProduct (ProductId)      
 SELECT DISTINCT OP.productid FROM Enterprise.OrganizationProduct OP 
 INNER JOIN Enterprise.GlobalProductConfiguration gpc ON gpc.ProductId = OP.ProductId AND op.ConfigurationId = gpc.ConfigurationId
 WHERE op.PartyId = @PartyId AND op.ThruDate IS NULL AND gpc.ThruDate IS NULL
			
 DROP TABLE IF EXISTS #DependentProducts
 CREATE TABLE #DependentProducts (ProductId int,BaseProductId int)  
 INSERT INTO #DependentProducts
 SELECT DISTINCT PS.ProductId,Ps.[Value] FROM Enterprise.productsettingtype PST 
 INNER JOIN Enterprise.ProductSetting PS on PST.productSettingTypeId = PS.productSettingTypeId and PST.[Name] = 'ProductUsernameDataSharedWithOtherProduct' 
 where Ps.[Value] not in (SELECT distinct productId from @CompanyOrganizationProduct)

 INSERT INTO #PersonaProduct (ProductId, PersonaId )        
 SELECT DISTINCT DP.ProductId , PC.PersonaId FROM #DependentProducts DP inner join Enterprise.PersonaConfiguration PC on PC.ProductId = DP.BaseProductId 
 where PC.StatusTypeId = '8' AND  DP.BaseProductId NOT IN (SELECT distinct productId from @CompanyOrganizationProduct)

          
 IF(@filterProductId = 37) -- 37 Property Photos product Id          
 BEGIN          
          
  SET @filterProductId = 9  -- Marketing Center product Id          
           
  INSERT INTO #PersonaProduct         
  (          
  PersonaId,          
  ProductId        
  )          
  SELECT p.PersonaID,          
   pec.ProductId          
  FROM           
   Person.Persona p          
   INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId          
   INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId             
   INNER JOIN [security].PersonaRole sp on sp.PersonaId = p.PersonaId           
   INNER JOIN [Security].RoleRight srr on srr.RoleId = sp.RoleId           
   INNER JOIN [Security].[Right] sr on sr.RightId = srr.RightId AND sr.RightName = 'AccessPropertyPhotos' --- Access to Property Photos (requires Marketing Center access)          
  WHERE          
    ULP.OrganizationPartyId = @PartyId          
  AND  pec.ProductId = 9          
  AND pec.StatusTypeId = 8        
  AND  ((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))          
        AND     ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))          
               
        
 END  
 ELSE  IF  @filterProductId=4      
 BEGIN          
  ;WITH AOPersonaProductCTE AS (  
    SELECT DISTINCT p.PersonaID   
    FROM   
     Person.Persona p          
     INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId          
     INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId          
     WHERE   
     ULP.OrganizationPartyId = @PartyId          
     AND pec.StatusTypeId = 8        
     AND pec.ProductId IN (29,30,31,32,33,51,52,53,54,66,4)   
     AND ((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))          
     AND ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))  
)  
  
INSERT INTO #PersonaProduct (  
 PersonaId,   
 ProductId  
 )            
SELECT p.PersonaID,   
 pec.ProductId            
FROM   
 Person.Persona p            
 INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId            
 INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId  
 INNER JOIN AOPersonaProductCTE APP ON APP.PersonaId = P.PersonaId  
WHERE   
 ULP.OrganizationPartyId = @PartyId            
 AND pec.StatusTypeId = 8          
 AND pec.ProductId NOT IN (19, 24, 25, 34, 39)   
 AND ((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))            
 AND ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))  
  
 END  
 ELSE          
 BEGIN          
   INSERT INTO #PersonaProduct (          
   PersonaId,          
   ProductId          
  )          
  SELECT p.PersonaID,          
    pec.ProductId          
   FROM           
    Person.Persona p          
    INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId          
    INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId          
   WHERE          
 ULP.OrganizationPartyId = @PartyId          
 AND pec.StatusTypeId = 8        
 AND  pec.ProductId NOT IN (19, 24, 25, 34, 39) --Product Learning Portal, Black Book, Self-provisioning portal, Benchmarking, Integration Marketplace          
 AND  ((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))          
 AND     ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))     
 END          
        
        
 INSERT INTO #PersonaProductError (        
   PersonaId,IsProductError          
  )          
  SELECT pe.PersonaId  ,1        
  FROM Enterprise.PersonaProductError PPE          
  INNER JOIN Person.Persona PE ON PE.PersonaId = PPE.PersonaId          
  INNER JOIN Ident.UserLoginPersona ULP ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId        
  WHERE  ULP.OrganizationPartyId = @PartyId         
          
 DROP INDEX IF EXISTS [NCI_Temp_PersonaProduct_ProductId] ON [dbo].[#PersonaProduct]          
 CREATE NONCLUSTERED INDEX [NCI_Temp_PersonaProduct_ProductId] ON [dbo].[#PersonaProduct] (PersonaId) INCLUDE (ProductId)         
           
 DECLARE @baseProductId int = (SELECT TOP 1 BaseProductId FROM #DependentProducts where productId = @filterProductId)
 IF (@baseProductId IS NOT NULL)
  BEGIN
  SET @filterProductId = @baseProductId
  END

 DROP TABLE IF EXISTS #UserLogin          
          
 CREATE TABLE #UserLogin          
 (          
  PersonaId BIGINT PRIMARY KEY,            
  UserLoginPersonaId BIGINT NULL,          
  PersonPartyId BIGINT NULL,            
  UserId BIGINT NULL,            
  LoginName VARCHAR(255) NULL,            
  LastLogin DATETIME NULL,           
  FromDate DATETIME NULL,          
  ThruDate DATETIME NULL,          
  IdentityProviderTypeId INT NULL,            
  StatusId INT,            
  StatusName VARCHAR(50) NULL,        
  StatusThruDate DATETIME NULL,          
  PasswordModifiedDate SMALLDATETIME NULL        
 )          
          
 INSERT INTO #UserLogin          
 (          
 PersonaId,UserLoginPersonaId,PersonPartyId,UserId,LoginName,LastLogin ,FromDate,ThruDate          
 ,IdentityProviderTypeId ,StatusId,StatusName,StatusThruDate,PasswordModifiedDate          
 )          
 SELECT             
  pe.PersonaId,           
  iulp.UserLoginPersonaId,           
  ul.PersonPartyId,            
  ul.UserId,            
  ul.LoginName,            
  iulp.LastLoginDate AS LastLogin,           
  iulp.FromDate,          
  iulp.ThruDate,         
  ul.IdentityProviderTypeId,            
  iulp.StatusTypeId AS StatusId,            
  CASE            
  WHEN ((iulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NULL)) THEN 'Pending'            
  WHEN ((iulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NOT NULL)) THEN 'Active'            
  ELSE est.Name            
  END AS 'StatusName',            
  iulp.StatusThruDate,          
  ul.PasswordModifiedDate         
 FROM Person.Persona pe            
  INNER JOIN Ident.UserLoginPersona iulp ON (pe.UserLoginPersonaId = iulp.UserLoginPersonaId)            
  INNER JOIN Ident.UserLogin ul ON iulp.UserLoginId = ul.UserId            
  INNER JOIN Enterprise.StatusType est ON iulp.StatusTypeId = est.StatusTypeId            
  LEFT OUTER JOIN @filterStatus fs ON (est.StatusTypeId = fs.StatusTypeId)            
 WHERE iulp.OrganizationPartyId = @PartyId     
 AND ( iulp.IsRPEmployee = 0 OR @EmployeeCompanyPartyId =  iulp.OrganizationPartyId)    
 AND  pe.personaId  NOT IN ( SELECT ISNULL(PersonaId, 0) FROM @HoldPersona)            
 AND  (            
  pe.PersonaId IN            
  (            
  SELECT PersonaID            
  FROM #PersonaProduct            
  WHERE PE.PersonaId = PersonaID AND (@filterProductId=4  OR ProductId = @filterProductId)            
  )            
  OR @filterProductId IS NULL            
 )            
 AND  ((@filterStatusTypeId = 0) OR (NOT fs.StatusTypeId IS NULL))            
  AND 1 = (case     
 when ((select count(1) from @filterStatus where StatusTypeId = 2) = 0) then (case when ((iulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NULL)) then 0 else 1 end)    
 else 1 end)    
                 
 DROP TABLE IF EXISTS #UserEnterpriseRole          
          
 CREATE TABLE #UserEnterpriseRole          
 (          
  PersonaId BIGINT PRIMARY KEY,            
  RoleTemplateId INT NULL,          
  EnterpriseRoleName Varchar(256) NULL          
 )          
          
 INSERT INTO #UserEnterpriseRole          
 SELECT UL.PersonaId, RTUM.RoleTemplateId, SRT.RoleTemplateName          
 FROM #UserLogin UL          
 INNER JOIN [Security].[RoleTemplateUserMapping] RTUM  ON UL.PersonaId  = RTUM.PersonaId          
 INNER JOIN [Security].[RoleTemplate] SRT ON SRT.RoleTemplateId = RTUM.RoleTemplateId          
 Where SRT.PartyID = @PartyId          
          
 IF(@filterEnterpriseRoleCount > 0)          
 BEGIN          
  DELETE FROM #UserEnterpriseRole           
  Where RoleTemplateId NOT IN (SELECT RoleTemplateId FROM @filterEnterpriseRole)          
          
  DELETE FROM #UserLogin           
  Where PersonaId NOT IN (SELECT PersonaId FROM #UserEnterpriseRole)          
 END           
   
 DROP TABLE IF EXISTS #UserProperties          
          
 CREATE TABLE #UserProperties          
 (          
  PersonaId BIGINT PRIMARY KEY     
 )     
  
 IF(@PrimaryPropertiesCount > 0)          
 BEGIN        
 INSERT INTO #UserProperties  
 SELECT distinct UL.PersonaId      
 FROM #UserLogin UL    
 JOIN Enterprise.PropertyInstanceMapping PIM ON PIM.PersonaId = UL.PersonaId  
 WHERE PIM.PropertyInstanceId IN (SELECT Properties FROM @tblPrimaryProperties)  
 AND PIM.Active=1 AND PIM.ProductId=3  
   
 DELETE FROM #UserLogin           
  Where PersonaId NOT IN (SELECT PersonaId FROM #UserProperties)  
 END  
  
 DROP TABLE IF EXISTS #PartyContactMechanism          
          
 CREATE TABLE #PartyContactMechanism          
 (          
  PartyId BIGINT PRIMARY KEY,          
  ContactMechanismId INT,          
  RowNo INT          
 )          
        
 INSERT INTO #PartyContactMechanism(PartyId,ContactMechanismId,RowNo)          
 SELECT PartyId,ContactMechanismId,RowNo           
 FROM          
 (          
 SELECT PartyId,ContactMechanismId,ROW_NUMBER() OVER(PARTITION BY PartyId ORDER BY FromDate DESC) AS RowNo          
  FROm Enterprise.PartyContactMechanism          
  WHERE ThruDate > GETUTCDATE()     
 ) X          
 WHERE X.RowNo = 1          
          
 DROP INDEX IF EXISTS [NCI_cteUserLogin_PersonPartyId] ON [dbo].[#UserLogin]          
 CREATE NONCLUSTERED INDEX [NCI_cteUserLogin_PersonPartyId]  ON [dbo].[#UserLogin] ([PersonPartyId])          
 INCLUDE ([UserLoginPersonaId],[PersonaId],[UserId],[LoginName],[LastLogin],[FromDate],[ThruDate],[IdentityProviderTypeId],[StatusId],[StatusName],[StatusThruDate],[PasswordModifiedDate])          
          
 ;WITH cteUsersFinal          
 (          
  RealPageID,          
  PartyId,          
  FirstName,          
  MiddleName,          
  LastName,          
  EmployeeId,          
  UserId,          
  LoginName,        
  LastLogin,        
  FromDate,        
  ThruDate,        
  StatusId,          
  StatusName,        
  StatusThruDate,        
  Is3rdPartyIDP,          
  UserType,          
  PartyRoleTypeId,         
  PasswordModifiedDate,        
  EntepriseRoleName,          
  RoleTemplateId,      
  Operator,    
  --OperatorRealPageId,    
  UserRelationshipType,        
  CompanyName,      
  PersonaHasProductError,        
  OffsetMinutes,          
  TotalRecords,          
  RowNumber,        
  PersonaId        
 )        
 AS           
 (          
    SELECT           
    pa.RealpageId AS RealPageID,            
    p.PartyId,            
    p.FirstName,            
    p.MiddleName,            
    p.LastName,            
    UE.Employee as EmployeeId,            
    ulp.UserId,            
    ulp.LoginName,        
 ulp.LastLogin,        
 ulp.FromDate,          
 ulp.ThruDate,        
    ulp.StatusId,            
    ulp.StatusName,         
 ulp.StatusThruDate,         
    CASE            
     WHEN ipt.Name = 'ID3' THEN 0            
     ELSE 1            
    END AS 'Is3rdPartyIDP',            
    ISNULL(rt.Name, '') AS UserType,            
    prs.RoleTypeIdFrom AS PartyRoleTypeId,          
 ulp.PasswordModifiedDate,        
    UER.EnterpriseRoleName,          
    UER.RoleTemplateId,      
 CASE WHEN ISNULL(EUR.OperatorValue, '') <> '' THEN RIGHT(EUR.OperatorValue, (LEN(EUR.OperatorValue)-CHARINDEX('|', EUR.OperatorValue))) ELSE NULL END [Operator],    
    TPR.ThirdPartyRelationship AS UserRelationshipType,        
   CASE         
    WHEN TPR.ThirdPartyRelationshipId = 1 THEN NULL          
    WHEN TPR.ThirdPartyRelationshipId IN (2,3) THEN EUR.CompanyName        
    END AS CompanyName,        
 ISNULL(PPE.IsProductError, 0) AS 'PersonaHasProductError',        
    @OffsetMinutes,            
    COUNT(1) OVER () AS TotalRecords,          
    CASE @sortValue            
      WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName ASC)            
     -- WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(pct.ProductCount,0) ASC, p.FirstName + ' ' + p.LastName ASC)             
      WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin ASC, p.FirstName + ' ' + p.LastName ASC)            
      WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName ASC, p.FirstName + ' ' + p.LastName ASC)            
      WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName ASC, p.FirstName + ' ' + p.LastName ASC)            
      WHEN 105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UE.Employee,'') ASC, p.FirstName + ' ' + p.LastName ASC)          
      WHEN 106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UER.EnterpriseRoleName,'') ASC, p.FirstName + ' ' + p.LastName ASC)        
      WHEN 107 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(EUR.OperatorValue,'') ASC, p.FirstName + ' ' + p.LastName ASC)        
      WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName DESC)            
     -- WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(pct.ProductCount,0)  DESC, p.FirstName + ' ' + p.LastName DESC)              
      WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin DESC, p.FirstName + ' ' + p.LastName DESC)            
      WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName DESC, p.FirstName + ' ' + p.LastName DESC)            
      WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName DESC, p.FirstName + ' ' + p.LastName DESC)            
      WHEN -105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UE.Employee,'') DESC, p.FirstName + ' ' + p.LastName DESC)            
      WHEN -106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UER.EnterpriseRoleName,'') DESC, p.FirstName + ' ' + p.LastName DESC)          
      WHEN -107 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(EUR.OperatorValue,'') DESC, p.FirstName + ' ' + p.LastName DESC)        
     END AS RowNumber,        
  ulp.PersonaId         
    FROM #UserLogin ulp            
    INNER JOIN Person.Person p ON p.PartyId = ulp.PersonPartyId            
    INNER JOIN Enterprise.Party pa ON p.PartyId = pa.PartyID            
    LEFT JOIN #PartyContactMechanism PCM ON ulp.PersonPartyId = PCM.PartyId          
    LEFT JOIN Enterprise.ElectronicAddress EA ON PCM.ContactMechanismId=EA.ContactMechanismID          
    INNER JOIN Enterprise.PartyRelationship prs ON prs.PartyIdFrom = ulp.PersonPartyId AND prs.PartyIdTo = @PartyId            
    INNER JOIN Enterprise.RelationshipType rst ON rst.RelationshipTypeId = prs.PartyRelationshipTypeId            
    INNER JOIN Enterprise.RoleType rt ON (rt.PartyRoleTypeId = rst.RoleTypeIdValidFrom)            
    INNER JOIN Ident.IdentityProviderType ipt ON ulp.IdentityProviderTypeId = ipt.IdentityProviderTypeId             
    LEFT OUTER JOIN Enterprise.UserEmployeeId UE ON ulp.UserLoginPersonaId = UE.UserLoginPersonaId            
    LEFT OUTER JOIN #UserEnterpriseRole UER  ON ulp.PersonaId  = UER.PersonaId      
 LEFT OUTER JOIN Enterprise.ExternalUserRelationship EUR ON EUR.UserLoginPersonaId = ulp.UserLoginPersonaId        
    LEFT OUTER JOIN Enterprise.ThirdPartyRelationship TPR ON TPR.ThirdPartyRelationshipId = EUR.ThirdPartyRelationshipId 
	LEFT OUTER JOIN Enterprise.UserRelationShip EURS ON EURS.PartyRoleTypeId = prs.RoleTypeIdFrom and EURS.ThirdPartyRelationshipId = TPR.ThirdPartyRelationshipId  
 LEFT OUTER JOIN #PersonaProductError PPE ON PPE.PersonaId = ulp.PersonaId       
    WHERE  (            
    (@filterName IS NULL)            
    OR (CHARINDEX(@filterName, FirstName + ' ' + LastName, 1) > 0)            
    OR (CHARINDEX(@filterName, ulp.LoginName, 1) > 0)            
    OR (CHARINDEX(@filterName, UE.Employee, 1) > 0)            
    OR (CHARINDEX(@filterName, EA.ElectronicAddressString, 1) > 0)  
      )            
    AND  ((@NOW BETWEEN prs.FromDate AND prs.ThruDate) OR (@NOW >= prs.FromDate AND prs.ThruDate IS NULL))            
    AND  ((@ParentPartyRoleTypeId IS NULL) OR (rt.ParentPartyRoleTypeId = @ParentPartyRoleTypeId))            
    AND  ((@filterUserTypeCount = 0) OR (EURS.PartyRoleTypeId IN (SELECT UserTypeId from @filterUserType)) OR (EURS.Id IN (SELECT UserTypeId from @filterUserType)))      
    --AND  ((@filterPartyRoleTypeId IS NULL) OR (prs.RoleTypeIdFrom = @filterPartyRoleTypeId))        
    AND  ((@filterOperatorCount = 0 ) OR (EUR.OperatorValue in (select OperatorId from @filterOperator)))        
 AND  ((@filterPersonaProductError IS NULL) OR (PPE.IsProductError = @filterPersonaProductError))        
 )          
 SELECT          
    TotalRecords,          
    RealPageID,          
    PartyId,          
    FirstName,          
    MiddleName,          
    LastName,          
    EmployeeId,          
    EntepriseRoleName,          
    RoleTemplateId,       
    Operator,    
 --OperatorRealPageId,    
    UserRelationshipType,        
    CompanyName,        
    PersonaHasProductError,        
    UserId,          
    LoginName,        
    LastLogin,        
    FromDate,        
    ThruDate,        
    StatusId,          
    StatusName,        
    StatusThruDate,        
    Is3rdPartyIDP,         
    PasswordModifiedDate,        
    OffsetMinutes,              
    UserType,          
    PartyRoleTypeId,        
    PersonaId        
 INTO #Temp_Final        
 FROM cteUsersFinal    
  ORDER BY RowNumber          
 OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS          
 FETCH NEXT(@RowsPerPage) ROWS ONLY          
 OPTION (RECOMPILE)          
        
         
        
;WITH CTE AS         
(        
  SELECT           
   pp.PersonaId,            
   COUNT(pp.ProductId) AS ProductCount          
  FROM #PersonaProduct pp            
   INNER JOIN @AssignedProductIds ap ON (ap.ProductId = pp.ProductId)           
   INNER JOIN #Temp_Final F ON pp.PersonaId = F.PersonaId        
  GROUP BY pp.PersonaId         
)        
SELECT         
    F.TotalRecords,          
    F.RealPageID,          
    F.PartyId,          
    F.FirstName,          
    F.MiddleName,          
    F.LastName,          
    F.EmployeeId,          
    F.EntepriseRoleName,          
    F.RoleTemplateId,        
    Operator,    
    UserRelationshipType,        
    CompanyName,        
    F.PersonaHasProductError,        
    F.UserId,          
    F.LoginName,        
    F.LastLogin,        
    F.FromDate,        
    F.ThruDate,        
    F.StatusId,          
    F.StatusName,        
    F.StatusThruDate,        
    F.Is3rdPartyIDP,         
    F.PasswordModifiedDate,        
    F.OffsetMinutes,              
    ISNULL(pct.ProductCount, 0) AS Products,          
    F.UserType,          
    F.PartyRoleTypeId         
FROM         
 #Temp_Final F        
LEFT JOIN CTE pct ON pct.PersonaId = F.PersonaId         
          
 DROP INDEX IF EXISTS [NCI_cteUserLogin_PersonPartyId] ON [dbo].[#UserLogin]          
 DROP TABLE IF EXISTS #UserLogin          
 DROP TABLE IF EXISTS #PersonaProduct          
 DROP TABLE IF EXISTS #PartyContactMechanism         
 DROP TABLE IF EXISTS #PersonaProductError        
 DROP TABLE IF EXISTS #Temp_Final        
        
END;
GO