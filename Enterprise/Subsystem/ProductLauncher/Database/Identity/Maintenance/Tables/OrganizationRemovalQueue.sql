CREATE TABLE [Maintenance].[OrganizationRemovalQueue]
(
	[OrganizationRemovalQueueId] INT IDENTITY NOT NULL,
	[OrganizationPartyId] BIGINT NOT NULL,
	[OrganizationRealPageId] UNIQUEIDENTIFIER NOT NULL,
	[OrganizationCustomerMasterId] BIGINT NOT NULL,
	[OrganizationDomain] NVARCHAR(20),
	[OrganizationName] NVARCHAR(150) NOT NULL,
	[OrganizationRemoveUDMData] TINYINT NOT NULL DEFAULT 0, 
	[OrganizationRemovalQueueStatusId] INT NOT NULL DEFAULT 0,
	[OrganizationRemovalRetryCount] TINYINT NOT NULL DEFAULT 0,
	[RequestedBy] NVARCHAR(50) NOT NULL,
	[AddedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
	CONSTRAINT [PK_OrganizationRemovalQueueId] PRIMARY KEY CLUSTERED ([OrganizationRemovalQueueId]),
    CONSTRAINT [FK_OrganizationRemovalQueue_OrganizationRemovalQueueStatus_OrganizationRemovalQueueStatusId] FOREIGN KEY ([OrganizationRemovalQueueStatusId]) REFERENCES [Maintenance].[OrganizationRemovalQueueStatus]([OrganizationRemovalQueueStatusId]),
)