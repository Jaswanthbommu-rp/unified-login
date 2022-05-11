
CREATE PROCEDURE [Security].[UpdateRolesAssignedToADGroup](  
    @AdGroupId INT,  
 @RoleIdsList nvarchar(max),  
 @CreatedBy INT  
)  
AS  
BEGIN  
DECLARE @CreatedDate datetime = GETUTCDATE()  
DECLARE @Roles TABLE (    
 RoleId int PRIMARY KEY    
)    
IF (LEN(@RoleIdsList) > 0)    
 BEGIN    
  INSERT INTO @Roles (    
   RoleId    
  )    
  SELECT CONVERT(int, value)    
  FROM STRING_SPLIT(@RoleIdsList, ',');    
 END  
 DELETE  
 FROM [Security].ADGroupRole  
 WHERE ADGroupId = @AdGroupId  
     
 INSERT INTO [Security].ADGroupRole(ADGroupId, RoleId, CreatedBy, CreatedDate)  
 SELECT @AdGroupId, RoleId, @CreatedBy, @CreatedDate  
 FROM @Roles  
END
GO

