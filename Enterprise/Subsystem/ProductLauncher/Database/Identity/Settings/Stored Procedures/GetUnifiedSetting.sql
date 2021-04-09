Create PROCEDURE [Settings].[GetUnifiedSetting] (
	@PartyId bigint,
	@Category Varchar(50))
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @settings TABLE (			
			MappingName varchar(200),
			MappingValue varchar(100),
			Editable bit,
			Hidden bit
		)
	INSERT INTO @settings
	Select OS.MappingName AS 'Name',
		   OS.[MappingValue] AS 'Value',
		   OS.[Editable] AS 'Editable',
           OS.[Hidden] AS 'Hidden'
	From [Settings].[OrganizationSettings] OS
	JOIN [Settings].[SettingCategoryType] SCT ON
		OS.SettingCategoryTypeId = SCT.SettingCategoryTypeId
	WHERE OS.PartyId = @PartyId
	AND SCT.Name = @Category
	
	INSERT INTO @settings
	SELECT	iat.[ActivityCode] AS 'Name',
				CASE
					WHEN iat.ActivityCode = 'Login' THEN CONVERT(VARCHAR(10), iac.MaxActivityAttemptCount)
					WHEN iat.ActivityCode = 'NewUserRegistration' THEN iac.ActivityTokenExpirationMinutes / 1440
					ELSE iac.ActivityTokenExpirationMinutes
				END AS 'Value',
				1 AS 'Editable',
                0 AS 'Hidden'
	FROM	[Ident].[ActivityConfiguration] iac
				INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = iac.ActivityTypeId
	WHERE	iac.PartyId = @PartyId
	AND		iat.ActivityCode IN ('Login', 'ForcedLock', 'NewUserRegistration')

	Select * From @settings
END
