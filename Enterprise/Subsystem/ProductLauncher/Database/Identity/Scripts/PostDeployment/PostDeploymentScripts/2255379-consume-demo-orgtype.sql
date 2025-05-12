IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.OrganizationType WHERE [Name] = 'Demo')
BEGIN
	INSERT INTO Enterprise.OrganizationType(Name,CreateDate,FromDate,ThruDate)
	VALUES('Demo', GETUTCDATE(), GETUTCDATE(), NULL)
END