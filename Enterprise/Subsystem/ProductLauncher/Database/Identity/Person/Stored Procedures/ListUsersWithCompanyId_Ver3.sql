--EXEC [Person].[ListUsersWithCompanyId_Ver3]  9895,'BlueBook',NULL,0,1,NULL,NULL,NULL
--EXEC [Person].[ListUsersWithCompanyId_Ver3]  9895,'BlueBook','26',0,1,'analyst',NULL,NULL
--EXEC [Person].[ListUsersWithCompanyId_Ver3]  9895,'BlueBook','26',0,1,NULL,NULL,NULL
CREATE PROCEDURE [Person].[ListUsersWithCompanyId_Ver3]  
(@CompanyId   INT,   
 @Source      NVARCHAR(50)  = 'BlueBook',   
 @ProductId   NVARCHAR(200) = NULL,   
 @RowsPerPage INT           = 0,   
 @PageNumber  INT           = 1,  
 @Roles    NVARCHAR(1000) = NULL,  
 @Rights   NVARCHAR(1000) = NULL,  
 @Properties  NVARCHAR(MAX) = NULL,
 @CompanyDomain NVARCHAR(20) = 'primary'
)AS  
BEGIN  
   

 DECLARE @domainId INT = 0
 DECLARE @Now DATETIME= GETUTCDATE();  
 DECLARE @RoleList TABLE(RoleShortName NVARCHAR(255));  
 DECLARE @RightList TABLE(RightName NVARCHAR(255));  
 DECLARE @ProductCount INT= 1;  
 DECLARE @RoleCount INT= 1;  
 DECLARE @RightCount INT= 1;  
 DECLARE @OrganizationPartyId BIGINT  
 DECLARE @ProductIds Enterprise.ProductIdType  
 DECLARE @PartyId INT;
   
  SELECT   
  @RowsPerPage = CASE  
      WHEN @RowsPerPage <= 0  
                        THEN 2147483647  
                        ELSE @RowsPerPage  
    END;  
  

SELECT @domainId = OrganizationDomainId
FROM Enterprise.OrganizationDomain
WHERE NAME = @CompanyDomain


