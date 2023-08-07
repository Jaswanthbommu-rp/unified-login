CREATE  PROCEDURE [Security].[UpdateDeleteDelegateAdminRoleTemplate]            
(            
 @UserLoginPersonaId BIGINT,            
 @RoleTemplateId BigInt,          
 @IsDeleted BIT = 0           
)          
AS            
BEGIN            
  Declare  @DelegateRoleID INT , @ErrorLogID INT;   
    
       IF @IsDeleted =1         
    
     IF EXISTS( SELECT 1  FROM [Security].[DelegatedAdminRoleTemplate] WHERE UserLoginPersonaId = @UserLoginPersonaId  AND RoleTemplateId = @RoleTemplateId )     
       BEGIN    
           BEGIN TRY          
                 DELETE FROM [Security].[DelegatedAdminRoleTemplate]   WHERE UserLoginPersonaId = @UserLoginPersonaId  AND RoleTemplateId = @RoleTemplateId       
                 SELECT @DelegateRoleID = SCOPE_IDENTITY();        
                 SELECT @DelegateRoleID AS Id, '' AS ErrorMessage      
           END TRY        
            BEGIN CATCH                 
              EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;        
              SELECT 0 AS Id, ErrorMessage        
              FROM dbo.ErrorLog        
              WHERE ErrorLogID = @ErrorLogID;        
            END CATCH;        
        END  
        
        IF @IsDeleted =0        
              
      IF NOT EXISTS  ( SELECT 1 FROM [Security].[DelegatedAdminRoleTemplate] WHERE UserLoginPersonaId = @UserLoginPersonaId AND RoleTemplateId = @RoleTemplateId)     
       BEGIN  
          BEGIN  TRY    
           INSERT INTO [Security].[DelegatedAdminRoleTemplate] (UserLoginPersonaId,  RoleTemplateId )  VALUES (@UserLoginPersonaId,@RoleTemplateId);       
                              
           SELECT @DelegateRoleID = SCOPE_IDENTITY();        
           SELECT @DelegateRoleID AS Id,'' AS ErrorMessage;      
                                  
          END TRY    
          BEGIN CATCH                   
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;        
            SELECT 0 AS Id, ErrorMessage        
            FROM dbo.ErrorLog        
            WHERE ErrorLogID = @ErrorLogID;        
          END CATCH;  
     END     
END