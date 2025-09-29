--User Story 2514506: Add new product setting to control whether product is included in User Audit Reporting

IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'ExcludeInUserAuditReporting')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'ExcludeInUserAuditReporting', 'Setting to exclude product from audit reports', 0)
END