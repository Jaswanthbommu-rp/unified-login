CREATE PROCEDURE [Auth].[GetToken]
	@TokenKey		NVARCHAR (128),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Tokens WHERE TokenKey = @TokenKey and TokenType=@TokenType

END
GO