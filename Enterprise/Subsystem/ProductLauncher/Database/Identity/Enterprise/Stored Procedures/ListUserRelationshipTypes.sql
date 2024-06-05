CREATE PROCEDURE [Enterprise].[ListUserRelationshipTypes]  
  @partyId bigint
AS  
BEGIN  
	SELECT   ur.Id
			,ur.UserRelationshipName  
			,ur.Description  
			,ur.PartyRoleTypeId  
			,pr.PartyId  
			,ur.SortIndex
			,ur.ThirdPartyRelationshipId
    FROM  [Enterprise].[UserRelationShip] ur  
		INNER Join  Enterprise.PartyRole  pr  On ur.PartyRoleTypeId = pr.RoleTypeId  
	WHERE (@partyId IS NULL OR pr.PartyId = @partyId)  
    ORDER BY ur.SortIndex;
END  