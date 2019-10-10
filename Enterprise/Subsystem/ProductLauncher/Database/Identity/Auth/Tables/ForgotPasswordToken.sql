CREATE TABLE [Auth].[PasswordToken]
(
	[PasswordTokenId] INT NOT NULL PRIMARY KEY, 
	[UserId] int NOT NULL,
    [Token] UNIQUEIDENTIFIER NOT NULL, 
    [CreateDateTime] SMALLDATETIME NOT NULL
)
