CREATE TABLE [Enterprise].[Right](
	[RightID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL DEFAULT -1,
     [RightValueTypeId] INT NOT NULL DEFAULT -1, 
    [PartyId] BIGINT NOT NULL DEFAULT -1, 
    [LastUpdateDateTime] DATETIME NULL DEFAULT GETUTCDATE(), 
    CONSTRAINT [FK_Right_Role] FOREIGN KEY([RoleID]) REFERENCES [Enterprise].[Role] ([RoleID]) ON UPDATE CASCADE,
 CONSTRAINT [FK_Right_RoleValueType] FOREIGN KEY([RightValueTypeID]) REFERENCES [Enterprise].[RightValueType] ([RightValueTypeID]) ON UPDATE CASCADE,
 CONSTRAINT FK_Right_Party FOREIGN KEY (	PartyId) REFERENCES Enterprise.Party (PartyId) ON UPDATE  NO ACTION  ON DELETE  NO ACTION ,
 CONSTRAINT [PK_Right] PRIMARY KEY CLUSTERED 
(
	[RightID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Right] ON [Enterprise].[Right]
(
	[RightID] ASC,
	[RightValueTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IDX_Right_RoleId ON [Enterprise].[Right]([RoleID]) INCLUDE([RightValueTypeId]);
GO

CREATE INDEX [IX_Right_RightValueTypeId] ON [Enterprise].[Right] ([RightValueTypeId]) INCLUDE ([PartyId])
GO

CREATE INDEX [IX_Right_PartyId] ON [Enterprise].[Right] ([PartyId]) INCLUDE ([RightValueTypeId])
GO

CREATE NONCLUSTERED INDEX [IX_Enterprise_Right_RightId_RoleID] ON [Enterprise].[Right]
(
	[RightID] ASC,
	[RoleID] ASC
) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO

CREATE INDEX [IX_Right_RightValueTypeId_RP]
ON [Enterprise].[Right]
( [RightValueTypeId], [PartyId]
) 
	   INCLUDE( [RightID], [RoleID], [LastUpdateDateTime] );
GO