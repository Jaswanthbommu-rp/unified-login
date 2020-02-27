CREATE TABLE [UserManagement].[ProductPageControl] (
    [ProductPageControlId] INT      IDENTITY (1, 1) NOT NULL,
    [ProductPageId]        INT      NOT NULL,
    [ControlId]            INT      NOT NULL,
    [CreatedBy]            BIGINT   NOT NULL,
    [CreatedDate]          DATETIME NOT NULL,
    CONSTRAINT [PK_TabProductControl] PRIMARY KEY CLUSTERED ([ProductPageControlId] ASC),
    CONSTRAINT [FK_ProductPageControl_Control] FOREIGN KEY ([ControlId]) REFERENCES [UserManagement].[Control] ([ControlId]),
    CONSTRAINT [FK_ProductPageControl_ProductPage] FOREIGN KEY ([ProductPageId]) REFERENCES [UserManagement].[ProductPage] ([ProductPageId])
);



