CREATE TABLE [Enterprise].[PersonaConfiguration] (
    [PersonaConfigurationId] BIGINT   IDENTITY (1, 1) NOT NULL,
    [PersonaId]              BIGINT   NOT NULL,
    [ConfigurationId]        INT      NOT NULL,
    [ProductId]              INT      NOT NULL,
    [UsePrimaryProperties] TINYINT NOT NULL DEFAULT 0, 
    [FromDate]               DATETIME DEFAULT (getutcdate()) NOT NULL,
    [ThruDate]               DATETIME NULL,
    [StatusTypeId] INT NOT NULL DEFAULT 0, 
    [IsFavorite] TINYINT NOT NULL DEFAULT 0,
    [ProductDeactivationDate] DATETIME NULL,
    CONSTRAINT [PK_PersonaConfiguration] PRIMARY KEY CLUSTERED ([PersonaConfigurationId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_PersonaConfiguration_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PersonaConfiguration_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PersonaConfiguration_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
);
GO
ALTER TABLE [Enterprise].[PersonaConfiguration]
ADD CONSTRAINT FK_PersonaConfiguration_Product
FOREIGN KEY (ProductId) REFERENCES [Enterprise].[Product] ([ProductId])  ON DELETE CASCADE;

GO
CREATE NONCLUSTERED INDEX IDX_PersonaConfiguration_Comp02 ON [Enterprise].[PersonaConfiguration]
(
	[ConfigurationId] ASC,
	[FromDate] ASC,
	[ThruDate] ASC,
	[ProductId] ASC,
	[PersonaId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX IDX_PersonaConfiguration_Comp01 ON [Enterprise].[PersonaConfiguration]
(
	[PersonaId] ASC,
	[ProductId] ASC,
	[FromDate] ASC,
	[ThruDate] ASC
)
INCLUDE ( 	[ConfigurationId],[StatusTypeid]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PersonaConfiguration_ProductId_FromDate_ThruDate]
    ON [Enterprise].[PersonaConfiguration]([ProductId] ASC, [FromDate] ASC, [ThruDate] ASC)
    INCLUDE([PersonaId], [ConfigurationId])
GO
CREATE NONCLUSTERED INDEX [IX_PersonaConfiguration_ThruDate]
ON [Enterprise].[PersonaConfiguration] ([ThruDate])
INCLUDE ([PersonaId],[ProductId],[StatusTypeId],[IsFavorite])
GO
CREATE NONCLUSTERED INDEX [IDX_PersonaConfiguration_Comp01_NEW] ON [Enterprise].[PersonaConfiguration]
(
	[PersonaId] ASC,
	[ProductId] ASC,
	[FromDate] ASC,
	[ThruDate] ASC
)
INCLUDE([ConfigurationId], isFavorite, StatusTypeID) 

GO

CREATE INDEX IX_PersonaConfig_PersonaId_ProductId
    ON Enterprise.PersonaConfiguration(PersonaId, ProductId, ThruDate)
    INCLUDE (ConfigurationId)

GO




