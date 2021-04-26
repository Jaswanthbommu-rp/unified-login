CREATE OR alter PROCEDURE [Maintenance].[UpdateOrganizationRemovalQueueStatus]
(
    @OrganizationRemovalQueueId INT,
	@OrganizationRemovalQueueStatus NVARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OrganizationRemovalQueueStatusId INT = -1
	SELECT @OrganizationRemovalQueueStatusId = @OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = @OrganizationRemovalQueueStatus
	
	IF @OrganizationRemovalQueueStatusId <> -1
	BEGIN
		UPDATE Maintenance.OrganizationRemovalQueue SET OrganizationRemovalQueueStatusId = @OrganizationRemovalQueueStatusId WHERE OrganizationRemovalQueueId = @OrganizationRemovalQueueId
	END

	SELECT @OrganizationRemovalQueueId AS Id
END;
GO
