
CREATE TABLE [Enterprise].[PropertyMapping](
	[PropertyMappingID] [bigint] IDENTITY(1,1) NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[PropertyId] [bigint] NOT NULL,
	[ProductId] [int] NOT NULL,
	[FromDate] [datetime] NOT NULL,
	[ThruDate] [datetime] NULL,
 CONSTRAINT [PK_PropertyMapping] PRIMARY KEY CLUSTERED
 (
	[PropertyMappingID] ASC
),
 CONSTRAINT [FK_PropertyMapping_Persona] FOREIGN KEY([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ,
 CONSTRAINT [FK_PropertyMapping_Product] FOREIGN KEY([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]),
) ON [PRIMARY]
GO




