CREATE PROCEDURE [Auth].[GetClientDetails]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT ClientRedirectUriId, ClientId, Uri FROM Auth.ClientRedirectUris WHERE ClientId = @ClientId
	SELECT ClientScopeId,ClientId,Scope FROM Auth.ClientScopes WHERE ClientId = @ClientId
	SELECT ClientSecretId,ClientId,Value,Type,Description,Expiration FROM Auth.ClientSecrets WHERE ClientId = @ClientId
	SELECT ClientPostLogoutRedirectUriId,ClientId,Uri FROM Auth.ClientPostLogoutRedirectUris WHERE ClientId = @ClientId
	SELECT ClientClaimsId, ClientId, Type, Value FROM Auth.ClientClaims WHERE ClientId = @ClientId
END
