CREATE PROCEDURE [Auth].ScopeSecretsSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT       
		ScopeSecretId as Id
		, ScopeId
		, Description
		, Type
		, Value
		, Expiration
	FROM
		Auth.ScopeSecrets
END
