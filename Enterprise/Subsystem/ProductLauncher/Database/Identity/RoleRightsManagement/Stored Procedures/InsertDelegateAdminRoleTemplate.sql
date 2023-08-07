CREATE PROCEDURE [Security].[InsertDelegateAdminRoleTemplate]       
(        
 @UserLoginPersonaId BIGINT,        
 @RoleTemplateId BigInt      
)        
AS        
BEGIN        
       
 BEGIN TRY        
            
  declare @id int;    
     
  BEGIN        
   INSERT INTO [Security].[DelegatedAdminRoleTemplate]  ( UserLoginPersonaId, RoleTemplateId )        
   VALUES        
    ( @UserLoginPersonaId, @RoleTemplateId)        
  END        
        
 set @id = scope_identity();    
  SELECT  @id AS Id,        
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
END 