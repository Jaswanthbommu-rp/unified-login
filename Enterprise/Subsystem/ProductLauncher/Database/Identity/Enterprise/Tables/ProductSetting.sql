CREATE TABLE [Enterprise].[ProductSetting] (
    [ProductSettingId]     INT             IDENTITY (1, 1) NOT NULL,
    [ProductId]            INT             NOT NULL,
    [ProductSettingTypeId] INT             NOT NULL,
    [Value]                NVARCHAR (1000) NOT NULL,
    [FromDate]             DATETIME        DEFAULT (getutcdate()) NOT NULL,
    [ThruDate]             DATETIME        NULL,
    CONSTRAINT [PK_ProductSetting] PRIMARY KEY CLUSTERED ([ProductSettingId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_ProductSetting_ProductSettingType] FOREIGN KEY ([ProductSettingTypeId]) REFERENCES [Enterprise].[ProductSettingType] ([ProductSettingTypeId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_ProductSetting_ProductType] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO

CREATE INDEX [IX_ProductSetting_ProductId_ProductSettingTypeId] ON [Enterprise].[ProductSetting] ([ProductId],[ProductSettingTypeId], [Value])
GO

CREATE NONCLUSTERED INDEX [IX_Enterprise_ProductSetting_ProductSettingTypeId_ProductSettingId]
	ON [Enterprise].[ProductSetting] (
		[ProductSettingTypeId] ASC,
		[ProductSettingId] ASC
	)
	INCLUDE (
		[Value],
		[FromDate],
		[ThruDate]
	) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_ProductSetting_ProductSettingTypeId]
    ON [Enterprise].[ProductSetting]([ProductSettingTypeId] ASC, [ProductId] ASC, [FromDate] ASC)
    INCLUDE([ProductSettingId], [Value], [ThruDate]);

