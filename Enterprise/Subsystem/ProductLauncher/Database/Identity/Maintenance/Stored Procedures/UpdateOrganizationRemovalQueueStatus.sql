CREATE PROCEDURE [Maintenance].[UpdateOrganizationRemovalQueueStatus]
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
		IF @OrganizationRemovalQueueStatusId <> ( SELECT TOP (1) OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueue WHERE OrganizationRemovalQueueId = @OrganizationRemovalQueueId )
		BEGIN
			INSERT INTO Maintenance.OrganizationRemovalQueueHistory ( OrganizationRemovalQueueId, OrganizationRemovalQueueStatusId )
				VALUES ( @OrganizationRemovalQueueId, @OrganizationRemovalQueueStatusId )
			UPDATE Maintenance.OrganizationRemovalQueue SET OrganizationRemovalQueueStatusId = @OrganizationRemovalQueueStatusId WHERE OrganizationRemovalQueueId = @OrganizationRemovalQueueId
		END
	END

	SELECT @OrganizationRemovalQueueId AS Id
END;
GO
