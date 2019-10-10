CREATE TABLE [Ident].[ContactMechanismIdentity]
(
	[ContactMechanismIdentityId] INT NOT NULL IDENTITY, 
    [ContactMechanismId] INT NOT NULL, 
    [IdentityProviderSettingId] INT NOT NULL, 
    [FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    [ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_ContactMechanismIdentity] PRIMARY KEY ([ContactMechanismIdentityId]), 
    CONSTRAINT [FK_ContactMechanismIdentity_ContactMechanism] FOREIGN KEY (ContactMechanismId) REFERENCES Enterprise.ContactMechanism(ContactMechanismID) ON DELETE CASCADE ON UPDATE CASCADE,	
    CONSTRAINT [FK_ContactMechanismIdentity_IdentityProviderSetting] FOREIGN KEY (IdentityProviderSettingId) REFERENCES Ident.IdentityProviderSetting(IdentityProviderSettingId) ON DELETE CASCADE ON UPDATE CASCADE
)
