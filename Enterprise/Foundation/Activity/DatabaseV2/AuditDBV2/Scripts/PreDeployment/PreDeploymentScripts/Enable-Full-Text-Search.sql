GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'FullTextCatalog') 
BEGIN 
	CREATE FULLTEXT CATALOG FullTextCatalog AS DEFAULT;
END
GO
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'PK_Activity_ActivityId' AND object_id = OBJECT_ID('[Logging].[Activity]')) 
BEGIN 
	CREATE FULLTEXT INDEX ON [Logging].[Activity] (Message) KEY INDEX PK_Activity_ActivityId WITH STOPLIST = OFF 
END
GO