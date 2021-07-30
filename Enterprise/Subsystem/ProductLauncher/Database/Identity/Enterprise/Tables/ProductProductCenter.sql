CREATE TABLE [Enterprise].[ProductProductCenter]
(
	[ProductProductCenterId]	INT		NOT NULL						CONSTRAINT [PK_ProductProductCenter_ProductProductCenterId] PRIMARY KEY IDENTITY(1, 1),
	[ProductId]					INT		NOT NULL,
	[ProductCenterId]			INT		NOT NULL,
	[CreatedDate]			DATETIME	NOT NULL						CONSTRAINT [DF_ProductProductCenter_CreatedDate] DEFAULT (GETUTCDATE()),
	[ModifiedDate]			DATETIME	NULL,
	CONSTRAINT [FK_ProductProductCenter_Product]						FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product]([ProductId]),
	CONSTRAINT [FK_ProductProductCenter_ProductCenter]				    FOREIGN KEY ([ProductCenterId]) REFERENCES [Enterprise].[ProductCenter]([ProductCenterId]),
	INDEX	   [IX_UQ_ProductProductCenter_ProductId_ProductCenterId]   UNIQUE NONCLUSTERED ([ProductId], [ProductCenterId]),
	INDEX	   [IX_ProductProductCenter_ProductCenterId_ProductId]		NONCLUSTERED ([ProductCenterId], [ProductId])
)