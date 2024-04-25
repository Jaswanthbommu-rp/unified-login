CREATE PROCEDURE [Auth].[ApiResourceScopesInsert]
(
	@Scope VARCHAR(200),
	@ApiResourceId INT = 0
)
AS
BEGIN
	IF NOT EXISTS (SELECT TOP(1) 1 FROM Auth.ApiScopes WHERE NAME = @Scope )
	BEGIN
		RETURN 0
	END
    
	IF EXISTS ( SELECT TOP (1) 1 FROM Auth.ApiResourceScopes WHERE ApiResourceId = @ApiResourceId AND Scope = @Scope )
	BEGIN
		SELECT 
			 [Id]
			,[Scope]
			,[ApiResourceId]
		FROM 
			[Auth].[ApiResourceScopes]
		WHERE ApiResourceId = @ApiResourceId AND Scope = @Scope
	END
	ELSE
	BEGIN
		INSERT INTO [Auth].[ApiResourceScopes]
		(
			Scope,
			ApiResourceId
		)
		VALUES
		(   @Scope,
			@ApiResourceId
		)
	
		SELECT 
			 [Id]
			,[Scope]
			,[ApiResourceId]
		FROM 
			[Auth].[ApiResourceScopes]
		WHERE
			(Id = SCOPE_IDENTITY())	
	END
END


GO
