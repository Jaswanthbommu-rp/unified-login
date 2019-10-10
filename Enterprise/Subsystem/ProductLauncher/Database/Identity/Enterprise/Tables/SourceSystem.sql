CREATE TABLE [Enterprise].[SourceSystem]
(
	[SourceSystemId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_SourceSystem] PRIMARY KEY ([SourceSystemId]), 
    CONSTRAINT [AK_SourceSystem_Name] UNIQUE ([Name])
)
