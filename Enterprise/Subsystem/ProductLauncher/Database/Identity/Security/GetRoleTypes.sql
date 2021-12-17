Create Procedure [Security].GetRoleTypes  
AS  
Begin  
 select RoleTypeId as Id, Value as Name, Description  
 from [security].RoleType  
End