CREATE TABLE [Enterprise].[GeographicBoundary]
(
[GeographicBoundaryId] [int] NOT NULL IDENTITY(1, 1),
[GeographicBoundaryTypeId] [int] NOT NULL,
[Name] [nvarchar] (50) NOT NULL,
[GeographicBoundaryCode] [nvarchar] (50) NULL,
[Abbreviation] [nvarchar] (10) NULL
)
GO
ALTER TABLE [Enterprise].[GeographicBoundary] ADD CONSTRAINT [PK_GeographicBoundary] PRIMARY KEY CLUSTERED  ([GeographicBoundaryId])
GO
ALTER TABLE [Enterprise].[GeographicBoundary] ADD CONSTRAINT [AK_GeographicBoundary_Name_GeographicBoundaryTypeId_GeographicBoundaryCode] UNIQUE NONCLUSTERED  ([Name], [GeographicBoundaryTypeId], [Abbreviation])
GO
ALTER TABLE [Enterprise].[GeographicBoundary] ADD CONSTRAINT [FK_GeographicBoundary_GeographicBoundaryType] FOREIGN KEY ([GeographicBoundaryTypeId]) REFERENCES [Enterprise].[GeographicBoundaryType] ([GeographicBoundaryTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'If the geographic boundary has a known standard abbreviation, then it would go here, such as TX for Texas.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'Abbreviation'
GO
EXEC sp_addextendedproperty N'MS_Description', N'If the geographic boundary has a specific code, this is where it goes.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'GeographicBoundaryCode'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Backing Field', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'GeographicBoundaryId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Geographic Boundary Type, which can be any type defined by the GeographicBoundaryType table.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'GeographicBoundaryTypeId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The named value of this Geographic Boundary, such as 75078 in the case of a ZipCode, or Texas if it''s a state.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundary', 'COLUMN', N'Name'
GO
