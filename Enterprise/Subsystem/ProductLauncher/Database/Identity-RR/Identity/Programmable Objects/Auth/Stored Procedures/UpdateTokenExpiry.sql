IF OBJECT_ID('[Auth].[UpdateTokenExpiry]') IS NOT NULL
	DROP PROCEDURE [Auth].[UpdateTokenExpiry];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[UpdateTokenExpiry]
	@TokenKey		NVARCHAR (128)     ,
	@Expiry			DATETIMEOFFSET (7)
AS
BEGIN

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Update [Auth].[Tokens] set [Expiry] = @Expiry where [TokenKey]=@TokenKey

END
GO
