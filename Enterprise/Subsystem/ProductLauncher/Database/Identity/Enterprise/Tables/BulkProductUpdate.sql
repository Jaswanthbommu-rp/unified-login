CREATE TABLE [Enterprise].[BulkProductUpdate]
(
	[IdentityColmnId] INT NOT NULL PRIMARY KEY,
    [EditorUserpersonaId] BIGINT NOT NULL,
    [SubjectUserpersonaId] BIGINT NOT NULL, 
    [ProductId] INT NOT NULL,
    [CreateDate] [datetime] NULL,
)
