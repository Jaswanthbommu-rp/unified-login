CREATE PROCEDURE [Enterprise].[ListCommunicationEvents]
	@FromRPID UNIQUEIDENTIFIER,
	@ToRPID UNIQUEIDENTIFIER
AS
BEGIN
	BEGIN TRY
		IF (@FromRPID IS NULL AND @ToRPID IS NULL)
		BEGIN
			SELECT NULL AS Id, 'Either FromRealPageID or ToRealPageID must be provided.' AS ErrorMessage
		END

		DECLARE @FromPartyID BIGINT = NULL
		DECLARE @ToPartyID BIGINT = NULL

		SET @FromPartyID = (SELECT PartyID FROM Enterprise.Party WHERE RealPageId = @FromRPID)
		SET @ToPartyID = (SELECT PartyID FROM Enterprise.Party WHERE RealPageId = @ToRPID)
		
		IF (@FromPartyID IS NOT NULL OR @ToPartyID IS NOT NULL)
		BEGIN
			SELECT CommunicationEventID,
				StatusTypeID,
				PartyContactMechanismIdFrom,
				PartyContactMechanismIdTo,
				[Started],
				[Ended],
				[Note]
			FROM Enterprise.CommunicationEvent
			WHERE (PartyContactMechanismIdFrom = @FromPartyID OR @FromPartyID IS NULL)
				AND (PartyContactMechanismIdTo = @ToPartyID OR @ToPartyID IS NULL);
		END
		ELSE
		BEGIN
			SELECT NULL AS Id, 'Given RealPageId not a valid Party.' AS ErrorMessage;
		END
	END TRY  
	BEGIN CATCH
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;