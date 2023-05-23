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
		, [PostLogoutRedirectUri]) 
	VALUES 
		(@ClientId
		, @Uri);
	
	SELECT 
		  Id
		, ClientId
		, PostLogoutRedirectUri 
	FROM 
		Auth.ClientPostLogoutRedirectUris 
	WHERE (Id = SCOPE_IDENTITY())
END
