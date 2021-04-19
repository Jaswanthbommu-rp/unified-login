
CREATE TABLE [Logging].[Activity]
( 
		[ActivityId] bigint IDENTITY(1, 1) NOT NULL, [LogTypeId] int NULL, [CorrelationId] nvarchar(36) NULL, [Message] nvarchar(400) NULL, [FromUserId] bigint NULL, [ToUserId] bigint NULL, [BooksMasterOrganizationId] int NULL, [BooksMasterPropertyId] bigint CONSTRAINT [DF_Activity_BooksMasterPropertyId] DEFAULT(NULL) NULL, [IsSystemAdminActivity] bit CONSTRAINT [DF_Activity_IsSystemAdminTask] DEFAULT(( 0 )) NULL, [ProductId] int NULL, [ServerId] int NULL, [ApplicationTimeStamp] datetime CONSTRAINT [DF__Activity__Timest__6FE99F9F] DEFAULT(GETUTCDATE()) NOT NULL, [DatabaseTimeStamp] datetime NULL, [OrganizationPartyId] INT NULL DEFAULT 0, SourceId NVARCHAR(50) NULL, MappingKey NVARCHAR(200) NULL, ContextId INT NULL, 
    CONSTRAINT [PK_Activity_1] PRIMARY KEY CLUSTERED([ActivityId] ASC), CONSTRAINT [FK_Activity_LogType] FOREIGN KEY([LogTypeId]) REFERENCES [Logging].[LogType]([LogTypeId]), CONSTRAINT [FK_Activity_Product] FOREIGN KEY([ProductId]) REFERENCES [Logging].[Product]([ProductID]), CONSTRAINT [FK_Activity_ServerName] FOREIGN KEY([ServerId]) REFERENCES [Logging].[ServerName]([ServerId]), CONSTRAINT [FK_Activity_UserLogin_FromUserId] FOREIGN KEY([FromUserId]) REFERENCES [Logging].[UserLogin]([UserId]), CONSTRAINT [FK_Activity_UserLogin_ToUserId] FOREIGN KEY([ToUserId]) REFERENCES [Logging].[UserLogin]([UserId])
);
GO

CREATE INDEX [IX_Activity_Comp01]
ON [Logging].[Activity]
( [BooksMasterOrganizationId], [ApplicationTimeStamp]
) 
	   INCLUDE( [ActivityId], [LogTypeId], [Message], [FromUserId], [ToUserId], [BooksMasterPropertyId], [IsSystemAdminActivity] );
GO

CREATE INDEX [IX_Activity_Comp02]
ON [Logging].[Activity]
( [LogTypeId], [BooksMasterOrganizationId], [ApplicationTimeStamp]
) 
	   INCLUDE( [ActivityId], [Message], [FromUserId], [ToUserId], [BooksMasterPropertyId], [IsSystemAdminActivity] );
GO

CREATE INDEX [IX_Activity_OrgPartyId_Comp01]
ON [Logging].[Activity]
( [OrganizationPartyId], [ApplicationTimeStamp]
) 
	   INCLUDE( [ActivityId], [LogTypeId], [Message], [FromUserId], [ToUserId], [BooksMasterPropertyId], [IsSystemAdminActivity] );
GO

CREATE INDEX [IX_Activity_OrgPartyId_Comp02]
ON [Logging].[Activity]
( [LogTypeId], [OrganizationPartyId], [ApplicationTimeStamp]
) 
	   INCLUDE( [ActivityId], [Message], [FromUserId], [ToUserId], [BooksMasterPropertyId], [IsSystemAdminActivity] );
GO

CREATE INDEX [IX_UserLogin_RealPageId_01]
ON [Logging].[UserLogin]
( [RealPageId]
);
GO
--CREATE INDEX [IX_Activity_CompPLA]
--ON [Logging].[Activity]
--( [ProductId], [LogTypeId], [ApplicationTimeStamp]
--) 
--	   INCLUDE( [ActivityId], [FromUserId] );

CREATE INDEX [IX_Activity_COMPLA01]
ON [Logging].[Activity]
( [LogTypeId], [ApplicationTimeStamp]
) 
	   INCLUDE( [ActivityId], [FromUserId], [ProductId] );
GO

CREATE INDEX [IX_Activity_CompLFBA]
ON [Logging].[Activity]
( [LogTypeId], [FromUserId], [BooksMasterOrganizationId], [ApplicationTimeStamp]
) 
	   INCLUDE( [Message], [ToUserId] );
GO
--CREATE INDEX [IX_Activity_LogTypeId_FromUserId_BooksMasterOrganizationId_ApplicationTimeStamp_F96DF]
--ON [Logging].[Activity]
--( [LogTypeId], [FromUserId], [BooksMasterOrganizationId], [ApplicationTimeStamp]
--) 
--	   INCLUDE( [ToUserId] );
--GO

CREATE INDEX [IX_Activity_CompPLA02]
ON [Logging].[Activity]
( [ProductId], [LogTypeId], [ApplicationTimeStamp]
) 
	   INCLUDE( [FromUserId] );
GO

CREATE INDEX [IX_Activity_CompLTBA]
ON [Logging].[Activity]
( [LogTypeId], [ToUserId], [BooksMasterOrganizationId], [ApplicationTimeStamp]
) 
	   INCLUDE( [FromUserId] );
GO







