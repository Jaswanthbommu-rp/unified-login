CREATE PROCEDURE [Auth].ClientRedirectUrisSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		  Id
		, ClientId
		, RedirectUri
	FROM 
		[Auth].[ClientRedirectUris]

END