CREATE PROCEDURE Ident.GetUserSelectedSecurityQuestions (
	@realPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	usa.SecurityQuestionId,
					sq.Question
	FROM		Ident.UserLoginPersona ulp
					INNER JOIN Ident.UserLogin ul ON ul.UserId = ulp.UserLoginId
					INNER JOIN Ident.UserSecurityAnswer AS usa ON ulp.UserLoginId = usa.UserId
					INNER JOIN Ident.SecurityQuestion AS sq ON usa.SecurityQuestionId = sq.SecurityQuestionId
					INNER JOIN Enterprise.Party AS p ON ul.PersonPartyId = p.PartyId
	WHERE	p.RealPageId = @realPageId
	AND			ulp.PrimaryOrganization = 'true'
END;