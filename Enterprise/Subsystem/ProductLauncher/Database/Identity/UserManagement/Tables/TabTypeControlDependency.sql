CREATE TABLE [UserManagement].[TabTypeControlDependency] (
    [TabtypeControlDependencyId] INT            IDENTITY (1, 1) NOT NULL,
    [TabTypeControlId]           INT            NOT NULL,
    [TabTypeId]                  INT            NOT NULL,
    [ControlTypeValue]           NVARCHAR (255) NOT NULL,
    [ComparatorID]               INT            NOT NULL,
    [CreatedBy]                  INT            NOT NULL,
    [CreatedDate]                DATETIME       CONSTRAINT [DF_TabVisibility_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_TabVisibility] PRIMARY KEY CLUSTERED ([TabtypeControlDependencyId] ASC),
    CONSTRAINT [FK_TabTypeControlDependency_Comparator] FOREIGN KEY ([ComparatorID]) REFERENCES [Enterprise].[Comparator] ([ComparatorId]),
    CONSTRAINT [FK_TabTypeControlDependency_TabType] FOREIGN KEY ([TabTypeId]) REFERENCES [UserManagement].[TabType] ([TabTypeId]),
    CONSTRAINT [FK_TabTypeControlDependency_TabTypeControl] FOREIGN KEY ([TabTypeControlId]) REFERENCES [UserManagement].[TabTypeControl] ([TabTypeControlId])
);

