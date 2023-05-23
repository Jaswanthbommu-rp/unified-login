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
			,[RedirectUri] = @Uri
	WHERE
		(([Id] = @Original_ClientRedirectUriId) AND ([ClientId] = @Original_ClientId) AND ([RedirectUri] = @Original_Uri));

	SELECT 
		  Id
		, ClientId
		, RedirectUri
	FROM [Auth].[ClientRedirectUris] 
	WHERE 
		(Id = @ClientRedirectUriId)

END
