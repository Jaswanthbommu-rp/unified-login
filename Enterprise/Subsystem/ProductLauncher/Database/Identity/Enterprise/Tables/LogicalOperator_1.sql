CREATE TABLE [Enterprise].[LogicalOperator] (
    [LogicalOperatorId] TINYINT        IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (5)   NOT NULL,
    [CreatedDate]       DATETIME       CONSTRAINT [DF_LogicalOperator_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [CreatedBy]         NVARCHAR (325) CONSTRAINT [DF_LogicalOperator_CreatedBy] DEFAULT ('00000000-0000-0000-0000-000000000000') NOT NULL,
    CONSTRAINT [PK_LogicalOperator] PRIMARY KEY CLUSTERED ([LogicalOperatorId] ASC)
);

