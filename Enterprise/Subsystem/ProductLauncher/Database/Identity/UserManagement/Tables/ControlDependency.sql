CREATE TABLE [UserManagement].[ControlDependency] (
    [ControlDependencyId]    INT            IDENTITY (1, 1) NOT NULL,
    [MasterTabTypeControlId] INT            NOT NULL,
    [SlaveTabTypeControlID]  INT            NOT NULL,
    [MasterControlValue]     NVARCHAR (MAX) NOT NULL,
    [ComparatorID]           INT            NOT NULL,
    [CreatedBy]              BIGINT         NOT NULL,
    [CreatedDate]            DATETIME       CONSTRAINT [DF_ControlDependency_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ControlDependency] PRIMARY KEY CLUSTERED ([ControlDependencyId] ASC),
    CONSTRAINT [FK_ControlDependency_Comparator] FOREIGN KEY ([ComparatorID]) REFERENCES [Enterprise].[Comparator] ([ComparatorId]),
    CONSTRAINT [FK_ControlDependency_TabTypeControl] FOREIGN KEY ([MasterTabTypeControlId]) REFERENCES [UserManagement].[TabTypeControl] ([TabTypeControlId]),
    CONSTRAINT [FK_ControlDependency_TabTypeControl1] FOREIGN KEY ([SlaveTabTypeControlID]) REFERENCES [UserManagement].[TabTypeControl] ([TabTypeControlId])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UNIQUEControlDependency]
    ON [UserManagement].[ControlDependency]([MasterTabTypeControlId] ASC, [SlaveTabTypeControlID] ASC);

