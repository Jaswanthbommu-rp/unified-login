CREATE TABLE [Security].[Role](
	[RoleId] INT IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](255) NOT NULL,
	[ShortName] [nvarchar](255) ,
	[Description] [nvarchar](255) NULL,	
	[RoleTypeID] [int] NOT NULL,
	[OrgPartyID] [bigint] NULL,
	[ProductId] [int] NOT NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityRole] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[Role] ADD  CONSTRAINT [DF_SecurityRole_CreatedBy]  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [CreatedBy]
GO

ALTER TABLE [Security].[Role] ADD  CONSTRAINT [DF_SecurityRole_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[Role]  WITH CHECK ADD  CONSTRAINT [FK_ProductRole_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO

ALTER TABLE [Security].[Role] CHECK CONSTRAINT [FK_ProductRole_Product]
GO

ALTER TABLE [Security].[Role]  WITH CHECK ADD  CONSTRAINT [FK_Role_Party] FOREIGN KEY([OrgPartyID])
REFERENCES [Enterprise].[Organization] ([PartyId])
ON UPDATE CASCADE
GO

ALTER TABLE [Security].[Role]  WITH CHECK ADD  CONSTRAINT [FK_Role_RoleType] FOREIGN KEY([RoleTypeID])
REFERENCES [Security].[RoleType] ([RoleTypeId])
GO
CREATE NONCLUSTERED INDEX [IX_SecurityRole_Productid]
ON [Security].[Role] ([ProductId])
INCLUDE ([RoleId],[OrgPartyID])
GO
CREATE NONCLUSTERED INDEX [IX_Security_Role_OrgPartyID]
ON [Security].[Role] ([OrgPartyID])
INCLUDE ([RoleId],[RoleName],[ShortName])

GO