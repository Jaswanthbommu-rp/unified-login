CREATE PROCEDURE [Auth].ClientSecretsSelect
(
	@ClientId INT = 0
)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  Id
		, ClientId
		, Value
		, Type
		, Description
		, Created
		, Expiration
	FROM
		Auth.ClientSecrets
	WHERE
		@ClientId = 0 OR ClientId = @ClientId
END
