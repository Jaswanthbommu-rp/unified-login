CREATE PROCEDURE [Security].[GetEnterpriseRoleNewProductsByRoleTemplateId]
	(
	@RoleTemplateId int,
	@CreatedDateTime datetime
)
AS
BEGIN
	DECLARE @5secago datetime = DATEADD(SECOND,-5,@CreatedDateTime)
	;WITH olddata (RoleTemplateProductId,RoleTemplateId,ProductId) AS 
		(SELECT RoleTemplateProductId,RoleTemplateId,ProductId 
		 FROM security.RoleTemplateProduct 
		 FOR SYSTEM_TIME AS OF @5secago 
		 WHERE Roletemplateid = @RoleTemplateId)
	
	SELECT Distinct ProductId 
	FROM security.RoleTemplateProduct rt 
	WHERE Roletemplateid = @RoleTemplateId
	AND rt.RoleTemplateProductId NOT IN (SELECT RoleTemplateProductId FROM olddata )
END;
