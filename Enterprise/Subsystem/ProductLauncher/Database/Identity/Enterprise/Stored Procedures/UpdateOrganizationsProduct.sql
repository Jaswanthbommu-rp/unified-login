CREATE PROCEDURE [Enterprise].[UpdateOrganizationsProduct] (  
  @Value NVARCHAR(150)
 ,@ProductId INT )  
AS  
BEGIN  
   
  CREATE TABLE #OrganizationsType(            
  OrganizationsTypeIds NVARCHAR(150)
 )  
 INSERT INTO #OrganizationsType(OrganizationsTypeIds) SELECT OrganizationTypeId FROM Enterprise.OrganizationType WHERE Name IN (SELECT value FROM string_split(@Value,','))

 IF EXISTS(SELECT 1 FROM #OrganizationsType)  
  UPDATE Enterprise.OrganizationProduct SET ThruDate = GETUTCDATE() WHERE ProductId = @ProductId and partyid NOT IN (SELECT PartyId FROM Enterprise.Organization WHERE OrganizationTypeId IN (SELECT * FROM #OrganizationsType));  
    
  DROP TABLE if exists #OrganizationsType

END;