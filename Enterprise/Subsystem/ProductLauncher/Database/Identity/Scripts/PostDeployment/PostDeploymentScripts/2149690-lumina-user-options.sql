
--User Story 2149690: Lumina Setting: Refine the Lumina Settings Schema for User Level Settings and Banner Configuration.
GO
IF NOT EXISTS (SELECT 1 FROM settings.SettingPicklist WHERE CategoryName = N'aichat' AND MappingKeyName = N'aichatmappingkeyuseroptions' AND MappingName = N'All Users' AND MappingValue = 1)
BEGIN
    INSERT INTO settings.SettingPicklist(CategoryName, MappingKeyName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate, ThruDate)
    VALUES (N'aichat', N'aichatmappingkeyuseroptions', N'All Users', 1, NULL, 0, GETDATE(), NULL);
END

IF NOT EXISTS (SELECT 1 FROM settings.SettingPicklist WHERE CategoryName = N'aichat' AND MappingKeyName = N'aichatmappingkeyuseroptions' AND MappingName = N'Users assigned a specific right' AND MappingValue = 2)
BEGIN
    INSERT INTO settings.SettingPicklist(CategoryName, MappingKeyName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate, ThruDate)
    VALUES (N'aichat', N'aichatmappingkeyuseroptions', N'Users assigned a specific right', 2, NULL, 0, GETDATE(), NULL);
END

IF NOT EXISTS (SELECT 1 FROM settings.SettingPicklist WHERE CategoryName = N'aichat' AND MappingKeyName = N'aichatmappingkeyuseroptions' AND MappingName = N'Nobody in the company' AND MappingValue = 3)
BEGIN
    INSERT INTO settings.SettingPicklist(CategoryName, MappingKeyName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate, ThruDate)
    VALUES (N'aichat', N'aichatmappingkeyuseroptions', N'Nobody in the company', 3, NULL, 0, GETDATE(), NULL);
END
GO