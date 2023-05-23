CREATE PROCEDURE [Auth].ClientPostLogoutRedirectUrisSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  Id
		, ClientId
		, PostLogoutRedirectUri
	FROM
	Auth.ClientPostLogoutRedirectUris
END