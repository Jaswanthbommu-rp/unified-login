CREATE TABLE [Enterprise].[OrganizationProduct]
(
[OrganizationProductId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[ConfigurationId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__Organizat__FromD__0A688BB1] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[OrganizationProduct] ADD CONSTRAINT [PK_OrganizationProduct] PRIMARY KEY CLUSTERED  ([OrganizationProductId])
GO
ALTER TABLE [Enterprise].[OrganizationProduct] ADD CONSTRAINT [FK_OrganizationProduct_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration] ([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[OrganizationProduct] ADD CONSTRAINT [FK_OrganizationProduct_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
