CREATE PROCEDURE [Ident].[CreateUpdateUserTokenDetail]
	@UserId BIGINT,
	@LoginProvider NVARCHAR(100),
	@Name NVARCHAR(100),
	@Value NVARCHAR(MAX)
AS
BEGIN
	IF NOT EXISTS ( SELECT TOP (1) 1 FROM Ident.UserTokens WHERE UserId = @UserId AND LoginProvider = @LoginProvider AND Name = @Name )
	BEGIN
		INSERT INTO Ident.UserTokens 
		(
			UserId,
			LoginProvider,
			Name,
			Value
		)
		VALUES
		(   
			@UserId,
			@LoginProvider,
			@Name,
			@Value
		)

	END
	ELSE
	BEGIN
		UPDATE	Ident.UserTokens
			SET [Value] = @Value
		WHERE
			UserId = @UserId
			AND
			LoginProvider = @LoginProvider
			AND
			Name = @Name
	END
END

