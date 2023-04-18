

CREATE TABLE [Enterprise].[UserRelationShip](
	[Id] [int] NOT NULL,
	[SortIndex] [int] NOT NULL,
	[UserRelationshipName] [nvarchar](100) NULL,
	[Description] [nvarchar](255) NULL,
	[PartyRoleTypeId] [int] NOT NULL,
	[ThirdPartyRelationshipId] [tinyint] NOT NULL
 CONSTRAINT [PK_EnterpriseUserRelationShip] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Enterprise].[UserRelationShip]  WITH CHECK ADD  CONSTRAINT [FK_EnterpriseUserRelationShip_PartyRoleTypeId] FOREIGN KEY([PartyRoleTypeId])
REFERENCES [Enterprise].[RoleType] ([PartyRoleTypeId])
GO
ALTER TABLE [Enterprise].[UserRelationShip] CHECK CONSTRAINT [FK_EnterpriseUserRelationShip_PartyRoleTypeId]
GO
ALTER TABLE [Enterprise].[UserRelationShip]  WITH CHECK ADD  CONSTRAINT [FK_EnterpriseUserRelationShip_ThirdPartyRelationshipId] FOREIGN KEY([ThirdPartyRelationshipId])
REFERENCES [Enterprise].[ThirdPartyRelationship] ([ThirdPartyRelationshipId])
GO
ALTER TABLE [Enterprise].[UserRelationShip] CHECK CONSTRAINT [FK_EnterpriseUserRelationShip_ThirdPartyRelationshipId]
GO
ALTER TABLE [Enterprise].[UserRelationShip] ADD CONSTRAINT [UQ_PartyRoleTypeId_ThirdPartyRelationshipId] UNIQUE(PartyRoleTypeId, ThirdPartyRelationshipId)
GO