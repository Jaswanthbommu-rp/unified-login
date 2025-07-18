--User Story 2375427: Create new product setting 'FriendlyURLProductName'

IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'FriendlyURLProductName')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('FriendlyURLProductName', 'A user-friendly URL slug used to navigate directly to a specific product page from Unity', 0)
END