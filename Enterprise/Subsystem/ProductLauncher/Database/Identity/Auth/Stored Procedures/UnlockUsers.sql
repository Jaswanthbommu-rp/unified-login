CREATE PROCEDURE [Auth].[UnlockUsers] 
(
	@enterpriseUserIds NVARCHAR(MAX)
)
AS
BEGIN
	SELECT 'Deprecated: Moved to ident schema.' AS Information

	--DECLARE @forgotPassword INT = 2
	--DECLARE @questionAttempts INT = 5
	--DECLARE @verifyAnswers INT = 6

	--BEGIN TRY
 --       BEGIN TRANSACTION; 			
	--		UPDATE	[Auth].[Users] 
	--		SET		IsLocked = 0 
	--		WHERE	UserId IN (SELECT * FROM STRING_SPLIT (@enterpriseUserIds, ','))
	       
	--		-- Reset user activity attempts for ForgotPassword, QuestionAttempts and VerifyAnswers
	--		UPDATE	[Auth].[ActivityAttempts] 
	--		SET		AttemptCount = 0 
	--		WHERE	EnterpriseUserName IN (
	--					SELECT	LoginId 
	--						FROM (
	--							SELECT	value, 
	--									U.LoginId 
	--							FROM	STRING_SPLIT (@enterpriseUserIds, ',') SS 
	--						INNER JOIN [Auth].[Users] U WITH(NOLOCK) ON U.UserId = SS.value) SSU
	--				)
	--				AND ActivityId IN (@forgotPassword, @questionAttempts, @verifyAnswers)
              
	--		SELECT  UserId,
	--				LoginId,
	--				Firstname,
	--				LastName,
	--				IsActive,
	--				PasswordHash,
	--				PasswordSalt,
	--				IdentityProvider,
	--				Title,
	--				Email,
	--				Phone,
	--				IsLocked,
	--				LastPasswordModifiedDateTime,
	--				AccountExpiration
	--		FROM	[Auth].[Users] WITH(NOLOCK) 
	--		WHERE	UserId IN (SELECT * FROM STRING_SPLIT (@enterpriseUserIds, ',')) AND IsLocked = 0			

	--	COMMIT;
	--END TRY  
	--BEGIN CATCH
 --       ROLLBACK;
		
 --       DECLARE @ErrorLogID INT;
 --       EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

 --       SELECT  0 AS Id,
	--			ErrorMessage
 --       FROM    dbo.ErrorLog WITH(NOLOCK)
 --       WHERE   ErrorLogID = @ErrorLogID;
	--END CATCH
END;