CREATE PROCEDURE [Auth].[SearchIdentityResourceClaims]
	@IdentityResourceId INT = NULL
AS
BEGIN
	SELECT [Id]
		  ,[IdentityResourceId]
		  ,[Type]
	FROM 
		[Auth].[IdentityResourceClaims]
	WHERE
		@IdentityResourceId IS NULL OR @IdentityResourceId = IdentityResourceId		
END
GO
