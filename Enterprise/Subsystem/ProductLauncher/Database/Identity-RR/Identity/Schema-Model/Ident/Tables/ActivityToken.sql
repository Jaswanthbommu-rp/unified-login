CREATE TABLE [Ident].[ActivityToken]
(
[ActivityTokenId] [int] NOT NULL IDENTITY(1, 1),
[ActivityId] [int] NOT NULL,
[RealPageId] [uniqueidentifier] NOT NULL,
[ActivityToken] [nvarchar] (100) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_ActivityToken_IsActive] DEFAULT ((0)),
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_ActivityToken_CreateDateTime] DEFAULT (getutcdate()),
[ExpireDateTime] [smalldatetime] NOT NULL
)
GO
ALTER TABLE [Ident].[ActivityToken] ADD CONSTRAINT [PK_ActivityToken] PRIMARY KEY CLUSTERED  ([ActivityTokenId])
GO
ALTER TABLE [Ident].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Ident].[Activity] ([ActivityId])
GO
ALTER TABLE [Ident].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Party] FOREIGN KEY ([RealPageId]) REFERENCES [Enterprise].[Party] ([RealPageId])
GO
