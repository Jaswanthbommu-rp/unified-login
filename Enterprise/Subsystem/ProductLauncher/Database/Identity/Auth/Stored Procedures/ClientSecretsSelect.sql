CREATE PROCEDURE [Auth].ClientSecretsSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  Id
		, ClientId
		, Value
		, Type
		, Description
		, Expiration
	FROM
		Auth.ClientSecrets
END
