CREATE PROCEDURE [Ident].[UpdateUserLoginTwoFactor]
	@UserId BIGINT,
	@Status TINYINT
AS
BEGIN
	UPDATE Ident.UserLogin
		SET TwoFactorEnabled = @Status
	WHERE
		UserId = @UserId
		
END
 