CREATE PROCEDURE [Auth].[ClientRedirectUrisDelete]
(
	@Original_ClientRedirectUriId INT
	,@Original_ClientId INT
	,@IsNull_Uri INT
	,@Original_Uri nvarchar(2000)
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ClientRedirectUris]
	WHERE
		(([ClientRedirectUriId] = @Original_ClientRedirectUriId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ((@IsNull_Uri = 1 AND [Uri] IS NULL) OR ([Uri] = @Original_Uri)))
END