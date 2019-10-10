CREATE TABLE [Enterprise].[ProductType]
(
[ProductTypeId] [int] NOT NULL,
[ParentProductTypeId] [int] NULL,
[Name] [varchar] (50) NOT NULL,
[Description] [varchar] (1000) NULL,
[ProductTypeGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF__ProductTy__Produ__0E391C95] DEFAULT (newid())
)
GO
ALTER TABLE [Enterprise].[ProductType] ADD CONSTRAINT [PK_ProductType] PRIMARY KEY CLUSTERED  ([ProductTypeId])
GO
