IF OBJECT_ID('[Auth].[GetAllScopes]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetAllScopes];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetAllScopes]
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Scopes

END
GO
