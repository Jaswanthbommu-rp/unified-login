CREATE TABLE [Enterprise].[ProductRelationship]
(
	[ProductRelationshipId] INT NOT NULL IDENTITY, 
    [PartyIdFrom] BIGINT NOT NULL, 
    [ProductIdTo] INT NOT NULL, 
    [RoleTypeIdFrom] INT NOT NULL, 
    [RoleTypeIdTo] INT NOT NULL, 
    [FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    [ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_ProductRelationship] PRIMARY KEY ([ProductRelationshipId]), 
    CONSTRAINT [FK_ProductRelationship_Party] FOREIGN KEY (PartyIdFrom) REFERENCES Enterprise.Party(PartyId) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_ProductRelationship_ProductType] FOREIGN KEY (ProductIdTo) REFERENCES Enterprise.[Product]([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
)
