CREATE PROCEDURE [Person].[GetSuperUsersCountByOrganization](
	@OrganizationPartyId bigint    
)      
AS        
BEGIN      
 SELECT     
count(PersonaId)  
     
FROM Ident.UserLogin ul  
INNER JOIN Ident.UserLoginPersona ulp   
    ON ul.UserId = ulp.UserLoginId AND ulp.ThruDate IS NULL  
INNER JOIN Person.Persona p   
    ON ulp.UserLoginPersonaId = p.UserLoginPersonaId  
INNER JOIN Enterprise.PartyRelationship pr   
    ON pr.PartyIdFrom = ul.PersonPartyId   
    AND pr.PartyIdTo = ulp.OrganizationPartyId   
    AND pr.ThruDate IS NULL  
INNER JOIN Enterprise.RoleType rt   
    ON rt.PartyRoleTypeId = pr.RoleTypeIdFrom   
    AND pr.RoleTypeIdTo = 205  
INNER JOIN Enterprise.Organization org   
    ON org.PartyId = ulp.OrganizationPartyId  
WHERE ulp.OrganizationPartyId = @OrganizationPartyId  
  AND pr.RoleTypeIdFrom = 402  
  AND NOT EXISTS (  
      SELECT 1  
      FROM Enterprise.OrganizationAdminUser oau  
      WHERE oau.UserLoginPersonaId = ulp.UserLoginPersonaId  
  )    
     
   AND   
   (   (ulp.statustypeid = 12 AND ulp.StatusThruDate > getutcdate() AND ulp.StatusThruDate is not null )   
    or (ulp.statustypeid = 2 AND ulp.StatusThruDate >= getutcdate() AND ulp.StatusThruDate is not null )  
 or (statustypeid = 1 AND ulp.FromDate <= getutcdate() AND (ulp.StatusThruDate is null OR ulp.StatusThruDate >= getutcdate()))  
   )  
    
END