CREATE PROCEDURE [Security].[GetEnterpriseRoleDeletedProductsByRoleTemplateId]
(
	@RoleTemplateId int
)
AS
BEGIN
	DECLARE @1minago datetime = DATEADD(mi,-1,GETUTCDATE())
	;WITH olddata (RoleTemplateProductId,RoleTemplateId,ProductId) AS 
		(SELECT RoleTemplateProductId,RoleTemplateId,ProductId 
		 FROM security.RoleTemplateProduct 
		 FOR SYSTEM_TIME AS OF @1minago 
		 WHERE Roletemplateid = @RoleTemplateId)

	 SELECT Distinct ProductId 
	 FROM olddata rt 
	 WHERE RT.RoleTemplateProductId NOT IN ( SELECT RoleTemplateProductId FROM security.RoleTemplateProduct WHERE Roletemplateid = @RoleTemplateId)
END;
