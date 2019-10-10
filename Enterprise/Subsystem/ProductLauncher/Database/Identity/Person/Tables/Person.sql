CREATE TABLE [Person].[Person] (
    [PartyId]    BIGINT           NOT NULL,
    [Title]      NVARCHAR (50)  NULL,
    [FirstName]  [dbo].[Name]  NOT NULL,
    [MiddleName] [dbo].[Name]  NULL,
    [LastName]   [dbo].[Name]  NOT NULL,
    [Suffix]     NVARCHAR (10) NULL,
    [PreferredContactMethodId] INT NULL, 
    CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ([PartyId] ASC),
    CONSTRAINT [FK_Person_Party] FOREIGN KEY ([PartyId]) REFERENCES [Enterprise].[Party] ([PartyId]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

EXECUTE sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'This table contains information on People.',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The title of the person, such as Mr., Mrs., Ms.',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person',
	@level2type = N'COLUMN',
	@level2name = N'Title';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The Suffix of the person, such as MD',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person',
	@level2type = N'COLUMN',
	@level2name = N'Suffix';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The Party Id of the person.',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person',
	@level2type = N'COLUMN',
	@level2name = N'PartyId';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The Middle Name of the Person',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person',
	@level2type = N'COLUMN',
	@level2name = N'MiddleName';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The Last Name of the Person',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person',
	@level2type = N'COLUMN',
	@level2name = N'LastName';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'The First Name of the Person',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person',
	@level2type = N'COLUMN',
	@level2name = N'FirstName';
GO

EXECUTE sp_addextendedproperty
	@name = N'MS_Description',
	@value = N'Preferred Contact Method for a person',
	@level0type = N'SCHEMA',
	@level0name = N'Person',
	@level1type = N'TABLE',
	@level1name = N'Person',
	@level2type = N'COLUMN',
	@level2name = 'PreferredContactMethodId';
GO
CREATE NONCLUSTERED INDEX IDX_Person_Comp01 ON [Person].[Person]
(
	[PartyId] ASC
)
INCLUDE ( 	[Title],
	[FirstName],
	[MiddleName],
	[LastName],
	[Suffix]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE INDEX [IX_Person_FirstName]
ON [Person].[Person]
( [FirstName]
);
GO
CREATE INDEX [IX_Person_LastName]
ON [Person].[Person]
( LastName
);