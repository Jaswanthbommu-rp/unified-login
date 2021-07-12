CREATE TABLE [Logging].[LogCategoryType] (
    [LogCategoryTypeId] INT           NOT NULL,
    [Name]              NVARCHAR (50)  CONSTRAINT [DF_LogCategoryType_Name] DEFAULT ('Unknown') NOT NULL,
    [Description]       NVARCHAR (200) NULL,
    CONSTRAINT [PK_LogCategoryType_LogCategoryTypeId] PRIMARY KEY CLUSTERED ([LogCategoryTypeId] ASC)
);

