CREATE PROCEDURE [Ident].[GetUserTokenDetail]
	@UserId BIGINT,
	@LoginProvider NVARCHAR(100),
	@Name NVARCHAR(100)
AS
BEGIN
	SELECT
		Id,
		UserId,
		LoginProvider,
		[Name],
		[Value]
	FROM
		Ident.UserTokens
	WHERE
		UserId = @UserId
		AND
		LoginProvider = @LoginProvider
		AND
		Name = @Name

END

