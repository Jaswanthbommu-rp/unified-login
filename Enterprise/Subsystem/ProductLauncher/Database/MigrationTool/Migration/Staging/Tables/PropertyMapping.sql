CREATE TABLE [Staging].[PropertyMapping] (
    [PropertyMappingId]  BIGINT IDENTITY (1, 1) NOT NULL,
    [ProductUserId]      INT    NULL,
    [PropertyInstanceId] INT    NULL,
    CONSTRAINT [PK_PropertyMapping] PRIMARY KEY CLUSTERED ([PropertyMappingId] ASC),
    CONSTRAINT [FK_PropertyMapping_ProductUser] FOREIGN KEY ([ProductUserId]) REFERENCES [Staging].[ProductUser] ([ProductUserId])
);

