CREATE PROCEDURE [Auth].ClientScopesSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  Id
		, ClientId
		, Scope
	FROM            
		Auth.ClientScopes
END