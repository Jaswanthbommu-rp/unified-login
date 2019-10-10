CREATE TABLE [Enterprise].[ContactMechanismBoundary]
(
[ContactMechanismBoundaryId] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismId] [int] NOT NULL,
[GeographicBoundaryId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL,
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [PK_PostalAddressBoundary] PRIMARY KEY CLUSTERED  ([ContactMechanismBoundaryId])
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [AK_ContactMechanismBoundary_ContactMechanismId_GeographicBoundaryId] UNIQUE NONCLUSTERED  ([ContactMechanismId], [GeographicBoundaryId], [ThruDate])
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [FK_ContactMechanismBoundary_ContactMechanism] FOREIGN KEY ([ContactMechanismId]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ContactMechanismBoundary] ADD CONSTRAINT [FK_ContactMechanismBoundary_GeographicBoundary] FOREIGN KEY ([GeographicBoundaryId]) REFERENCES [Enterprise].[GeographicBoundary] ([GeographicBoundaryId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table joins geographic boundaries to a contact mechanism.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'ContactMechanismBoundaryId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The contact mechanism id that this boundary definition applies to. There may be multiple rows for the same contact mechanism, such as a city,state, zip combination.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'ContactMechanismId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The date this boundary became active.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'FromDate'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The geographic boundary that applies to this boundary. ', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'GeographicBoundaryId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The date this boundary was deactivated.', 'SCHEMA', N'Enterprise', 'TABLE', N'ContactMechanismBoundary', 'COLUMN', N'ThruDate'
GO
