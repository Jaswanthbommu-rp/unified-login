CREATE TABLE [Enterprise].[ContactMechanismValidEmail]
(
[ContactMechanismValidEmailID] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismTypeID] [int] NOT NULL
)
GO
ALTER TABLE [Enterprise].[ContactMechanismValidEmail] ADD CONSTRAINT [PK_ContactMechanismValidEmail] PRIMARY KEY CLUSTERED  ([ContactMechanismValidEmailID])
GO
ALTER TABLE [Enterprise].[ContactMechanismValidEmail] ADD CONSTRAINT [FK_ContactMechanismValidEmail_ContactMechanismType] FOREIGN KEY ([ContactMechanismTypeID]) REFERENCES [Enterprise].[ContactMechanismType] ([ContactMechanismTypeID])
GO
