CREATE TABLE [Person].[Persona] (
    [PersonaId]                BIGINT   IDENTITY (1, 1) NOT NULL,
    [UserLoginPersonaId]       BIGINT   DEFAULT ((-1)) NOT NULL,
    [PersonaTypeId]            INT      NOT NULL,
    [PersonaEnvironmentTypeId] INT      CONSTRAINT [DF_Persona_PersonaEnvironmentTypeId] DEFAULT ((1)) NOT NULL,
    [FromDate]                 DATETIME CONSTRAINT [DF__Persona__FromDat__41049384] DEFAULT (getutcdate()) NOT NULL,
    [ThruDate]                 DATETIME NULL,
    [IsDefault]                BIT      CONSTRAINT [DF__Persona__IsDefau__41F8B7BD] DEFAULT ((0)) NOT NULL,
    [PersonaName]              NVARCHAR(50) DEFAULT (('Primary')) NOT NULL, 
    CONSTRAINT [PK_Persona] PRIMARY KEY CLUSTERED ([PersonaId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_Persona_PersonaEnvironmentType] FOREIGN KEY ([PersonaEnvironmentTypeId]) REFERENCES [Person].[PersonaEnvironmentType] ([PersonaEnvironmentTypeID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Persona_PersonaType] FOREIGN KEY ([PersonaTypeId]) REFERENCES [Person].[PersonaType] ([PersonaTypeId]) ON DELETE CASCADE ON UPDATE CASCADE,
    --CONSTRAINT [FK_Persona_UserLoginPersona] FOREIGN KEY ([UserLoginPersonaId]) REFERENCES [Ident].[UserLoginPersona] ([UserLoginPersonaId]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_Person_Persona_UserLoginPersonaId]
	ON [Person].[Persona] ([UserLoginPersonaId])INCLUDE (FromDate, ThruDate)
GO
CREATE NONCLUSTERED INDEX IDX_Persona_PersonaID ON Person.Persona(PersonaId ASC) INCLUDE(UserLoginPersonaId);
GO
--CREATE NONCLUSTERED INDEX [IX_Person_Persona_UserId_OrganizationPartyId]
----    ON [Person].[Persona]([UserLoginPersonaId] ASC, [OrganizationPartyId] ASC) WITH (FILLFACTOR = 80);
--GO
--CREATE INDEX [IX_Persona_FromDate_ThruDate] ON [Person].[Persona] ([FromDate], [ThruDate]) INCLUDE ([OrganizationPartyId])
