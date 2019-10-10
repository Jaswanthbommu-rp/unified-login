CREATE TABLE [Ident].[MergedUser] (
    [MergedUserId]    INT              IDENTITY (1, 1) NOT NULL,
    [Title]           NVARCHAR (20)    NULL,
    [FirstName]       NVARCHAR (50)    NULL,
    [MiddleName]      NVARCHAR (50)    NULL,
    [LastName]        NVARCHAR (50)    NULL,
    [Email]           NVARCHAR (100)   NULL,
    [LoginName]       NVARCHAR (50)    NULL,
    [DefaultPassword] NVARCHAR (50)    NULL,
    [StatusId]        TINYINT          NULL,
    [ActivtyTypeId]   TINYINT          NULL,
    [RealPageId]      UNIQUEIDENTIFIER NULL,
    [UserType]        INT              NULL,
    CONSTRAINT [PK_MergedUser] PRIMARY KEY CLUSTERED ([MergedUserId] ASC),
    CONSTRAINT [FK_MergedUser_ActivityType] FOREIGN KEY ([ActivtyTypeId]) REFERENCES [Ident].[ActivityType] ([ActivityTypeId]),
    CONSTRAINT [FK_MergedUser_Status] FOREIGN KEY ([StatusId]) REFERENCES [Ident].[Status] ([StatusId])
);

