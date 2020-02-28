CREATE TABLE [Enterprise].[Comparator] (
    [ComparatorId] TINYINT        IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [CreatedDate]  DATETIME       CONSTRAINT [DF_Comparator_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [CreatedBy]    NVARCHAR (325) CONSTRAINT [DF_Comparator_CreatedBy] DEFAULT ('00000000-0000-0000-0000-000000000000') NULL,
    CONSTRAINT [PK_Comparator] PRIMARY KEY CLUSTERED ([ComparatorId] ASC)
);

