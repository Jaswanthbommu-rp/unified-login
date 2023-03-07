CREATE PROCEDURE [Security].[AddRoleOrganizationType]    
 @RoleId int,    
 @OrganizationTypeIdsList nvarchar(255),    
 @CreatedBy int    
AS    
BEGIN    
 DECLARE @CreatedDate datetime = GETUTCDATE()    
 DECLARE @OrganizationType TABLE (      
 OrganizationTypeId int PRIMARY KEY      
)      
IF (LEN(@OrganizationTypeIdsList) > 0)      
 BEGIN      
  INSERT INTO @OrganizationType (      
   OrganizationTypeId      
  )      
  SELECT CONVERT(int, value)      
  FROM STRING_SPLIT(@OrganizationTypeIdsList, ',');      
 END   
  
 DELETE    
  FROM [Security].RoleOrganizationType    
  WHERE [Security].RoleOrganizationType.RoleId = @RoleId    
  
 INSERT INTO [Security].[RoleOrganizationType]    
  (RoleId,    
  OrganizationTypeId,    
  CreatedBy,    
  CreatedDate    
  )   
  
  SELECT @RoleId, OrganizationTypeId, @CreatedBy, @CreatedDate    
  FROM @OrganizationType    
   
END;