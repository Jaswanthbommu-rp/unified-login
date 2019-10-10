CREATE TABLE [Enterprise].[Party]
(
[PartyId] [bigint] NOT NULL IDENTITY(1, 1),
[RealPageId] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Party_rowguid] DEFAULT (newid()),
[CreateDate] [datetime] NOT NULL CONSTRAINT [DF_Party_CreateDate] DEFAULT (getutcdate())
)
GO
ALTER TABLE [Enterprise].[Party] ADD CONSTRAINT [PK_Party] PRIMARY KEY CLUSTERED  ([PartyId])
GO
CREATE UNIQUE NONCLUSTERED INDEX [AK_Party_rowguid] ON [Enterprise].[Party] ([RealPageId])
GO
