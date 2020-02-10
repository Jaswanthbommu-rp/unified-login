CREATE TABLE [UserManagement].[TabTypeControl] (
    [TabTypeControlId] INT      IDENTITY (1, 1) NOT NULL,
    [TabTypeId]        INT      NOT NULL,
    [ProductPageId]    INT      NULL,
    [ControlId]        INT      NOT NULL,
    [CreatedBy]        BIGINT   NOT NULL,
    [CreatedDate]      DATETIME NOT NULL,
    CONSTRAINT [PK_TabTypeControl] PRIMARY KEY CLUSTERED ([TabTypeControlId] ASC),
    CONSTRAINT [FK_TabTypeControl_Control] FOREIGN KEY ([ControlId]) REFERENCES [UserManagement].[Control] ([ControlId]),
    CONSTRAINT [FK_TabTypeControl_ProductPage] FOREIGN KEY ([ProductPageId]) REFERENCES [UserManagement].[ProductPage] ([ProductPageId]),
    CONSTRAINT [FK_TabTypeControl_TabType] FOREIGN KEY ([TabTypeId]) REFERENCES [UserManagement].[TabType] ([TabTypeId])
);

