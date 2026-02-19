CREATE TABLE [Enterprise].[OrganizationProduct]
(
	[OrganizationProductId] BIGINT	NOT NULL IDENTITY,
	[PartyId] BIGINT NOT NULL, 
    [ConfigurationId] INT NOT NULL, 
	[ProductId] INT NOT NULL, 
    [FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    [ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_OrganizationProduct] PRIMARY KEY ([OrganizationProductId]), 
    CONSTRAINT [FK_OrganizationProduct_Configuration] FOREIGN KEY ([ConfigurationId]) REFERENCES [Enterprise].[Configuration]([ConfigurationId]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [FK_OrganizationProduct_Product] FOREIGN KEY([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON UPDATE CASCADE ON DELETE CASCADE
)
GO
CREATE INDEX [IX_OrganizationProduct_PartyId_ProductId] ON [Enterprise].[OrganizationProduct] ([PartyId], [ProductId])
GO
CREATE INDEX [IX_OrganizationProduct_ProductId]
ON [Enterprise].[OrganizationProduct]
( [ProductId]
) INCLUDE( [OrganizationProductId], [PartyId], [ConfigurationId], [FromDate], [ThruDate] );

GO

CREATE INDEX IX_OrgProduct_PartyId_ProductId 
    ON Enterprise.OrganizationProduct(PartyId, ProductId, ThruDate)
    INCLUDE (ConfigurationId)
GO