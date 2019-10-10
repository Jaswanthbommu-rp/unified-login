CREATE TABLE [Staging].[ProductUser] (
    [ProductUserId]         INT            IDENTITY (1, 1) NOT NULL,
    [ProductOrganizationId] INT            NULL,
    [ProductId]             INT            NULL,
    [Title]                 NVARCHAR (20)  NULL,
    [FirstName]             NVARCHAR (50)  NULL,
    [MiddleName]            NVARCHAR (50)  NULL,
    [LastName]              NVARCHAR (50)  NULL,
    [EMail]                 NVARCHAR (100) NULL,
    [LoginName]             NVARCHAR (50)  NULL,
    [Phone]                 NVARCHAR (15)  NULL,
    [UserStatus]            NVARCHAR (50)  NULL,
    [StatusId]              TINYINT        NULL,
    CONSTRAINT [PK_ProductUsers] PRIMARY KEY CLUSTERED ([ProductUserId] ASC),
    CONSTRAINT [FK_ProductUsers_Status] FOREIGN KEY ([StatusId]) REFERENCES [Ident].[Status] ([StatusId])
);

