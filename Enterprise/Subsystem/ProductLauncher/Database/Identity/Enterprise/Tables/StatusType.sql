CREATE TABLE [Enterprise].[StatusType]
(
	[StatusTypeId] INT NOT NULL IDENTITY(1,1),
	[Name] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_StatusType] PRIMARY KEY (StatusTypeId),
)
