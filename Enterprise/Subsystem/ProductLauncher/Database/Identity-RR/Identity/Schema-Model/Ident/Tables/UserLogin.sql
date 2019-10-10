CREATE TABLE [Ident].[UserLogin]
(
[UserId] [bigint] NOT NULL IDENTITY(1, 1),
[PartyId] [bigint] NOT NULL,
[LoginName] [varchar] (255) NOT NULL,
[PasswordHash] [nvarchar] (255) NULL,
[PasswordSalt] [nvarchar] (255) NULL,
[PasswordModifiedDate] [smalldatetime] NULL,
[LastLoginDate] [datetime] NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__UserLogin__FromD__0F2D40CE] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Ident].[UserLogin] ADD CONSTRAINT [PK_UserLogin] PRIMARY KEY CLUSTERED  ([UserId])
GO
ALTER TABLE [Ident].[UserLogin] ADD CONSTRAINT [AK_UserLogin_LoginId] UNIQUE NONCLUSTERED  ([LoginName])
GO
ALTER TABLE [Ident].[UserLogin] ADD CONSTRAINT [FK_UserLogin_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table holds the RealPage user names and passwords of the users.', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity backing field', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'LoginName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The party this user login belongs to.', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'PartyId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'PBKDF2 hashed password', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'PasswordHash'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The salt used to hash the password.', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'PasswordSalt'
GO
EXEC sp_addextendedproperty N'MS_Description', N'User Name for the Party', 'SCHEMA', N'Ident', 'TABLE', N'UserLogin', 'COLUMN', N'UserId'
GO
