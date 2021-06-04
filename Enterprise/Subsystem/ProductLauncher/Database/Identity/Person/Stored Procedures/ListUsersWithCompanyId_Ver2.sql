--EXEC [Person].[ListUsersWithCompanyId_VER2]
CREATE PROCEDURE [Person].[ListUsersWithCompanyId_VER2]     
(@CompanyId   NVARCHAR(100) = NULL,     
 @Source      NVARCHAR(50)  = 'BlueBook',     
 @ProductId   NVARCHAR(200) = NULL,     
 @RowsPerPage INT           = 0,     
 @PageNumber  INT           = 1,  
 @companyDomain nvarchar(20) = 'primary'
)    
AS    
    BEGIN  
	    DECLARE @domainId INT = 0
        DECLARE @Now DATETIME= GETUTCDATE();    
        DECLARE @ProductIdList TABLE(ProductId INT);    
  DECLARE @ProductIdRightList TABLE(ProductId INT);    
    
        DECLARE @CompanyIdList TABLE(CompanyId INT);  
		DECLARE @PartyIdList TABLE(PartyId INT);
  
        DECLARE @ProductCount INT= 1;    
        DECLARE @CompanyIdCount INT= 1;    
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
		  
        INSERT INTO @ProductIdList(ProductId)    
               SELECT *    
               FROM STRING_SPLIT(@ProductId, ',');    

        INSERT INTO @CompanyIdList(CompanyId)    
            SELECT *    
            FROM STRING_SPLIT(@companyid, ',')    

        IF (SELECT COUNT(*) FROM @ProductIdList) = 0    
            BEGIN    
                SET @ProductCount = NULL;    
			END;    
        IF (SELECT COUNT(*) FROM @CompanyIdList) = 0    
            BEGIN    
                SET @CompanyIdCount = NULL;    
			END;    
    
	SELECT @domainId = OrganizationDomainId
	FROM Enterprise.OrganizationDomain
	WHERE NAME = @CompanyDomain

		INSERT INTO @PartyIdList(PartyId)
		SELECT m.PartyId
		FROM @CompanyIdList c
		JOIN Enterprise.VW_DataImportMapping m on m.CompanyMasterID = c.CompanyId
		JOIN Enterprise.Organization org on org.PartyId = m.PartyId
		Where org.OrganizationDomainId = @domainId


        SELECT @RowsPerPage = CASE    
                                  WHEN @RowsPerPage <= 0    
                                  THEN 2147483647    
                                  ELSE @RowsPerPage    
	                          END;   
    
 
  --Preferred mobile number logic    
  DECLARE @ContactPreference TABLE(PartyId INT, PreferredPhoneNumber VARCHAR(30))    
  INSERT INTO @ContactPreference(PartyId,PreferredPhoneNumber)    
  SELECT PCM.PartyId AS PartyId, ISNULL(TM.CountryCode,'') + TM.AreaCode + TM.PhoneNumber FROM     
         Enterprise.TelecommunicationsNumber tm     
         INNER JOIN Enterprise.PartyContactMechanism pcm ON tm.ContactMechanismID = pcm.ContactMechanismID    
         INNER JOIN Enterprise.[ContactMechanismPreference] CMP
		 ON CMP.ContactMechanismID = PCM.ContactMechanismId AND (PCM.ThruDate IS NULL OR PCM.ThruDate > GETUTCDATE())
		 JOIN Ident.UserLogin ul on ul.PersonPartyId = pcm.PartyId
		 JOIN Ident.UserLoginPersona ulp on ulp.UserLoginId = ul.UserId
		 Where ((@CompanyIdCount IS NULL) OR ulp.OrganizationPartyId in  
                 (    
                     SELECT PartyId   
                     FROM @PartyIdList AS cil 
                 ))

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
   AND ((@CompanyIdCount IS NULL) OR ulp.OrganizationPartyId in  
                 (    
                     SELECT PartyId   
                     FROM @PartyIdList AS cil 
                 ));    
    
	select *
	from @NotificationEmail

        ;WITH Products    
             AS (SELECT     
                        p.PersonaID,     
                        pp.ProductId    
                 FROM Person.Persona AS p    
                      INNER JOIN Ident.UserLoginPersona AS ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId     
                      INNER JOIN Enterprise.PersonaConfiguration AS pec ON p.PersonaId = pec.PersonaId     
                      INNER JOIN Enterprise.ProductConfiguration AS prc ON pec.ConfigurationId = prc.ConfigurationId    
					  INNER JOIN Enterprise.Product AS pp ON pp.ProductId = pec.ProductId    
                      INNER JOIN Enterprise.ProductSetting AS ps ON prc.ProductSettingId = ps.ProductSettingId    
                                                                    AND ps.Value = '8'    
                      INNER JOIN Enterprise.ProductSettingType AS pst ON ps.ProductSettingTypeId = pst.ProductSettingTypeId    
                                                                         AND pst.Name = 'ProductStatus'    
                 WHERE((@NOW BETWEEN pec.FromDate AND pec.ThruDate)    
                       OR (@NOW >= pec.FromDate    
                           AND pec.ThruDate IS NULL))    
                      AND ((@NOW BETWEEN prc.FromDate AND prc.ThruDate)    
                           OR (@NOW >= prc.FromDate    
                               AND prc.ThruDate IS NULL))    
                      AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate)    
                           OR (@NOW >= ps.FromDate    
                               AND ps.ThruDate IS NULL))    
                      AND pec.ProductId NOT IN(14, 19, 24, 25, 34, 39) --Client Portal, Product Learning Portal, Black Book, Self-provisioning portal, Benchmarking, Integration Marketplace    
       AND ((@ProductCount IS NULL)    
                           OR pp.ProductId IN    
                 (    
                     SELECT *    
                     FROM @ProductIdList    
                 ))),    
                
             Users    
             AS (SELECT ul.UserId,     
                        ul.LoginName,     
                        p.FirstName,     
                        p.LastName,     
      p2.PersonaId,     
                        CTPREF.PreferredPhoneNumber,    
      ne.Email    
                 FROM ident.UserLogin AS ul    
                      INNER JOIN ident.UserLoginPersona AS ulp ON ul.UserId = ulp.UserLoginId    
                      INNER JOIN person.Persona AS p2 ON ulp.UserLoginPersonaId = p2.UserLoginPersonaId    
                      INNER JOIN Person.Person AS p ON ul.PersonPartyId = p.partyid    
                      INNER JOIN Enterprise.Party AS pa ON pa.partyid = p.PartyId    
                      INNER JOIN Products AS cp ON cp.PersonaId = p2.PersonaId    
                      LEFT JOIN Ident.SamlUserAttribute AS sua ON sua.PersonaId = p2.PersonaId    
                                                                  AND sua.ProductId = cp.ProductId    
                                                                  AND sua.SamlAttributeId = 1    
                      LEFT OUTER JOIN  @ContactPreference AS CTPREF ON CTPREF.PartyId = PA.PartyId      
					  LEFT OUTER JOIN @NotificationEmail ne ON ne.PartyId = p.PartyId    
                          
                 WHERE ulp.StatusTypeId = 1    
                       AND ((@CompanyIdCount IS NULL)    
                       OR ulp.OrganizationPartyId in  
                 (    
                     SELECT PartyId   
                     FROM @PartyIdList AS cil 
                 )))    
    
   --- Add the users that UL is not thier user management     
   INSERT INTO #UserList    
   (UserId,     
   LoginName,     
   FirstName,     
   LastName,    
   PersonaId,    
   PreferredPhoneNumber,    
   Email    
   )    
   SELECT UserId,     
     LoginName,     
     FirstName,     
     LastName,    
     PersonaId,    
     PreferredPhoneNumber,    
     Email    
   FROM Users AS u;    
    
   --- Add the users that UL is the users management    
  INSERT INTO @ProductIdRightList(ProductId)    
  SELECT ProductId    
  FROM @ProductIdList    
            WHERE [@ProductIdList].ProductId IN(SELECT ProductId FROM Enterprise.ProductRight)    
    
        IF EXISTS (SELECT TOP 1 1 FROM @ProductIdRightList)    
  BEGIN    
   INSERT INTO #UserList    
   (         UserId,     
    LoginName,     
    FirstName,     
    LastName,    
    PersonaId,    
    PreferredPhoneNumber,    
    Email    
   )    
     SELECT ul.UserId,     
       ul.LoginName,     
       pp.FirstName,     
       pp.LastName,    
       p.PersonaId,    
       CTPREF.PreferredPhoneNumber,    
       ne.Email    
     FROM Ident.UserLogin ul    
      INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId    
      INNER JOIN Person.Persona p ON ulp.UserLoginPersonaId = p.UserLoginPersonaId    
      INNER JOIN Person.Person AS pp ON ul.PersonPartyId = pp.partyid    
      INNER JOIN Enterprise.Party pa ON pa.partyid = pp.PartyId    
    
      INNER JOIN [Security].[PersonaRole] AS pr ON p.PersonaId = pr.PersonaId        
      INNER JOIN [Security].[Role] AS r ON pr.RoleId = r.RoleId        
      INNER JOIN [Security].[RoleRight] AS rr ON r.RoleId = rr.RoleId        
      INNER JOIN [Security].[Right] AS r2 ON rr.RightId = r2.RightId    
      INNER JOIN Enterprise.ProductRight AS prt ON r2.TargetProductId = prt.ProductId    
      LEFT OUTER JOIN  @ContactPreference AS CTPREF ON CTPREF.PartyId = pa.PartyId    
      LEFT OUTER JOIN @NotificationEmail ne ON ne.PartyId = pp.PartyId    
          
     WHERE     
      ulp.StatusTypeId = 1    
      AND prt.ProductId IN (SELECT ProductId FROM @ProductIdRightList)    
           
       AND P.PersonaId NOT IN    
     (    
      SELECT pe.PersonaId    
      FROM Enterprise.MasterConfigurationType mct    
       INNER JOIN Enterprise.MasterSettingType MST ON mst.MasterConfigurationTypeId = mct.MasterCOnfigurationTypeId    
       INNER JOIN Enterprise.MasterSetting ms ON ms.MasterSettingTypeId = mst.MasterSettingTYpeId    
       INNER JOIN Enterprise.Party p ON CONVERT(NVARCHAR(40), p.RealPageId) = ms.Value    
       INNER JOIN ident.UserLogin ul ON UL.PersonPartyId = p.PartyId    
       INNER JOIN Ident.UserLoginPersona ulp ON ul.UserId = ulp.UserLoginId    
       INNER JOIN Person.Persona pe ON pe.UserLoginPersonaId = ulp.UserLoginPersonaId    
      WHERE mct.Name = 'Organization'    
        AND mst.Name = 'RealPageEmployeeAccessID'    
     )  AND ((@CompanyIdCount IS NULL)    
      OR ulp.OrganizationPartyId in
      (    
       SELECT PartyId    
       FROM @PartyIdList AS cil    
      ));    
        END;    
    
  ;with totalusers (UserId,     
               LoginName,     
               FirstName,     
               LastName,    
      PersonaId,    
      PreferredPhoneNumber,    
      Email    
      )  as (    
        SELECT distinct         
               UserId,     
               LoginName,     
               FirstName,     
               LastName,    
      PersonaId,    
      PreferredPhoneNumber,    
      Email    
      FROM #UserList ul    
   )    
  select UserId,     
               LoginName,     
               FirstName,     
               LastName,    
      PersonaId,    
      PreferredPhoneNumber,    
      COUNT(1) OVER() AS TotalRecords,    
      Email    
      FROM totalusers         
            
        ORDER BY UserId    
        OFFSET((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT(@RowsPerPage) ROWS ONLY;    
    
    END;