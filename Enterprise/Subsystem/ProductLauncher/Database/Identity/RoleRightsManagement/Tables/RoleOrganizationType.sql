CREATE TABLE [Security].[RoleOrganizationType](
	[RoleOrganizationTypeId] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [int] NOT NULL,
	[OrganizationTypeId] [int] NOT NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityRoleOrganizationType] PRIMARY KEY CLUSTERED 
(
	[RoleOrganizationTypeId] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[RoleOrganizationType] ADD  CONSTRAINT [DF_SecurityRoleOrganizationType_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[RoleOrganizationType] ADD  CONSTRAINT [DF_SecurityRoleOrganizationType_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[RoleOrganizationType]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationTypeRoleOrganizationType_OrganizationType] FOREIGN KEY([OrganizationTypeId])
REFERENCES [Enterprise].[OrganizationType] ([OrganizationTypeId])
GO

ALTER TABLE [Security].[RoleOrganizationType] CHECK CONSTRAINT [FK_OrganizationTypeRoleOrganizationType_OrganizationType]
GO

ALTER TABLE [Security].[RoleOrganizationType]  WITH CHECK ADD  CONSTRAINT [FK_SecurityRoleOrganizationType_SecurityRole] FOREIGN KEY([RoleId])
REFERENCES [Security].[Role] ([RoleId]) ON DELETE CASCADE
GO

ALTER TABLE [Security].[RoleOrganizationType] CHECK CONSTRAINT [FK_SecurityRoleOrganizationType_SecurityRole]
GO
