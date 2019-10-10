CREATE TABLE [Enterprise].[Configuration]
(
[ConfigurationId] [int] NOT NULL IDENTITY(1, 1),
[CreateDate] [datetime] NOT NULL CONSTRAINT [DF__Configura__Creat__0D44F85C] DEFAULT (getutcdate())
)
GO
ALTER TABLE [Enterprise].[Configuration] ADD CONSTRAINT [PK_Configuration] PRIMARY KEY CLUSTERED  ([ConfigurationId])
GO
