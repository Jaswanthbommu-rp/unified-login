CREATE PROCEDURE [Security].[GetEnterpriseRoleNewProductsByRoleTemplateId]
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
	FROM security.RoleTemplateProduct rt 
	WHERE Roletemplateid = @RoleTemplateId
	AND rt.RoleTemplateProductId NOT IN (SELECT RoleTemplateProductId FROM olddata )
END;
