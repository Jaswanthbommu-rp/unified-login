CREATE PROCEDURE [Security].[GetOrganizationTypeForRole](  
    @RoleId int  
)  
AS  
BEGIN  
 SELECT OGT.OrganizationTypeId, OGT.[Name]  
 FROM [Security].RoleOrganizationType ROT 
 INNER JOIN Enterprise.OrganizationType OGT ON OGT.OrganizationTypeId = ROT.OrganizationTypeId  
 WHERE ROT.RoleId = @RoleId
END