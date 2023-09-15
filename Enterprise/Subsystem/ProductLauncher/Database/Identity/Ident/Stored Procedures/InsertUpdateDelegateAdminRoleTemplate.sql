CREATE  PROCEDURE [Security].[InsertUpdateDelegateAdminRoleTemplate]            
(            
 @UserLoginPersonaId BIGINT,
 @TargetRoleTemplateLists [Enterprise].[IntListType] READONLY         
)          
AS            
BEGIN   
   BEGIN TRY
    
         IF NOT  EXISTS( SELECT 1  FROM [Security].[DelegatedAdminRoleTemplate] WHERE UserLoginPersonaId = @UserLoginPersonaId )     
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