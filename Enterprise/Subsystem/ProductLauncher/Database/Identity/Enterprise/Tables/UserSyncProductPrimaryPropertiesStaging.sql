CREATE TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging]
(
	[UserSyncPrimaryPropertyId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[PropertyInstanceId] [bigint] NOT NULL,
	[ProductPropertyId] [bigint] NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NOT NULL,
	[ModifiedDate] [datetime2](7) NULL,	
 CONSTRAINT [PK_UserSyncMatched_PrimaryProperty] PRIMARY KEY CLUSTERED 
(
	[UserSyncPrimaryPropertyId] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging]  WITH CHECK ADD  CONSTRAINT [FK_User_Sync_Modified_By] FOREIGN KEY([ModifiedBy])
REFERENCES [Ident].[UserLogin] ([UserId])
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging] CHECK CONSTRAINT [FK_User_Sync_Modified_By]
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging]  WITH CHECK ADD  CONSTRAINT [FK_User_Sync_Persona] FOREIGN KEY([PersonaId])
REFERENCES [Person].[Persona] ([PersonaId])
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging] CHECK CONSTRAINT [FK_User_Sync_Persona]
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging]  WITH CHECK ADD  CONSTRAINT [FK_User_Sync_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging] CHECK CONSTRAINT [FK_User_Sync_Product]
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging]  WITH CHECK ADD  CONSTRAINT [FK_User_Sync_Property_Instance] FOREIGN KEY([PropertyInstanceId])
REFERENCES [Enterprise].[PropertyInstance] ([PropertyInstanceId])
GO

ALTER TABLE [Enterprise].[UserSyncProductPrimaryPropertiesStaging] CHECK CONSTRAINT [FK_User_Sync_Property_Instance]
GO