CREATE TABLE [Enterprise].[ContactMechanismUsage]
(
[ContactMechanismUsageID] [int] NOT NULL IDENTITY(1, 1),
[PartyContactMechanismID] [bigint] NOT NULL,
[ContactMechanismUsageTypeID] [int] NOT NULL
)
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [PK_ContactMechanismUsage] PRIMARY KEY CLUSTERED  ([ContactMechanismUsageID])
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [AK_ContactMechanismUsage_PartyContactMechanismId_UsageId] UNIQUE NONCLUSTERED  ([PartyContactMechanismID], [ContactMechanismUsageTypeID])
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [FK_ContactMechanismUsage_ContactMechanismUsageType] FOREIGN KEY ([ContactMechanismUsageTypeID]) REFERENCES [Enterprise].[ContactMechanismUsageType] ([ContactMechanismUsageTypeID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ContactMechanismUsage] ADD CONSTRAINT [FK_ContactMechanismUsage_PartyContactMechanism] FOREIGN KEY ([PartyContactMechanismID]) REFERENCES [Enterprise].[PartyContactMechanism] ([PartyContactMechanismId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'A contact mechanism usage.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Backing Field', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', 'COLUMN', N'ContactMechanismUsageID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'References ContactMechanismUsageType.ContactMechanismUsageTypeID', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', 'COLUMN', N'ContactMechanismUsageTypeID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'References PartyContactMechanism.PartyContactMechanismID', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismUsage', 'COLUMN', N'PartyContactMechanismID'
GO
