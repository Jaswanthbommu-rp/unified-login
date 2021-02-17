CREATE TYPE [UserManagement].[ProductControlType] AS TABLE(
	[ControlId] [int] NULL,
	[ParentControlId] [int] NULL,
	[ProductPageId] [int] NULL,
	[ControlTypeId] [int] NULL,
	[UIId] [nvarchar](510) NULL,
	[DisplayName] [nvarchar](510) NULL,
	[DataSource] [ntext] NULL,
	[Sequence] [tinyint] NULL
)
GO


