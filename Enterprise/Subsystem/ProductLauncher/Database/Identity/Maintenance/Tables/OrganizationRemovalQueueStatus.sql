CREATE TABLE [Maintenance].[OrganizationRemovalQueueStatus]
(
	[OrganizationRemovalQueueStatusId] INT NOT NULL,
	[Name] NVARCHAR(100) NOT NULL,
	CONSTRAINT [PK_OrganizationRemovalQueueStatusId] PRIMARY KEY CLUSTERED ([OrganizationRemovalQueueStatusId])
)