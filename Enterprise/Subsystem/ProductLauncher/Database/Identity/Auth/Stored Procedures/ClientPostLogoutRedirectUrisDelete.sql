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
		(([ClientPostLogoutRedirectUriId] = @Original_ClientPostLogoutRedirectUriId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([Uri] = @Original_Uri))
END