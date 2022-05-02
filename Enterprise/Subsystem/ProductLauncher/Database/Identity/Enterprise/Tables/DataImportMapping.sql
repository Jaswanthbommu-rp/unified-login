CREATE TABLE [Enterprise].[DataImportMapping]
(
	[DataImportMappingId] INT NOT NULL IDENTITY,
	[DataImportApplicationId] INT NOT NULL,
	[SourceId] NVARCHAR(100) NOT NULL,
	[PartyId] BIGINT NOT NULL, 
	[DateCreated] DATETIME NOT NULL CONSTRAINT df_DataImportMapping_DateCreated DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_DataImportMapping] PRIMARY KEY ([DataImportMappingId]), 
    CONSTRAINT [FK_DataImportMapping_DataImportApplication] FOREIGN KEY ([DataImportApplicationId]) REFERENCES [Enterprise].[DataImportApplication]([DataImportApplicationId]) ON UPDATE CASCADE ON DELETE CASCADE, 
    CONSTRAINT [FK_DataImportMapping_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party]([PartyId]) ON UPDATE CASCADE ON DELETE CASCADE
)
GO
CREATE INDEX IDX_DataImportMapping ON Enterprise.DataImportMapping (DataImportApplicationId) INCLUDE (SourceId, PartyId)
GO


CREATE NONCLUSTERED INDEX [IDX_DataImportMapping_PartyId]
			ON [Enterprise].[DataImportMapping] ([DataImportApplicationId],[PartyId]) INCLUDE ([SourceId])
GO
