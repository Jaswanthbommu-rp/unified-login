CREATE TABLE [Security].[OrganizationOverRideRight](
	[OrganizationOverRideRightId] [bigint] IDENTITY(1,1) NOT NULL,
	[RightId] INT NOT NULL,
	[OrgPartyId] [bigint] NOT NULL,
	[VisibilityStatusId] [int] NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityOrganizationOverRideRight] PRIMARY KEY CLUSTERED 
(
	[OrganizationOverRideRightId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[OrganizationOverRideRight] ADD  CONSTRAINT [DF_SecurityOrganizationOverRideRight_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[OrganizationOverRideRight] ADD  CONSTRAINT [DF_SecurityOrganizationOverRideRight_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[OrganizationOverRideRight]  WITH CHECK ADD  CONSTRAINT [FK_SecurityOrganizationOverRideRighte_Right] FOREIGN KEY([RightId])
REFERENCES [Security].[Right] ([RightId])
GO

ALTER TABLE [Security].[OrganizationOverRideRight]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationOverRideRight_Party] FOREIGN KEY([OrgPartyID])
REFERENCES [Enterprise].[Organization] ([PartyId])
ON UPDATE CASCADE
GO
ALTER TABLE [Security].[OrganizationOverRideRight]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationOverRideRight_VisibilityStatus_StatusType] FOREIGN KEY([VisibilityStatusId])
REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
CREATE NONCLUSTERED INDEX [IX_Security_OrganizationOverRideRight_OrgPartyId]
ON [Security].[OrganizationOverRideRight] ([OrgPartyId])

GO