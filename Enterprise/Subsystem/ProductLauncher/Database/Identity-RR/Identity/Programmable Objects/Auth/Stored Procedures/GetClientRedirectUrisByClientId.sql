IF OBJECT_ID('[Auth].[GetClientRedirectUrisByClientId]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetClientRedirectUrisByClientId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetClientRedirectUrisByClientId]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
		SET NOCOUNT ON;

		SELECT * FROM Auth.ClientRedirectUris WHERE ClientId = @ClientId
END
GO
