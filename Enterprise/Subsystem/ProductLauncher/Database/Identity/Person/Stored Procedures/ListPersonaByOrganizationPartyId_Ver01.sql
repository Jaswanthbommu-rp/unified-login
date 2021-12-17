CREATE PROCEDURE [Person].[ListPersonaByOrganizationPartyId_Ver01] (  
 @OrganizationPartyId bigint,  
 @IsDefault bit = NULL,  
 @UserRoleType int = NULL  
)  
AS  
BEGIN  
 DECLARE @NOW datetime = GETUTCDATE()  
    
 SELECT pe.PersonaId,  
    UL.PersonPartyId,  
    p.RealPageId,  
    ULP.OrganizationPartyId,  
    pe.PersonaTypeId,  
    pe.PersonaEnvironmentTypeId,  
    pe.PersonaName AS 'Name',
    pe.FromDate,  
    pe.ThruDate,  
    pe.IsDefault,  
    ULP.UserLoginId AS UserId  
 FROM Person.Persona pe
		INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
		INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
    INNER JOIN Enterprise.Party p ON UL.PersonPartyId = p.PartyId    
    INNER JOIN Enterprise.PartyRelationship PR ON PR.PartyIdFrom = UL.PersonPartyId
 WHERE ULP.OrganizationPartyId = @OrganizationPartyId  
 AND  ((@IsDefault IS NULL) OR (pe.IsDefault = @IsDefault))  
 AND  ((@NOW BETWEEN pe.FromDate AND pe.ThruDate) OR (@NOW >= pe.FromDate AND pe.ThruDate IS NULL))  
 AND  ((@UserRoleType IS NULL) OR ((pr.RoleTypeIdFrom = @UserRoleType) AND (PR.PartyIdTo = @OrganizationPartyId) AND (((@NOW BETWEEN pr.FromDate AND pr.ThruDate) OR (@NOW >= pr.FromDate AND pr.ThruDate IS NULL)))))  
END