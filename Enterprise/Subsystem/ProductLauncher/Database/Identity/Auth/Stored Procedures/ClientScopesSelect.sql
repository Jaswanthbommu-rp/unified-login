CREATE PROCEDURE [Auth].ClientScopesSelect
(
	@ClientId INT = 0
)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  Id
		, ClientId
		, Scope
	FROM
		Auth.ClientScopes
	WHERE
		@ClientId = 0 OR ClientId = @ClientId
END