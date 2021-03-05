CREATE PROCEDURE [Ident].[UpdateUserTwoFactorLastNotifyDate]
	@UserId BIGINT
AS
BEGIN
	UPDATE Ident.UserLogin
		SET TwoFactorLastNotifyDate = GETUTCDATE()
	WHERE
		UserId = @UserId	
END
