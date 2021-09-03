CREATE TABLE [Enterprise].[PersonaProductError]
(
	[PersonaProductErrorId]		BIGINT				NOT NULL  IDENTITY(1,1), 
    [PersonaId]					BIGINT				NOT NULL,
    CONSTRAINT [PK_PersonaProductError] PRIMARY KEY (PersonaProductErrorId), 
    CONSTRAINT [FK_PersonaProductError_PersonaId] FOREIGN KEY (PersonaId) REFERENCES [Person].[Persona]([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [UC_PersonaProductError_PersonaId] UNIQUE  (PersonaId))
GO

CREATE NONCLUSTERED INDEX [IX_Enterprise_PersonaProductError_PersonaId]
	ON [Enterprise].[PersonaProductError] ([PersonaId])
GO