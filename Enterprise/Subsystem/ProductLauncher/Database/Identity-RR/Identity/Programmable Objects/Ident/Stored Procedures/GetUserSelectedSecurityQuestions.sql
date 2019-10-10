IF OBJECT_ID('[Ident].[GetUserSelectedSecurityQuestions]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetUserSelectedSecurityQuestions];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetUserSelectedSecurityQuestions]
	@realPageId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

		SELECT Ident.UserSecurityAnswer.SecurityQuestionId, Ident.SecurityQuestion.Question
		FROM Ident.UserLogin INNER JOIN Ident.UserSecurityAnswer ON Ident.UserLogin.UserId = Ident.UserSecurityAnswer.UserId
		INNER JOIN Ident.SecurityQuestion ON Ident.UserSecurityAnswer.SecurityQuestionId = Ident.SecurityQuestion.SecurityQuestionId
		INNER JOIN Enterprise.Party ON Ident.UserLogin.PartyId = Enterprise.Party.PartyId
		where Enterprise.Party.RealPageId = @realPageId

END
GO
