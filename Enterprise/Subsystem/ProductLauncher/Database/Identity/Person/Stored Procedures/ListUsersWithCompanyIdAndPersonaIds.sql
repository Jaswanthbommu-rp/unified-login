CREATE PROCEDURE [Person].[ListUsersWithCompanyIdAndPersonaIds]        
(@CompanyId   NVARCHAR(100) = 0,           
 @Source      NVARCHAR(50)  = 'BlueBook',         
 @ProductId   NVARCHAR(200) = NULL,         
 @RowsPerPage INT           = 0,         
 @PageNumber  INT           = 1,        
 @UserPersonaIds NVARCHAR(MAX) = NULL   
)AS    
BEGIN      
        
    DECLARE @Now DATETIME = GETUTCDATE();
	DECLARE @CompanyOrganizationProduct TABLE ( ProductId INT )         
	DECLARE @UserProducts TABLE ( PersonaId INT,ProductId INT, isFavorite TINYINT, StatusTypeId INT )        
	DECLARE @LearningProductID INT = 19       
	DECLARE @AdminPortalProductID INT = 89    
	DECLARE @SimonHelpProductID INT = 49   
    
    DECLARE @ProductIdList TABLE      
    (      
        ProductId INT      
    );   
	INSERT INTO @ProductIdList      
    (      
        ProductId      
    )      
    SELECT *      
    FROM STRING_SPLIT(@ProductId, ',');  
	  
    CREATE TABLE #OrganizationPartyIds      
    (      
        OrgPartyId INT      
    );    

    DECLARE @PersonaIds TABLE      
    (      
        PersonaId INT      
    );      
    INSERT INTO @PersonaIds      
    (      
        PersonaId      
    )      
    SELECT *      
    FROM STRING_SPLIT(@UserPersonaIds, ',');  

	DECLARE @ContactPreference TABLE      
    (      
        PartyId INT,      
        PreferredPhoneNumber VARCHAR(30)      
    );  

	DECLARE @NotificationEmail TABLE      
    (      
        PartyId BIGINT,      
        Email VARCHAR(255)      
    );   
	          
    CREATE TABLE #UserList      
    (      
        UserId BIGINT,      
        LoginName NVARCHAR(255),      
        FirstName NVARCHAR(50),      
        LastName NVARCHAR(50),      
        AddressString NVARCHAR(255),      
        PersonaId BIGINT,      
        PreferredPhoneNumber VARCHAR(30),      
        Email VARCHAR(255)      
    );      
    CREATE NONCLUSTERED INDEX [NC_Uerlist_userID]      
    ON #UserList ([UserId] ASC);      
      
    SELECT @RowsPerPage = CASE      
                              WHEN @RowsPerPage <= 0 THEN      
                                  2147483647      
                              ELSE      
                                  @RowsPerPage      
                          END;   

	INSERT INTO #OrganizationPartyIds(OrgPartyId)  
	SELECT m.PartyId        
	FROM Enterprise.VW_DataImportMapping m        
	JOIN Enterprise.Organization org on org.PartyId = m.PartyId        
	Where m.CompanyMasterId = @CompanyId  
 
	INSERT INTO @CompanyOrganizationProduct ( ProductId )        
	SELECT         
	DISTINCT OP.ProductId         
	FROM         
	Person.Persona P        
	INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId        
	INNER JOIN Enterprise.OrganizationProduct OP ON ULP.OrganizationPartyId = OP.PartyId        
	WHERE         
	OP.PartyId IN( SELECT OrgPartyId FROM #OrganizationPartyIds  )     
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
 END         
    
 -- User should subscribe to AD Group and access to employee company users only.    
 IF ((SELECT COUNT(1) FROM [security].ADGroupUser agu INNER JOIN  
[security].ADGroupProduct agp ON agp.ADGroupId = agu.ADGroupId WHERE  agu.PersonaId IN( SELECT PersonaId FROM @PersonaIds  )  ) > 0)  
  
 BEGIN  
  INSERT INTO @UserProducts ( PersonaId,ProductId, isFavorite, StatusTypeId )    
   SELECT adgu.PersonaId,ps.productid, 0, 8 from enterprise.GlobalProductConfiguration GPC             
   INNER JOIN Enterprise.ProductConfiguration PC on GPC.ConfigurationId = PC.ConfigurationId            
   INNER JOIN Enterprise.ProductSetting ps ON PC.ProductSettingId = PS.ProductSettingId            
   INNER JOIN Enterprise.ProductSettingType pst on ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.[name] =  'ProductAssignedViaADGroupWithoutUserCreation'     
   INNER JOIN [Security].[ADGroupProduct] adgp on  adgp.ProductId = ps.ProductId    
   INNER JOIN [Security].[ADGroup] adg on adg.ADGroupId = adgp.ADGroupId    
   INNER JOIN [Security].[ADGroupUser] adgu on adg.ADGroupId = adgu.ADGroupId      
   WHERE  adgu.PersonaId IN( SELECT PersonaId FROM @PersonaIds  ) and ps.[Value] = '1'      
   AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL AND ps.ThruDate IS NULL      
   END  

 -- ADD EASYLMS OR FIX ITS STATUS        
 IF EXISTS (SELECT TOP 1 1 FROM @UserProducts WHERE ProductId = @LearningProductID)        
 BEGIN        
  UPDATE @UserProducts SET StatusTypeId = 8 WHERE ProductId = @LearningProductID        
 END        
 ELSE        
 BEGIN        
  INSERT INTO @UserProducts ( PersonaId,ProductId, isFavorite, StatusTypeId )        
  SELECT        
    PersonaId,@LearningProductID, 0, 8   from @PersonaIds      
 END        
  
-- ADD SimonHelpCenter OR FIX ITS STATUS          
 INSERT INTO @UserProducts ( PersonaId,ProductId, isFavorite, StatusTypeId )          
  SELECT        
    PersonaId,@SimonHelpProductID, 0, 8   from @PersonaIds        
  
 IF EXISTS(SELECT TOP 1 1 FROM Ident.UserLoginPersona ULP   
    INNER JOIN Person.Persona P on ULP.UserLoginPersonaID = P.UserLoginPersonaId  
    INNER JOIN Enterprise.OrganizationProduct Org on Org.PartyId = ULP.OrganizationPartyId  
    WHERE P.PersonaId IN( SELECT PersonaId FROM @PersonaIds  )  
    and ProductId = @AdminPortalProductID   
    and Org.Thrudate is NULL)  
 BEGIN  
    IF NOT EXISTS( SELECT TOP 1 1 FROM Ident.SamlUserAttribute where PersonaId IN( SELECT PersonaId FROM @PersonaIds  ) and ProductId = @AdminPortalProductID)       
    BEGIN      
      INSERT INTO @UserProducts ( PersonaId,ProductId, isFavorite, StatusTypeId )                  
       SELECT        
    PersonaId,@AdminPortalProductID, 0, 8   from @PersonaIds                
    END  
END  
  
         
  INSERT INTO @UserProducts ( PersonaId,ProductId , isFavorite, StatusTypeId)       
  SELECT DISTINCT pc.PersonaId,p.ProductId, 0, 8           
  FROM Enterprise.PersonaProductCenter ppc         
  inner join Enterprise.ProductProductCenter p on ppc.ProductCenterId = p.ProductCenterId        
  inner join Enterprise.GlobalProductConfiguration gpc on gpc.ProductId = p.productId        
  inner join Enterprise.ProductConfiguration config on config.ConfigurationID = gpc.ConfigurationID       
  inner join Enterprise.ProductSetting ps on ps.ProductSettingId = config.ProductSettingId        
  inner join Enterprise.ProductSettingType pst on (ps.ProductSettingTypeId = pst.ProductSettingTypeId and pst.Name ='GetUserProductCenterEnabled')       
  inner join Enterprise.ProductUserDependency pud on p.ProductId = pud.ProductId      
  inner join Enterprise.PersonaConfiguration PC on pc.ProductId = pud.DependentProductId AND pc.PersonaId = ppc.PersonaId    
  WHERE ps.Value = '1'        
  and ppc.PersonaId IN( SELECT PersonaId FROM @PersonaIds  )       
  and gpc.ThruDate is null        
  and config.ThruDate is null        
  and ps.ThruDate is null      
     AND PC.ThruDate IS NULL       
  AND (PC.StatusTypeId = 8)      
      
 INSERT INTO @UserProducts ( PersonaId,ProductId, isFavorite, StatusTypeId )        
  SELECT     
  pc.PersonaId,   
   ProductId        
   ,IsFavorite        
   ,StatusTypeId        
  FROM Enterprise.PersonaConfiguration PC        
  WHERE        
   PC.PersonaId IN( SELECT PersonaId FROM @PersonaIds  )        
   AND PC.ThruDate IS NULL    
   
       DECLARE @UsersProductsList TABLE       
    (      
	PersonaId INT,
        ProductId INT ,
		Name NVARCHAR(200) ,
		BooksProductCode NVARCHAR(200)     
    );   


	INSERT INTO @UsersProductsList (PersonaId,ProductId,Name,BooksProductCode) SELECT      
 pc.PersonaId,   
  PC.ProductId        
  ,P.Name        
  ,P.BooksProductCode              
 FROM        
  @UserProducts PC        
  INNER JOIN Enterprise.Product P  ON PC.ProductId = P.ProductId        
  LEFT OUTER JOIN enterprise.producttype pt on p.ProductTypeId = pt.ProductTypeId        
  LEFT OUTER JOIN Enterprise.ProductType PT2 on PT.ParentProductTypeId = PT2.ProductTypeId        
  INNER JOIN @CompanyOrganizationProduct OP on P.ProductId = OP.ProductId             
 WHERE        
  PC.StatusTypeId = 8   and
  pc.PersonaId IN( SELECT PersonaId FROM @PersonaIds  )  AND p.ProductId IN( SELECT ProductId FROM @ProductIdList  )     
 UNION        
 SELECT       
 ppv.PersonaId, 
  pr.ProductId        
  ,P.Name        
  ,P.BooksProductCode                
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
 WHERE         
  ppv.PersonaId IN( SELECT PersonaId FROM @PersonaIds  )  AND pr.ProductId IN( SELECT ProductId FROM @ProductIdList  )
        
 ORDER BY  P.Name  
    
      
    --Preferred mobile number logic              
    INSERT INTO @ContactPreference      
    (      
        PartyId,      
        PreferredPhoneNumber      
    )      
    SELECT pcm.PartyId AS PartyId,      
           ISNULL(tm.CountryCode, '') + tm.AreaCode + tm.PhoneNumber      
    FROM Enterprise.TelecommunicationsNumber tm      
        INNER JOIN Enterprise.PartyContactMechanism pcm      
            ON tm.ContactMechanismID = pcm.ContactMechanismId      
        INNER JOIN Enterprise.[ContactMechanismPreference] CMP      
            ON CMP.ContactMechanismID = pcm.ContactMechanismId      
               AND      
               (      
                   pcm.ThruDate IS NULL      
                   OR pcm.ThruDate > GETUTCDATE()      
               )      
        JOIN Ident.UserLogin ul      
            ON ul.PersonPartyId = pcm.PartyId      
        JOIN Ident.UserLoginPersona ulp      
            ON ulp.UserLoginId = ul.UserId      
    WHERE ulp.OrganizationPartyId in (select OrgPartyId from #OrganizationPartyIds ); /*OPTION (RECOMPILE)*/      
      
    --Notification Email             
    INSERT INTO @NotificationEmail      
    (      
        PartyId,      
        Email      
    )      
    SELECT p.PartyId,      
           ea.ElectronicAddressString AS NotificationEmail      
    FROM Enterprise.ContactMechanismUsage cmu      
        INNER JOIN Enterprise.PartyContactMechanism pcm      
            ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID      
 INNER JOIN Enterprise.ContactMechanism cm      
            ON cm.ContactMechanismID = pcm.ContactMechanismId      
        INNER JOIN Enterprise.ElectronicAddress ea      
            ON ea.ContactMechanismID = cm.ContactMechanismID      
        INNER JOIN Enterprise.Party p      
            ON p.PartyId = pcm.PartyId      
        INNER JOIN Ident.UserLogin ul      
            ON ul.PersonPartyId = p.PartyId      
        INNER JOIN Ident.UserLoginPersona ulp      
            ON ulp.UserLoginId = ul.UserId      
    WHERE (      
              pcm.ThruDate IS NULL      
              OR pcm.ThruDate > GETUTCDATE()      
          )      
          AND cmu.ContactMechanismUsageTypeID = 301      
          AND ulp.OrganizationPartyId in (select OrgPartyId from #OrganizationPartyIds );      
   
      INSERT INTO #UserList      
    (      
        UserId,      
        LoginName,      
        FirstName,      
        LastName,      
        PersonaId,      
        PreferredPhoneNumber,      
        Email      
    )   SELECT ul.UserId,      
               ul.LoginName,      
               p.FirstName,      
               p.LastName,      
               p2.PersonaId,      
               CTPREF.PreferredPhoneNumber,      
               ne.Email      
        FROM Ident.UserLogin AS ul      
            INNER JOIN Ident.UserLoginPersona AS ulp      
                ON ul.UserId = ulp.UserLoginId      
            INNER JOIN Person.Persona AS p2      
                ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId      
            INNER JOIN Person.Person AS p      
                ON ul.PersonPartyId = p.PartyId      
            INNER JOIN Enterprise.Party AS pa      
                ON pa.PartyId = p.PartyId  
			INNER JOIN @UsersProductsList AS upl
			ON upl.PersonaId=p2.PersonaId    
            LEFT OUTER JOIN @ContactPreference AS CTPREF      
                ON CTPREF.PartyId = pa.PartyId      
            LEFT OUTER JOIN @NotificationEmail ne      
                ON ne.PartyId = p.PartyId      
        WHERE ulp.StatusTypeId = 1      
              AND ulp.OrganizationPartyId in (select OrgPartyId from #OrganizationPartyIds )    
     AND p2.personaId in (select personaId from @PersonaIds)

    ;WITH totalusers (UserId, LoginName, FirstName, LastName, PersonaId, PreferredPhoneNumber, Email)      
    AS (SELECT DISTINCT      
               UserId,      
               LoginName,      
               FirstName,      
               LastName,      
               PersonaId,      
               PreferredPhoneNumber,      
               Email      
        FROM #UserList ul)      
    SELECT UserId,      
           LoginName,      
           FirstName,      
           LastName,      
           PersonaId,      
           PreferredPhoneNumber,          
           Email      
    FROM totalusers      
    ORDER BY UserId OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT (@RowsPerPage) ROWS ONLY      
    OPTION (RECOMPILE);    
   
 drop table if exists #UserList  
 drop table if exists #OrganizationPartyIds 
  
END;