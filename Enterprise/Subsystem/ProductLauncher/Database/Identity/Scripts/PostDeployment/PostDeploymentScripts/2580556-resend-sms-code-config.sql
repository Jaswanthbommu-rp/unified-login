--User Story 2580556: MFA - Add Resend Code link in SMS method - DEV

IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'SmsResendCodeTimeoutSeconds')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('SmsResendCodeTimeoutSeconds', 'Time in seconds for resending SMS verification codes.', 0);
END
GO