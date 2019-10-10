IF OBJECT_ID('[Auth].[GetUserByLoginIdAndProvider]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetUserByLoginIdAndProvider];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetUserByLoginIdAndProvider]
	@LoginId		NVARCHAR (50),
	@IdentityProvider NVARCHAR(100) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Users WHERE LoginId = @LoginId AND IdentityProvider = @IdentityProvider

END
GO
