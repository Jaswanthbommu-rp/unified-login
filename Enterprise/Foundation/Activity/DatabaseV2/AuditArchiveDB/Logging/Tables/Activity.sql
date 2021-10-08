CREATE TABLE [Logging].[Activity] (
    [ActivityId]           BIGINT         NOT NULL,
    [OrganizationPartyId]  BIGINT         NOT NULL CONSTRAINT [DF_Activity_OrganizationPartyId] DEFAULT ((0)),
    [LogTypeId]            INT            NULL,
    [Message]              NVARCHAR (400) NULL,
    [ContextId]            NVARCHAR (100) NULL,
    [ContextReferenceId]   NVARCHAR (200) NULL,
    [ApplicationTimeStamp] DATETIME       NOT NULL CONSTRAINT [DF_Activity_ApplicationTimeStamp] DEFAULT (getutcdate()),
    [CreatedBy]            BIGINT         NULL,
    [CreatedDate]          DATETIME       NOT NULL CONSTRAINT [DF_Activity_CreatedDate] DEFAULT (getutcdate()),
    CONSTRAINT [PK_Activity_ActivityId] PRIMARY KEY CLUSTERED ([ActivityId] ASC),
    CONSTRAINT [FK_Activity_LogTypeId] FOREIGN KEY ([LogTypeId]) REFERENCES [Logging].[LogType] ([LogTypeId])
);

