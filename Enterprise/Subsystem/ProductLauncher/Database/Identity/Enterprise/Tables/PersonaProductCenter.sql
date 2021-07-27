CREATE TABLE [Enterprise].[PersonaProductCenter]
(
	[PersonaProductCenterId]	BIGINT		NOT NULL					CONSTRAINT [PK_PersonaProductCenter_PersonaProductCenterId] PRIMARY KEY IDENTITY(1, 1),
	[PersonaId]					BIGINT		NOT NULL,
	[ProductCenterId]			INT			NOT NULL,
	[CacheExpirationDate]		DATETIME	NOT NULL,
	[CreatedDate]				DATETIME	NOT NULL					CONSTRAINT [DF_PersonaProductCenter_CreatedDate] DEFAULT (GETUTCDATE()),
	[ModifiedDate]				DATETIME	NULL,
	CONSTRAINT [FK_ProductCenter_Persona]								FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona]([PersonaId]),
	CONSTRAINT [FK_ProductCenter_ProductCenter]							FOREIGN KEY ([ProductCenterId]) REFERENCES [Enterprise].[ProductCenter]([ProductCenterId]),
	INDEX	   [IX_UQ_PersonaProductCenter_PersonaId_ProductCenterId]   UNIQUE NONCLUSTERED ([PersonaId], [ProductCenterId]),
	INDEX	   [IX_PersonaProductCenter_ProductCenterId_PersonaId]		NONCLUSTERED ([ProductCenterId], [PersonaId])
)