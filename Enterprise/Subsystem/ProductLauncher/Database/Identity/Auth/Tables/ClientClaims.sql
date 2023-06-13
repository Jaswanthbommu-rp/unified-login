CREATE TABLE [Auth].[ClientClaims] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Type]     NVARCHAR (250) NOT NULL,
    [Value]    NVARCHAR (250) NOT NULL,
    [ClientId] INT            NOT NULL, 
    CONSTRAINT [PK_ClientClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClientClaims_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientClaims_ClientId_Type_Value] ON [Auth].[ClientClaims] ([ClientId], [Type], [Value]) ON [PRIMARY]

GO

