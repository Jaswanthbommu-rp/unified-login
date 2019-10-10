CREATE TABLE [Enterprise].[RoleValueType]
(
	[RoleValueTypeId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Value] NVARCHAR(200) NOT NULL DEFAULT -1, 
    [ShortName] nvarchar(50) NULL,
    [Description] NVARCHAR(200) NULL, 
    [StatusTypeId] INT NULL ,
	[LastUpdateDateTime] DATETIME NULL DEFAULT GETUTCDATE(), 
    CONSTRAINT FK_RoleValueType_StatusType FOREIGN KEY (StatusTypeId) REFERENCES Enterprise.StatusType	(StatusTypeId) 
)
GO
CREATE NONCLUSTERED INDEX [IX_RoleValueType] ON [Enterprise].[RoleValueType]
(
	[RoleValueTypeId] ASC,
	[Value] ASC,
	[StatusTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO