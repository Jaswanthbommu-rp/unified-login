GO

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.OrganizationDomain )
BEGIN
	INSERT INTO Enterprise.OrganizationDomain ( Name, FromDate ) VALUES ( 'Primary', GETUTCDATE())
END

GO
