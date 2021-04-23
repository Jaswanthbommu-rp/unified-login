CREATE TABLE [Maintenance].[OrganizationRemovalQueueError]
(
	[OrganizationRemovalQueueErrorId] INT IDENTITY NOT NULL,
	[OrganizationRemovalQueueId] INT NOT NULL,
	[ErrorMessage] VARCHAR(MAX) NOT NULL, 

	CONSTRAINT [PK_OrganizationRemovalQueueError] PRIMARY KEY CLUSTERED ([OrganizationRemovalQueueErrorId]),
    CONSTRAINT [FK_OrganizationRemovalQueueError_OrganizationRemovalQueue] FOREIGN KEY ([OrganizationRemovalQueueId]) REFERENCES [Maintenance].[OrganizationRemovalQueue]([OrganizationRemovalQueueId]),
)