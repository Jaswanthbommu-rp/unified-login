CREATE PROCEDURE [Maintenance].[ListOrganizationToDelete]
(
    @BatchSize INT,
    @RetryCount TINYINT = 3,
    @IncludeErrorRecord BIT = 'True'
)
AS
BEGIN

    SET NOCOUNT ON;
    DECLARE @PBFiltered TABLE
    (
		OrganizationRemovalQueueId INT NOT NULL,
		OrganizationPartyId INT NOT NULL,
		OrganizationRealPageId UNIQUEIDENTIFIER NOT NULL,
		OrganizationRemovalQueueStatusId INT NOT NULL
    );

	DECLARE @OrganizationRemovalQueueStatusId INT

	SELECT @OrganizationRemovalQueueStatusId = OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE [Name] = 'Database Removal Failed'

    BEGIN TRANSACTION; -- HAve to lock the tables so that another process can't come in and scoop up our waiting processes

	;WITH QueueErrorList ( errorcount, queueid ) AS (SELECT COUNT(1), OrganizationRemovalQueueId 
				FROM Maintenance.OrganizationRemovalQueueHistory ORQ 
				INNER JOIN Maintenance.OrganizationRemovalQueueStatus ORQS ON ORQS.OrganizationRemovalQueueStatusId = ORQ.OrganizationRemovalQueueStatusId
				WHERE ORQS.Name = 'Database Removal Failed' AND DATEDIFF(DD, StatusChangeDate, GETUTCDATE()) < 2 GROUP BY OrganizationRemovalQueueId
			)
	,batchtoprocess as (
		SELECT
			   OrganizationRemovalQueueId,
			   OrganizationPartyId,
			   OrganizationRealPageId,
			   OrganizationRemovalQueueStatusId

		FROM Maintenance.OrganizationRemovalQueue ORQ
		LEFT OUTER JOIN QueueErrorList QEL ON QEL.queueid = ORQ.OrganizationRemovalQueueId
		WHERE (
				  @IncludeErrorRecord = 'True'
				  AND
				  (
					  OrganizationRemovalQueueStatusId IN (SELECT OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name IN ('Database Removal Failed')
					  AND qel.errorcount < @RetryCount
				  )
			  )
			  OR
			  (
				OrganizationRemovalQueueStatusId IN (SELECT OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name IN ('Pending Processing'))
				AND @IncludeErrorRecord = 'False'
			  )
		)
	)
    INSERT INTO @PBFiltered
    (
		OrganizationRemovalQueueId,
		OrganizationPartyId,
		OrganizationRealPageId,
		OrganizationRemovalQueueStatusId
    )
	SELECT TOP (@BatchSize)
		OrganizationRemovalQueueId,
			   OrganizationPartyId,
			   OrganizationRealPageId,
			   @OrganizationRemovalQueueStatusId
		FROM
			batchtoprocess

    UPDATE ORQ
    SET OrganizationRemovalQueueStatusId = (SELECT OrganizationRemovalQueueStatusId FROM Maintenance.OrganizationRemovalQueueStatus WHERE name = 'Pending Database Removal' )
    FROM Maintenance.OrganizationRemovalQueue ORQ
        JOIN @PBFiltered F
            ON F.OrganizationRemovalQueueId = ORQ.OrganizationRemovalQueueId

    SELECT OrganizationRemovalQueueId,
           OrganizationPartyId,
           OrganizationRealPageId,
           OrganizationRemovalQueueStatusId
    FROM @PBFiltered;

    COMMIT TRANSACTION;
END;
GO
