CREATE TABLE [UserManagement].[TabType] (
    [TabTypeId]   INT            IDENTITY (1, 1) NOT NULL,
    [UIId]        NVARCHAR (255) NULL,
    [DisplayName] NVARCHAR (255) NOT NULL,
    [Sequence]    TINYINT        NULL,
    [CreatedBy]   BIGINT         NOT NULL,
    [CreatedDate] DATETIME       CONSTRAINT [DF_Tab_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_TabType] PRIMARY KEY CLUSTERED ([TabTypeId] ASC)
);