SELECT @PartyId = m.PartyId
FROM Enterprise.VW_DataImportMapping m
JOIN Enterprise.Organization org on org.PartyId = m.PartyId
Where m.CompanyMasterId = @CompanyId
AND org.OrganizationDomainId = @domainId


 IF (@Roles IS NULL AND @Rights IS NULL AND @Properties IS NULL)  
 BEGIN  
  EXEC [Person].[ListUsersWithCompanyId_VER2] @CompanyId = @CompanyId , @ProductId = @ProductId, @RowsPerPage = @RowsPerPage , @PageNumber = @PageNumber, @companyDomain = @CompanyDomain;  
  RETURN;  
 END  
  
 CREATE TABLE #ProductsList2  
 (  
  PersonaId   BIGINT,  
  ProductId   INT,  
  TargetProductId  INT  
 )  
  
 CREATE TABLE #UserList  
 (  
  UserId        BIGINT,   
  LoginName     NVARCHAR(255),   
  FirstName     NVARCHAR(50),   
  LastName      NVARCHAR(50),   
  AddressString NVARCHAR(255),  
  PersonaId     BIGINT,  
  PreferredPhoneNumber varchar(30),  
  Email VARCHAR(255)  
 );  
  
 INSERT INTO @ProductIds(ProductId)  
    (  
  SELECT *  
  FROM STRING_SPLIT(@ProductId, ',')  
 );  
  
 INSERT INTO @RoleList(RoleShortName)  
    (  
        SELECT *  
        FROM STRING_SPLIT(@Roles, ',')  
    );  
  
 INSERT INTO @RightList(RightName)  
    (  
        SELECT *  
        FROM STRING_SPLIT(@Rights, ',')  
    );  
  
 IF (SELECT COUNT(*)  
        FROM @ProductIds) = 0  
 BEGIN  
  SET @ProductCount = NULL;  
    END;  
  
 IF(SELECT COUNT(*)  
        FROM @RoleList) = 0  
    BEGIN  
  SET @RoleCount = NULL;  
    END;  
  
 IF(SELECT COUNT(*)  
        FROM @RightList) = 0  
    BEGIN  
  SET @RightCount = NULL;  
    END;  
  
 --Preferred mobile number logic  
 DECLARE @ContactPreference TABLE( PersonaId INT, PreferredPhoneNumber VARCHAR(30))  
 
 INSERT INTO @ContactPreference(PersonaId,PreferredPhoneNumber)  
 SELECT AP.PersonaId AS PersonaId, ISNULL(TM.CountryCode,'') + TM.AreaCode + TM.PhoneNumber FROM   
      Enterprise.TelecommunicationsNumber tm   
      INNER JOIN Enterprise.PartyContactMechanism pcm ON tm.ContactMechanismID = pcm.ContactMechanismID  
      INNER JOIN Person.ActivePersona AP ON AP.PartyId = PCM.PartyId  
      INNER JOIN Enterprise.[ContactMechanismPreference] CMP   
		ON CMP.ContactMechanismID = PCM.ContactMechanismId 
		AND (PCM.ThruDate IS NULL OR PCM.ThruDate > GETUTCDATE())  
	  JOIN Ident.UserLogin ul on ul.PersonPartyId = pcm.PartyId
	  JOIN Ident.UserLoginPersona ulp on ulp.UserLoginId = ul.UserId
	  Where ulp.OrganizationPartyId = @PartyId

 --Notification Email  
 DECLARE @NotificationEmail TABLE (PartyId BIGINT, Email VARCHAR(255))  
 INSERT INTO @NotificationEmail(PartyId , Email)  
 SELECT  
  p.PartyId,  
  ea.ElectronicAddressString AS NotificationEmail  
 FROM Enterprise.ContactMechanismUsage cmu  
  INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID  
  INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId  
  INNER JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID  
  INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
  INNER JOIN Ident.UserLogin ul on ul.PersonPartyId = p.PartyId
  INNER JOIN Ident.UserLoginPersona ulp on ulp.UserLoginId = ul.UserId  
 WHERE  
  (pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())  
  AND cmu.ContactMechanismUsageTypeID = 301
  AND ulp.OrganizationPartyId = @PartyId;  
  
 IF EXISTS (SELECT TOP 1 ProductId FROM @ProductIds)  
    BEGIN  
    
  CREATE TABLE #NoPersona  
  (  
   PersonaId BIGINT  
  )  
  
  INSERT INTO #NoPersona(PersonaId)  
  (  
   SELECT DISTINCT  
    pe.PersonaId    
   FROM Enterprise.MasterConfigurationType mct    
    INNER JOIN Enterprise.MasterSettingType MST ON mst.MasterConfigurationTypeId = mct.MasterCOnfigurationTypeId    
    INNER JOIN Enterprise.MasterSetting ms ON ms.MasterSettingTypeId = mst.MasterSettingTYpeId    
    INNER JOIN Enterprise.Party p ON CONVERT(NVARCHAR(40), p.RealPageId) = ms.Value    
    INNER JOIN ident.UserLogin ul ON UL.PersonPartyId = p.PartyId    
    INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId    
    INNER JOIN Person.Persona pe ON pe.UserLoginPersonaId = ulp.UserLoginPersonaId    
   WHERE    
    mct.Name = 'Organization'    
    AND mst.Name = 'RealPageEmployeeAccessID'  
  )  
  
  
