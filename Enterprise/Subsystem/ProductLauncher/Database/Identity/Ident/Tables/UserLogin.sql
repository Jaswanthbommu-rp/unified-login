CREATE TABLE [Ident].[UserLogin] (
    [UserId]                 BIGINT         IDENTITY (1, 1) NOT NULL,
    [PersonPartyId]          BIGINT         NULL,
    [LoginName]              VARCHAR (255)  NOT NULL,
    [IdentityProviderTypeId] INT            CONSTRAINT [DF__tmp_ms_xx__Ident__4E9E85C4] DEFAULT ((-1)) NOT NULL,
    [PasswordHash]           NVARCHAR (255) NULL,
    [PasswordSalt]           NVARCHAR (255) NULL,
    [PasswordModifiedDate]   SMALLDATETIME  NULL,
    [LastLoginDate]          DATETIME       NULL,
    [CreateUserSourceId]     INT            NULL,
    [CreateDate]             DATETIME       CONSTRAINT [DF__tmp_ms_xx__Creat__5086CE36] DEFAULT (getutcdate()) NOT NULL,
    [TwoFactorEnabled] TINYINT NOT NULL DEFAULT 0, 
    [TwoFactorLastNotifyDate] SMALLDATETIME NULL, 
    CONSTRAINT [PK_UserLogin] PRIMARY KEY CLUSTERED ([UserId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_UserLogin_Party] FOREIGN KEY ([PersonPartyId]) REFERENCES [Enterprise].[Party] ([PartyId]),
    CONSTRAINT [FK_UserLogin_StatusType] FOREIGN KEY ([CreateUserSourceId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId]) ON UPDATE CASCADE,
    CONSTRAINT [AK_UserLogin_LoginId] UNIQUE NONCLUSTERED ([LoginName] ASC) WITH (FILLFACTOR = 80)
);






GO

GO
EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'This table holds the RealPage user names and passwords of the users.',
	@level0type = N'SCHEMA',
	@level0name = N'Ident',
	@level1type = N'TABLE',
	@level1name = N'UserLogin';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'User Name for the Party',
	@level0type = N'SCHEMA',
	@level0name = N'Ident',
	@level1type = N'TABLE',
	@level1name = N'UserLogin',
	@level2type = N'COLUMN',
	@level2name = N'UserId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The salt used to hash the password.',
	@level0type = N'SCHEMA',
	@level0name = N'Ident',
	@level1type = N'TABLE',
	@level1name = N'UserLogin',
	@level2type = N'COLUMN',
	@level2name = N'PasswordSalt';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'PBKDF2 hashed password',
	@level0type = N'SCHEMA',
	@level0name = N'Ident',
	@level1type = N'TABLE',
	@level1name = N'UserLogin',
	@level2type = N'COLUMN',
	@level2name = N'PasswordHash';
GO


EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Identity backing field',
	@level0type = N'SCHEMA',
	@level0name = N'Ident',
	@level1type = N'TABLE',
	@level1name = N'UserLogin',
	@level2type = N'COLUMN',
	@level2name = N'LoginName';


GO


CREATE NONCLUSTERED INDEX [IX_UserLogin_PersonPartyId] ON [Ident].[UserLogin]
([PersonPartyId] ASC
) INCLUDE([UserId], LoginName) WITH(PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY];
GO
CREATE INDEX IDX_UserLogin_Comp01 ON Ident.UserLogin(UserId) INCLUDE(LoginName);
