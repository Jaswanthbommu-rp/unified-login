CREATE PROCEDURE [Security].[GetEnterpriseRoleDeletedProductsByRoleTemplateId]
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
	 FROM olddata rt 
	 WHERE RT.RoleTemplateProductId NOT IN ( SELECT RoleTemplateProductId FROM security.RoleTemplateProduct WHERE Roletemplateid = @RoleTemplateId)
END;
