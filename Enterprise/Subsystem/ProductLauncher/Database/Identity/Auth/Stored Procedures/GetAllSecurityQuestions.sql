CREATE PROCEDURE [Auth].[GetAllSecurityQuestions] (
	@enterpriseUserName as nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--TODO: Get system + user (custom) security questions
		SELECT [SecurityQuestionId]
			  ,[Question]
			  ,[IsActive]
		  FROM [Auth].[SecurityQuestion] where IsActive=1

END