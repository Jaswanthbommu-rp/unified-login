CREATE TABLE [Ident].[UserLoginPersona] (
    [UserLoginPersonaId]  BIGINT   IDENTITY (1, 1) NOT NULL,
    [UserLoginId]         BIGINT   NOT NULL,
    [StatusTypeId]        INT      NOT NULL,
    [OrganizationPartyId] BIGINT   NOT NULL,
    [PrimaryOrganization] BIT      CONSTRAINT [DF_UserLoginPersona_PrimaryProfile] DEFAULT ('True') NOT NULL,
    [FromDate]            DATETIME NOT NULL,
    [ThruDate]            DATETIME NULL,
    [StatusThruDate]      DATETIME NULL,
    [LastLoginDate]       DATETIME NULL,
    [IsDelegateAdmin]     BIT  NOT NULL DEFAULT ((0)),
    [IsRPEmployee]        BIT      NOT NULL CONSTRAINT [DF_UserLoginPersona_IsRPEmployee] DEFAULT ((0)),
    [UserDeactivationDate] DATETIME NULL DEFAULT NULL,
    CONSTRAINT [PK_UserLoginPersona] PRIMARY KEY CLUSTERED ([UserLoginPersonaId] ASC),
	CONSTRAINT [FK_UserLoginPersona_Organization] FOREIGN KEY ([OrganizationPartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]),
    CONSTRAINT [FK_UserLoginPersona_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId]),
    CONSTRAINT [FK_UserLoginPersona_UserLogin] FOREIGN KEY ([UserLoginId]) REFERENCES [Ident].[UserLogin] ([UserId])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UNIQUEUserLoginPersona]
    ON [Ident].[UserLoginPersona]([UserLoginId] ASC, [OrganizationPartyId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_UserLoginPersona_UserLoginId]
    ON [Ident].[UserLoginPersona]([UserLoginId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_UserLoginPersona_OrganizationPartyId]
    ON [Ident].[UserLoginPersona]([OrganizationPartyId] ASC)
    INCLUDE([UserLoginPersonaId], [UserLoginId], [StatusTypeId]);

