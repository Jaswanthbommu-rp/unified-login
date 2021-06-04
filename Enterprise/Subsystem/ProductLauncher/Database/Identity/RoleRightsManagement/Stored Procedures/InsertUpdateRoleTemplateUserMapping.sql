CREATE PROCEDURE [Security].[InsertUpdateRoleTemplateUserMapping] (
 @RoleTemplateId BIGINT,
 @PersonaId		 BIGINT
)  
AS  
BEGIN    
 BEGIN TRY
	DECLARE @RoleTemplateUserMappingId BIGINT
	SELECT @RoleTemplateUserMappingId =  ISNULL(RoleTemplateUserMappingId,'') FROM Security.RoleTemplateUserMapping WHERE PersonaId = @PersonaId
	IF (@RoleTemplateUserMappingId IS NULL OR @RoleTemplateUserMappingId = 0)
	BEGIN
		INSERT INTO Security.RoleTemplateUserMapping(RoleTemplateId,PersonaId)
		select @RoleTemplateId,@PersonaId
		set @RoleTemplateUserMappingId = SCOPE_IDENTITY();
		SELECT	@RoleTemplateUserMappingId AS Id ,
			'' AS ErrorMessage  
	END
	ELSE
	BEGIN
		UPDATE Security.RoleTemplateUserMapping
		SET RoleTemplateId = @RoleTemplateId
		WHERE PersonaId = @PersonaId
	END
	SELECT	@RoleTemplateUserMappingId AS Id ,
			'' AS ErrorMessage  
 END TRY    
 BEGIN CATCH   
        DECLARE @ErrorLogID INT;  
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;  
  
        SELECT  0 AS Id,  
		ErrorMessage  
        FROM    dbo.ErrorLog  
        WHERE   ErrorLogID = @ErrorLogID;  
 END CATCH  
END;