CREATE TABLE [Enterprise].[ContactMechanismUsageType]
(
[ContactMechanismUsageTypeID] [int] NOT NULL,
[ParentContactMechanismUsageTypeID] [int] NULL,
[Name] [nvarchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[ContactMechanismUsageType] ADD CONSTRAINT [PK_ContactMechanismUsageType] PRIMARY KEY CLUSTERED  ([ContactMechanismUsageTypeID])
GO
ALTER TABLE [Enterprise].[ContactMechanismUsageType] ADD CONSTRAINT [AK_ContactMechanismUsageType_Name] UNIQUE NONCLUSTERED  ([Name])
GO
ALTER TABLE [Enterprise].[ContactMechanismUsageType] ADD CONSTRAINT [FK_ContactMechanismUsageType_ParentContactMechanism] FOREIGN KEY ([ParentContactMechanismUsageTypeID]) REFERENCES [Enterprise].[ContactMechanismUsageType] ([ContactMechanismUsageTypeID])
GO
EXEC sp_addextendedproperty N'MS_Description', N'Defines the seed data for contact usage. Such as Personal, Work, or Account Recovery.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsageType', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Backing Field', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsageType', 'COLUMN', N'ContactMechanismUsageTypeID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The usage type name', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsageType', 'COLUMN', N'Name'
GO
