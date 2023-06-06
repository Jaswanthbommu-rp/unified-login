CREATE TABLE [Auth].[ClientProperties] (
    [Id]       INT             IDENTITY (1, 1) NOT NULL,
    [ClientId] INT             NOT NULL,
    [Key]      NVARCHAR (250)  NOT NULL,
    [Value]    NVARCHAR (2000) NOT NULL,
    CONSTRAINT [PK_ClientProperties] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientProperties_ClientId_Key] ON [Auth].[ClientProperties]([ClientId] ASC, [Key] ASC);
