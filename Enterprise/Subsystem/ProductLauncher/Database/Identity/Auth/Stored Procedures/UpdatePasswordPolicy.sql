CREATE PROCEDURE [Auth].[UpdatePasswordPolicy] (
	@PasswordPolicyId [int],
	@PortfolioId [int],
	@MinimumLength [tinyint],
	@MaximumLength [tinyint],
	@MinimumLowercase [tinyint],
	@MinimumUppercase [tinyint],
	@MinimumNumeric [tinyint],
	@MinimumSpecialCharacter [tinyint],
	@AllowUsersToChangeOwnPassword [bit],
	@EnablePasswordExpiration [bit],
	@PasswordExpirationPeriodInDays [smallint],
	@PreventPasswordReuse [bit],
	@NumberOfPasswordsToRemember [tinyint],
	@UserId bigint
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		-- Insert statements for procedure here
		UPDATE	[Auth].[PasswordPolicy]
		SET		[MinimumLength] = @MinimumLength,
				[MaximumLength] = @MaximumLength,
				[MinimumLowercase] = @MinimumLowercase,
				[MinimumUppercase] = @MinimumUppercase,
				[MinimumNumeric] = @MinimumNumeric,
				[MinimumSpecialCharacter] = @MinimumSpecialCharacter,
				[AllowUsersToChangeOwnPassword] = @AllowUsersToChangeOwnPassword,
				[EnablePasswordExpiration] = @EnablePasswordExpiration,
				[PasswordExpirationPeriodInDays] = @PasswordExpirationPeriodInDays,
				[PreventPasswordReuse] = @PreventPasswordReuse,
				[NumberOfPasswordsToRemember] = @NumberOfPasswordsToRemember,
				[UserId] = @UserId
		OUTPUT	inserted.PasswordPolicyId AS Id,
				'' AS ErrorMessage
		WHERE	[PasswordPolicyId] = @PasswordPolicyId

		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO