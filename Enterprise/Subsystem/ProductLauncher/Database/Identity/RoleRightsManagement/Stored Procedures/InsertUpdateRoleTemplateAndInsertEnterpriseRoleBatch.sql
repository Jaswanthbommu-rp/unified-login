CREATE PROCEDURE [Security].[InsertUpdateRoleTemplateAndInsertEnterpriseRoleBatch] (  
 @RoleTemplateId BIGINT,  
 @EditorPersonaId BIGINT,     
 @PersonaIdList [Enterprise].[IntListType] READONLY    
)    
AS    
BEGIN      
 BEGIN TRY  
 MERGE [Security].RoleTemplateUserMapping AS TARGET
USING @PersonaIdList AS SOURCE ON (TARGET.PersonaId = SOURCE.ID) 

WHEN MATCHED 
THEN UPDATE SET TARGET.RoleTemplateId = @RoleTemplateId

WHEN NOT MATCHED BY TARGET 
THEN INSERT (RoleTemplateId,PersonaId) VALUES (@RoleTemplateId, SOURCE.ID);


 INSERT INTO Batch.[EnterpriseRoleBatchProcess] (EditorUserPersonaId,SubjectUserPersonaId,EnterpriseRoleTemplateId,StatusTypeId,CreatedDateTime, BatchProcessTypeId)    
 SELECT @EditorPersonaId, RTUM.PersonaId, RTUM.RoleTemplateId, 5, GETUTCDATE(), 15 FROM [Security].RoleTemplateUserMapping RTUM
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


