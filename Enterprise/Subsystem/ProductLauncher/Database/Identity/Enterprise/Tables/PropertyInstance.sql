CREATE TABLE [Enterprise].[PropertyInstance]
(
	[PropertyInstanceId] [BIGINT] NOT NULL IDENTITY, 
	[Name] NVARCHAR(250) NOT NULL,
    [Address] NVARCHAR(250) NOT NULL,
	[City] NVARCHAR(250) NOT NULL,
	[State] NVARCHAR(150) NULL,
	[PostalCode] NVARCHAR(50) NOT NULL,
	[Country] NVARCHAR(250) NOT NULL,
	[County] NVARCHAR(250) NULL,
	[Latitude] NVARCHAR(100) NULL,
	[Longitude] NVARCHAR(100) NULL, 
    [InstanceId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
    CONSTRAINT [PK_PropertyInstance] PRIMARY KEY ([PropertyInstanceId]),
)
