CREATE PROCEDURE [Auth].ClientRedirectUrisSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		ClientRedirectUriId as Id
		, ClientId
		, Uri 
	FROM 
		[Auth].[ClientRedirectUris]

END