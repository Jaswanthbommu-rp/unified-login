CREATE TABLE [Ident].[PasswordHistory](
	[PasswordHistoryId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[ActivityConfigurationId] [int] NOT NULL,
	[ChangedPasswordHash] [nvarchar](1000) NOT NULL,
	[ChangedPasswordSalt] [nvarchar](255) NULL,
	[ChangedPasswordDateTime] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_PasswordHistory01] PRIMARY KEY CLUSTERED 
(
	[PasswordHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Ident].[PasswordHistory] ADD  CONSTRAINT [DF_PasswordHistory_ChangedPasswordDateTime]  DEFAULT (getutcdate()) FOR [ChangedPasswordDateTime]
GO

ALTER TABLE [Ident].[PasswordHistory]  WITH CHECK ADD  CONSTRAINT [FK_PasswordHistory_ActivityConfiguration] FOREIGN KEY([ActivityConfigurationId])
REFERENCES [Ident].[ActivityConfiguration] ([ActivityConfigurationId])
GO

ALTER TABLE [Ident].[PasswordHistory] CHECK CONSTRAINT [FK_PasswordHistory_ActivityConfiguration]
GO

ALTER TABLE [Ident].[PasswordHistory]  WITH CHECK ADD  CONSTRAINT [FK_PasswordHistory_UserLogin] FOREIGN KEY([UserId])
REFERENCES [Ident].[UserLogin] ([UserId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [Ident].[PasswordHistory] CHECK CONSTRAINT [FK_PasswordHistory_UserLogin]
GO



CREATE INDEX [IX_PasswordHistory_UserId]
ON [Ident].[PasswordHistory]
( [UserId]
) 
INCLUDE( [ChangedPasswordHash], [ChangedPasswordSalt], [ChangedPasswordDateTime] );