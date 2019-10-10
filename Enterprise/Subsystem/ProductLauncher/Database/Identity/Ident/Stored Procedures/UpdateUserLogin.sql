CREATE PROCEDURE [Ident].[UpdateUserLogin] (
	@RealPageId uniqueidentifier,
	@LoginName varchar(255) = NULL,
	@PasswordHash nvarchar(255) = NULL,
	@PasswordSalt nvarchar(255) = NULL,
	@FromDate datetime = NULL,
	@ThruDate datetime = NULL,
	@PartyId int
)
AS
BEGIN
	DECLARE @ActivityConfigurationId int,
		@Now datetime = GETUTCDATE()

	BEGIN TRY
		DECLARE @UserLogin TABLE(UserId bigint);

		BEGIN TRANSACTION;
			--Enforce the last X passwords functionality by adding the old password to the password history table
			IF ((NOT @PasswordHash IS NULL) AND (NOT @PasswordSalt IS NULL))
			BEGIN
				SELECT	@ActivityConfigurationId = ActivityConfigurationId
				FROM		Ident.ActivityConfiguration AC
								INNER JOIN Ident.ActivityType AT ON AT.ActivityTypeId = AC.ActivityTypeId
				WHERE	AT.ActivityTypeId = 2
				AND			AC.PartyId = @PartyId;

				INSERT INTO Ident.PasswordHistory (
					[UserId],
					[ActivityConfigurationId],
					[ChangedPasswordHash],
					[ChangedPasswordSalt],
					[ChangedPasswordDateTime]
				)
				SELECT	UL.UserId,
								@ActivityConfigurationId, --ForgotPassword
								@PasswordHash,
								@PasswordSalt,
								@Now
				FROM		Ident.UserLogin UL
								INNER JOIN Enterprise.Party P ON P.PartyId = UL.PersonPartyId
				WHERE	RealPageId = @RealPageId;
			END;

			UPDATE	UserLogin
			SET	[LoginName] = ISNULL(@LoginName, UL.LoginName),
					[PasswordHash] = ISNULL(@PasswordHash, UL.PasswordHash),
					[PasswordSalt] = ISNULL(@PasswordSalt, UL.PasswordSalt)
			OUTPUT inserted.UserId INTO @UserLogin(UserId)
			FROM	Ident.UserLogin UL
						INNER JOIN Enterprise.Party P ON P.PartyId = UL.PersonPartyId
			WHERE	RealPageId = @RealPageId;

			UPDATE	ULP
			SET			[FromDate] = ISNULL(@FromDate, FromDate),
						[ThruDate] = CASE
								WHEN CONVERT(DATE, @ThruDate) = '12/31/9999' THEN NULL
								ELSE ISNULL(@ThruDate, ThruDate)
							END
			FROM 
				Ident.UserLoginPersona ULP
				INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
				INNER JOIN Enterprise.Party P ON P.PartyId = UL.PersonPartyId
			WHERE 
				P.RealPageId = @RealPageId
				AND
				ULP.OrganizationPartyId = @PartyId;

			SELECT	UserID AS Id,
							'' AS ErrorMessage
			FROM		@UserLogin;
		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		DECLARE @ErrorLogID INT;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END;