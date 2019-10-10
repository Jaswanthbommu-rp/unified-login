CREATE PROCEDURE [Ident].[CreateUserSelectedSecurityQuestions]
(@realPageId AS  UNIQUEIDENTIFIER,
 @questionId1 AS INT,
 @questionId2 AS INT,
 @questionId3 AS INT,
 @answer1     NVARCHAR(50),
 @answer2     NVARCHAR(50),
 @answer3     NVARCHAR(50)
)
AS
     BEGIN
         BEGIN TRY
             SET NOCOUNT ON;
             BEGIN TRANSACTION;
             DECLARE @UserId AS BIGINT, @insertDateTime AS SMALLDATETIME;
             SELECT @insertDateTime = GETUTCDATE();

				-- Get UserId

             SELECT @UserId = ULP.UserLoginId
             FROM Enterprise.Party p
			INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
			INNER JOIN Person.Persona PE ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
             WHERE p.RealPageId = @realPageId;

				-- delete old questions
             DELETE FROM [Ident].[UserSecurityAnswer]
             WHERE UserId = @UserId; -- delete old questions

             INSERT INTO [Ident].[UserSecurityAnswer]
             ([UserId],
              [SecurityQuestionId],
              [Answer],
              [CreateDateTime]
             )
             VALUES
             (@UserId,
              @questionId1,
              @answer1,
              @insertDateTime
             ),
             (@UserId,
              @questionId2,
              @answer2,
              @insertDateTime
             ),
             (@UserId,
              @questionId3,
              @answer3,
              @insertDateTime
             );
             SET NOCOUNT OFF;
             COMMIT;
             SELECT @UserId AS UserId;
         END TRY
         BEGIN CATCH
             ROLLBACK;
             DECLARE @ErrorLogID INT;
             EXEC dbo.LogError
                  @ErrorLogID = @ErrorLogID OUTPUT;
             SELECT 0 AS Id,
                    ErrorMessage
             FROM [dbo].ErrorLog
             WHERE ErrorLogID = @ErrorLogID;
         END CATCH;
     END;