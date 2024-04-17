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
		Auth.ClientScopes CS
	INNER JOIN 
		Auth.ApiScopes APS ON APS.Name = CS.Scope
	WHERE
		@ClientId = 0 OR ClientId = @ClientId
END