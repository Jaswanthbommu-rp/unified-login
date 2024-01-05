GO

DECLARE	@RealPageEmployeePartyId BIGINT, @PlatformLoginPageCategoryId SMALLINT, @PrivacyUrl NVARCHAR(100), @ServerName SYSNAME = @@SERVERNAME

SELECT @RealPageEmployeePartyId = PartyId FROM Enterprise.Organization WHERE Name = 'realpage employee';
SELECT @PlatformLoginPageCategoryId = SettingCategoryTypeId FROM [Settings].[SettingCategoryType] WHERE Name = 'platformloginpage';

IF @ServerName IN ('reagbkdbsql001','repgbkdbsql001a','repgbkdbsql001b') --EUSAT, EUPROD
BEGIN
	SET @PrivacyUrl = N'https://www.realpage.co.uk/privacy-policy'
END
ELSE
BEGIN
	SET @PrivacyUrl = N'https://www.realpage.com/privacy-policy'
END

IF NOT EXISTS (SELECT * FROM [Settings].[OrganizationSettings] WHERE PartyId = @RealPageEmployeePartyId AND SettingCategoryTypeId=@PlatformLoginPageCategoryId AND MappingName = 'privacyurl')
BEGIN
	INSERT INTO Settings.OrganizationSettings(PartyId,SettingCategoryTypeId,MappingName,MappingValue,Editable,Hidden,CreatedBy,CreatedDate,UpdatedDate)
	VALUES(@RealPageEmployeePartyId, @PlatformLoginPageCategoryId, N'privacyurl', @PrivacyUrl, DEFAULT, DEFAULT, 0, DEFAULT, NULL)
END
GO