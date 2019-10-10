CREATE PROCEDURE [Auth].ClientPostLogoutRedirectUrisInsert
(
	@ClientId int,
	@Uri nvarchar(2000)
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ClientPostLogoutRedirectUris] 
		([ClientId]
		, [Uri]) 
	VALUES 
		(@ClientId
		, @Uri);
	
	SELECT 
		ClientPostLogoutRedirectUriId as Id
		, ClientId
		, Uri 
	FROM 
		Auth.ClientPostLogoutRedirectUris 
	WHERE (ClientPostLogoutRedirectUriId = SCOPE_IDENTITY())
END
