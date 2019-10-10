IF OBJECT_ID('[Auth].[GetUserSecurityQuestionAnswer]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetUserSecurityQuestionAnswer];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetUserSecurityQuestionAnswer]
	@EnterpriseUserName nvarchar(50) 
AS
BEGIN
	SET NOCOUNT ON;
	declare @UserId as bigint
	select @UserId=userid from users where LoginId=@EnterpriseUserName

SELECT        Auth.Users.UserId, Auth.Users.LoginId, Auth.UserSecurityAnswer.SecurityQuestionId AS SecurityQuestionId, Auth.UserSecurityAnswer.Answer, Auth.SecurityQuestion.Question, 
                         Auth.UserSecurityAnswer.UserSecurityAnswerId
FROM            Auth.SecurityQuestion INNER JOIN
                         Auth.UserSecurityAnswer ON Auth.SecurityQuestion.SecurityQuestionId = Auth.UserSecurityAnswer.SecurityQuestionId INNER JOIN
                         Auth.Users ON Auth.UserSecurityAnswer.UserId = Auth.Users.UserId
WHERE        (Auth.Users.userId = @UserId)

END
GO
