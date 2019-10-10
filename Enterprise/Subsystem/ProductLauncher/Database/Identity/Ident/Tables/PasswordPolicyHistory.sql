CREATE TABLE [Ident].[PasswordPolicyHistory] (
    [PasswordPolicyId]               INT           NOT NULL,
    [PartyId]                        BIGINT        NOT NULL,
    [MinimumLength]                  TINYINT       NOT NULL,
    [MaximumLength]                  TINYINT       NOT NULL,
    [MinimumLowercase]               TINYINT       NOT NULL,
    [MinimumUppercase]               TINYINT       NOT NULL,
    [MinimumNumeric]                 TINYINT       NOT NULL,
    [MinimumSpecialCharacter]        TINYINT       NOT NULL,
    [AllowUsersToChangeOwnPassword]  BIT           NOT NULL,
    [EnablePasswordExpiration]       BIT           NOT NULL,
    [PasswordExpirationPeriodInDays] SMALLINT      NULL,
    [PreventPasswordReuse]           BIT           NOT NULL,
    [NumberOfPasswordsToRemember]    TINYINT       NULL,
    [UserId]                         BIGINT        NOT NULL,
    [SysStartDateTime]               DATETIME2 (0) NOT NULL,
    [SysEndDateTime]                 DATETIME2 (0) NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_PasswordPolicyHistory]
    ON [Ident].[PasswordPolicyHistory]([SysEndDateTime] ASC, [SysStartDateTime] ASC) WITH (DATA_COMPRESSION = PAGE);

