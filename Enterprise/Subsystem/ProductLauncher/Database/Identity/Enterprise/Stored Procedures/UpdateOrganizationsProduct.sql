CREATE PROCEDURE [Enterprise].[UpdateOrganizationsProduct] (  
  @Value NVARCHAR(150)
 ,@ProductId INT )  
AS  
BEGIN  
   
  CREATE TABLE #OrganizationsType(            
  OrganizationsTypeIds NVARCHAR(150)
 )  

 CREATE TABLE #OrgPartyIds(            
  PartyIds NVARCHAR(150)
 )  

 INSERT INTO #OrganizationsType(OrganizationsTypeIds) SELECT OrganizationTypeId FROM Enterprise.OrganizationType WHERE Name IN (SELECT value FROM string_split(@Value,','))
 
 INSERT INTO #OrgPartyIds(PartyIds) SELECT PartyId FROM Enterprise.Organization WHERE OrganizationTypeId IN (SELECT OrganizationsTypeIds FROM #OrganizationsType)

 IF EXISTS(SELECT 1 FROM #OrganizationsType) 
 BEGIN
  UPDATE Enterprise.OrganizationProduct SET ThruDate = GETUTCDATE() WHERE ProductId = @ProductId and partyid NOT IN (SELECT PartyIds FROM #OrgPartyIds)
 END
    
  DROP TABLE if exists #OrganizationsType

  DROP TABLE if exists #OrgPartyIds

END