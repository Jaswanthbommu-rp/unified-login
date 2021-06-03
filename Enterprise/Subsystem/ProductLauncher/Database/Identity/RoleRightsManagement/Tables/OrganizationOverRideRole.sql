CREATE TABLE [Security].[OrganizationOverRideRole](
	[OrganizationOverRideRoleId] [bigint] IDENTITY(1,1) NOT NULL,
	[RoleId] INT NOT NULL,
	[OrgPartyId] [bigint] NOT NULL,	
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityOrganizationOverRideRole] PRIMARY KEY CLUSTERED 
(
	[OrganizationOverRideRoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[OrganizationOverRideRole] ADD  CONSTRAINT [DF_SecurityOrganizationOverRideRole_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[OrganizationOverRideRole] ADD  CONSTRAINT [DF_SecurityOrganizationOverRideRole_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[OrganizationOverRideRole]  WITH CHECK ADD  CONSTRAINT [FK_SecurityOrganizationOverRideRole_Role] FOREIGN KEY([RoleId])
REFERENCES [Security].[Role] ([RoleId])
GO

ALTER TABLE [Security].[OrganizationOverRideRole]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationOverRideRole_Party] FOREIGN KEY([OrgPartyID])
REFERENCES [Enterprise].[Organization] ([PartyId])
ON UPDATE CASCADE
GO

CREATE NONCLUSTERED INDEX [IX_Security_OrganizationOverRideRole_OrgPartyId]
ON [Security].[OrganizationOverRideRole] ([OrgPartyId])

GO