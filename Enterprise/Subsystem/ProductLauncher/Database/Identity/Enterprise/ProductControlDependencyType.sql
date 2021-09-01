CREATE TYPE [UserManagement].[ProductControlDependencyType] AS TABLE(
	[ControlDependencyId] [int] NULL,
	[MasterControlId] [int] NOT NULL,
	[SlaveControlID] [int] NOT NULL,
	[MasterControlValue] [nvarchar](50) NOT NULL,
	[ComparatorId] [tinyint] NOT NULL
)
GO
