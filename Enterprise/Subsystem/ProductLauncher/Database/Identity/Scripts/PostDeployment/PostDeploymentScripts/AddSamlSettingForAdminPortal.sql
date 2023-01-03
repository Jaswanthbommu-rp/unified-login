Go
-- Adding Role Type saml for Admin Portal Product
IF NOT EXISTS(SELECT TOP 1 1 FROM Ident.SamlAttribute where [Name] = 'RoleType')
BEGIN
	INSERT INTO Ident.SamlAttribute([Name], SamlAttributeTypeId, DisplayName)
	VALUES('RoleType',1,'Role Type')
END
Go