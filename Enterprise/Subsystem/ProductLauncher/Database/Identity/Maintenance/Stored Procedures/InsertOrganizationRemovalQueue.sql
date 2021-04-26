CREATE PROCEDURE [Maintenance].[InsertOrganizationRemovalQueue]
(
		@OrganizationPartyId BIGINT,
		@OrganizationRealPageId UNIQUEIDENTIFIER,
		@OrganizationCustomerMasterId BIGINT,
		@OrganizationDomain NVARCHAR(20),
		@OrganizationName NVARCHAR(150),
		@OrganizationRemoveUDMData TINYINT,
		@OrganizationRemovalQueueStatusId INT,
		@OrganizationRemovalRetryCount TINYINT,
		@RequestedBy NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

	INSERT INTO Maintenance.OrganizationRemovalQueue
	(
	    OrganizationPartyId,
	    OrganizationRealPageId,
	    OrganizationCustomerMasterId,
	    OrganizationDomain,
	    OrganizationName,
	    OrganizationRemoveUDMData,
	    OrganizationRemovalQueueStatusId,
	    OrganizationRemovalRetryCount,
	    RequestedBy
	)
	VALUES
	(   @OrganizationPartyId,
	    @OrganizationRealPageId,
	    @OrganizationCustomerMasterId,
	    @OrganizationDomain,
	    @OrganizationName,
	    @OrganizationRemoveUDMData,
	    @OrganizationRemovalQueueStatusId,
	    @OrganizationRemovalRetryCount,
	    @RequestedBy
	);

    SELECT 
		OrganizationRemovalQueueId,
		OrganizationPartyId,
		OrganizationRealPageId,
		OrganizationCustomerMasterId,
		OrganizationDomain,
		OrganizationName,
		OrganizationRemoveUDMData,
		OrganizationRemovalQueueStatusId,
		OrganizationRemovalRetryCount,
		RequestedBy,
		AddedDate 
	FROM 
	Maintenance.OrganizationRemovalQueue 
		WHERE OrganizationRemovalQueueId = SCOPE_IDENTITY()
END;
