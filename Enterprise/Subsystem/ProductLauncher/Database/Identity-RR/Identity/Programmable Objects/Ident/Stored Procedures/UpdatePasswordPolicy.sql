IF OBJECT_ID('[Ident].[UpdatePasswordPolicy]') IS NOT NULL
	DROP PROCEDURE [Ident].[UpdatePasswordPolicy];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[UpdatePasswordPolicy] (
	@PasswordPolicyId [int],
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
		UPDATE	Ident.[PasswordPolicy]
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
