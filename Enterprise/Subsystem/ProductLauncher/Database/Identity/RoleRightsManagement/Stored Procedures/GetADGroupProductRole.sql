CREATE PROCEDURE [Security].[GetADGroupProductRoleByProductId]
	@ProductId INT
AS
BEGIN
	SELECT ADGroupProductRoleId, 
			ADGroupID,	
			ProductId,
			RoleName,
			IsAdminRole
	FROM Security.ADGroupProductRole
	WHERE
		ProductId = @ProductId

END