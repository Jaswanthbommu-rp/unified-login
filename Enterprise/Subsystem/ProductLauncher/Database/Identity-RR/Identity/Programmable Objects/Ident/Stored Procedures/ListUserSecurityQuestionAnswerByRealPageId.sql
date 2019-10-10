IF OBJECT_ID('[Ident].[ListUserSecurityQuestionAnswerByRealPageId]') IS NOT NULL
	DROP PROCEDURE [Ident].[ListUserSecurityQuestionAnswerByRealPageId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
Create PROCEDURE [Ident].[ListUserSecurityQuestionAnswerByRealPageId]
	@realPageId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

SELECT Ident.UserLogin.UserId, Ident.UserLogin.PartyId, Ident.UserLogin.LoginName, Ident.UserSecurityAnswer.SecurityQuestionId, Ident.UserSecurityAnswer.Answer, Ident.SecurityQuestion.Question, 
            Ident.UserSecurityAnswer.UserSecurityAnswerId 
FROM Ident.UserLogin INNER JOIN
            Ident.UserSecurityAnswer ON Ident.UserLogin.UserId = Ident.UserSecurityAnswer.UserId INNER JOIN
            Ident.SecurityQuestion ON Ident.UserSecurityAnswer.SecurityQuestionId = Ident.SecurityQuestion.SecurityQuestionId INNER JOIN
            Enterprise.Party ON Ident.UserLogin.PartyId = Enterprise.Party.PartyId
			where  Enterprise.Party.RealPageId=@realPageId

END
GO
