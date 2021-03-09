GO
-- Move Password policy settings data in to new organization settings table
DECLARE @UserId bigint,
	@ProductId int ,
	@SettingCategoryTypeId smallint,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @SettingCategoryTypeId = SettingCategoryTypeId
From [Ident].[SettingCategoryType]
Where Name = 'Security'

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'NumberOfPasswordsToRemember')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'NumberOfPasswordsToRemember',
		NumberOfPasswordsToRemember,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'PreventPasswordReuse')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'PreventPasswordReuse',
		PreventPasswordReuse,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'PasswordExpirationPeriodInDays')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'PasswordExpirationPeriodInDays',
		PasswordExpirationPeriodInDays,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'EnablePasswordExpiration')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'EnablePasswordExpiration',
		EnablePasswordExpiration,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'AllowUsersToChangeOwnPassword')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'AllowUsersToChangeOwnPassword',
		AllowUsersToChangeOwnPassword,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'MinimumSpecialCharacter')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumSpecialCharacter',
		MinimumSpecialCharacter,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'MinimumNumeric')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumNumeric',
		MinimumNumeric,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'MinimumUppercase')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumUppercase',
		MinimumUppercase,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'MinimumLowercase')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumLowercase',
		MinimumLowercase,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'MaximumLength')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MaximumLength',
		MaximumLength,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END

IF NOT EXISTS (SELECT 1 From [Ident].[OrganizationSettings] Where MappingName = 'MinimumLength')
BEGIN
	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumLength',
		MinimumLength,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
END
GO