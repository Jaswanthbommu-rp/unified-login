CREATE TABLE [Person].[ActivePersona]
(
[PartyId] [bigint] NOT NULL,
[PersonaId] [bigint] NOT NULL
)
GO
ALTER TABLE [Person].[ActivePersona] ADD CONSTRAINT [PK_ActivePersona] PRIMARY KEY CLUSTERED  ([PartyId], [PersonaId])
GO
ALTER TABLE [Person].[ActivePersona] ADD CONSTRAINT [FK_ActivePersona_Person] FOREIGN KEY ([PartyId]) REFERENCES [Person].[Person] ([PartyId])
GO
ALTER TABLE [Person].[ActivePersona] ADD CONSTRAINT [FK_ActivePersona_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
