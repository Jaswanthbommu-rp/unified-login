CREATE PROCEDURE [Ident].[GetUserSecurityQuestionAnswer] @EnterpriseUserName NVARCHAR(255)
AS
     BEGIN
         SET NOCOUNT ON;
         SELECT ul.UserId,
                UL.PersonPartyId,
                ul.LoginName,
                usa.SecurityQuestionId,
                usa.Answer,
                sq.Question,
                usa.UserSecurityAnswerId
         FROM Ident.UserLogin ul
              INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
              INNER JOIN Person.Persona PE ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
              INNER JOIN Ident.UserSecurityAnswer usa ON ULP.UserLoginId = usa.UserId
              INNER JOIN Ident.SecurityQuestion sq ON usa.SecurityQuestionId = sq.SecurityQuestionId
         WHERE(ul.LoginName = @EnterpriseUserName);
     END;