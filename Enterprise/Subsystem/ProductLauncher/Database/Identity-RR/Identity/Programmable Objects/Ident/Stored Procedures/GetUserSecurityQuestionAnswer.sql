IF OBJECT_ID('[Ident].[GetUserSecurityQuestionAnswer]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetUserSecurityQuestionAnswer];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
Create PROCEDURE [Ident].[GetUserSecurityQuestionAnswer]
	@EnterpriseUserName nvarchar(255) 
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Ident.UserLogin.UserId, Ident.UserLogin.PartyId, Ident.UserLogin.LoginName, Ident.UserSecurityAnswer.SecurityQuestionId, Ident.UserSecurityAnswer.Answer, 
	Ident.SecurityQuestion.Question, Ident.UserSecurityAnswer.UserSecurityAnswerId FROM Ident.UserLogin 
	INNER JOIN Ident.UserSecurityAnswer ON Ident.UserLogin.UserId = Ident.UserSecurityAnswer.UserId 
	INNER JOIN Ident.SecurityQuestion ON Ident.UserSecurityAnswer.SecurityQuestionId = Ident.SecurityQuestion.SecurityQuestionId
	WHERE ([Ident].UserLogin.LoginName = @EnterpriseUserName)

END
GO
