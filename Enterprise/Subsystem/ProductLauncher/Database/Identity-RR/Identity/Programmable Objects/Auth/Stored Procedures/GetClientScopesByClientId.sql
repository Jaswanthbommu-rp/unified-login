IF OBJECT_ID('[Auth].[GetClientScopesByClientId]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetClientScopesByClientId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetClientScopesByClientId]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ClientScopes WHERE ClientId = @ClientId

END
GO
