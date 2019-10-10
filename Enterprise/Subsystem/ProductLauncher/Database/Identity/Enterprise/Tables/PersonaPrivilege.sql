CREATE TABLE [Enterprise].[PersonaPrivilege] (
    [UserPrivilegeID] INT      IDENTITY (1, 1) NOT NULL,
    [PersonaId]       BIGINT   NULL,
    [RoleID]          INT      NULL,
    [GroupId]         INT      DEFAULT ((1)) NULL,
    [FromDate]        DATETIME NULL,
    [ThruDate]        DATETIME NULL,
    CONSTRAINT [PK_UserPrivilege] PRIMARY KEY CLUSTERED ([UserPrivilegeID] ASC),
    CONSTRAINT [FK_PersonaPrivilege_Group] FOREIGN KEY ([GroupId]) REFERENCES [Enterprise].[Group] ([GroupId]),
    CONSTRAINT [FK_PersonaPrivilege_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_UserPrivilege_Role] FOREIGN KEY ([RoleID]) REFERENCES [Enterprise].[Role] ([RoleID])
);
GO

/****** Object:  Index [IX_PersonaPrivilege]    Script Date: 7/24/2017 3:11:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_PersonaPrivilege] ON [Enterprise].[PersonaPrivilege]
(
	[GroupId] ASC,
	[PersonaId] ASC,
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX IX_PersonaPrivilege_PersonaId
ON [Enterprise].[PersonaPrivilege] ([PersonaId])
GO

CREATE NONCLUSTERED INDEX [IX_Enterprise_PersonaPrivilege_PersonaId_RoleId_UserPrivilegeID] ON [Enterprise].[PersonaPrivilege]
(
	[PersonaId] ASC,
	[RoleID] ASC,
	[UserPrivilegeID] ASC
) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO

