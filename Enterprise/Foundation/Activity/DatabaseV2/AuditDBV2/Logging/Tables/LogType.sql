CREATE TABLE [Logging].[LogType] (
    [LogTypeId]         INT           NOT NULL,
    [LogcategoryTypeId] INT            NULL,
    [Name]              NVARCHAR (100) NULL,
    [Description]       NVARCHAR (200) NULL,
    CONSTRAINT [PK_LogType_LogTypeId] PRIMARY KEY CLUSTERED ([LogTypeId] ASC),
    CONSTRAINT [FK_LogType_LogCategoryTypeId] FOREIGN KEY ([LogcategoryTypeId]) REFERENCES [Logging].[LogCategoryType] ([LogCategoryTypeId])
);

