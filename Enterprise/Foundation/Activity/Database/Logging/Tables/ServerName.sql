CREATE TABLE [Logging].[ServerName] (
    [ServerId]      INT           IDENTITY (1, 1) NOT NULL,
    [ServerName]    NVARCHAR (50) NULL,
    [ServerProfile] NVARCHAR (50) NULL,
    CONSTRAINT [PK_ServerName] PRIMARY KEY CLUSTERED ([ServerId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ServerName]
    ON [Logging].[ServerName]([ServerName] ASC);

