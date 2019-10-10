CREATE PROCEDURE [Auth].ClientPostLogoutRedirectUrisSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		ClientPostLogoutRedirectUriId as Id
		, ClientId
		, Uri
	FROM
	Auth.ClientPostLogoutRedirectUris
END