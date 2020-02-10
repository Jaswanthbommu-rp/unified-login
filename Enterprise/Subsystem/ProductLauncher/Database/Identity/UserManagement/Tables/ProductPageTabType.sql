CREATE TABLE [UserManagement].[ProductPageTabType] (
    [ProductPageTabTypeId] INT      IDENTITY (1, 1) NOT NULL,
    [ProductPageId]        INT      NOT NULL,
    [TabTypeId]            INT      NOT NULL,
    [Sequence]             TINYINT  NOT NULL,
    [CreatedBy]            BIGINT   NOT NULL,
    [CreatedDate]          DATETIME CONSTRAINT [DF_ProductPageTabType_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProductPageTabType] PRIMARY KEY CLUSTERED ([ProductPageTabTypeId] ASC),
    CONSTRAINT [FK_ProductPageTabType_ProductPage] FOREIGN KEY ([ProductPageId]) REFERENCES [UserManagement].[ProductPage] ([ProductPageId]),
    CONSTRAINT [FK_ProductPageTabType_TabType] FOREIGN KEY ([TabTypeId]) REFERENCES [UserManagement].[TabType] ([TabTypeId])
);

