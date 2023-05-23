CREATE PROCEDURE [Auth].ClientScopesUpdate
(
	@ClientId int,
	@Scope nvarchar(200),
	@Original_ClientScopeId int,
	@Original_ClientId int,
	@Original_Scope nvarchar(200),
	@ClientScopeId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ClientScopes] 
		SET 
		[ClientId] = @ClientId
		, [Scope] = @Scope 
	WHERE 
		(([Id] = @Original_ClientScopeId) 
		AND 
		([ClientId] = @Original_ClientId) 
		AND
		([Scope] = @Original_Scope));
	
	SELECT 
		   Id
		, ClientId
		, Scope 
	FROM Auth.ClientScopes 
	WHERE 
		(Id = @ClientScopeId)
END
