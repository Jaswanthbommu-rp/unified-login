CREATE TABLE [Person].[Persona]
(
[PersonaId] [bigint] NOT NULL IDENTITY(1, 1),
[PersonPartyId] [bigint] NOT NULL,
[OrganizationPartyId] [bigint] NOT NULL,
[PersonaTypeId] [int] NOT NULL,
[PersonaEnvironmentTypeId] [int] NOT NULL CONSTRAINT [DF_Persona_PersonaEnvironmentTypeId] DEFAULT ((1)),
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__Persona__FromDat__41049384] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL,
[IsDefault] [bit] NOT NULL CONSTRAINT [DF__Persona__IsDefau__41F8B7BD] DEFAULT ((0))
)
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE TRIGGER [Person].[insertPersonaTrigger]
ON [Person].[Persona]
AFTER INSERT
AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO Person.ActivePersona (   PartyId ,
                                             PersonaId
                                         )
                    SELECT Inserted.PersonPartyId ,
                           Inserted.PersonaId
                    FROM   Inserted;

    END;
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [PK_Persona] PRIMARY KEY CLUSTERED  ([PersonaId])
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [FK_Persona_Party] FOREIGN KEY ([PersonPartyId]) REFERENCES [Person].[Person] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [FK_Persona_PersonaEnvironmentType] FOREIGN KEY ([PersonaEnvironmentTypeId]) REFERENCES [Person].[PersonaEnvironmentType] ([PersonaEnvironmentTypeID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Person].[Persona] ADD CONSTRAINT [FK_Persona_PersonaType] FOREIGN KEY ([PersonaTypeId]) REFERENCES [Person].[PersonaType] ([PersonaTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
