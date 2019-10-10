CREATE PROCEDURE [Auth].ClientPostLogoutRedirectUrisUpdate
(
	@ClientId int,
	@Uri nvarchar(2000),
	@Original_ClientPostLogoutRedirectUriId int,
	@Original_ClientId int,
	@Original_Uri nvarchar(2000),
	@ClientPostLogoutRedirectUriId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ClientPostLogoutRedirectUris] 
	SET 
		[ClientId] = @ClientId
		, [Uri] = @Uri 
	WHERE 
		(([ClientPostLogoutRedirectUriId] = @Original_ClientPostLogoutRedirectUriId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([Uri] = @Original_Uri));
	
	SELECT ClientPostLogoutRedirectUriId as Id
		, ClientId
		, Uri 
	FROM Auth.ClientPostLogoutRedirectUris 
	WHERE 
		(ClientPostLogoutRedirectUriId = @ClientPostLogoutRedirectUriId)
END