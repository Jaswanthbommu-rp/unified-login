CREATE PROCEDURE Ident.UpdateSecuritySetting (
	@SourceId bigint,
	@DataImportApplicationId int,
	@JsonSecuritySettings nvarchar(max)
)
AS
BEGIN
	BEGIN TRY
		DECLARE @Inserted TABLE (
			Id int
		)
		UPDATE	ipp
		SET			ipp.MinimumLength = ISNULL(u.MinimumLength, ipp.MinimumLength),
						ipp.PasswordExpirationPeriodInDays = ISNULL(u.PasswordExpirationPeriodInDays, ipp.PasswordExpirationPeriodInDays),
						ipp.NumberOfPasswordsToRemember = ISNULL(u.NumberOfPasswordsToRemember, ipp.NumberOfPasswordsToRemember)
		OUTPUT	inserted.PasswordPolicyId AS Id INTO @Inserted
		FROM	[Ident].[PasswordPolicy] ipp
					INNER JOIN Enterprise.DataImportMapping edim ON ipp.PartyId = edim.PartyId 
					INNER JOIN (
											SELECT		MinimumLength,
															PasswordExpirationPeriodInDays,
															NumberOfPasswordsToRemember
											FROM (
															SELECT		Name,
																			Value
															FROM	OPENJSON (@JsonSecuritySettings)
																		WITH
																		(
																			Name varchar(max) '$.Name',
																			Value int '$.Value'
																		)
															WHERE	ISJSON(@JsonSecuritySettings) > 0
															AND			Name IN ('MinimumLength', 'PasswordExpirationPeriodInDays', 'NumberOfPasswordsToRemember')
											) AS SourceTable
											PIVOT
											(
												MIN([Value]) 
												FOR Name IN (MinimumLength, PasswordExpirationPeriodInDays, NumberOfPasswordsToRemember)
											) AS p
					) u ON (1 = 1)
		WHERE	edim.SourceId = @SourceId 
		AND			edim.DataImportApplicationId = @DataImportApplicationId

		UPDATE	iac
						SET	iac.MaxActivityAttemptCount =
							CASE
								WHEN ((u.Name = 'ForgotPassword') AND (u.Value > 0)) THEN u.Value
								ELSE iac.MaxActivityAttemptCount
							END,
							iac.ActivityTokenExpirationMinutes =
							CASE
								WHEN ((u.Name = 'NewUserRegistration') AND (u.Value > 0) AND (u.Value % 1 = 0)) THEN u.Value * 1440
								WHEN u.Value IS NOT NULL THEN u.Value
								ELSE iac.ActivityTokenExpirationMinutes
							END
		OUTPUT	inserted.ActivityConfigurationId AS Id INTO @Inserted
		FROM		[Ident].[ActivityConfiguration] iac
						INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = iac.ActivityTypeId
						INNER JOIN Enterprise.DataImportMapping edim ON iac.PartyId = edim.PartyId 
						INNER JOIN (
												SELECT		Name,
																Value
												FROM	OPENJSON (@JsonSecuritySettings)
															WITH
															(
																Name varchar(max) '$.Name',
																Value int '$.Value'
															)
												WHERE	ISJSON(@JsonSecuritySettings) > 0
												AND			Name IN ('ForcedLock', 'ForgotPassword', 'NewUserRegistration')
					) u ON (iat.ActivityCode = u.Name)
		WHERE	edim.SourceId = @SourceId 
		AND			edim.DataImportApplicationId = @DataImportApplicationId

		SELECT	COUNT(Id) AS Id,
					'' AS ErrorMessage
		FROM	@Inserted
	END TRY  
	BEGIN CATCH
        DECLARE @ErrorLogID int;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
					ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
	END CATCH
END