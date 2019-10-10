IF OBJECT_ID('[Auth].[GetAllScopeClaims]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetAllScopeClaims];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetAllScopeClaims]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.ScopeClaims

END
GO
