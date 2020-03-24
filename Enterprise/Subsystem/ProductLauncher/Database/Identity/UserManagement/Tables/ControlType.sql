CREATE TABLE [UserManagement].[ControlType] (
    [ControlTypeId] INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (50)  NOT NULL,
    [Description]   NVARCHAR (255) NULL,
    [CreatedBy]     BIGINT         NOT NULL,
    [CreatedDate]   DATETIME       CONSTRAINT [DF_ControlType_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ControlType] PRIMARY KEY CLUSTERED ([ControlTypeId] ASC)
);

