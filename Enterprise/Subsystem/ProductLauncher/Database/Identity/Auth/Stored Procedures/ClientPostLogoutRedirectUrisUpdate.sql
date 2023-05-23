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
		, [PostLogoutRedirectUri] = @Uri 
	WHERE 
		(([Id] = @Original_ClientPostLogoutRedirectUriId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([PostLogoutRedirectUri] = @Original_Uri));
	
	SELECT 
	      Id
		, ClientId
		, PostLogoutRedirectUri
	FROM Auth.ClientPostLogoutRedirectUris 
	WHERE 
		(Id = @ClientPostLogoutRedirectUriId)
END