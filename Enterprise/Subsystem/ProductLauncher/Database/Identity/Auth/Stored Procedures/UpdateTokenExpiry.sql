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

