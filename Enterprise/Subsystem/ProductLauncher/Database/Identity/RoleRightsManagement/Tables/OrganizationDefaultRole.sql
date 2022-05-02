CREATE TABLE [Security].[OrganizationDefaultRole](
	[OrganizationDefaultRoleId] [bigint] IDENTITY(1,1) NOT NULL,
	[RoleId] INT NOT NULL,
	[OrgPartyId] [bigint] NOT NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityOrganizationDefaultRole] PRIMARY KEY CLUSTERED 
(
	[OrganizationDefaultRoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[OrganizationDefaultRole] ADD  CONSTRAINT [DF_SecurityOrganizationDefaultRole_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[OrganizationDefaultRole] ADD  CONSTRAINT [DF_SecurityOrganizationDefaultRole_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[OrganizationDefaultRole]  WITH CHECK ADD  CONSTRAINT [FK_SecurityOrganizationDefaultRole_Role] FOREIGN KEY([RoleId])
REFERENCES [Security].[Role] ([RoleId])
GO

ALTER TABLE [Security].[OrganizationDefaultRole]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationDefaultRole_Party] FOREIGN KEY([OrgPartyID])
REFERENCES [Enterprise].[Organization] ([PartyId])
ON UPDATE CASCADE

GO

CREATE NONCLUSTERED INDEX [IDX_OrganizationDefaultRole_OrgPartyId]
ON [Security].[OrganizationDefaultRole] ([OrgPartyId])

GO
