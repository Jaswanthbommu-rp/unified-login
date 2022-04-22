CREATE TABLE [Security].[ADGroupRole](
	[ADGroupRoleId] [int] IDENTITY(1,1) NOT NULL,
	[ADGroupId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ADGroupRoleId] PRIMARY KEY CLUSTERED 
(
	[ADGroupRoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[ADGroupRole]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupRole_ADGroup] FOREIGN KEY([ADGroupId])
REFERENCES [Security].[ADGroup] ([ADGroupId])
GO

ALTER TABLE [Security].[ADGroupRole] CHECK CONSTRAINT [FK_ADGroupRole_ADGroup]
GO

ALTER TABLE [Security].[ADGroupRole]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupRole_Role] FOREIGN KEY([RoleId])
REFERENCES [Security].[Role] ([RoleId])
GO

ALTER TABLE [Security].[ADGroupRole] CHECK CONSTRAINT [FK_ADGroupRole_Role]
GO

ALTER TABLE [Security].[ADGroupRole]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupRole_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])

GO
