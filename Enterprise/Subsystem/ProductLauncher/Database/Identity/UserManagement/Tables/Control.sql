CREATE TABLE [UserManagement].[Control] (
    [ControlId]       INT            IDENTITY (1, 1) NOT NULL,
    [ParentControlId] INT            NULL,
    [ControlTypeId]   INT            NOT NULL,
    [UIId]            NVARCHAR (255) NULL,
    [DisplayName]     NVARCHAR (255) NULL,
    [DataSource]      NVARCHAR (MAX) NULL,
    [Sequence]        TINYINT        NULL,
    [CreatedBy]       BIGINT         NOT NULL,
    [CreatedDate]     DATETIME       CONSTRAINT [DF_Control_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Control] PRIMARY KEY CLUSTERED ([ControlId] ASC),
    CONSTRAINT [FK_Control_Control] FOREIGN KEY ([ParentControlId]) REFERENCES [UserManagement].[Control] ([ControlId]),
    CONSTRAINT [FK_Control_ControlType] FOREIGN KEY ([ControlTypeId]) REFERENCES [UserManagement].[ControlType] ([ControlTypeId])
);

