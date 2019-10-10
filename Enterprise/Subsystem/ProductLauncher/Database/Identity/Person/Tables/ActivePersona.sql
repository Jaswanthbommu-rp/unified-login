CREATE TABLE [Person].[ActivePersona]
(
	[PartyId] BIGINT NOT NULL , 
    [PersonaId] BIGINT NOT NULL, 
    CONSTRAINT [PK_ActivePersona] PRIMARY KEY ([PartyId],[PersonaId]), 
    CONSTRAINT [FK_ActivePersona_Person] FOREIGN KEY ([PartyId]) REFERENCES [Person].[Person]([PartyId]) , 
    CONSTRAINT [FK_ActivePersona_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona]([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE
)
