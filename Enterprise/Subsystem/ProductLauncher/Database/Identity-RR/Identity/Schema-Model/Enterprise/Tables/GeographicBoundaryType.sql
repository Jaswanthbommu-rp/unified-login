CREATE TABLE [Enterprise].[GeographicBoundaryType]
(
[GeographicBoundaryTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL
)
GO
ALTER TABLE [Enterprise].[GeographicBoundaryType] ADD CONSTRAINT [PK_GeographicBoundaryType] PRIMARY KEY CLUSTERED  ([GeographicBoundaryTypeId])
GO
ALTER TABLE [Enterprise].[GeographicBoundaryType] ADD CONSTRAINT [AK_GeographicBoundaryType_Name] UNIQUE NONCLUSTERED  ([Name])
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table defines the different kinds of Geographic Boundaries. Please note: Geographic "locations" are NOT the same as boundaries. Do not store Geographic "location" types here.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundaryType', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundaryType', 'COLUMN', N'GeographicBoundaryTypeId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The name of the type that will be used to group things under. For example, State, Region, Country as it relates to a geographic boundary.', 'SCHEMA', N'Enterprise', 'TABLE', N'GeographicBoundaryType', 'COLUMN', N'Name'
GO
