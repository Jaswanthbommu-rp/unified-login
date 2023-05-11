--exec [Enterprise].[ListULMappingPersonaIdForProductUserId] 1308,1,'1192422|jreames1,1192422|mmacatee,1192422|FLastName,1192422'  
CREATE PROCEDURE [Enterprise].[ListULMappingPersonaIdForProductUserId]  
( 	
    @CompanyId INT = 0,  
    @UPFMId UNIQUEIDENTIFIER = NULL, 
	@ProductId INT,  
    @TargetProductUserIds nvarchar(max)
)  
AS  
BEGIN  
  
 Declare @SamlAttributeId int;  
 DECLARE @ProductUserIdList TABLE(ProductUserId nvarchar(max));  
 DECLARE @TargetProductUserPersonaList TABLE(PersonaId bigint);  
 DECLARE @OrgPartyIdList TABLE(OrgPartyId bigint);
 --Preferred mobile number   
 DECLARE @ContactPreference TABLE( PersonaId INT  
         , PreferredPhoneNumber VARCHAR(30))  

IF(@UPFMId IS NOT NULL)
BEGIN
 insert into @OrgPartyIdList(OrgPartyId)
	select distinct ep.PartyId from enterprise.party ep
	join Enterprise.Organization eo on ep.PartyId = eo.PartyId
	where RealPageId = @UPFMId
END
ELSE
BEGIN
    insert into @OrgPartyIdList(OrgPartyId)
	select distinct m.PartyId  
	FROM Enterprise.VW_DataImportMapping m  
	JOIN Enterprise.Organization org on org.PartyId = m.PartyId  
    JOIN Enterprise.DataImportMapping dim on dim.PartyId = org.PartyId 							 
	Where m.CompanyMasterId = @CompanyId  AND dim.SourceId = @CompanyId AND dim.DataImportApplicationId = 2
