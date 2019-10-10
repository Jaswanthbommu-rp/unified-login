CREATE TABLE [Auth].[Product]
(
[ProductId] [int] NOT NULL IDENTITY(1, 1),
[ProductName] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[ClientID] [int] NULL,
[ClassName] [nvarchar] (50) NULL,
[SettingsUrl] [nvarchar] (500) NULL,
[ProductUrl] [nvarchar] (1000) NULL,
[SubDescription] [nvarchar] (500) NULL,
[TitleId] [nvarchar] (25) NULL,
[TitleUniqueId] [uniqueidentifier] NULL,
[IsNewTab] [bit] NULL CONSTRAINT [DF_Product_IsNewTab] DEFAULT ((0)),
[MetatagUniqueId] [nvarchar] (100) NULL
)
GO
ALTER TABLE [Auth].[Product] ADD CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED  ([ProductId])
GO
ALTER TABLE [Auth].[Product] ADD CONSTRAINT [FK_Clients_ClientID] FOREIGN KEY ([ClientID]) REFERENCES [Auth].[Clients] ([ClientId])
GO
