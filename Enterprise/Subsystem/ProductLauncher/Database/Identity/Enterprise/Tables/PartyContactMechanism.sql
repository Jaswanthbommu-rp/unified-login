CREATE TABLE [Enterprise].[PartyContactMechanism]
(
	[PartyContactMechanismId] BIGINT NOT NULL IDENTITY , 
    [PartyId] BIGINT NOT NULL, 
    [ContactMechanismId] INT NOT NULL, 
    [FromDate] DATETIME NOT NULL, 
    [ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_PartyContactMechanism] PRIMARY KEY NONCLUSTERED ([PartyContactMechanismId]), 
    CONSTRAINT [FK_PartyContactMechanism_ContactMechanism] FOREIGN KEY (ContactMechanismId) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_PartyContactMechanism_Party] FOREIGN KEY (PartyId) REFERENCES Enterprise.Party(PartyId) ON DELETE CASCADE ON UPDATE CASCADE
)

GO
-- TODO: Fix this Unique Constraint
CREATE CLUSTERED INDEX [CLI_PartyContactMechanism_PartyId_ContactMechanismId_FromDate] ON [Enterprise].[PartyContactMechanism] ([PartyId], [ContactMechanismId], ThruDate)
go
CREATE INDEX [IX_PartyContactMechanism_Comp01] 
ON [Enterprise].[PartyContactMechanism]([ContactMechanismId]) 
INCLUDE([PartyContactMechanismId], [PartyId], [FromDate], [ThruDate]);
GO 
CREATE NONCLUSTERED INDEX IDX_PartyContactMechanism_Comp01 ON [Enterprise].[PartyContactMechanism]
([ThruDate]
) INCLUDE([PartyContactMechanismId], [FromDate]);
GO
CREATE NONCLUSTERED INDEX [IX_CLI_NEW] ON [Enterprise].[PartyContactMechanism]
(
	[ThruDate] ASC,
	[PartyId] ASC,
	[ContactMechanismId] ASC,
	[FromDate] DESC
)