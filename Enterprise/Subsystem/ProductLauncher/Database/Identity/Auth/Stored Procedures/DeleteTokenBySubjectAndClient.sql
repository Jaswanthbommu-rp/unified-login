CREATE PROCEDURE [Auth].[DeleteTokenBySubjectAndClient]
	@SubjectCode		NVARCHAR (200),
	@ClientCode NVARCHAR (200),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	delete FROM Auth.Tokens WHERE SubjectCode = @SubjectCode and ClientCode=@ClientCode and TokenType=@TokenType

END
GO