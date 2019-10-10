CREATE PROCEDURE [Auth].[CreatePasswordPolicy] (
	@PortfolioId [int],
	@MinimumLength [tinyint] = 8,
	@MaximumLength [tinyint] = 128,
	@MinimumLowercase [tinyint] = 0,
	@MinimumUppercase [tinyint] = 0,
	@MinimumNumeric [tinyint] = 0,
	@MinimumSpecialCharacter [tinyint] = 0,
	@AllowUsersToChangeOwnPassword [bit] = 1,
	@EnablePasswordExpiration [bit] = 0,
	@PasswordExpirationPeriodInDays [smallint] = NULL,
	@PreventPasswordReuse [bit] = 0,
	@NumberOfPasswordsToRemember [tinyint] = NULL,
	@UserId [bigint]
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION; 
		-- Insert statements for procedure here
		INSERT INTO [Auth].[PasswordPolicy] (
			[PortfolioId],
			[MinimumLength],
			[MaximumLength],
			[MinimumLowercase],
			[MinimumUppercase],
			[MinimumNumeric],
			[MinimumSpecialCharacter],
			[AllowUsersToChangeOwnPassword],
			[EnablePasswordExpiration],
			[PasswordExpirationPeriodInDays],
			[PreventPasswordReuse],
			[NumberOfPasswordsToRemember],
			[UserId]
		)
		VALUES (
			@PortfolioId,
			@MinimumLength,
			@MaximumLength,
			@MinimumLowercase,
			@MinimumUppercase,
			@MinimumNumeric,
			@MinimumSpecialCharacter,
			@AllowUsersToChangeOwnPassword,
			@EnablePasswordExpiration,
			@PasswordExpirationPeriodInDays,
			@PreventPasswordReuse,
			@NumberOfPasswordsToRemember,
			@UserId
		)

		SELECT	SCOPE_IDENTITY() AS Id,
				'' AS ErrorMessage
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