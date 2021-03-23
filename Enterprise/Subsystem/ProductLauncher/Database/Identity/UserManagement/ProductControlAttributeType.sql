CREATE TYPE [UserManagement].[ProductControlAttributeType] AS TABLE(
	[ControlAttributeId] [int] NULL,
	[ControlId] [int] NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[VALUE] [nvarchar](50) NULL
)
GO