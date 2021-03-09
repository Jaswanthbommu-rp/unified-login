CREATE TABLE [Enterprise].[PropertyInstance]
(
	[PropertyInstanceId] [BIGINT] NOT NULL IDENTITY, 
	[Name] NVARCHAR(250) NOT NULL,
    [Address] NVARCHAR(200) NOT NULL,
	[City] NVARCHAR(60) NOT NULL,
	[State] NVARCHAR(20) NULL,
	[PostalCode] NVARCHAR(25) NULL,
	[Country] NVARCHAR(25) NULL,
	[County] NVARCHAR(60) NULL,
	[Latitude] DECIMAL(9, 6) NULL,
	[Longitude] DECIMAL(9, 6) NULL, 
    [InstanceId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
	[CustomerPropertyId] BIGINT NOT NULL,
	[Domain] NVARCHAR(50) NOT NULL,
	[IsDeleted] TinyInt NOT NULL DEFAULT 0,
	[ThruDate] Datetime2 NULL,
	[IsActive] TinyInt NOT NULL DEFAULT 1,
    CONSTRAINT [PK_PropertyInstance] PRIMARY KEY ([PropertyInstanceId]),
)

GO

CREATE INDEX [IX_PropertyInstance_InstanceId] ON [Enterprise].[PropertyInstance] ([InstanceId])
