  
  
CREATE PROCEDURE [Security].[InsertUpdateRoleTemplateAndInsertEnterpriseRoleBatch] (      
 @RoleTemplateId BIGINT,      
 @EditorPersonaId BIGINT,   
 @OrganizationPartyId BIGINT,   
 @userGuids [dbo].[PartyGUID] READONLY,
 @UseAPIV2 BIT = 0
)        
AS        
BEGIN          
 BEGIN TRY      
  
  
  
  declare @PersonaIdList as table(ID bigint)  

 insert into @PersonaIdList  
 select  distinct PEA.PersonaId  from Enterprise.Party P    
 INNER JOIN @userGuids UG ON UG.RealPageID = P.RealPageId  
 INNER JOIN Person.Person PER ON PER.PartyId = P.PartyId    
 INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId    
 INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId    
 INNER JOIN Person.Persona PEA ON PEA.UserLoginPersonaId = ULP.UserLoginPersonaId    
 INNER JOIN Enterprise.PartyRelationShip PR ON (PR.PartyIdFrom = UL.PersonPartyId AND pr.PartyIdTo = ULP.OrganizationPartyId)    
 INNER JOIN Enterprise.RoleType RT ON (RT.PartyROleTypeId = PR.RoleTypeIdFrom AND RT.ParentPartyRoleTypeId = 400)    
 INNER JOIN Enterprise.Organization O ON ULP.OrganizationPartyId = O.PartyId    
 WHERE PR.ThruDate IS NULL AND O.PartyId = @OrganizationPartyId AND ulp.StatusTypeId NOT IN (19,24) 
  
  
  
  
 MERGE [Security].RoleTemplateUserMapping AS TARGET    
USING @PersonaIdList AS SOURCE ON (TARGET.PersonaId = SOURCE.ID)     
    
WHEN MATCHED     
THEN UPDATE SET TARGET.RoleTemplateId = @RoleTemplateId    
    
WHEN NOT MATCHED BY TARGET     
THEN INSERT (RoleTemplateId,PersonaId) VALUES (@RoleTemplateId, SOURCE.ID);    
    
    
 INSERT INTO Batch.[EnterpriseRoleBatchProcess] (EditorUserPersonaId,SubjectUserPersonaId,EnterpriseRoleTemplateId,StatusTypeId,CreatedDateTime, BatchProcessTypeId, UseAPIV2)        
 SELECT @EditorPersonaId, RTUM.PersonaId, RTUM.RoleTemplateId, 5, GETUTCDATE(), 15, @UseAPIV2 FROM [Security].RoleTemplateUserMapping RTUM    
 INNER JOIN @PersonaIdList PIL ON PIL.ID = RTUM.PersonaId       
    
  SELECT @EditorPersonaId AS Id, '' AS ErrorMessage      
    
 END TRY          
 BEGIN CATCH         
        DECLARE @ErrorLogID INT;        
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;        
        
        SELECT  0 AS Id, ErrorMessage FROM    dbo.ErrorLog        
        WHERE   ErrorLogID = @ErrorLogID;        
 END CATCH        
END;  
  
  