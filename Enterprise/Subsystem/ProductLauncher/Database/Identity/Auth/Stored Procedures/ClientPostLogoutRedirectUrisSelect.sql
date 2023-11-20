CREATE PROCEDURE [Auth].ClientPostLogoutRedirectUrisSelect
(
	@ClientId INT = 0
)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  Id
		, ClientId
		, PostLogoutRedirectUri
	FROM
		Auth.ClientPostLogoutRedirectUris
	WHERE
		@ClientId = 0 OR ClientId = @ClientId
END
