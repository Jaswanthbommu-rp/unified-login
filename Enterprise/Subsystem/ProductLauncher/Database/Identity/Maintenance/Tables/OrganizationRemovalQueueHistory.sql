CREATE TABLE [Maintenance].[OrganizationRemovalQueueHistory]
(
	[OrganizationRemovalQueueHistoryId] INT IDENTITY NOT NULL,
	[OrganizationRemovalQueueId] INT NOT NULL,
	[OrganizationRemovalQueueStatusId] INT NOT NULL, 
	[StatusChangeDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 

	CONSTRAINT [PK_OrganizationRemovalQueueHistoryId] PRIMARY KEY CLUSTERED ([OrganizationRemovalQueueHistoryId]),
    CONSTRAINT [FK_OrganizationRemovalQueueHistory_OrganizationRemovalQueue] FOREIGN KEY ([OrganizationRemovalQueueId]) REFERENCES [Maintenance].[OrganizationRemovalQueue]([OrganizationRemovalQueueId]),
	CONSTRAINT [FK_OrganizationRemovalQueueHistory_OrganizationRemovalQueueStatus] FOREIGN KEY ([OrganizationRemovalQueueStatusId]) REFERENCES [Maintenance].[OrganizationRemovalQueueStatus]([OrganizationRemovalQueueStatusId]),
)