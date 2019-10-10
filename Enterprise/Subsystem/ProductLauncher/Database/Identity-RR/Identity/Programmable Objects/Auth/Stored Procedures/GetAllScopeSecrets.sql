IF OBJECT_ID('[Auth].[GetAllScopeSecrets]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetAllScopeSecrets];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetAllScopeSecrets]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ScopeSecrets

END
GO
