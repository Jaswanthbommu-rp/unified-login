CREATE TABLE [Enterprise].[PersonaProductPropertiesSyncHistory]
(
	[PersonaProductPropertiesSyncHistoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[ProductPropertiesSyncDate] [datetime2](7) NOT NULL,
CONSTRAINT [PK_PersonaProductPropertiesSyncHistory] PRIMARY KEY CLUSTERED 
(
	[PersonaProductPropertiesSyncHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Enterprise].[PersonaProductPropertiesSyncHistory]  WITH CHECK ADD  CONSTRAINT [FK_PersonaProductPropertiesSyncHistory_Persona] FOREIGN KEY([PersonaId])
REFERENCES [Person].[Persona] ([PersonaId])
GO

ALTER TABLE [Enterprise].[PersonaProductPropertiesSyncHistory] CHECK CONSTRAINT [FK_PersonaProductPropertiesSyncHistory_Persona]
GO

ALTER TABLE [Enterprise].[PersonaProductPropertiesSyncHistory]  WITH CHECK ADD  CONSTRAINT [FK_PersonaProductPropertiesSyncHistory_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO

ALTER TABLE [Enterprise].[PersonaProductPropertiesSyncHistory] CHECK CONSTRAINT [FK_PersonaProductPropertiesSyncHistory_Product]
GO
CREATE NONCLUSTERED INDEX [IDX_PersonaProductPropertiesSyncHistory_PersonaId] ON [Enterprise].[PersonaProductPropertiesSyncHistory] ([PersonaId])
GO
