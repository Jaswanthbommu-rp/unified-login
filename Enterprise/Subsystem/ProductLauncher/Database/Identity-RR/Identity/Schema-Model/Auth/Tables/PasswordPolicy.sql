CREATE TABLE [Auth].[PasswordPolicy]
(
[PasswordPolicyId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioId] [int] NOT NULL,
[MinimumLength] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumLength] DEFAULT ((8)),
[MaximumLength] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MaximumLength] DEFAULT ((128)),
[MinimumLowercase] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumLowercase] DEFAULT ((0)),
[MinimumUppercase] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumUppercase] DEFAULT ((0)),
[MinimumNumeric] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumNumeric] DEFAULT ((0)),
[MinimumSpecialCharacter] [tinyint] NOT NULL CONSTRAINT [DF_PasswordPolicy_MinimumSpecialCharacter] DEFAULT ((0)),
[AllowUsersToChangeOwnPassword] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_AllowUsersToChangeOwnPassword] DEFAULT ((1)),
[EnablePasswordExpiration] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_EnablePasswordExpiration] DEFAULT ((0)),
[PasswordExpirationPeriodInDays] [smallint] NULL,
[PreventPasswordReuse] [bit] NOT NULL CONSTRAINT [DF_PasswordPolicy_PreventPasswordReuse] DEFAULT ((0)),
[NumberOfPasswordsToRemember] [tinyint] NULL,
[UserId] [bigint] NOT NULL,
[SysStartDateTime] [datetime2] (0) GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_PasswordPolicy_SysStartDateTime] DEFAULT (sysutcdatetime()),
[SysEndDateTime] [datetime2] (0) GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_PasswordPolicy_SysEndDateTime] DEFAULT (sysutcdatetime()),
PERIOD FOR SYSTEM_TIME (SysStartDateTime, SysEndDateTime),
CONSTRAINT [PK_Auth.PasswordPolicy] PRIMARY KEY CLUSTERED  ([PasswordPolicyId])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Auth].[PasswordPolicyHistory])
)
GO
ALTER TABLE [Auth].[PasswordPolicy] ADD CONSTRAINT [CHK_PasswordPolicy_PasswordExpirationPeriodInDays] CHECK (([EnablePasswordExpiration]=(0) AND isnull([PasswordExpirationPeriodInDays],(0))=(0) OR [EnablePasswordExpiration]=(1) AND [PasswordExpirationPeriodInDays]>(0)))
GO
ALTER TABLE [Auth].[PasswordPolicy] ADD CONSTRAINT [CHK_PasswordPolicy_NumberOfPasswordsToRemember] CHECK (([PreventPasswordReuse]=(0) AND isnull([NumberOfPasswordsToRemember],(0))=(0) OR [PreventPasswordReuse]=(1) AND [NumberOfPasswordsToRemember]>(0)))
GO
ALTER TABLE [Auth].[PasswordPolicy] ADD CONSTRAINT [FK_PasswordPolicy_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table holds the RealPage Password Policy for each Portofio.', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Permit Users to Change Their Own Password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'AllowUsersToChangeOwnPassword'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Enable user passwords to be valid for only the specified number of days', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'EnablePasswordExpiration'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Specify the maximum number of characters allowed in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MaximumLength'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Specify the minimum number of characters allowed in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumLength'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum lowercase characters required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumLowercase'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum numbers required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumNumeric'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum special characters required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumSpecialCharacter'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Minimum uppercase characters required in a user password', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'MinimumUppercase'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Number of previous passwords a user is not allowed to reuse', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'NumberOfPasswordsToRemember'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Number of days a password is valid for', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PasswordExpirationPeriodInDays'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Unique Password Policy ID', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PasswordPolicyId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Prevent users from reusing a specified number of previous passwords', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'PreventPasswordReuse'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Created by User ID', 'SCHEMA', N'Auth', 'TABLE', N'PasswordPolicy', 'COLUMN', N'UserId'
GO
