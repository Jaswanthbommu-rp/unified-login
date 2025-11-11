CREATE TABLE [Logging].[Activity] (
    [ActivityId]           BIGINT         NOT NULL,
    [OrganizationPartyId]  BIGINT         NOT NULL CONSTRAINT [DF_Activity_OrganizationPartyId] DEFAULT ((0)),
    [LogTypeId]            INT            NULL,
    [Message]              NVARCHAR (800) NULL,
    [ContextId]            NVARCHAR (100) NULL,
    [ContextReferenceId]   NVARCHAR (200) NULL,
    [ApplicationCorrelationId]  NVARCHAR(200)   NULL,
    [ApplicationTimeStamp] DATETIME       NOT NULL CONSTRAINT [DF_Activity_ApplicationTimeStamp] DEFAULT (getutcdate()),
    [CreatedBy]            BIGINT         NULL,
    [CreatedDate]          DATETIME       NOT NULL CONSTRAINT [DF_Activity_CreatedDate] DEFAULT (getutcdate()),
    [IsRealPageEmployee]   BIT            NULL CONSTRAINT [DF_Activity_IsRealPageEmployee] DEFAULT((0)),  
    CONSTRAINT [PK_Activity_ActivityId] PRIMARY KEY CLUSTERED ([ActivityId] ASC),
    CONSTRAINT [FK_Activity_LogTypeId] FOREIGN KEY ([LogTypeId]) REFERENCES [Logging].[LogType] ([LogTypeId])
);
GO

CREATE NONCLUSTERED INDEX [IX_Activity_OrgPartyId_AppTimeStamp] 
ON [Logging].[Activity]
(
	[OrganizationPartyId] ASC,
	[ApplicationTimeStamp] ASC
)
INCLUDE([ActivityId],[LogTypeId],[Message],[ContextId],[ContextReferenceId],[CreatedBy]) 
GO

CREATE NONCLUSTERED INDEX [IX_Activity_OrgPartyId_AppTimeStamp_LogtypeId]
ON [Logging].[Activity] 
(
	[OrganizationPartyId],
	[LogTypeId],
	[ApplicationTimeStamp]
)
INCLUDE ([ActivityId],[Message],[ContextId],[ContextReferenceId],[CreatedBy])
GO


CREATE NONCLUSTERED INDEX [IX_Activity_OrgPartyId_AppTimeStamp_CreatedBy]
ON [Logging].[Activity] 
(
	[OrganizationPartyId],
	[CreatedBy],
	[ApplicationTimeStamp]
)
INCLUDE ([LogTypeId],[ContextReferenceId])
GO

CREATE NONCLUSTERED INDEX [IX_Activity_LTId_IsRPE_ATS] ON [Logging].[Activity]
(
	[LogTypeId] ASC,
	[IsRealPageEmployee] ASC,
	[ApplicationTimeStamp] ASC
)
INCLUDE([Message],[ContextId],[ContextReferenceId],[CreatedBy]) ON [PRIMARY]
GO
