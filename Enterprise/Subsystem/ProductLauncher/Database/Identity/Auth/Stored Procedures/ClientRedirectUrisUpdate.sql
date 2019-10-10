CREATE PROCEDURE [Auth].[ClientRedirectUrisUpdate]
(
	@ClientId INT
	,@Uri nvarchar(2000)
	,@Original_ClientRedirectUriId INT
	,@Original_ClientId INT
	,@Original_Uri nvarchar(2000)
	,@ClientRedirectUriId INT
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ClientRedirectUris]
		SET
			[ClientId] = @ClientId
			,[Uri] = @Uri
	WHERE
		(([ClientRedirectUriId] = @Original_ClientRedirectUriId) AND ([ClientId] = @Original_ClientId) AND ([Uri] = @Original_Uri));

	SELECT 
		ClientRedirectUriId as Id
		, ClientId
		, Uri 
	FROM [Auth].[ClientRedirectUris] 
	WHERE 
		(ClientRedirectUriId = @ClientRedirectUriId)

END
