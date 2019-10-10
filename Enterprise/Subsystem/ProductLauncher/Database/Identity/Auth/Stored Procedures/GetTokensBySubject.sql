CREATE PROCEDURE [Auth].[GetTokensBySubject]
	@SubjectCode		NVARCHAR (200),
	@TokenType INT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT * FROM Auth.Tokens WHERE Subjectcode = @SubjectCode and TokenType=@TokenType

END
GO