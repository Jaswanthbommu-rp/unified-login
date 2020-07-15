
CREATE TABLE [Enterprise].[PropertyInstanceMapping](
	[PropertyInstanceMappingID] [bigint] IDENTITY(1,1) NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[PropertyInstanceId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[FromDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
	[ThruDate] [datetime] NULL,
	[Active] BIT NOT NULL DEFAULT 1

 CONSTRAINT [PK_PropertyInstanceMapping] PRIMARY KEY CLUSTERED
 (
	[PropertyInstanceMappingID] ASC
),
 CONSTRAINT [FK_PropertyInstanceMapping_PropertyInstance] FOREIGN KEY([PropertyInstanceId]) REFERENCES [Enterprise].[PropertyInstance] ([PropertyInstanceId]) ,
    CONSTRAINT [FK_PropertyInstanceMapping_Persona] FOREIGN KEY([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ,
 CONSTRAINT [FK_PropertyInstanceMapping_Product] FOREIGN KEY([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]),
) ON [PRIMARY]
GO




