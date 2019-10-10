CREATE PROCEDURE [Ident].[ListUserSecurityQuestionAnswerByRealPageId] @realPageId UNIQUEIDENTIFIER
AS
     BEGIN
         SET NOCOUNT ON;
         SELECT ul.UserId,
                ul.PersonPartyId,
                ul.LoginName,
                usa.SecurityQuestionId,
                usa.Answer,
                sq.Question,
                usa.UserSecurityAnswerId
         FROM Ident.UserLogin ul
              INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
              INNER JOIN Person.Persona PE ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
              INNER JOIN Ident.UserSecurityAnswer usa ON ul.UserId = usa.UserId
              INNER JOIN Ident.SecurityQuestion  sq ON usa.SecurityQuestionId = sq.SecurityQuestionId
              INNER JOIN Enterprise.Party p ON UL.PersonPartyId = p.PartyId
         WHERE p.RealPageId = @realPageId;
     END;