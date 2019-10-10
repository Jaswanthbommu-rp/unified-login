CREATE TABLE [Ident].[PasswordPolicy] (
    [PasswordPolicyId]               INT                                         IDENTITY (1, 1) NOT NULL,
    [PartyId]                        BIGINT                                      NOT NULL,
    [MinimumLength]                  TINYINT                                     CONSTRAINT [DF_PasswordPolicy_MinimumLength] DEFAULT ((8)) NOT NULL,
    [MaximumLength]                  TINYINT                                     CONSTRAINT [DF_PasswordPolicy_MaximumLength] DEFAULT ((128)) NOT NULL,
    [MinimumLowercase]               TINYINT                                     CONSTRAINT [DF_PasswordPolicy_MinimumLowercase] DEFAULT ((0)) NOT NULL,
    [MinimumUppercase]               TINYINT                                     CONSTRAINT [DF_PasswordPolicy_MinimumUppercase] DEFAULT ((0)) NOT NULL,
    [MinimumNumeric]                 TINYINT                                     CONSTRAINT [DF_PasswordPolicy_MinimumNumeric] DEFAULT ((0)) NOT NULL,
    [MinimumSpecialCharacter]        TINYINT                                     CONSTRAINT [DF_PasswordPolicy_MinimumSpecialCharacter] DEFAULT ((0)) NOT NULL,
    [AllowUsersToChangeOwnPassword]  BIT                                         CONSTRAINT [DF_PasswordPolicy_AllowUsersToChangeOwnPassword] DEFAULT ((1)) NOT NULL,
    [EnablePasswordExpiration]       BIT                                         CONSTRAINT [DF_PasswordPolicy_EnablePasswordExpiration] DEFAULT ((0)) NOT NULL,
    [PasswordExpirationPeriodInDays] SMALLINT                                    NULL,
    [PreventPasswordReuse]           BIT                                         CONSTRAINT [DF_PasswordPolicy_PreventPasswordReuse] DEFAULT ((0)) NOT NULL,
    [NumberOfPasswordsToRemember]    TINYINT                                     NULL,
    [UserId]                         BIGINT                                      NOT NULL,
    [SysStartDateTime]               DATETIME2 (0) GENERATED ALWAYS AS ROW START CONSTRAINT [DF_PasswordPolicy_SysStartDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [SysEndDateTime]                 DATETIME2 (0) GENERATED ALWAYS AS ROW END   CONSTRAINT [DF_PasswordPolicy_SysEndDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Ident.PasswordPolicy] PRIMARY KEY CLUSTERED ([PasswordPolicyId] ASC),
    CONSTRAINT [CHK_PasswordPolicy_NumberOfPasswordsToRemember] CHECK ([PreventPasswordReuse]=(0) AND isnull([NumberOfPasswordsToRemember],(0))=(0) OR [PreventPasswordReuse]=(1) AND [NumberOfPasswordsToRemember]>(0)),
    CONSTRAINT [CHK_PasswordPolicy_PasswordExpirationPeriodInDays] CHECK ([EnablePasswordExpiration]=(0) AND isnull([PasswordExpirationPeriodInDays],(0))=(0) OR [EnablePasswordExpiration]=(1) AND [PasswordExpirationPeriodInDays]>(0)),
    CONSTRAINT [FK_PasswordPolicy_PartyId] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]),
    PERIOD FOR SYSTEM_TIME ([SysStartDateTime], [SysEndDateTime])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[Ident].[PasswordPolicyHistory], DATA_CONSISTENCY_CHECK=ON));


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Created by User ID', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'UserId';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Number of previous passwords a user is not allowed to reuse', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'NumberOfPasswordsToRemember';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Prevent users from reusing a specified number of previous passwords', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'PreventPasswordReuse';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Number of days a password is valid for', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'PasswordExpirationPeriodInDays';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Enable user passwords to be valid for only the specified number of days', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'EnablePasswordExpiration';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Permit Users to Change Their Own Password', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'AllowUsersToChangeOwnPassword';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Minimum special characters required in a user password', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'MinimumSpecialCharacter';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Minimum numbers required in a user password', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'MinimumNumeric';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Minimum uppercase characters required in a user password', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'MinimumUppercase';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Minimum lowercase characters required in a user password', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'MinimumLowercase';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Specify the maximum number of characters allowed in a user password', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'MaximumLength';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Specify the minimum number of characters allowed in a user password', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'MinimumLength';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique Password Policy ID', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy', @level2type = N'COLUMN', @level2name = N'PasswordPolicyId';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'This table holds the RealPage Password Policy for each Portofio.', @level0type = N'SCHEMA', @level0name = N'Ident', @level1type = N'TABLE', @level1name = N'PasswordPolicy';

