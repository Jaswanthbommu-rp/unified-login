CREATE TABLE [UserManagement].[ControlAttribute] (
    [ControlAttributeId] INT           IDENTITY (1, 1) NOT NULL,
    [ControlId]          INT           NOT NULL,
    [Key]                NVARCHAR (50) NOT NULL,
    [Value]              NVARCHAR (50) NOT NULL,
    [CreatedBy]          BIGINT        NOT NULL,
    [CreatedDate]        DATETIME      CONSTRAINT [DF_ControlAttribute_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ControlAttribute] PRIMARY KEY CLUSTERED ([ControlAttributeId] ASC),
    CONSTRAINT [FK_ControlAttribute_Control] FOREIGN KEY ([ControlId]) REFERENCES [UserManagement].[Control] ([ControlId])
);

