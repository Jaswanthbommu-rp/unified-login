CREATE PROCEDURE [Auth].[GetClientDetails]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, ClientId, RedirectUri FROM Auth.ClientRedirectUris WHERE ClientId = @ClientId
	SELECT Id, ClientId, Scope FROM Auth.ClientScopes WHERE ClientId = @ClientId
	SELECT Id, ClientId, [Value], [Type], [Description], Expiration FROM Auth.ClientSecrets WHERE ClientId = @ClientId
	SELECT Id, ClientId, PostLogoutRedirectUri FROM Auth.ClientPostLogoutRedirectUris WHERE ClientId = @ClientId
	SELECT Id, ClientId, Type, Value FROM Auth.ClientClaims WHERE ClientId = @ClientId
END
