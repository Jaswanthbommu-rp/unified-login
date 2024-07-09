CREATE PROCEDURE [Enterprise].[ListUserRelationshipTypes]  
  @partyId bigint
AS  
BEGIN  
select    Id
         ,UserRelationshipName  
         ,Description
         ,PartyRoleTypeId
          ,PartyId
          ,SortIndex
    from  [Enterprise].[UserRelationShip] ur
                     Inner Join  enterprise.partyrole  pr
					 On ur.PartyRoleTypeId = pr.RoleTypeId
          WHERE    (@partyId IS NULL  OR pr.PartyId = @partyId)
          Order By SortIndex;
                        
END