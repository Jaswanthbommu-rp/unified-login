CREATE TABLE [dbo].[DatabaseLog]
(
[DatabaseLogID] [int] NOT NULL IDENTITY(1, 1),
[PostTime] [datetime] NOT NULL,
[DatabaseUser] [sys].[sysname] NOT NULL,
[Event] [sys].[sysname] NOT NULL,
[Schema] [sys].[sysname] NULL,
[Object] [sys].[sysname] NULL,
[TSQL] [nvarchar] (max) NOT NULL,
[XmlEvent] [xml] NOT NULL
)
GO
ALTER TABLE [dbo].[DatabaseLog] ADD CONSTRAINT [PK_DatabaseLog_DatabaseLogID] PRIMARY KEY NONCLUSTERED  ([DatabaseLogID])
GO
EXEC sp_addextendedproperty N'MS_Description', N'Log to show schema changes that occur against the database and who performed them.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', NULL, NULL
GO
EXEC sp_addextendedproperty N'MS_Description', N'Identity Column for the table', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseLogID'
GO
EXEC sp_addextendedproperty N'MS_Description', N'User that performed the action', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'DatabaseUser'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The type of event.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'Event'
GO
EXEC sp_addextendedproperty N'MS_Description', N'Date the event occurred.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'PostTime'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The T-SQL that was run.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'TSQL'
GO
EXEC sp_addextendedproperty N'MS_Description', N'The XML details of the event.', 'SCHEMA', N'dbo', 'TABLE', N'DatabaseLog', 'COLUMN', N'XmlEvent'
GO
