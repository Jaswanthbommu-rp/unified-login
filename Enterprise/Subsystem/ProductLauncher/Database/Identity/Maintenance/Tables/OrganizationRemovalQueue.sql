CREATE TABLE [Maintenance].[OrganizationRemovalQueue]
(
	[OrganizationRemovalQueueId] INT IDENTITY NOT NULL,
	[OrganizationPartyId] BIGINT NOT NULL,
	[OrganizationRealPageId] UNIQUEIDENTIFIER NOT NULL,
	[OrganizationName] NVARCHAR(300) NOT NULL,
	[OrganizationRemovalQueueStatusId] INT NOT NULL DEFAULT 0, 
	[AddedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
	[CompletedDate] DATETIME2 NULL,
	CONSTRAINT [PK_OrganizationRemovalQueueId] PRIMARY KEY CLUSTERED ([OrganizationRemovalQueueId]),
    CONSTRAINT [FK_OrganizationRemovalQueue_OrganizationRemovalQueueStatus_OrganizationRemovalQueueStatusId] FOREIGN KEY ([OrganizationRemovalQueueStatusId]) REFERENCES [Maintenance].[OrganizationRemovalQueueStatus]([OrganizationRemovalQueueStatusId]),
)