END
  
 SELECT @SamlAttributeId = SamlAttributeId FROM Ident.SamlAttribute  
 WHERE Name = 'UserId'  
  
 INSERT INTO @ProductUserIdList(ProductUserId)  
 (  
  SELECT *  
  FROM STRING_SPLIT(@TargetProductUserIds, ',')  
 );   
  


 IF((@UPFMId IS NOT NULL) OR (@CompanyId IS NOT NULL AND @CompanyId > 0))
 BEGIN

  INSERT INTO @ContactPreference(PersonaId,PreferredPhoneNumber)  
  SELECT   
   AP.PersonaId AS PersonaId, ISNULL(TM.CountryCode,'') + TM.AreaCode + TM.PhoneNumber   
  FROM Enterprise.TelecommunicationsNumber tm   
   INNER JOIN Enterprise.PartyContactMechanism pcm ON tm.ContactMechanismID = pcm.ContactMechanismID  
   INNER JOIN Person.ActivePersona AP ON AP.PartyId = PCM.PartyId  
   INNER JOIN Person.Persona p on p.PersonaId = AP.PersonaId  
   INNER JOIN Ident.UserLoginPersona ulp on p.UserLoginPersonaId = ulp.UserLoginPersonaId  
   INNER JOIN @OrgPartyIdList opl on ulp.OrganizationPartyId = opl.OrgPartyId
   INNER JOIN Enterprise.[ContactMechanismPreference] CMP ON CMP.ContactMechanismID = PCM.ContactMechanismId AND (PCM.ThruDate IS NULL OR PCM.ThruDate > GETUTCDATE())  


  SELECT   distinct
   sua.Value as ProductUserId,   
   sua.PersonaId as UnifiedLoginPersonaId ,  
   cp.PreferredPhoneNumber,  
   ne.NotificationEmail AS Email  
  FROM Ident.SamlUserAttribute sua  
   INNER JOIN @ProductUserIdList puid on sua.Value = puid.ProductUserId   
   INNER JOIN Person.Persona p on p.PersonaId = sua.PersonaId  
   INNER JOIN Ident.UserLoginPersona ulp on p.UserLoginPersonaId = ulp.UserLoginPersonaId  
   INNER JOIN Ident.UserLogin ul ON ul.UserId = ulp.UserLoginId  
   INNER JOIN Person.Person pe ON pe.PartyId = ul.PersonPartyId  
   INNER JOIN @OrgPartyIdList opl on ulp.OrganizationPartyId = opl.OrgPartyId
   LEFT OUTER JOIN @ContactPreference CP ON CP.PersonaId = P.PersonaId  
   LEFT OUTER JOIN  
     (  
      SELECT p.PartyId,  
         ea.ElectronicAddressString AS NotificationEmail  
      FROM Enterprise.ContactMechanismUsage cmu  
         INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID  
         INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId  
         INNER JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID  
         INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId  
      WHERE (pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())  
      AND   cmu.ContactMechanismUsageTypeID = 301  
    ) ne ON ne.PartyId = pe.PartyId  
  
  WHERE  
         ProductId = @ProductId  
         AND SamlAttributeId = @SamlAttributeId  

 END
 Else  
 Begin  
  INSERT INTO @TargetProductUserPersonaList (PersonaId )  
  SELECT DISTINCT SA.PersonaId  
  From Ident.SamlUserAttribute SA  
  INNER JOIN Enterprise.PersonaConfiguration PC ON  
   PC.PersonaId = SA.PersonaId  
   And PC.ProductId = SA.ProductId  
   And PC.StatusTypeId = 8  
  Where SamlAttributeId = @SamlAttributeId  
  And SA.ProductId = @ProductId  
  And Value IN (Select ProductUserId From @ProductUserIdList)  
  
  INSERT INTO @ContactPreference(PersonaId,PreferredPhoneNumber)  
  SELECT   
   AP.PersonaId AS PersonaId, ISNULL(TM.CountryCode,'') + TM.AreaCode + TM.PhoneNumber   
  FROM Enterprise.TelecommunicationsNumber tm   
   INNER JOIN Enterprise.PartyContactMechanism pcm ON tm.ContactMechanismID = pcm.ContactMechanismID  
   INNER JOIN Person.ActivePersona AP ON AP.PartyId = PCM.PartyId  
   INNER JOIN Person.Persona p on p.PersonaId = AP.PersonaId  
   INNER JOIN @TargetProductUserPersonaList tpp ON p.PersonaId = tpp.PersonaId  
   INNER JOIN Ident.UserLoginPersona ulp on p.UserLoginPersonaId = ulp.UserLoginPersonaId     
   INNER JOIN Enterprise.[ContactMechanismPreference] CMP   
   ON CMP.ContactMechanismID = PCM.ContactMechanismId AND (PCM.ThruDate IS NULL OR PCM.ThruDate > GETUTCDATE())  
     
   SELECT   
    sua.Value as ProductUserId,   
    sua.PersonaId as UnifiedLoginPersonaId ,  
    cp.PreferredPhoneNumber,  
    ne.NotificationEmail AS Email  
   FROM Ident.SamlUserAttribute sua  
    INNER JOIN @ProductUserIdList puid on sua.Value = puid.ProductUserId   
    INNER JOIN Person.Persona p on p.PersonaId = sua.PersonaId  
    INNER JOIN @TargetProductUserPersonaList tpp ON p.PersonaId = tpp.PersonaId  
    INNER JOIN Ident.UserLoginPersona ulp on p.UserLoginPersonaId = ulp.UserLoginPersonaId  
    INNER JOIN Ident.UserLogin ul ON ul.UserId = ulp.UserLoginId  
    INNER JOIN Person.Person pe ON pe.PartyId = ul.PersonPartyId      
    LEFT OUTER JOIN @ContactPreference CP ON CP.PersonaId = P.PersonaId  
    LEFT OUTER JOIN  
      (  
       SELECT p.PartyId,  
          ea.ElectronicAddressString AS NotificationEmail  
       FROM Enterprise.ContactMechanismUsage cmu  
          INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID  
          INNER JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId  
          INNER JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID  
          INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId  
       WHERE (pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())  
       AND   cmu.ContactMechanismUsageTypeID = 301  
     ) ne ON ne.PartyId = pe.PartyId  
  
   WHERE ProductId = @ProductId  
   AND SamlAttributeId = @SamlAttributeId  
 End   
   
END;