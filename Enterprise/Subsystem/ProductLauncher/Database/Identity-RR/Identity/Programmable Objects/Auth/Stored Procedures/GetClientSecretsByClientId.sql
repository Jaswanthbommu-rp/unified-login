IF OBJECT_ID('[Auth].[GetClientSecretsByClientId]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetClientSecretsByClientId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetClientSecretsByClientId]
	@ClientId		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ClientSecrets WHERE ClientId = @ClientId

END
GO
