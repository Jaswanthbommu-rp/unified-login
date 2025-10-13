CREATE TABLE [Enterprise].[EmailDomain]
(
	[EmailDomainId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[EmailAddress] VARCHAR(255) NOT NULL, 
	[IsActive] BIT NOT NULL DEFAULT (0)
)