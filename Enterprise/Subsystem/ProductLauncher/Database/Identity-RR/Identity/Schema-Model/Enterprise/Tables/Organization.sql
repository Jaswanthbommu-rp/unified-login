CREATE TABLE [Enterprise].[Organization]
(
[PartyId] [bigint] NOT NULL,
[Name] [nvarchar] (50) NULL
)
GO
ALTER TABLE [Enterprise].[Organization] ADD CONSTRAINT [PK_Organization] PRIMARY KEY CLUSTERED  ([PartyId])
GO
ALTER TABLE [Enterprise].[Organization] ADD CONSTRAINT [FK_Organization_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
