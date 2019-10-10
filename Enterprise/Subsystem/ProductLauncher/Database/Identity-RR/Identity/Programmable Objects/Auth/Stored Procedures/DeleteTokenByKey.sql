IF OBJECT_ID('[Auth].[DeleteTokenByKey]') IS NOT NULL
	DROP PROCEDURE [Auth].[DeleteTokenByKey];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[DeleteTokenByKey]
	@TokenKey		NVARCHAR (128),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Delete FROM Auth.Tokens WHERE TokenKey = @TokenKey and TokenType=@TokenType

END
GO
