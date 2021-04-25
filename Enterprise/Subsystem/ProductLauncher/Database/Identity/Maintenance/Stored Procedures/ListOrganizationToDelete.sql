CREATE OR alter PROCEDURE [Maintenance].[ListOrganizationToDelete]
(
    @BatchSize INT,
    @RetryCount TINYINT = 3
)
AS
BEGIN

    SET NOCOUNT ON;
    DECLARE @PBFiltered TABLE
    (
		OrganizationRemovalQueueId INT NOT NULL,
		OrganizationPartyId INT NOT NULL,
		OrganizationRealPageId UNIQUEIDENTIFIER NOT NULL,
		OrganizationRemovalQueueStatusId INT NOT NULL,
		OrganizationRemoveUDMData TINYINT NOT NULL,
		OrganizationRemovalRetryCount TINYINT NOT NULL
    );

	DECLARE @OrganizationRemovalQueueStatusId INT

	SELECT @OrganizationRemovalQueueStatusId = OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE [Name] = 'Pending Database Removal'

    BEGIN TRANSACTION; -- HAve to lock the tables so that another process can't come in and scoop up our waiting processes

	;WITH BatchToProcess as (
		SELECT
			   OrganizationRemovalQueueId,
			   OrganizationPartyId,
			   OrganizationRealPageId,
			   OrganizationRemovalQueueStatusId,
			   OrganizationRemoveUDMData,
			   OrganizationRemovalRetryCount

		FROM Maintenance.OrganizationRemovalQueue
		WHERE
			OrganizationRemovalRetryCount <= @RetryCount
			AND
			OrganizationRemovalQueueStatusId IN (SELECT OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name IN ('Pending Processing', 'Database Removal Failed'))
		)
    INSERT INTO @PBFiltered
    (
		OrganizationRemovalQueueId,
		OrganizationPartyId,
		OrganizationRealPageId,
		OrganizationRemovalQueueStatusId,
		OrganizationRemoveUDMData,
		OrganizationRemovalRetryCount
    )
	SELECT TOP (@BatchSize)
		OrganizationRemovalQueueId,
			   OrganizationPartyId,
			   OrganizationRealPageId,
			   @OrganizationRemovalQueueStatusId,
			   OrganizationRemoveUDMData,
			   OrganizationRemovalRetryCount
		FROM
			BatchToProcess

    UPDATE ORQ
    SET OrganizationRemovalQueueStatusId = (SELECT TOP (1) OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE name = 'Pending Database Removal' ORDER BY OrganizationRemovalQueueStatusId )
    FROM Maintenance.OrganizationRemovalQueue ORQ
        JOIN @PBFiltered F
            ON F.OrganizationRemovalQueueId = ORQ.OrganizationRemovalQueueId

    SELECT 
		OrganizationRemovalQueueId,
		OrganizationPartyId,
		OrganizationRealPageId,
		OrganizationRemovalQueueStatusId,
		OrganizationRemoveUDMData,
		OrganizationRemovalRetryCount
    FROM @PBFiltered;

    COMMIT TRANSACTION;
END;
GO
