CREATE TABLE [UserManagement].[ProductPage] (
    [ProductPageId] INT            IDENTITY (1, 1) NOT NULL,
    [ProductId]     INT            NOT NULL,
    [DisplayName]   NVARCHAR (255) NOT NULL,
    [CreatedBy]     BIGINT         NOT NULL,
    [CreatedDate]   DATETIME       NOT NULL,
    CONSTRAINT [PK_ProductPage] PRIMARY KEY CLUSTERED ([ProductPageId] ASC)
);

