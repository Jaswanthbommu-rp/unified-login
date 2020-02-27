CREATE TABLE [UserManagement].[ControlDependency] (
    [ControlDependencyId] INT            IDENTITY (1, 1) NOT NULL,
    [MasterControlId]     INT            NOT NULL,
    [SlaveControlID]      INT            NOT NULL,
    [MasterControlValue]  NVARCHAR (MAX) NOT NULL,
    [ComparatorID]        INT            NOT NULL,
    [CreatedBy]           BIGINT         NOT NULL,
    [CreatedDate]         DATETIME       CONSTRAINT [DF_ControlDependency_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ControlDependency] PRIMARY KEY CLUSTERED ([ControlDependencyId] ASC),
    CONSTRAINT [FK_ControlDependency_Comparator] FOREIGN KEY ([ComparatorID]) REFERENCES [Enterprise].[Comparator] ([ComparatorId]),
    CONSTRAINT [FK_ControlDependency_Control] FOREIGN KEY ([MasterControlId]) REFERENCES [UserManagement].[Control] ([ControlId]),
    CONSTRAINT [FK_ControlDependency_Control1] FOREIGN KEY ([SlaveControlID]) REFERENCES [UserManagement].[Control] ([ControlId])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UNIQUEControlDependency]
    ON [UserManagement].[ControlDependency]([MasterControlId] ASC, [SlaveControlID] ASC);



