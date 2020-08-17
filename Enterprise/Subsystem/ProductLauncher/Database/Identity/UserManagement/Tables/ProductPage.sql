CREATE TABLE [UserManagement].[ProductPage] (
    [ProductPageId] INT            IDENTITY (1, 1) NOT NULL,
    [ProductId]     INT            NOT NULL,
    [DisplayName]   NVARCHAR (255) NOT NULL,
    [CreatedBy]     BIGINT         NOT NULL,
    [CreatedDate]   DATETIME       NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 0, 
    [ProductPageTypeId] INT NOT NULL DEFAULT 1, 
    CONSTRAINT [PK_ProductPage] PRIMARY KEY CLUSTERED ([ProductPageId] ASC)
);
GO
ALTER TABLE [UserManagement].[ProductPage]  WITH CHECK ADD  CONSTRAINT [FK_ProductPage_Type] FOREIGN KEY([ProductPageTypeId])
REFERENCES [UserManagement].[ProductPageType] ([ProductPageTypeId])
GO
