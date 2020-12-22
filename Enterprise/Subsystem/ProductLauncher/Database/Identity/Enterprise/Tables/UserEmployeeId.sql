CREATE TABLE [Enterprise].[UserEmployeeId] (
    [UserEmployeeId] INT   IDENTITY (1, 1) NOT NULL,
    [UserLoginPersonaId] BIGINT  NOT NULL,
    [Employee] NVARCHAR(200),
    CONSTRAINT [PK_UserEmployee_UserEmployeeId] PRIMARY KEY CLUSTERED ([UserEmployeeId] ASC) WITH (FILLFACTOR = 80),
	CONSTRAINT [FK_UserEmployeeId_UserLoginPersona] FOREIGN KEY (UserLoginPersonaId) REFERENCES [Ident].[UserLoginPersona]([UserLoginPersonaId])
);
