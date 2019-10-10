IF OBJECT_ID('[Ident].[CreateUserSelectedSecurityQuestions]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateUserSelectedSecurityQuestions];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateUserSelectedSecurityQuestions]
	@realPageId as uniqueidentifier,
    @questionId1 as int,
	@questionId2 as int,
	@questionId3 as int,
	@answer1 NVARCHAR(50),
	@answer2 NVARCHAR(50),
	@answer3 NVARCHAR(50)
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;
			BEGIN TRANSACTION;
				declare @UserId as bigint, @insertDateTime as smalldatetime

				select @insertDateTime=getutcdate()

				-- Get UserId
				SELECT @UserId=Ident.UserLogin.UserId FROM Enterprise.Party 
				INNER JOIN Ident.UserLogin ON Enterprise.Party.PartyId = Ident.UserLogin.PartyId
				where Enterprise.Party.RealPageId=@realPageId

				-- delete old questions
				delete from [Ident].[UserSecurityAnswer] where UserId=@UserId -- delete old questions
				
				INSERT INTO [Ident].[UserSecurityAnswer]
					([UserId],[SecurityQuestionId],[Answer],[CreateDateTime])
				VALUES
					(@UserId,@questionId1,@answer1,@insertDateTime),
					(@UserId,@questionId2,@answer2,@insertDateTime),
					(@UserId,@questionId3,@answer3,@insertDateTime)

			SET NOCOUNT OFF;

			COMMIT;
			SELECT @UserId AS UserId
		END TRY
		BEGIN CATCH
			ROLLBACK;

			DECLARE @ErrorLogID INT;
			EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

			SELECT  0 AS Id,
					ErrorMessage
			FROM    [dbo].ErrorLog
			WHERE   ErrorLogID = @ErrorLogID;
		END CATCH
    END;
GO
