CREATE PROCEDURE [Security].[GetOrganizationTypesForADGroup] @adGroupId int    
AS    
BEGIN    
	SELECT p.OrganizationTypeId as OrganizationTypeId, p.Name, ap.CreatedDate
	FROM Security.[ADGroupOrganizationType] ap    
	JOIN Enterprise.OrganizationType p on p.OrganizationTypeId = ap.OrganizationTypeId    
	WHERE AP.ADGroupId = @adGroupId    
	ORDER BY p.Name    
END
GO

