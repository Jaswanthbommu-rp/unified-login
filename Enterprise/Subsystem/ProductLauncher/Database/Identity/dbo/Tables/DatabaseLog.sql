CREATE TABLE [dbo].[DatabaseLog] (
    [DatabaseLogID] INT            IDENTITY (1, 1) NOT NULL,
    [PostTime]      DATETIME       NOT NULL,
    [DatabaseUser]  [sysname]      NOT NULL,
    [Event]         [sysname]      NOT NULL,
    [Schema]        [sysname]      NULL,
    [Object]        [sysname]      NULL,
    [TSQL]          NVARCHAR (MAX) NOT NULL,
    [XmlEvent]      XML            NOT NULL,
    CONSTRAINT [PK_DatabaseLog_DatabaseLogID] PRIMARY KEY NONCLUSTERED ([DatabaseLogID] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_DatabaseLog_PostTime]
ON [dbo].[DatabaseLog] ([PostTime]) WITH (ONLINE = ON)
GO

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Log to show schema changes that occur against the database and who performed them.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DatabaseLog';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The XML details of the event.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DatabaseLog', @level2type = N'COLUMN', @level2name = N'XmlEvent';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The T-SQL that was run.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DatabaseLog', @level2type = N'COLUMN', @level2name = N'TSQL';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the event occurred.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DatabaseLog', @level2type = N'COLUMN', @level2name = N'PostTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The type of event.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DatabaseLog', @level2type = N'COLUMN', @level2name = N'Event';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'User that performed the action', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DatabaseLog', @level2type = N'COLUMN', @level2name = N'DatabaseUser';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Identity Column for the table', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DatabaseLog', @level2type = N'COLUMN', @level2name = N'DatabaseLogID';

