CREATE PROCEDURE [Auth].ClientScopesSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		ClientScopeId as Id
		, ClientId
		, Scope
	FROM            Auth.ClientScopes
END