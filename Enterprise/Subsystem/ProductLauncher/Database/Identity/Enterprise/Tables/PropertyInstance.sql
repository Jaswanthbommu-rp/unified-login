CREATE TABLE [Enterprise].[PropertyInstance]
(
	[PropertyInstanceId] [BIGINT] NOT NULL IDENTITY, 
	[Name] NVARCHAR(250) NOT NULL,
    [Address] NVARCHAR(250) NOT NULL,
	[City] NVARCHAR(200) NOT NULL,
	[State] NVARCHAR(10) NULL,
	[PostalCode] NVARCHAR(50) NOT NULL,
	[Country] NVARCHAR(10) NOT NULL,
	[County] NVARCHAR(60) NULL,
	[Latitude] DECIMAL(9, 6) NULL,
	[Longitude] DECIMAL(9, 6) NULL, 
    [InstanceId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
    CONSTRAINT [PK_PropertyInstance] PRIMARY KEY ([PropertyInstanceId]),
		
)

GO

CREATE INDEX [IX_PropertyInstance_InstanceId] ON [Enterprise].[PropertyInstance] ([InstanceId])
