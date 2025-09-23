-- EXEC [Security].[InsertUpdateRoleTemplate] 1,'Leasing Agent', 'Leasing Agent Description', 350
CREATE PROCEDURE [Security].[InsertUpdateRoleTemplate] (
 @RoleTemplateId BIGINT = NULL,
 @RoleTemplateName varchar(100), 
 @RoleTemplateDescription	varchar(255),
 @PartyId BIGINT,
 @RoleType varchar(50) = 'Custom',
 @RoleTemplateNotification NVARCHAR(MAX) = NULL
)  
AS  
BEGIN    
 BEGIN TRY     
   IF(@RoleTemplateId is null or @RoleTemplateId = 0 )
	BEGIN
		INSERT INTO Security.RoleTemplate(RoleTemplateName,RoleTemplateDescription,RoleType,PartyID,RoleTemplateNotification)
		select @RoleTemplateName,@RoleTemplateDescription,@RoleType,@PartyId,@RoleTemplateNotification
		set @RoleTemplateId = SCOPE_IDENTITY();
		SELECT	@RoleTemplateId AS Id ,
            '' AS ErrorMessage  
	END
	ELSE
	BEGIN
		UPDATE Security.RoleTemplate
		SET RoleTemplateName = @RoleTemplateName,
			RoleTemplateDescription = @RoleTemplateDescription,
			RoleTemplateNotification = @RoleTemplateNotification
		WHERE RoleTemplateId = @RoleTemplateId
	END
	SELECT	@RoleTemplateId AS Id ,
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