CREATE TABLE [Enterprise].[SameSiteValue] (
    [SameSiteValueId] INT            IDENTITY (1, 1) NOT NULL,
    [SameSiteName]    NVARCHAR (255) NOT NULL,
    [ComparatorId]    TINYINT        NOT NULL,
    [CreatedBy]       BIGINT         NOT NULL,
    [CreatedDate]     DATETIME       CONSTRAINT [DF_SameSiteValue_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SameSiteValue] PRIMARY KEY CLUSTERED ([SameSiteValueId] ASC),
    CONSTRAINT [FK_SameSiteValue_Comparator] FOREIGN KEY ([ComparatorId]) REFERENCES [Enterprise].[Comparator] ([ComparatorId])
);

