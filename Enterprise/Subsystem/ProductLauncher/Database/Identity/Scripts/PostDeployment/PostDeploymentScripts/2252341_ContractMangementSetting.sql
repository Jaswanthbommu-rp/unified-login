IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ProductUsernameDataSharedWithOtherProduct')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('ProductUsernameDataSharedWithOtherProduct', 'Product username data shared with other product', 0)
END

GO
