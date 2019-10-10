CREATE PROCEDURE [Auth].ClientScopesInsert
(
	@ClientId int,
	@Scope nvarchar(200)
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ClientScopes] 
		([ClientId]
		, [Scope]) 
	VALUES 
		(@ClientId
		, @Scope);
	
	SELECT 
		ClientScopeId as Id
		, ClientId
		, Scope 
	FROM Auth.ClientScopes 
	WHERE 
		(ClientScopeId = SCOPE_IDENTITY())
END