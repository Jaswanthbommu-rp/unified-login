CREATE TABLE [Enterprise].[ProductRelationship]
(
[ProductRelationshipId] [int] NOT NULL IDENTITY(1, 1),
[PartyIdFrom] [bigint] NOT NULL,
[ProductIdTo] [int] NOT NULL,
[RoleTypeIdFrom] [int] NOT NULL,
[RoleTypeIdTo] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ProductRe__FromD__0697FACD] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Enterprise].[ProductRelationship] ADD CONSTRAINT [PK_ProductRelationship] PRIMARY KEY CLUSTERED  ([ProductRelationshipId])
GO
ALTER TABLE [Enterprise].[ProductRelationship] ADD CONSTRAINT [FK_ProductRelationship_Party] FOREIGN KEY ([PartyIdFrom]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Enterprise].[ProductRelationship] ADD CONSTRAINT [FK_ProductRelationship_ProductType] FOREIGN KEY ([ProductIdTo]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
