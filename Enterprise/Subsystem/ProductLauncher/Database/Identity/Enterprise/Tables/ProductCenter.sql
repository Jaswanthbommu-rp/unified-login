CREATE TABLE [Enterprise].[ProductCenter]
(
	[ProductCenterId]		INT				NOT NULL			CONSTRAINT [PK_ProductCenter_ProductCenterId] PRIMARY KEY IDENTITY(1, 1),
	[ProductCenterSourceId]	NVARCHAR (25)   NOT NULL,
	[Source]				NVARCHAR (25)   NOT NULL,
	[Name]					NVARCHAR (100)  NOT NULL,
	[CreatedDate]			DATETIME		NOT NULL			CONSTRAINT [DF_ProductCenter_CreatedDate] DEFAULT (GETUTCDATE()),
	[ModifiedDate]			DATETIME		NULL,
	CONSTRAINT [AK_ProductCenter_ProductCenterSourceId_Source]  UNIQUE ([ProductCenterSourceId], [Source]),
)