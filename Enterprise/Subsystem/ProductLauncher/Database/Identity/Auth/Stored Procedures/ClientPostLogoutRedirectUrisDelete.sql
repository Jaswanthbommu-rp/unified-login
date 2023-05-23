CREATE PROCEDURE [Auth].ClientPostLogoutRedirectUrisDelete
(
	@Original_ClientPostLogoutRedirectUriId int,
	@Original_ClientId int,
	@Original_Uri nvarchar(2000)
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ClientPostLogoutRedirectUris] 
	WHERE 
		(([Id] = @Original_ClientPostLogoutRedirectUriId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([PostLogoutRedirectUri] = @Original_Uri))
END