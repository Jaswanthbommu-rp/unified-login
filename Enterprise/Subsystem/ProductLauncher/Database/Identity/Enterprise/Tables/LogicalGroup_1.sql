CREATE TABLE [Enterprise].[LogicalGroup] (
    [LogicalGroupId]    INT      IDENTITY (1, 1) NOT NULL,
    [LogicalGrouper]    INT      NOT NULL,
    [SameSiteIdLeft]    INT      NOT NULL,
    [SameSiteIdRight]   INT      NOT NULL,
    [LogicalOperatorId] TINYINT  NOT NULL,
    [Sequence]          TINYINT  NOT NULL,
    [CreatedBy]         BIGINT   NOT NULL,
    [CreatedDate]       DATETIME CONSTRAINT [DF_LogicalGroup_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_LogicalGroup] PRIMARY KEY CLUSTERED ([LogicalGroupId] ASC),
    CONSTRAINT [FK_LogicalGroup_LogicalOperator] FOREIGN KEY ([LogicalOperatorId]) REFERENCES [Enterprise].[LogicalOperator] ([LogicalOperatorId]),
    CONSTRAINT [FK_LogicalGroup_SameSiteValue] FOREIGN KEY ([SameSiteIdLeft]) REFERENCES [Enterprise].[SameSiteValue] ([SameSiteValueId]),
    CONSTRAINT [FK_LogicalGroup_SameSiteValue1] FOREIGN KEY ([SameSiteIdRight]) REFERENCES [Enterprise].[SameSiteValue] ([SameSiteValueId])
);

