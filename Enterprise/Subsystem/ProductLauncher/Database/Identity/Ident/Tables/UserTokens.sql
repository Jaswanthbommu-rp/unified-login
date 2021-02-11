CREATE TABLE [Ident].[UserTokens]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] BIGINT NOT NULL, 
    [LoginProvider] NCHAR(100) NOT NULL, 
    [Name] NCHAR(50) NOT NULL, 
    [Value] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_UserTokens_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin]([UserId])
)

GO

CREATE NONCLUSTERED INDEX [IX_UserTokens_UserId_LoginProvider_Name] ON [Ident].[UserTokens] ([UserId], [LoginProvider], [Name])
