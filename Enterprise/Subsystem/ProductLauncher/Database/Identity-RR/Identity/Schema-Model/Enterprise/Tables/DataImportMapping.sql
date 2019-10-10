CREATE TABLE [Enterprise].[DataImportMapping]
(
[DataImportMappingId] [int] NOT NULL IDENTITY(1, 1),
[DataImportApplicationId] [int] NOT NULL,
[SourceId] [nvarchar] (100) NOT NULL,
[PartyId] [bigint] NOT NULL,
[DateCreated] [datetime] NOT NULL CONSTRAINT [df_DataImportMapping_DateCreated] DEFAULT (getutcdate())
)
GO
ALTER TABLE [Enterprise].[DataImportMapping] ADD CONSTRAINT [PK_DataImportMapping] PRIMARY KEY CLUSTERED  ([DataImportMappingId])
GO
ALTER TABLE [Enterprise].[DataImportMapping] ADD CONSTRAINT [FK_DataImportMapping_DataImportApplication] FOREIGN KEY ([DataImportApplicationId]) REFERENCES [Enterprise].[DataImportApplication] ([DataImportApplicationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[DataImportMapping] ADD CONSTRAINT [FK_DataImportMapping_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
