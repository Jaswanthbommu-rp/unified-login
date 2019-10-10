CREATE PROCEDURE Ident.UpdateActivityConfiguration (
	@JsonActivityConfiguration nvarchar(max)
)
AS
BEGIN
	BEGIN TRY
		UPDATE	iac
		SET		iac.MaxActivityAttemptCount = ISNULL(jac.MaxActivityAttemptCount, iac.MaxActivityAttemptCount),
					iac.ActivityTokenExpirationMinutes =
					CASE
						WHEN ((jac.ActivityCode = 'ForcedLock') AND (jac.ActivityTokenExpirationDays > 0) AND (jac.ActivityTokenExpirationDays % 1 = 0)) THEN jac.ActivityTokenExpirationDays * 1440
						WHEN jac.ActivityTokenExpirationMinutes IS NOT NULL THEN jac.ActivityTokenExpirationMinutes
						ELSE iac.ActivityTokenExpirationMinutes
					END
		OUTPUT	inserted.ActivityConfigurationId AS Id,
						'' AS ErrorMessage
		FROM	Ident.ActivityConfiguration iac
					INNER JOIN OPENJSON (@jsonActivityConfiguration)
					WITH
					(
						ActivityConfigurationId int '$.ActivityConfigurationId',
						ActivityTypeId int '$.ActivityTypeId',
						ActivityCode varchar(50) '$.ActivityCode',
						[Description] varchar(100) '$.Description',
						MaxActivityAttemptCount tinyint '$.MaxActivityAttemptCount',
						ActivityTokenExpirationMinutes int '$.ActivityTokenExpirationMinutes',
						ActivityTokenExpirationDays int '$.ActivityTokenExpirationDays'
					) jac ON (iac.ActivityConfigurationId = jac.ActivityConfigurationId)
		WHERE	ISJSON(@jsonActivityConfiguration) > 0
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
