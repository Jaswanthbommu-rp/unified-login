IF OBJECT_ID('[Auth].[GetUserById]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetUserById];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetUserById]
	@UserId	bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Users WHERE UserId = @UserId

END
GO
