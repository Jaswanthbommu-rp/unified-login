CREATE PROCEDURE [Auth].ClientScopesDelete
(
	@Original_ClientScopeId int,
	@Original_ClientId int,
	@Original_Scope nvarchar(200)
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ClientScopes] 
	WHERE 
		(([Id] = @Original_ClientScopeId) 
		AND 
		([ClientId] = @Original_ClientId) 
		AND 
		([Scope] = @Original_Scope))
END
