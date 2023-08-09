CREATE  PROCEDURE [Security].[InsertUpdateDelegateAdminRoleTemplate]            
(            
 @UserLoginPersonaId BIGINT,
 @IsDelegateFlag BIT =0,
 @TargetRoleTemplateLists [Enterprise].[IntListType] READONLY         
)          
AS            
BEGIN   
   BEGIN TRY
     IF (SELECT COUNT(*) FROM @TargetRoleTemplateLists) = 0  
     BEGIN  
           SELECT 0 AS Id  ,'Target Template Role  list is empty.'; 
           RETURN;  
     END; 
    
     IF @IsDelegateFlag =1
        DELETE FROM [Security].[DelegatedAdminRoleTemplate] where UserLoginPersonaId = @UserLoginPersonaId;

     IF @IsDelegateFlag =0
     IF NOT  EXISTS( SELECT 1  FROM [Security].[DelegatedAdminRoleTemplate] WHERE UserLoginPersonaId = @UserLoginPersonaId  AND RoleTemplateId in (Select Id from @TargetRoleTemplateLists) )     
     BEGIN
         INSERT INTO [Security].[DelegatedAdminRoleTemplate] (UserLoginPersonaId,RoleTemplateId)
         SELECT @UserLoginPersonaId, RT.ID
         FROM  @TargetRoleTemplateLists RT

         SELECT	@UserLoginPersonaId AS Id, '' AS ErrorMessage
     END
     ELSE
     BEGIN
      -- SELECT * FROM [Security].[DelegatedAdminRoleTemplate] where UserLoginPersonaId = @UserLoginPersonaId;
         DELETE FROM [Security].[DelegatedAdminRoleTemplate] where UserLoginPersonaId = @UserLoginPersonaId;

         INSERT INTO [Security].[DelegatedAdminRoleTemplate] (UserLoginPersonaId,RoleTemplateId)
         SELECT @UserLoginPersonaId, RT.ID
         FROM  @TargetRoleTemplateLists RT

          SELECT	@UserLoginPersonaId AS Id, '' AS ErrorMessage
     END
  END TRY
  BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END

