CREATE PROCEDURE [Enterprise].[ListOrganizationByLoginName](  
 @LoginName varchar(255)  ,
 @OrganizationRealPageId uniqueidentifier = null
)  
AS  
BEGIN  
 DECLARE @NOW  DATETIME = GETUTCDATE();  
  
 SELECT  
  ep.PartyId AS 'OrganizationPartyId',  
  ep.RealPageId AS 'OrganizationRealPageId',  
  ert.PartyRoleTypeId,  
  ert.Name,  
  ULP.PrimaryOrganization,  
  edim.MasterId AS 'BooksMasterId',  
  Edim.CompanyMasterId AS 'BooksCustomerMasterId',  
  P.PersonaId,    
  O.[Name] OrganizationName    
 FROM    
  Enterprise.Party ep  
  INNER JOIN Ident.UserLoginPersona ULP ON ULP.OrganizationPartyId = ep.PartyId  
  INNER JOIN Ident.UserLogin iul ON (ULP.UserLoginId = iul.UserId)  
  INNER JOIN Person.Persona P ON P.UserLoginPersonaId = ULP.UserLoginPersonaId    
  INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId   
  INNER JOIN Enterprise.PartyRelationship epr ON (iul.PersonPartyId = epr.PartyIdFrom) and ULP.OrganizationPartyId = epr.PartyIdTo  
  INNER JOIN Enterprise.RoleType ert ON (epr.RoleTypeIdFrom = ert.PartyRoleTypeId)  
  INNER JOIN Enterprise.RoleType eprt ON (ert.ParentPartyRoleTypeId = eprt.PartyRoleTypeId)  
  LEFT OUTER JOIN Enterprise.VW_DataImportMapping edim ON (ep.PartyId = edim.PartyId)  
 WHERE   
  iul.LoginName = @LoginName  AND (ep.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId is null)
  AND eprt.Name = 'User Role'  
  AND (@NOW BETWEEN epr.FromDate AND epr.ThruDate OR @NOW >= epr.FromDate AND epr.ThruDate IS NULL)  
  AND ( ULP.PrimaryOrganization = 'true'  
   OR (  
    @NOW BETWEEN ulp.FromDate AND ulp.ThruDate) OR (@NOW >= ulp.FromDate AND ulp.ThruDate IS NULL  
   ))  
END;
