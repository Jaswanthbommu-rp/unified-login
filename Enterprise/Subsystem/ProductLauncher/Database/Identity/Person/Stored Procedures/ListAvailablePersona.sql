CREATE PROCEDURE [Person].[ListAvailablePersona] (  
 @RealPageId uniqueidentifier,  
 @IsDefault bit = NULL  
)  
AS  
BEGIN  
 DECLARE @NOW datetime = GETUTCDATE()  
  
 SELECT pe.PersonaId,  
     UL.PersonPartyId,  
     p.RealPageId,  
     ULP.UserLoginPersonaId,  
     ULP.OrganizationPartyId,  
     ULP.StatusTypeId,  
     pe.PersonaTypeId,  
     pe.PersonaEnvironmentTypeId,  
     pe.PersonaName AS 'Name', 
     pe.FromDate,  
     pe.ThruDate,  
     pe.IsDefault ,  
     ULP.UserLoginId AS UserId,
     ULP.IsRPEmployee,
     ULP.IsRealPartner
 FROM  Person.Persona PE  
     INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId  
     INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId  
     INNER JOIN Enterprise.Party p ON UL.PersonPartyId = P.PartyId
 WHERE p.RealPageId = @RealPageId  
 AND   ((@IsDefault IS NULL) OR (pe.IsDefault = @IsDefault))  
 AND   ((@NOW BETWEEN pe.FromDate AND pe.ThruDate) OR (@NOW >= pe.FromDate AND pe.ThruDate IS NULL))  
 AND   ((@NOW BETWEEN ulp.FromDate AND ulp.ThruDate) OR (@NOW >= ulp.FromDate AND ulp.ThruDate IS NULL))  
END