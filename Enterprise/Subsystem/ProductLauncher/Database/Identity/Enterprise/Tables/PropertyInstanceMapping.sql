CREATE TABLE [Enterprise].[PropertyInstanceMapping](
	[PropertyInstanceMappingID] [BIGINT] NOT NULL IDENTITY, 
	[PersonaId] [bigint] NOT NULL,
	[PropertyInstanceId] [bigint] NOT NULL,
	[ProductId] [int] NOT NULL,
	[FromDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
	[ThruDate] [datetime] NULL,
	[Active] BIT NOT NULL DEFAULT 1

 CONSTRAINT [FK_PropertyInstanceMapping_PropertyInstance] FOREIGN KEY([PropertyInstanceId]) REFERENCES [Enterprise].[PropertyInstance] ([PropertyInstanceId]) ,
    CONSTRAINT [FK_PropertyInstanceMapping_Persona] FOREIGN KEY([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ,
 CONSTRAINT [FK_PropertyInstanceMapping_Product] FOREIGN KEY([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]), 
    CONSTRAINT [PK_PropertyInstanceMapping] PRIMARY KEY ([PropertyInstanceMappingID]),
) ON [PRIMARY]