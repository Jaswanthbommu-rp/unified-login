CREATE PROCEDURE Ident.[UpdatePasswordPolicy] (
	@PasswordPolicyId [int],
	@MinimumLength [tinyint] = NULL,
	@MaximumLength [tinyint] = NULL,
	@MinimumLowercase [tinyint] = NULL,
	@MinimumUppercase [tinyint] = NULL,
	@MinimumNumeric [tinyint] = NULL,
	@MinimumSpecialCharacter [tinyint] = NULL,
	@AllowUsersToChangeOwnPassword [bit] = NULL,
	@EnablePasswordExpiration [bit] = NULL,
	@PasswordExpirationPeriodInDays [smallint] = NULL,
	@PreventPasswordReuse [bit] = NULL,
	@NumberOfPasswordsToRemember [tinyint] = NULL,
	@UserId bigint = NULL
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		-- Insert statements for procedure here
		UPDATE	Ident.[PasswordPolicy]
		SET		[MinimumLength] = ISNULL(@MinimumLength, [MinimumLength]),
				[MaximumLength] = ISNULL(@MaximumLength, [MaximumLength]),
				[MinimumLowercase] = ISNULL(@MinimumLowercase, [MinimumLowercase]),
				[MinimumUppercase] = ISNULL(@MinimumUppercase, [MinimumUppercase]),
				[MinimumNumeric] = ISNULL(@MinimumNumeric, [MinimumNumeric]),
				[MinimumSpecialCharacter] = ISNULL(@MinimumSpecialCharacter, [MinimumSpecialCharacter]),
				[AllowUsersToChangeOwnPassword] = ISNULL(@AllowUsersToChangeOwnPassword, [AllowUsersToChangeOwnPassword]),
				[EnablePasswordExpiration] = ISNULL(@EnablePasswordExpiration, [EnablePasswordExpiration]),
				[PasswordExpirationPeriodInDays] = ISNULL(@PasswordExpirationPeriodInDays, [PasswordExpirationPeriodInDays]),
				[PreventPasswordReuse] = ISNULL(@PreventPasswordReuse, [PreventPasswordReuse]),
				[NumberOfPasswordsToRemember] = ISNULL(@NumberOfPasswordsToRemember, [NumberOfPasswordsToRemember]),
				[UserId] = ISNULL(@UserId, UserId)
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