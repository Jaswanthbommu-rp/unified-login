CREATE PROCEDURE [Security].[GetEnterpriseRoleProductsByOrganization]
(
	@RoleTemplateId int, 
	@OrganizationRealPageId UNIQUEIDENTIFIER = NULL,
    @PartyId BIGINT = NULL
)
AS
BEGIN
	SELECT DISTINCT RTP.ProductId
	FROM [Security].[RoleTemplate] RT
	JOIN [Security].[RoleTemplateProduct] RTP ON
		RT.RoleTemplateId = RTP.RoleTemplateId
	JOIN Enterprise.Party P ON
		p.PartyId = RT.PartyID
	WHERE RT.RoleTemplateId = @RoleTemplateId
	AND (P.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)
	AND (P.PartyId = @PartyId OR @PartyId IS NULL)
END;
