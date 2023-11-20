CREATE PROCEDURE [Auth].ClientRedirectUrisSelect
(
	@ClientId INT = 0
)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		  Id
		, ClientId
		, RedirectUri
	FROM 
		[Auth].[ClientRedirectUris]
	WHERE
		@ClientId = 0 OR ClientId = @ClientId
END