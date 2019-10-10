CREATE PROCEDURE [Auth].[GetClientScopesByClientId]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ClientScopes WHERE ClientId = @ClientId

END