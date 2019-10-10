CREATE TABLE [Logging].[LogType] (
    [LogTypeId]         INT            IDENTITY (1, 1) NOT NULL,
    [LogcategoryTypeId] INT            NULL,
    [Name]              NVARCHAR (100) NULL,
    [Description]       NVARCHAR (200) NULL,
    CONSTRAINT [PK_LogType] PRIMARY KEY CLUSTERED ([LogTypeId] ASC),
    CONSTRAINT [FK_LogType_LogCategoryType] FOREIGN KEY ([LogcategoryTypeId]) REFERENCES [Logging].[LogCategoryType] ([LogCategoryTypeId])
);







