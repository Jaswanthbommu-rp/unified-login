CREATE TABLE [Enterprise].[PersonaSuggestedProperties](
	[PersonaSuggestedPropertyId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[PropertyInstanceId] [bigint] NOT NULL,
	[ProductPropertyId] [bigint] NOT NULL,
	[ModifiedBy] [bigint] NOT NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[CreateDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Persona_Suggested_Property] PRIMARY KEY CLUSTERED 
(
	[PersonaSuggestedPropertyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties]  WITH CHECK ADD  CONSTRAINT [FK_Modified_By] FOREIGN KEY([ModifiedBy])
REFERENCES [Ident].[UserLogin] ([UserId])
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties] CHECK CONSTRAINT [FK_Modified_By]
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties]  WITH CHECK ADD  CONSTRAINT [FK_Persona] FOREIGN KEY([PersonaId])
REFERENCES [Person].[Persona] ([PersonaId])
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties] CHECK CONSTRAINT [FK_Persona]
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties]  WITH CHECK ADD  CONSTRAINT [FK_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties] CHECK CONSTRAINT [FK_Product]
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties]  WITH CHECK ADD  CONSTRAINT [FK_Property_Instance] FOREIGN KEY([PropertyInstanceId])
REFERENCES [Enterprise].[PropertyInstance] ([PropertyInstanceId])
GO

ALTER TABLE [Enterprise].[PersonaSuggestedProperties] CHECK CONSTRAINT [FK_Property_Instance]
GO
