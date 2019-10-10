CREATE PROCEDURE [Ident].[GetSecuritySetting] (
	@SourceId bigint,
	@DataImportApplicationId int = 1
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	[Name],
				[Value]
	FROM
	(
		SELECT	CONVERT(int,[MinimumLength]) [MinimumLength],
					CONVERT(int,[PasswordExpirationPeriodInDays]) [PasswordExpirationPeriodInDays],
					CONVERT(int,[NumberOfPasswordsToRemember]) [NumberOfPasswordsToRemember]
		FROM	[Ident].[PasswordPolicy] ipp
					INNER JOIN Enterprise.DataImportMapping edim ON ipp.PartyId = edim.PartyId 
		WHERE	edim.SourceId = @SourceId 
		AND			edim.DataImportApplicationId = @DataImportApplicationId
	) SourceTable
	UNPIVOT 
	(
		   [Value]
		   FOR [Name] IN ([MinimumLength],   [PasswordExpirationPeriodInDays], [NumberOfPasswordsToRemember])
	) AS UnPivotTable
	UNION
	SELECT	iat.[ActivityCode] AS 'Name',
				CASE
					WHEN iat.ActivityCode = 'ForgotPassword' THEN iac.MaxActivityAttemptCount
					WHEN iat.ActivityCode = 'NewUserRegistration' THEN iac.ActivityTokenExpirationMinutes / 1440
					ELSE iac.ActivityTokenExpirationMinutes
				END AS 'Value'
	FROM	[Ident].[ActivityConfiguration] iac
				INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = iac.ActivityTypeId
				INNER JOIN Enterprise.DataImportMapping edim ON iac.PartyId = edim.PartyId 
	WHERE	edim.SourceId = @SourceId
	AND			edim.DataImportApplicationId = @DataImportApplicationId
	AND			iat.ActivityCode IN ('ForgotPassword', 'ForcedLock', 'NewUserRegistration')
END