CREATE TABLE [Person].[Person]
(
[PartyId] [bigint] NOT NULL,
[Title] [nvarchar] (50) NULL,
[FirstName] [dbo].[Name] NOT NULL,
[MiddleName] [dbo].[Name] NULL,
[LastName] [dbo].[Name] NOT NULL,
[Suffix] [nvarchar] (10) NULL,
[PreferredContactMethodId] [int] NULL
)
GO
ALTER TABLE [Person].[Person] ADD CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED  ([PartyId])
GO
ALTER TABLE [Person].[Person] ADD CONSTRAINT [FK_Person_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
EXEC sp_addextendedproperty N'MS_Description', N'This table contains information on People.', 'SCHEMA', N'Person', 'TABLE', N'Person', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'The First Name of the Person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'FirstName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Last Name of the Person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'LastName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Middle Name of the Person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'MiddleName'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Party Id of the person.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'PartyId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Preferred Contact Method for a person', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'PreferredContactMethodId'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The Suffix of the person, such as MD', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Suffix'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The title of the person, such as Mr., Mrs., Ms.', 'SCHEMA', N'Person', 'TABLE', N'Person', 'COLUMN', N'Title'
GO