SELECT @OrganizationPartyId = o.PartyId  
FROM Enterprise.VW_DataImportMapping m
JOIN Enterprise.Organization o on o.PartyId = m.PartyId
WHERE CompanyMasterId = @CompanyId
AND o.OrganizationDomainId = @domainId
    
  IF EXISTS(SELECT TOP 1 1 FROM STRING_SPLIT(@Properties,','))  
  BEGIN  
   DECLARE @IDS TABLE(propertyId NVARCHAR(255))   
   DECLARE @TableInstance TABLE(value varchar(2) , productId int);  
   DECLARE @ProductIdsAux TABLE(ProductId INT);  
  
   INSERT INTO @ProductIdsAux (ProductId)  
    SELECT CASE WHEN ProductId = 45 OR ProductId = 56 THEN 3 ELSE ProductId END FROM @ProductIds;   
  
   INSERT INTO @TableInstance(value , productId)  
   (  
    SELECT   
     CASE WHEN ps.Value IS NULL THEN '0' ELSE ps.Value END AS value,  
     pdx.ProductId  
    FROM Enterprise.GlobalProductConfiguration gpc  
      JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
      JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
      JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
      JOIN @ProductIdsAux AS pdx ON gpc.ProductId = pdx.ProductId  
    WHERE    
    gpc.ProductId IN (SELECT ProductId FROM @ProductIdsAux)   
    AND (gpc.ThruDate IS NULL)  
    AND ( pc.ThruDate IS NULL)  
    AND ( ps.ThruDate IS NULL)  
    And PST.Name = 'UsePropertyInstanceUnifiedLogin'  
   );  
  
   IF EXISTS(SELECT TOP 1 1 FROM @TableInstance WHERE value = '1')  
   BEGIN  
    DECLARE @GUIDS TABLE(propertyGuid UNIQUEIDENTIFIER)   
  
    INSERT INTO @GUIDS(propertyGuid)  
    (  
     SELECT *    
     FROM STRING_SPLIT(@Properties, ',')    
     WHERE value LIKE'%-%'  
    )  
  
    INSERT INTO  #UserList  
    (  
     UserId,     
     LoginName,     
     FirstName,     
     LastName,    
     PersonaId,  
     PreferredPhoneNumber,  
     Email  
    )  
    SELECT DISTINCT  
     ul.UserId,   
     ul.LoginName,     
     p2.FirstName,     
     p2.LastName,     
     p.PersonaId,  
     CP.PreferredPhoneNumber,  
     ne.Email  
    FROM Enterprise.PropertyInstanceMapping AS pim  
     INNER JOIN Enterprise.PropertyInstance AS pi1 ON pim.PropertyInstanceId = pi1.PropertyInstanceId and pi1.IsDeleted = 0  
     INNER JOIN @TableInstance AS ti ON pim.ProductId = ti.productId  
     INNER JOIN Person.Persona AS p ON pim.PersonaId = p.PersonaId  
     INNER JOIN Ident.UserLoginPersona AS ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId    
     INNER JOIN ident.UserLogin AS ul ON ulp.UserLoginId = ul.UserId    
     INNER JOIN Person.Person AS p2 ON ul.PersonPartyId = p2.PartyId  
     LEFT OUTER JOIN @ContactPreference CP ON CP.PersonaId = P.PersonaId  
     LEFT OUTER JOIN @NotificationEmail ne ON ne.PartyId = p2.PartyId  
    WHERE  
     pim.ProductId IN (SELECT ti.ProductId FROM @TableInstance ti WHERE ti.value = '1')  
     AND pi1.InstanceId IN( SELECT propertyGuid FROM @GUIDS)  
     AND ulp.StatusTypeId = 1    
     AND ulp.OrganizationPartyId = @OrganizationPartyId  
     AND P.PersonaId NOT IN (SELECT PersonaId FROM #NoPersona)  
     
   END  
     
   INSERT INTO @IDS(propertyId)  
   (  
    SELECT *   
    FROM STRING_SPLIT(@Properties, ',')    
    WHERE value NOT LIKE'%-%'  
   )  
  
   IF EXISTS (SELECT TOP 1 1 FROM @IDS)  
   BEGIN  
    INSERT INTO  #UserList  
    (  
     UserId,     
     LoginName,     
     FirstName,     
     LastName,    
     PersonaId,  
     CP.PreferredPhoneNumber,  
     Email  
    )  
    SELECT DISTINCT  
     ul.UserId,   
     ul.LoginName,     
     p2.FirstName,     
     p2.LastName,     
     p.PersonaId,  
     cp.PreferredPhoneNumber,  
     ne.Email  
    FROM Enterprise.propertymapping AS pm  
     INNER JOIN Person.Persona AS p ON pm.PersonaId = p.PersonaId  
     INNER JOIN Ident.UserLoginPersona AS ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId    
     INNER JOIN ident.UserLogin AS ul ON ulp.UserLoginId = ul.UserId    
     INNER JOIN Person.Person AS p2 ON ul.PersonPartyId = p2.PartyId  
     LEFT OUTER JOIN @ContactPreference CP ON CP.PersonaId = P.PersonaId  
     LEFT OUTER JOIN @NotificationEmail ne ON ne.PartyId = p2.PartyId  
    WHERE  
     pm.ProductId IN (SELECT ProductId  
         FROM @ProductIdsAux pdx   
         WHERE pdx.productId NOT IN  
          (  
           SELECT ti.ProductId   
           FROM @TableInstance ti   
           WHERE ti.value = '1'))  
  
     AND pm.PropertyId IN( SELECT CONVERT(BIGINT, propertyId) FROM @IDS)  
     AND ulp.StatusTypeId = 1    
     AND ulp.OrganizationPartyId = @OrganizationPartyId  
     AND P.PersonaId NOT IN (SELECT PersonaId FROM #NoPersona)  
   END  
  END  
  
  IF (@RoleCount IS NOT NULL OR @RightCount IS NOT NULL)  
  BEGIN  
  
   INSERT INTO #ProductsList2  
    EXEC [Security].[GetPersonaProductsByOrganizationPartyId] @ProductIds = @ProductIds, @OrganizationPartyId = @OrganizationPartyId;  
  
   CREATE TABLE #result (Userid BIGINT  
        , LoginName VARCHAR(200)  
        , firstname VARCHAR(200)  
        , Lastname VARCHAR(200)  
        , personaid INT  
        , TargetProductId INT  
        , ProductId INT  
        , PreferredPhoneNumber VARCHAR(30)  
        , Email VARCHAR(255)  
        )  
  
   INSERT INTO #result  
   SELECT distinct  
    ul.UserId,   
    ul.LoginName,     
    p2.FirstName,     
    p2.LastName,     
    p.PersonaId ,  
    r2.TargetProductId,  
    r2.ProductId,  
    CPR.PreferredPhoneNumber,  
    ne.Email  
   FROM #ProductsList2 AS cp    
    INNER JOIN Person.Persona AS p ON cp.PersonaId = p.PersonaId    
    INNER JOIN [Security].[PersonaRole] AS pr ON p.PersonaId = pr.PersonaId    
    INNER JOIN [Security].[Role] AS r ON pr.RoleId = r.RoleId AND cp.ProductId = r.ProductId    
    INNER JOIN [Security].[RoleRight] AS rr ON r.RoleId = rr.RoleId    
    INNER JOIN [Security].[Right] AS r2 ON rr.RightId = r2.RightId    
    INNER JOIN Ident.UserLoginPersona AS ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId    
    INNER JOIN ident.UserLogin AS ul ON ulp.UserLoginId = ul.UserId    
    INNER JOIN Person.Person AS p2 ON ul.PersonPartyId = p2.PartyId  
    LEFT OUTER JOIN @ContactPreference CPR ON CPR.PersonaId = P.PersonaId  
    LEFT OUTER JOIN @NotificationEmail ne ON ne.PartyId = p2.PartyId  
   WHERE     
    ulp.StatusTypeId = 1    
    AND ulp.OrganizationPartyId = @OrganizationPartyId    
    AND (@RoleCount IS NULL OR r.ShortName IN (SELECT RoleShortName FROM @RoleList))    
    AND (@RightCount IS NULL OR r2.RightName IN (SELECT RightName FROM @RightList))    
    
    AND P.PersonaId NOT IN (SELECT PersonaId FROM #NoPersona)    
    
   ;WITH Users    
   AS     
   ((    
    SELECT DISTINCT  
     r2.UserId,     
     r2.LoginName,     
     r2.FirstName,     
     r2.LastName,     
     r2.PersonaId,  
     r2.PreferredPhoneNumber,  
     r2.Email  
       
    FROM #result r2    
     INNER JOIN Enterprise.PersonaConfiguration AS pc ON pc.PersonaId = r2.PersonaId     
    UNION    
    SELECT DISTINCT  
     r2.UserId,     
     r2.LoginName,     
     r2.FirstName,     
     r2.LastName,     
     r2.PersonaId,  
     r2.PreferredPhoneNumber,  
     r2.Email  
    FROM #result r2  
     INNER JOIN Enterprise.productright AS pc ON r2.TargetProductId = pc.ProductId     
    WHERE    
     r2.TargetProductId IN (SELECT TargetProductId FROM #ProductsList2 )    
     AND (r2.TargetProductId <> r2.ProductId)  
   ))    
    
   INSERT INTO #UserList    
   (  
    UserId,     
    LoginName,     
    FirstName,     
    LastName,    
    PersonaId,  
    PreferredPhoneNumber,  
    Email  
   )    
   SELECT  DISTINCT  
    UserId,     
    LoginName,     
    FirstName,     
    LastName,    
    PersonaId,  
    PreferredPhoneNumber,  
    Email  
   FROM Users AS u;  
  END  
 END    
   
 CREATE TABLE #totalusers (UserId int  
        , LoginName varchar(200)  
        , FirstName varchar(200)  
        , LastName varchar(200)  
        , PersonaId  int  
        , PreferredPhoneNumber VARCHAR(30)  
        , Email VARCHAR(255))  
   
 INSERT INTO #totalusers  
 SELECT DISTINCT         
  UserId,     
  LoginName,     
  FirstName,     
  LastName,    
  PersonaId,  
  PreferredPhoneNumber,  
  Email  
 FROM #UserList    
  
 SELECT    
  UserId,     
  LoginName,     
  FirstName,     
  LastName,    
  PersonaId,  
  PreferredPhoneNumber,  
  COUNT(1) OVER() AS TotalRecords,  
  Email  
 FROM #totalusers         
  ORDER BY UserId    
  OFFSET((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT(@RowsPerPage) ROWS ONLY;    
  
END;