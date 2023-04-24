IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UserAccessDetails_ProductsWithNoProperties')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('UserAccessDetails_ProductsWithNoProperties', 'Comma seperated product ids that dont have properties tab and excluded from the user access summary', 0)
END

IF NOT EXISTS (Select Top 1 1 from  Enterprise.ThirdPartyRelationship where ThirdPartyRelationship ='System Administrator')
BEGIN
     INSERT INTO Enterprise.ThirdPartyRelationship values (8,'System Administrator')
END