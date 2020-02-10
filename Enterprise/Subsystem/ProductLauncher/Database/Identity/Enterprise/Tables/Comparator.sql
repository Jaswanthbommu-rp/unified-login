CREATE TABLE [Enterprise].[Comparator] (
    [ComparatorId] INT           IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50) NOT NULL,
    [CreatedDate]  DATETIME      CONSTRAINT [DF_Comparator_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [CreatedBy]    BIGINT        NOT NULL,
    CONSTRAINT [PK_Comparator] PRIMARY KEY CLUSTERED ([ComparatorId] ASC)
);

