CREATE TABLE [Enterprise].[Configuration]
(
	[ConfigurationId] INT NOT NULL IDENTITY, 
    [CreateDate] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_Configuration] PRIMARY KEY ([ConfigurationId])	
)
