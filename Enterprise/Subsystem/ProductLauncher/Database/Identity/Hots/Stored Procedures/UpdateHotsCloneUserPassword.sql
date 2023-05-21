CREATE PROCEDURE [Hots].[UpdateHotsCloneUserPassword]
(
	@UserId INT,
    @PasswordHash nvarchar(255),
	@PasswordSalt nvarchar(255)
)
AS
BEGIN
	DECLARE @IdentityProviderTypeId INT
	SELECT @IdentityProviderTypeId = IdentityProviderTypeId FROM Ident.IdentityProviderType WHERE Name = 'ID3'

	UPDATE Ident.UserLogin 
		SET
			PasswordHash = @PasswordHash,
			PasswordSalt = @PasswordSalt,
			IdentityProviderTypeId = @IdentityProviderTypeId,
			PasswordModifiedDate = DATEADD(mm, 2, GETUTCDATE())
	WHERE
		UserId = @UserId

	UPDATE Ident.UserLoginPersona
		SET
			StatusTypeId = 1,
			ThruDate = NULL,
			StatusThruDate = NULL
	WHERE
		UserLoginId = @UserId

	SELECT @UserId [Id], '' [ErrorMessage]
END
GO
