CREATE TABLE [Person].[PersonaType]
(
	[PersonaTypeId] INT NOT NULL IDENTITY , 
	[Name] VARCHAR(50), 
    CONSTRAINT [PK_PersonaType] PRIMARY KEY ([PersonaTypeId])
)
