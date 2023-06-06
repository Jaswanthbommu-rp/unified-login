CREATE PROCEDURE [Auth].[SearchApiScopeClaims]
	@ScopeId int = 0
AS
BEGIN
	SELECT [Id]
		  ,[ScopeId]
		  ,[Type]
	FROM 
	[Auth].[ApiScopeClaims]
	WHERE
	 @ScopeId IS NULL OR @ScopeId = ScopeId
END
GO
