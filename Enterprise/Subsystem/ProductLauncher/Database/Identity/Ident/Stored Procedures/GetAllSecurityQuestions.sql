CREATE PROCEDURE [Ident].[GetAllSecurityQuestions] (
	@enterpriseUserName as nvarchar(255) -- added to get user (custom) security questions in the future
)
AS
BEGIN
	 
	SET NOCOUNT ON;
	  
	SELECT [SecurityQuestionId],[Question],[IsActive]
	FROM [Ident].[SecurityQuestion] where IsActive=1

END