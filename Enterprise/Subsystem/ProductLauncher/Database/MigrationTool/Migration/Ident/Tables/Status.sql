CREATE TABLE [Ident].[Status] (
    [StatusId] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (50) NULL,
    CONSTRAINT [PK_Status] PRIMARY KEY CLUSTERED ([StatusId] ASC)
);

