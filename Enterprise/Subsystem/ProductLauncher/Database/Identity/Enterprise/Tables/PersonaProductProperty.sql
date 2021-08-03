CREATE TABLE [Enterprise].[PersonaProductProperty]
(
	[PersonaProductPropertyId]	BIGINT				NOT NULL  IDENTITY(1,1), 
    [PersonaId]					BIGINT				NOT NULL, 
    [ProductId]					INT					NOT NULL,
	[PropertyId]				VARCHAR(100)		NOT NULL,
	[PropertyInstanceId]		VARCHAR(200)		NULL,
	[CreateDate]				DATETIME2			NOT NULL,
    CONSTRAINT [PK_PersonaProductProperty] PRIMARY KEY (PersonaProductPropertyId), 
    CONSTRAINT [FK_PersonaProductProperty_PersonaId] FOREIGN KEY (PersonaId) REFERENCES [Person].[Persona]([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_PersonaProductProperty_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId])
)
GO

CREATE NONCLUSTERED INDEX [IX_Enterprise_PersonaProductProperty_PersonaId]
	ON [Enterprise].[PersonaProductProperty] ([PersonaId])
GO