CREATE TABLE [Logging].[UserLogin] (
    [UserId]        BIGINT           NOT NULL,
    [LoginName]     NVARCHAR (255)   NULL,
    [FirstName]     NVARCHAR (50)    NULL,
    [LastName]      NVARCHAR (50)    NULL,
    [RealPageId]    UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_UserLogin_UserId] PRIMARY KEY CLUSTERED ([UserId] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_UserLogin_RealPageId]
ON [Logging].[UserLogin] 
(
	[RealPageId]
)
GO


