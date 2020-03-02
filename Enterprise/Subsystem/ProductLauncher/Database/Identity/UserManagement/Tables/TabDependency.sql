CREATE TABLE [UserManagement].[TabDependency] (
    [TabDependencyId] INT            IDENTITY (1, 1) NOT NULL,
    [ControlId]       INT            NOT NULL,
    [TabTypeId]       INT            NOT NULL,
    [ControlValue]    NVARCHAR (255) NOT NULL,
    [ComparatorID]    INT            NOT NULL,
    [CreatedBy]       INT            NOT NULL,
    [CreatedDate]     DATETIME       CONSTRAINT [DF_TabDependency_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_TabDependency] PRIMARY KEY CLUSTERED ([TabDependencyId] ASC),
    CONSTRAINT [FK_TabDependency_Control] FOREIGN KEY ([ControlId]) REFERENCES [UserManagement].[Control] ([ControlId]),
    CONSTRAINT [FK_TabDependency_Tab] FOREIGN KEY ([TabTypeId]) REFERENCES [UserManagement].[TabType] ([TabTypeId])
);





