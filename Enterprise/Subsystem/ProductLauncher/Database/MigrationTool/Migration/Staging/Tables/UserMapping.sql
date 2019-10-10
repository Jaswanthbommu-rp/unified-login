CREATE TABLE [Staging].[UserMapping] (
    [ProductUserId] INT NULL,
    [MergedUserId]  INT NULL,
    CONSTRAINT [FK_UserMapping_MergedUser] FOREIGN KEY ([MergedUserId]) REFERENCES [Ident].[MergedUser] ([MergedUserId]),
    CONSTRAINT [FK_UserMapping_ProductUsers] FOREIGN KEY ([ProductUserId]) REFERENCES [Staging].[ProductUser] ([ProductUserId])
);

