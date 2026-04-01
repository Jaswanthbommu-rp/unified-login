CREATE PROCEDURE [Person].[ListPersons_Export]   
(  
 @RealPageId uniqueidentifier = NULL,  
 @ParentPartyRoleTypeId int = NULL,  
 @UserListFilterType tinyint = 0,  
 @AssignedProducts nvarchar(max),  
 @FilterBy nvarchar(max),  
 @SortBy nvarchar(max),  
 @RowsPerPage int = 0,  
 @PageNumber int = 1  
)  
AS  
BEGIN      
    
  DECLARE @PartyId bigint,    
  @NOW datetime= GETUTCDATE(),    
  @sortOrder nvarchar(128),    
  @sortDirection nvarchar(4),    
  @sortValue int = 100,    
  @filterName nvarchar(512),    
  @filterProductId int = NULL,    
  @filterStatusTypeId int = 0,    
  @filterEnterpriseRoleCount int = 0,    
  @filterUserTypeCount INT = 0,
  @filterPartyRoleTypeId VARCHAR(MAX) = NULL,    
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
    
 SELECT @EmployeeCompanyPartyId = PartyId FROM Enterprise.Party P    
 WHERE P.RealPageId = '0D018E46-C20E-477D-ADED-4E5A35FB8F99'    
    
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
  AND  pec.StatusTypeId = 8    
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
    
 DROP INDEX IF EXISTS [NCI_Temp_PersonaProduct_ProductId] ON [dbo].[#PersonaProduct]    
 CREATE NONCLUSTERED INDEX [NCI_Temp_PersonaProduct_ProductId] ON [dbo].[#PersonaProduct] ([ProductId]) INCLUDE ([PersonaId])    
     
 DROP TABLE IF EXISTS #CustomFields    
    
 SELECT  Id,UserLoginPersonaId,FieldValue,Enabled,Name,Value,Sequence    
 INTO #CustomFields    
 From (    
  Select sr.[SettingTableRowId] AS 'Id',    
      st.[PartyId] AS 'OrganizationId',    
      srv.UserLoginPersonaId 'UserLoginPersonaId',    
      srv.Value 'FieldValue',    
      [TableColumnName],    
      [TableColumnValue]    
  from [Settings].[SettingTableColumn] stc    
  join [Settings].[SettingTableRow] sr on    
   stc.[SettingTableRowId] = sr.[SettingTableRowId]    
  join Settings.SettingTableRowValue SRV on    
   srv.SettingTableRowId = sr.SettingTableRowId    
  join [Settings].[SettingTable] st on    
   st.[SettingTableId] = sr.[SettingTableId]    
  where st.[PartyId] = @PartyId) As SourceTable    
 PIVOT    
 (    
  MIN([TableColumnValue])    
  FOR [TableColumnName] IN (Enabled,Name,Value,Sequence)    
 ) AS PivotOutput    
    
 Delete from #CustomFields     
 Where Enabled = 0    
 AND ((FieldValue IS NOT NULL) OR (LEN(FieldValue) > 0) )      
    
 SELECT @minSequence = MIN(Sequence)    
 FROM  #CustomFields    
    
 Delete from #CustomFields     
 Where [Sequence] <> @minSequence    
    
    
 INSERT INTO #PersonaProduct ( PersonaId, ProductId )    
 SELECT PC.PersonaId,36 from Ident.SAmlUserAttribute SUA
 INNER JOIN Enterprise.[PersonaConfiguration] PC on PC.PersonaId = SUA.PersonaId 
 INNER JOIN @AssignedProductIds ASP ON ASP.ProductId = SUA.ProductId
 WHERE SUA.productId = 36 and  PC.productId = 36 and PC.StatusTypeId = 0 

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
  PasswordModifiedDate smalldatetime NULL    
 )    
    
 INSERT INTO #UserLogin    
 (    
 PersonaId,UserLoginPersonaId,PersonPartyId,UserId,LoginName,LastLogin,FromDate,ThruDate,IdentityProviderTypeId    
 ,StatusId,StatusName,StatusThruDate,PasswordModifiedDate    
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
  WHERE PE.PersonaId = PersonaID AND (@filterProductId=4 OR ProductId = @filterProductId)      
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
  
 DROP TABLE IF EXISTS #ProductCount    
    
 CREATE TABLE #ProductCount    
 (    
 PersonaId INT NOT NULL PRIMARY KEY,    
 ProductCount INT NOT NULL    
 )    
    
 INSERT INTO #ProductCount    
 (    
  PersonaId,ProductCount    
 )    
 SELECT     
  pp.PersonaId,      
  COUNT(pp.ProductId) AS ProductCount    
 FROM #PersonaProduct pp      
  INNER JOIN @AssignedProductIds ap ON (ap.ProductId = pp.ProductId)      
 GROUP BY pp.PersonaId      
    
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
   SELECT pcm.PartyId,pcm.ContactMechanismId,ROW_NUMBER() OVER(PARTITION BY pcm.PartyId ORDER BY pcm.FromDate DESC) AS RowNo    
   FROM Enterprise.PartyContactMechanism pcm    
   INNER JOIN #UserLogin UL ON pcm.PartyId = UL.PersonPartyId    
   WHERE pcm.ThruDate > GETUTCDATE()    
 ) X    
 WHERE X.RowNo = 1    
    
 DROP TABLE IF EXISTS #UserSupervisor    
    
 CREATE TABLE #UserSupervisor    
 (    
  UserId BIGINT PRIMARY KEY,    
  SupervisorFirstName VARCHAR(100) NULL,    
  SupervisorLastName VARCHAR(100) NULL,    
  SupervisorLoginName VARCHAR(255) NULL    
 )    
    
 INSERT INTO #UserSupervisor (UserId, SupervisorFirstName, SupervisorLastName, SupervisorLoginName)    
 SELECT DISTINCT UL.UserId, SP.FirstName, SP.LastName, SUL.LoginName    
 FROM #UserLogin UL    
 INNER JOIN Enterprise.UserSuperVisor USV ON USV.UserId = UL.UserId    
 INNER JOIN Ident.UserLogin SUL ON SUL.UserId = USV.SuperVisorUserId    
 INNER JOIN Person.Person SP ON SP.PartyId = SUL.PersonPartyId    
    
 DROP TABLE IF EXISTS #UserPhoneNumber    
    
 CREATE TABLE #UserPhoneNumber    
 (    
  PersonPartyId BIGINT PRIMARY KEY,    
  PhoneNumber VARCHAR(40) NULL,    
  PhoneNumberType NVARCHAR(50) NULL    
 )    
    
 INSERT INTO #UserPhoneNumber (PersonPartyId, PhoneNumber, PhoneNumberType)
 SELECT PersonPartyId, PhoneNumber, PhoneNumberType
 FROM (
  SELECT UL.PersonPartyId,
   tm.PhoneNumber AS PhoneNumber,
   cmut.Name AS PhoneNumberType,
   ROW_NUMBER() OVER (PARTITION BY UL.PersonPartyId ORDER BY pcm.FromDate DESC) AS RowNo
  FROM #UserLogin UL
  INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyId = UL.PersonPartyId
  INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
  INNER JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
  INNER JOIN Enterprise.ContactMechanismUsage cmu ON cmu.PartyContactMechanismID = pcm.PartyContactMechanismId
  INNER JOIN Enterprise.ContactMechanismUsageType cmut ON cmut.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
  WHERE tm.[Default] = 1
  AND (pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())
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
  Title,    
  Suffix,    
  CustomField,    
  UserId,    
  LoginName,    
  LastLogin,    
  FromDate,    
  ThruDate,    
  StatusId,    
  StatusName,    
  StatusThruDate,    
  Is3rdPartyIDP,    
  Products,    
  Properties,    
  UserType,    
  PartyRoleTypeId,    
  PasswordModifiedDate,    
  EntepriseRoleName,    
  RoleTemplateId,    
  Operator,    
  UserRelationshipType,    
  CompanyName,    
  SupervisorFirstName,    
  SupervisorLastName,    
  SupervisorLoginName,    
  PhoneNumber,    
  PhoneNumberType,    
  OffsetMinutes,    
  TotalRecords,    
  RowNumber    
 )    
 AS     
 (    
    SELECT     
    pa.RealpageId AS RealPageID,      
    p.PartyId,      
    p.FirstName,      
    p.MiddleName,      
    p.LastName,      
    UE.Employee AS EmployeeId,      
    p.Title,      
    p.Suffix,      
    CASE      
     WHEN cf.FieldValue IS NULL THEN ''      
     ELSE cf.FieldValue      
    END AS 'CustomFieldValue',      
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
    ISNULL(pct.ProductCount, 0) AS Products,      
    0 AS Properties,      
    ISNULL(rt.Name, '') AS UserType,      
    prs.RoleTypeIdFrom AS PartyRoleTypeId,      
    ulp.PasswordModifiedDate,     
    UER.EnterpriseRoleName,    
    UER.RoleTemplateId,    
    CASE WHEN ISNULL(EUR.OperatorValue, '') <> '' THEN RIGHT(EUR.OperatorValue, (LEN(EUR.OperatorValue)-CHARINDEX('|', EUR.OperatorValue))) ELSE NULL END [Operator],    
    TPR.ThirdPartyRelationship as UserRelationshipType,    
    EUR.CompanyName AS CompanyName,    
    USUP.SupervisorFirstName,    
    USUP.SupervisorLastName,    
    USUP.SupervisorLoginName,    
    UPHN.PhoneNumber,    
    UPHN.PhoneNumberType,    
    @OffsetMinutes,      
    COUNT(1) OVER () AS TotalRecords,    
    CASE @sortValue      
      WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName ASC)      
      WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(pct.ProductCount,0) ASC, p.FirstName + ' ' + p.LastName ASC)      
      WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin ASC, p.FirstName + ' ' + p.LastName ASC)      
      WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName ASC, p.FirstName + ' ' + p.LastName ASC)      
      WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName ASC, p.FirstName + ' ' + p.LastName ASC)      
      WHEN 105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UE.Employee,'') ASC, p.FirstName + ' ' + p.LastName ASC)    
      WHEN 106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UER.EnterpriseRoleName,'') ASC, p.FirstName + ' ' + p.LastName ASC)     
      WHEN 107 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(EUR.OperatorValue,'') ASC, p.FirstName + ' ' + p.LastName ASC)    
      WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName DESC)      
      WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(pct.ProductCount,0)  DESC, p.FirstName + ' ' + p.LastName DESC)      
      WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ulp.LastLogin DESC, p.FirstName + ' ' + p.LastName DESC)      
      WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY ulp.LoginName DESC, p.FirstName + ' ' + p.LastName DESC)      
      WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY ulp.StatusName DESC, p.FirstName + ' ' + p.LastName DESC)      
      WHEN -105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UE.Employee,'') DESC, p.FirstName + ' ' + p.LastName DESC)      
      WHEN -106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(UER.EnterpriseRoleName,'') DESC, p.FirstName + ' ' + p.LastName DESC)     
      WHEN -107 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(EUR.OperatorValue,'') DESC, p.FirstName + ' ' + p.LastName DESC)     
     END AS RowNumber    
    FROM #UserLogin ulp      
    INNER JOIN Person.Person p ON p.PartyId = ulp.PersonPartyId      
    INNER JOIN Enterprise.Party pa ON p.PartyId = pa.PartyID      
    LEFT JOIN #PartyContactMechanism PCM ON ulp.PersonPartyId = PCM.PartyId    
    LEFT JOIN Enterprise.ElectronicAddress EA ON PCM.ContactMechanismId=EA.ContactMechanismID    
    INNER JOIN Enterprise.PartyRelationship prs ON prs.PartyIdFrom = ulp.PersonPartyId AND prs.PartyIdTo = @PartyId      
    INNER JOIN Enterprise.RelationshipType rst ON rst.RelationshipTypeId = prs.PartyRelationshipTypeId      
    INNER JOIN Enterprise.RoleType rt ON (rt.PartyRoleTypeId = rst.RoleTypeIdValidFrom)      
    INNER JOIN Ident.IdentityProviderType ipt ON ulp.IdentityProviderTypeId = ipt.IdentityProviderTypeId      
    LEFT OUTER JOIN #ProductCount pct ON pct.PersonaId = ulp.PersonaId      
    LEFT OUTER JOIN #CustomFields cf ON (cf.UserLoginPersonaId = ulp.UserLoginPersonaId)      
    LEFT OUTER JOIN Enterprise.UserEmployeeId UE ON ulp.UserLoginPersonaId = UE.UserLoginPersonaId      
    LEFT OUTER JOIN #UserEnterpriseRole UER  ON ulp.PersonaId  = UER.PersonaId    
    LEFT OUTER JOIN Enterprise.ExternalUserRelationship EUR ON EUR.UserLoginPersonaId = ulp.UserLoginPersonaId    
    LEFT OUTER JOIN Enterprise.ThirdPartyRelationship TPR ON TPR.ThirdPartyRelationshipId = EUR.ThirdPartyRelationshipId
    LEFT OUTER JOIN Enterprise.UserRelationShip EURS ON EURS.PartyRoleTypeId = prs.RoleTypeIdFrom and EURS.ThirdPartyRelationshipId = TPR.ThirdPartyRelationshipId
    LEFT OUTER JOIN #UserSupervisor USUP ON USUP.UserId = ulp.UserId    
    LEFT OUTER JOIN #UserPhoneNumber UPHN ON UPHN.PersonPartyId = ulp.PersonPartyId    
    WHERE  (      
    (@filterName IS NULL)      
    OR (CHARINDEX(@filterName, FirstName + ' ' + LastName, 1) > 0)      
    OR (CHARINDEX(@filterName, ulp.LoginName, 1) > 0)      
    OR (CHARINDEX(@filterName, cf.FieldValue, 1) > 0)      
    OR (CHARINDEX(@filterName, UE.Employee, 1) > 0)      
    OR (CHARINDEX(@filterName, EA.ElectronicAddressString, 1) > 0)  
      )      
    AND  ((@NOW BETWEEN prs.FromDate AND prs.ThruDate) OR (@NOW >= prs.FromDate AND prs.ThruDate IS NULL))      
    AND  ((@ParentPartyRoleTypeId IS NULL) OR (rt.ParentPartyRoleTypeId = @ParentPartyRoleTypeId))      
    --AND  ((@filterPartyRoleTypeId IS NULL) OR (prs.RoleTypeIdFrom = @filterPartyRoleTypeId))    
    AND  ((@filterUserTypeCount = 0) OR (EURS.PartyRoleTypeId IN (SELECT UserTypeId from @filterUserType)) OR (EURS.Id IN (SELECT UserTypeId from @filterUserType)))
    AND  ((@filterOperatorCount = 0 ) OR (EUR.OperatorValue in (select OperatorId from @filterOperator)))        
 )    
 SELECT TotalRecords,    
    RealPageID,    
    PartyId,    
    FirstName,    
    MiddleName,    
    LastName,    
    EmployeeId,    
    Title,    
    Suffix,    
    CustomField,    
    EntepriseRoleName,    
    RoleTemplateId,    
    Operator,    
    UserRelationshipType,    
    CompanyName,    
    SupervisorFirstName,    
    SupervisorLastName,    
    SupervisorLoginName,    
    PhoneNumber,    
    PhoneNumberType,    
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
    Products,    
    Properties,    
    UserType,    
    PartyRoleTypeId        
 FROM cteUsersFinal    
 ORDER BY RowNumber    
 OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS    
 FETCH NEXT(@RowsPerPage) ROWS ONLY    
 OPTION (RECOMPILE)    
    
 DROP INDEX IF EXISTS [NCI_cteUserLogin_PersonPartyId] ON [dbo].[#UserLogin]    
 DROP TABLE IF EXISTS #ProductCount     
 DROP TABLE IF EXISTS #UserLogin    
 DROP TABLE IF EXISTS #PersonaProduct    
 DROP TABLE IF EXISTS #PartyContactMechanism    
 DROP TABLE IF EXISTS #UserSupervisor    
 DROP TABLE IF EXISTS #UserPhoneNumber    
    
END;