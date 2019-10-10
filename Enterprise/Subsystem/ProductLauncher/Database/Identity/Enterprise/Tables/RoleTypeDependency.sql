CREATE TABLE [Enterprise].[RoleTypeDependency](
	[ParentRoleTypeId] [int] NULL,
	[ChildRoleTypeId] [int] NULL,
	[SortOrder] [int] NULL
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[RoleTypeDependency]  WITH CHECK ADD  CONSTRAINT [FK_RoleTypeDependency_RoleType] FOREIGN KEY([ParentRoleTypeId])
REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO

ALTER TABLE [Enterprise].[RoleTypeDependency] CHECK CONSTRAINT [FK_RoleTypeDependency_RoleType]
GO

