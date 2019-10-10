CREATE TABLE [Enterprise].[PartyContactMechanism]
(
[PartyContactMechanismId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[ContactMechanismId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL,
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[PartyContactMechanism] ADD CONSTRAINT [PK_PartyContactMechanism] PRIMARY KEY NONCLUSTERED  ([PartyContactMechanismId])
GO
CREATE CLUSTERED INDEX [CLI_PartyContactMechanism_PartyId_ContactMechanismId_FromDate] ON [Enterprise].[PartyContactMechanism] ([PartyId], [ContactMechanismId], [ThruDate])
GO
ALTER TABLE [Enterprise].[PartyContactMechanism] ADD CONSTRAINT [FK_PartyContactMechanism_ContactMechanism] FOREIGN KEY ([ContactMechanismId]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[PartyContactMechanism] ADD CONSTRAINT [FK_PartyContactMechanism_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
