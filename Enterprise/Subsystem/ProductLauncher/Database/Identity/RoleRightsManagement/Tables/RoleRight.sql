CREATE TABLE [Security].[RoleRight](
	[RoleRightId] BIGINT IDENTITY(1,1) NOT NULL,
	[RoleId] INT NOT NULL,
	[RightId] INT NOT NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityRoleRight] PRIMARY KEY CLUSTERED 
(
	[RoleRightId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[RoleRight] ADD  CONSTRAINT [DF_SecurityRightSecurityRole_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[RoleRight] ADD  CONSTRAINT [DF_SecurityRightSecurityRole_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[RoleRight]  WITH CHECK ADD  CONSTRAINT [FK_SecurityRightSecurityRole_SecurityRight] FOREIGN KEY([RightId])
REFERENCES [Security].[Right] ([RightId])
GO

ALTER TABLE [Security].[RoleRight] CHECK CONSTRAINT [FK_SecurityRightSecurityRole_SecurityRight]
GO

ALTER TABLE [Security].[RoleRight]  WITH CHECK ADD  CONSTRAINT [FK_SecurityRightSecurityRole_SecurityRole] FOREIGN KEY([RoleId])
REFERENCES [Security].[Role] ([RoleId])
GO

ALTER TABLE [Security].[RoleRight] CHECK CONSTRAINT [FK_SecurityRightSecurityRole_SecurityRole]
GO
CREATE NONCLUSTERED INDEX [IX_Security_RoleRight_RightID]
ON [Security].[RoleRight] ([RightId])
INCLUDE ([RoleId])
GO
CREATE NONCLUSTERED INDEX [NCIX_Security_RoleRight_RoleID]
ON [Security].[RoleRight] ([RoleId])
GO